"""
Typecho 到 Mblog 数据迁移脚本
功能：
  1. 解析 typecho.sql 导出文件
  2. 通过 API 创建分类（中文名 + 英文 slug）
  3. 创建文章（slug=show-{cid}，HTML 自动转 Markdown）
  4. 自动创建标签并提取文章关键词作为标签
  5. 自动关联分类和标签

用法：
  python scripts/migrate_typecho.py

环境变量（可选）：
  MBLOG_API_BASE  - API 地址，默认 http://localhost:5000
  MBLOG_USERNAME  - 用户名，默认 zhuge
  MBLOG_PASSWORD  - 密码，默认 admin123
"""

import re
import json
import sys
import os
import time
import html
from datetime import datetime
from collections import defaultdict
from urllib.parse import quote

import requests
from pypinyin import lazy_pinyin
import html2text

# ==================== 配置 ====================

API_BASE = os.environ.get("MBLOG_API_BASE", "http://localhost:5000")
USERNAME = os.environ.get("MBLOG_USERNAME", "zhuge")
PASSWORD = os.environ.get("MBLOG_PASSWORD", "admin123")
SQL_FILE = os.path.join(os.path.dirname(os.path.dirname(__file__)), "typecho.sql")

# 分类配色（品牌色）
CATEGORY_COLORS = [
    "#6366f1", "#ec4899", "#f59e0b", "#10b981", "#3b82f6",
    "#8b5cf6", "#ef4444", "#14b8a6", "#f97316", "#06b6d4",
    "#84cc16", "#a855f7", "#e11d48", "#0ea5e9", "#d946ef",
    "#22c55e", "#64748b", "#78716c", "#a3e635", "#2dd4bf",
]

# 标签预定义颜色
TAG_COLORS = [
    "#6366f1", "#512BD4", "#000000", "#61DAFB", "#3178C6",
    "#336791", "#2496ED", "#FF6B35", "#E63946", "#2A9D8F",
    "#8B5CF6", "#EC4899", "#F59E0B", "#10B981", "#3B82F6",
    "#EF4444", "#14B8A6", "#F97316", "#06B6D4", "#84CC16",
    "#A855F7", "#E11D48", "#0EA5E9", "#D946EF", "#22C55E",
]

session = requests.Session()
token = None
mapping = {
    "categories": {},  # mid -> guid (本地 ID 映射)
    "tags": {},  # tag_name -> guid
    "articles": {},  # cid -> guid
    "parent_cats": {},  # mid -> parent_mid
}

# ==================== 工具函数 ====================


def slugify_chinese(text: str) -> str:
    """中文转拼音 slug"""
    text = text.strip()
    # 如果全是英文/数字，直接处理
    if re.match(r'^[a-zA-Z0-9\s\-_]+$', text):
        return re.sub(r'[\s_]+', '-', text.lower()).strip('-')
    # 中文转拼音
    pinyin = ''.join(lazy_pinyin(text))
    return re.sub(r'[^a-z0-9]+', '-', pinyin.lower()).strip('-')


def html_to_markdown(html_content: str) -> str:
    """HTML 转 Markdown"""
    h = html2text.HTML2Text()
    h.body_width = 0
    h.ignore_links = False
    h.ignore_images = False
    h.ignore_emphasis = False
    h.protect_links = True
    h.unicode_snob = True
    h.skip_internal_links = False
    h.mark_code = True
    return h.handle(html_content).strip()


def strip_typecho_markdown(content: str) -> str:
    """
    处理 Typecho 的内容格式。
    Typecho 用 <!--markdown--> 前缀标记内容为 Markdown。
    如果内容包含 <!--markdown-->，去掉前缀后直接返回（已为 Markdown）。
    否则按 HTML 处理。
    """
    content = content.strip()
    if content.startswith("<!--markdown-->"):
        return content[len("<!--markdown-->"):].strip()
    # 否则尝试 HTML -> Markdown
    return html_to_markdown(content)


def extract_keywords(text: str, max_keywords: int = 5) -> list:
    """
    从文章内容中提取关键词作为标签。
    简单策略：提取代码块外的英文技术词汇 + 中文关键词
    """
    # 去除代码块
    text_no_code = re.sub(r'```[\s\S]*?```', '', text)
    text_no_code = re.sub(r'`[^`]+`', '', text_no_code)

    # 去除 URL
    text_no_code = re.sub(r'https?://\S+', '', text_no_code)

    keywords = []

    # 提取常见技术词汇
    tech_patterns = [
        r'\b(Docker|Kubernetes|K8S|Nginx|MySQL|PostgreSQL|Redis|MongoDB)\b',
        r'\b(Python|Java|JavaScript|TypeScript|Go|Rust|C#|PHP|Ruby)\b',
        r'\b(React|Vue|Angular|Next\.js|Nuxt|Svelte|Node\.js|Django|Flask)\b',
        r'\b(Linux|Ubuntu|CentOS|Debian|Windows|macOS|Android|iOS)\b',
        r'\b(API|REST|GraphQL|gRPC|WebSocket|HTTP|HTTPS|TCP|UDP)\b',
        r'\b(Git|GitHub|GitLab|CI/CD|DevOps|Jenkins|GitHub Actions)\b',
        r'\b(AWS|Azure|GCP|阿里云|腾讯云|华为云)\b',
        r'\b(微服务|容器|云原生|Serverless|边缘计算)\b',
        r'\b(AI|ML|深度学习|机器学习|神经网络|NLP|LLM|ChatGPT)\b',
        r'\b(小程序|微信|支付宝|抖音|App|H5)\b',
        r'\b(OpenClaw|OpenAI|Claude|Gemini|Copilot)\b',
        r'\b(dotNET|\.NET|ASP\.NET|Entity Framework|Blazor|MAUI)\b',
        r'\b(数据库|缓存|消息队列|负载均衡|反向代理|防火墙)\b',
        r'\b(前端|后端|全栈|架构|设计模式|算法|数据结构)\b',
        r'\b(安全|加密|认证|授权|OAuth|JWT|SSO)\b',
        r'\b(敏捷|Scrum|Kanban|项目管理|需求|产品)\b',
        r'\b(CSS|HTML|Sass|Less|Tailwind|Bootstrap)\b',
        r'\b(性能优化|SEO|可访问性|响应式|PWA|SPA|SSR)\b',
    ]

    found = set()
    for pattern in tech_patterns:
        matches = re.findall(pattern, text_no_code, re.IGNORECASE)
        for m in matches:
            if isinstance(m, tuple):
                m = m[0]
            found.add(m)

    # 限制数量
    keywords = list(found)[:max_keywords]
    return keywords


# ==================== API 调用 ====================


def api_post(path: str, data: dict) -> dict:
    """POST 请求"""
    headers = {"Content-Type": "application/json"}
    if token:
        headers["Authorization"] = f"Bearer {token}"
    url = f"{API_BASE}{path}"
    resp = session.post(url, json=data, headers=headers)
    return resp.json()


def api_put(path: str, data: dict) -> dict:
    """PUT 请求"""
    headers = {"Content-Type": "application/json"}
    if token:
        headers["Authorization"] = f"Bearer {token}"
    url = f"{API_BASE}{path}"
    resp = session.put(url, json=data, headers=headers)
    return resp.json()


def api_get(path: str) -> dict:
    """GET 请求"""
    headers = {}
    if token:
        headers["Authorization"] = f"Bearer {token}"
    url = f"{API_BASE}{path}"
    resp = session.get(url, headers=headers)
    return resp.json()


def login() -> bool:
    """登录获取 Token"""
    global token
    print(f"[*] 登录 {API_BASE} ...")
    resp = api_post("/api/auth/login", {
        "username": USERNAME,
        "password": PASSWORD,
    })
    if resp.get("code") == 200:
        token = resp["data"]["accessToken"]
        print(f"[+] 登录成功")
        return True
    else:
        print(f"[-] 登录失败: {resp}")
        return False


def get_existing_tags() -> dict:
    """获取已存在的标签"""
    resp = api_get("/api/admin/tags")
    if resp.get("code") == 200:
        tags = {}
        for t in resp.get("data", []):
            tags[t["name"]] = t["id"]
        return tags
    return {}


def get_existing_categories() -> dict:
    """获取已存在的分类"""
    resp = api_get("/api/admin/categories")
    if resp.get("code") == 200:
        cats = {}
        for c in resp.get("data", []):
            cats[c["name"]] = c["id"]
            _collect_cats(c, cats)
        return cats

    # 如果树形接口不行，尝试 flat
    resp = api_get("/api/admin/categories/flat")
    if resp.get("code") == 200:
        return {c["name"]: c["id"] for c in resp.get("data", [])}
    return {}


def _collect_cats(node: dict, result: dict):
    """递归收集分类"""
    result[node["name"]] = node["id"]
    for child in node.get("children", []):
        _collect_cats(child, result)


def create_category(name: str, slug: str, parent_id: str = None) -> str | None:
    """创建分类，返回 ID"""
    data = {
        "name": name,
        "slug": slug,
        "sortOrder": 0,
    }
    if parent_id:
        data["parentId"] = parent_id

    resp = api_post("/api/admin/categories", data)
    if resp.get("code") == 200:
        cat_id = resp["data"]["id"]
        print(f"    [+] 分类: {name} (slug={slug})")
        return cat_id
    else:
        print(f"    [-] 分类创建失败: {name} - {resp.get('message', resp)}")
        return None


def create_tag(name: str, slug: str) -> str | None:
    """创建标签，返回 ID"""
    color_idx = abs(hash(name)) % len(TAG_COLORS)
    color = TAG_COLORS[color_idx]

    data = {
        "name": name,
        "slug": slug,
        "color": color,
    }
    resp = api_post("/api/admin/tags", data)
    if resp.get("code") == 200:
        tag_id = resp["data"]["id"]
        print(f"    [+] 标签: {name} (color={color})")
        return tag_id
    else:
        # 可能是已存在
        if "已存在" in str(resp.get("message", "")):
            # 查找已有 ID
            existing = get_existing_tags()
            return existing.get(name)
        print(f"    [-] 标签创建失败: {name} - {resp.get('message', resp)}")
        return None


def create_article(
    title: str,
    slug: str,
    content: str,
    category_ids: list,
    tag_ids: list,
    published_at: str = None,
    created_at: str = None,
    updated_at: str = None,
) -> str | None:
    """创建文章，返回 ID"""
    # 生成摘要（取前 200 字符纯文本）
    text = re.sub(r'[#*`>\[\]()!\-|~]', '', content)
    text = re.sub(r'\s+', ' ', text).strip()
    summary = text[:200] if len(text) > 200 else text

    data = {
        "title": title,
        "slug": slug,
        "summary": summary,
        "content": content,
        "categoryIds": category_ids,
        "tagIds": tag_ids,
        "status": "published",
        "isTop": False,
        "isRecommend": False,
    }

    # 传入历史时间（保持与原始数据一致）
    if published_at:
        data["publishedAt"] = published_at
    if created_at:
        data["createdAt"] = created_at
    if updated_at:
        data["updatedAt"] = updated_at

    resp = api_post("/api/admin/articles", data)
    if resp.get("code") == 200:
        article_id = resp["data"]["id"]
        print(f"    [+] 文章: {title[:50]}... (slug={slug})")
        return article_id
    else:
        # 检查是否是 slug 冲突
        msg = resp.get("message", "")
        if "slug" in msg.lower() or "已存在" in msg:
            print(f"    [!] slug 冲突: {slug}, 重试...")
            # 用带时间戳的 slug 重试
            data["slug"] = f"{slug}-{int(time.time())}"
            resp = api_post("/api/admin/articles", data)
            if resp.get("code") == 200:
                article_id = resp["data"]["id"]
                print(f"    [+] 文章(重试): {title[:50]}... (slug={data['slug']})")
                return article_id

        print(f"    [-] 文章创建失败: {title[:50]} - {resp.get('message', resp)}")
        return None


def update_article_published_at(article_id: str, published_at: str):
    """更新文章的发布时间（通过更新文章 status）"""
    # 这个 API 可能不支持直接改时间，需要通过 UpdateArticleRequest
    # 尝试用 PUT 更新
    data = {
        "title": "",  # 会被忽略，但需要填
        "slug": "",
        "content": "",
        "status": "published",
    }
    # 实际上 UpdateArticleRequest 没有 PublishedAt 字段
    # 跳过此步骤，发布时间就是创建时间
    pass


# ==================== SQL 解析 ====================


def parse_typecho_sql(filepath: str) -> tuple:
    """
    解析 typecho.sql 文件
    返回: (categories, tags, articles, relationships)

    使用逐行解析，通过跟踪括号深度处理复杂的 VALUES 块。
    """
    with open(filepath, 'r', encoding='utf-8') as f:
        lines = f.readlines()

    categories = []
    tags = []
    articles = []
    relationships = []

    # 状态机
    in_metas = False
    in_contents = False
    in_relationships = False
    values_buffer = ""
    collecting = False

    for line in lines:
        stripped = line.strip()

        # 检测进入哪个表
        if stripped.startswith("INSERT INTO `typecho_metas`"):
            in_metas = True
            in_contents = False
            in_relationships = False
            # 获取 VALUES 之后的内容
            idx = stripped.find("VALUES")
            if idx >= 0:
                values_buffer = stripped[idx + 6:].strip()
                collecting = True
            else:
                values_buffer = ""
                collecting = False
            continue
        elif stripped.startswith("INSERT INTO `typecho_contents`"):
            in_metas = False
            in_contents = True
            in_relationships = False
            idx = stripped.find("VALUES")
            if idx >= 0:
                values_buffer = stripped[idx + 6:].strip()
                collecting = True
            else:
                values_buffer = ""
                collecting = False
            continue
        elif stripped.startswith("INSERT INTO `typecho_relationships`"):
            in_metas = False
            in_contents = False
            in_relationships = True
            idx = stripped.find("VALUES")
            if idx >= 0:
                values_buffer = stripped[idx + 6:].strip()
                collecting = True
            else:
                values_buffer = ""
                collecting = False
            continue

        # 检测表结束（下一个 CREATE TABLE 或 ALTER TABLE）
        if stripped.startswith("CREATE TABLE") or stripped.startswith("ALTER TABLE"):
            # 处理缓冲区中剩余的数据
            if collecting and values_buffer.strip():
                _process_buffer(values_buffer, in_metas, in_contents, in_relationships,
                                categories, tags, articles, relationships)
            collecting = False
            values_buffer = ""
            in_metas = False
            in_contents = False
            in_relationships = False
            continue

        # 如果正在收集 VALUES
        if collecting:
            values_buffer += stripped

            # 检查是否以分号结束
            if stripped.endswith(';'):
                values_buffer = values_buffer.rstrip(';')
                _process_buffer(values_buffer, in_metas, in_contents, in_relationships,
                                categories, tags, articles, relationships)
                collecting = False
                values_buffer = ""

    # 处理最后可能残留的数据
    if collecting and values_buffer.strip():
        _process_buffer(values_buffer, in_metas, in_contents, in_relationships,
                        categories, tags, articles, relationships)

    return categories, tags, articles, relationships


def _process_buffer(buffer: str, is_metas: bool, is_contents: bool, is_relationships: bool,
                    categories: list, tags: list, articles: list, relationships: list):
    """处理收集到的 VALUES 缓冲区，提取行数据"""
    if is_relationships:
        # relationships 格式简单：(cid, mid),(cid, mid),...
        rows = re.findall(r'\((\d+),\s*(\d+)\)', buffer)
        for row in rows:
            relationships.append({"cid": int(row[0]), "mid": int(row[1])})
        return

    # 解析 metas 或 contents 的行
    rows = _parse_value_rows(buffer)
    for row_data in rows:
        if is_metas:
            if len(row_data) < 8:
                continue
            try:
                mid = int(row_data[0])
            except ValueError:
                continue
            name = _unescape_sql(row_data[1])
            slug = _unescape_sql(row_data[2])
            meta_type = _unescape_sql(row_data[3])
            parent_str = row_data[7].strip() if len(row_data) > 7 else "0"
            try:
                parent = int(parent_str)
            except ValueError:
                parent = 0

            if meta_type == "category":
                categories.append({"mid": mid, "name": name, "slug": slug, "parent": parent})
            elif meta_type == "tag":
                tags.append({"mid": mid, "name": name, "slug": slug})

        elif is_contents:
            if len(row_data) < 11:
                continue
            try:
                cid = int(row_data[0])
            except ValueError:
                continue
            title = _unescape_sql(row_data[1])
            slug = _unescape_sql(row_data[2])
            created_str = row_data[3].strip()
            try:
                created = int(created_str)
            except ValueError:
                created = 0
            modified_str = row_data[4].strip()
            try:
                modified = int(modified_str)
            except ValueError:
                modified = 0
            text = _unescape_sql(row_data[5])  # 索引 5 是 text 字段
            content_type = _unescape_sql(row_data[9]) if len(row_data) > 9 else "post"
            status = _unescape_sql(row_data[10]) if len(row_data) > 10 else "publish"

            if content_type == "post" and status == "publish":
                articles.append({
                    "cid": cid,
                    "title": title,
                    "slug": slug,
                    "created": created,
                    "modified": modified,
                    "text": text,
                })


def _parse_value_rows(buffer: str) -> list:
    """
    解析 VALUES 块中的行数据。
    通过 SQL 字符串引号跟踪来正确处理内容中的括号。
    只有在引号外的 ( 才开始新行，引号外的 ) 才结束行。
    """
    rows = []
    in_string = False
    depth = 0
    current = ""
    i = 0
    while i < len(buffer):
        ch = buffer[i]

        # 处理 SQL 转义
        if ch == '\\' and in_string and i + 1 < len(buffer):
            current += ch + buffer[i + 1]
            i += 2
            continue

        if ch == "'":
            in_string = not in_string
            current += ch
        elif ch == '(' and not in_string:
            if depth == 0:
                current = ""
            else:
                current += ch
            depth += 1
        elif ch == ')' and not in_string:
            depth -= 1
            if depth == 0:
                rows.append(_split_sql_values(current))
                current = ""
            else:
                current += ch
        else:
            if depth > 0:
                current += ch

        i += 1

    return rows


def _split_sql_values(row: str) -> list:
    """按逗号分割 SQL VALUES 行，注意引号内的逗号不分割"""
    parts = []
    current = ""
    in_quotes = False
    i = 0
    while i < len(row):
        ch = row[i]

        # 处理转义
        if ch == '\\' and in_quotes and i + 1 < len(row):
            current += ch + row[i + 1]
            i += 2
            continue

        if ch == "'":
            in_quotes = not in_quotes
            current += ch
        elif ch == ',' and not in_quotes:
            parts.append(current.strip())
            current = ""
        else:
            current += ch
        i += 1

    if current.strip():
        parts.append(current.strip())
    return parts


def _unescape_sql(val: str) -> str:
    """去除 SQL 字符串引号并处理转义"""
    val = val.strip()
    # 处理 NULL
    if val.upper() == "NULL":
        return ""

    # 去除首尾单引号
    if val.startswith("'") and val.endswith("'"):
        val = val[1:-1]

    # 处理转义
    val = val.replace("\\'", "'")
    val = val.replace("\\\"", "\"")
    val = val.replace("\\r\\n", "\n")
    val = val.replace("\\r", "\r")
    val = val.replace("\\n", "\n")
    val = val.replace("\\\\", "\\")

    return val


# ==================== 主流程 ====================


def migrate_categories(categories: list) -> dict:
    """
    迁移分类（按层级顺序创建）
    返回 mid -> guid 映射
    """
    print("\n[1/4] 迁移分类...")
    mid_to_guid = {}

    # 获取已有分类
    existing = get_existing_categories()
    existing_slugs = set()

    # 先创建顶级分类，再创建子分类
    top_cats = [c for c in categories if c["parent"] == 0]
    sub_cats = [c for c in categories if c["parent"] != 0]

    for cat in top_cats:
        name = cat["name"]
        slug = slugify_chinese(name)

        # 跳过"默认分类"
        if name == "默认分类":
            # 查找已有的默认分类
            if name in existing:
                mid_to_guid[cat["mid"]] = existing[name]
            continue

        # 检查是否已存在
        if name in existing:
            mid_to_guid[cat["mid"]] = existing[name]
            print(f"    [~] 跳过已存在: {name}")
            continue

        if slug in existing_slugs:
            slug = f"{slug}-{cat['mid']}"
        existing_slugs.add(slug)

        cat_id = create_category(name, slug)
        if cat_id:
            mid_to_guid[cat["mid"]] = cat_id
        time.sleep(0.1)

    # 创建子分类
    for cat in sub_cats:
        name = cat["name"]
        slug = slugify_chinese(name)

        if name in existing:
            mid_to_guid[cat["mid"]] = existing[name]
            print(f"    [~] 跳过已存在: {name}")
            continue

        parent_guid = mid_to_guid.get(cat["parent"])
        if slug in existing_slugs:
            slug = f"{slug}-{cat['mid']}"
        existing_slugs.add(slug)

        cat_id = create_category(name, slug, parent_guid)
        if cat_id:
            mid_to_guid[cat["mid"]] = cat_id
        time.sleep(0.1)

    print(f"    共 {len(mid_to_guid)} 个分类")
    return mid_to_guid


def migrate_tags(articles_data: list, typecho_tags: list) -> dict:
    """
    从文章内容和 typecho 已有 tag 中提取标签并创建
    返回 tag_name -> guid 映射
    """
    print("\n[2/4] 迁移标签...")
    tag_map = get_existing_tags()

    # 先创建 typecho 中已有的 tag
    for t in typecho_tags:
        name = t["name"]
        if name in tag_map:
            continue
        slug = slugify_chinese(name)
        tag_id = create_tag(name, slug)
        if tag_id:
            tag_map[name] = tag_id
        time.sleep(0.1)

    # 从文章内容中提取关键词
    all_keywords = set()
    for article in articles_data:
        keywords = extract_keywords(article["text"])
        for kw in keywords:
            all_keywords.add(kw)

    # 创建新标签
    for kw in sorted(all_keywords):
        if kw in tag_map:
            continue
        slug = slugify_chinese(kw)
        tag_id = create_tag(kw, slug)
        if tag_id:
            tag_map[kw] = tag_id
        time.sleep(0.05)

    print(f"    共 {len(tag_map)} 个标签")
    return tag_map


def migrate_articles(
    articles: list,
    relationships: list,
    cat_map: dict,
    tag_map: dict,
) -> dict:
    """
    迁移文章
    """
    print(f"\n[3/4] 迁移文章（共 {len(articles)} 篇）...")
    cid_to_guid = {}

    # 构建 cid -> [mid, ...] 映射
    cid_mids = defaultdict(list)
    for rel in relationships:
        cid_mids[rel["cid"]].append(rel["mid"])

    # 获取已有文章 slug 列表（检查冲突）
    existing_slugs = set()

    for i, article in enumerate(articles):
        cid = article["cid"]
        title = article["title"]
        slug = f"show-{cid}"
        text = article["text"]
        created_ts = article["created"]
        modified_ts = article.get("modified", created_ts)

        # 转换内容
        content = strip_typecho_markdown(text)

        # 确定分类
        mids = cid_mids.get(cid, [])
        category_ids = []
        for mid in mids:
            if mid in cat_map:
                category_ids.append(cat_map[mid])

        # 提取标签
        tag_ids = []
        keywords = extract_keywords(content, max_keywords=3)
        for kw in keywords:
            if kw in tag_map:
                tag_ids.append(tag_map[kw])

        # 时间转换：Typecho 的 Unix 时间戳是 UTC+8 的绝对秒数
        # 例如 1649643900 表示 2022-04-11 11:25:00 UTC+8 = 2022-04-11 03:25:00 UTC
        # 后端使用 DateTimeOffset，我们传入 ISO 8601 UTC 格式
        def ts_to_utc_iso(unix_ts: int) -> str | None:
            if unix_ts <= 0:
                return None
            # Unix 时间戳本身是 UTC 的，直接转换
            return datetime.utcfromtimestamp(unix_ts).strftime("%Y-%m-%dT%H:%M:%SZ")

        published_at = ts_to_utc_iso(created_ts)
        created_at = ts_to_utc_iso(created_ts)
        updated_at = ts_to_utc_iso(modified_ts)

        # 创建文章
        if slug in existing_slugs:
            slug = f"{slug}-{int(time.time())}"
        existing_slugs.add(slug)

        article_id = create_article(
            title=title,
            slug=slug,
            content=content,
            category_ids=category_ids,
            tag_ids=tag_ids,
            published_at=published_at,
            created_at=created_at,
            updated_at=updated_at,
        )

        if article_id:
            cid_to_guid[cid] = article_id

        # 进度
        if (i + 1) % 10 == 0:
            print(f"    进度: {i + 1}/{len(articles)}")

        time.sleep(0.05)

    print(f"    成功: {len(cid_to_guid)}/{len(articles)}")
    return cid_to_guid


def main():
    print("=" * 60)
    print("Typecho -> Mblog 数据迁移工具")
    print("=" * 60)

    # 1. 登录
    if not login():
        print("[-] 登录失败，退出")
        sys.exit(1)

    # 2. 解析 SQL
    print(f"\n[*] 解析 {SQL_FILE} ...")
    if not os.path.exists(SQL_FILE):
        print(f"[-] 文件不存在: {SQL_FILE}")
        sys.exit(1)

    categories, tags, articles, relationships = parse_typecho_sql(SQL_FILE)
    print(f"    分类: {len(categories)}, 标签: {len(tags)}, 文章: {len(articles)}, 关联: {len(relationships)}")

    # 3. 迁移分类
    cat_map = migrate_categories(categories)

    # 4. 迁移标签
    tag_map = migrate_tags(articles, tags)

    # 5. 迁移文章
    article_map = migrate_articles(articles, relationships, cat_map, tag_map)

    # 6. 统计
    print("\n" + "=" * 60)
    print("迁移完成！")
    print(f"  分类: {len(cat_map)} 个")
    print(f"  标签: {len(tag_map)} 个")
    print(f"  文章: {len(article_map)} 篇")
    print("=" * 60)


if __name__ == "__main__":
    main()
