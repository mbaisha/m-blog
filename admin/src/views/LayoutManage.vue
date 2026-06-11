<script setup lang="ts">
import { ref, onMounted, computed, watch, nextTick } from 'vue'
import { ElMessage, ElMessageBox } from 'element-plus'
import Sortable from 'sortablejs'
import type { ModuleLayout } from '@/types/pages'
import { getLayoutModulesApi, saveLayoutModulesApi, resetLayoutModulesApi } from '@/api/layout'
import { getCategoryFlatListApi } from '@/api/category'
import { MdEditor, NormalToolbar } from 'md-editor-v3'
import 'md-editor-v3/lib/style.css'
import request from '@/utils/request'

/** ===== 页面分组定义 ===== */
const pageGroups = [
  {
    label: '首页', icon: 'HomeFilled',
    pages: [
      { key: 'home', label: '首页' },
    ]
  },
  {
    label: '文章', icon: 'DocumentCopy',
    pages: [
      { key: 'articles', label: '文章列表页' },
      { key: 'article_detail', label: '文章详情页' },
      { key: 'archive', label: '归档页' },
      { key: 'search', label: '搜索页' },
    ]
  },
  {
    label: '项目', icon: 'Monitor',
    pages: [
      { key: 'projects', label: '项目列表页' },
      { key: 'project_detail', label: '项目详情页' },
    ]
  },
  {
    label: '分类与标签', icon: 'CollectionTag',
    pages: [
      { key: 'categories', label: '全部分类页' },
      { key: 'category_detail', label: '分类文章页' },
      { key: 'tags', label: '标签云页' },
      { key: 'tag_detail', label: '标签文章页' },
    ]
  },
  {
    label: '其他页面', icon: 'Grid',
    pages: [
      { key: 'about', label: '关于我页' },
      { key: 'friends', label: '友情链接页' },
      { key: 'guestbook', label: '留言板页' },
    ]
  },
]

/** ===== 通用模块定义（所有页面都可添加，支持多次添加） ===== */
const universalModuleDefs: ModuleDef[] = [
  { key: 'image_carousel', label: '图片轮播', icon: 'PictureFilled', color: '#f97316', multiInstance: true, desc: '多图自动轮播展示。可多次添加，每轮播独立配置', configFields: [
    { key: 'images', label: '图片列表 JSON', type: 'textarea', default: '[{"url":"https://picsum.photos/800/400?random=1","link":"/articles","title":"示例图片1"},{"url":"https://picsum.photos/800/400?random=2","link":"/articles","title":"示例图片2"}]', placeholder: '[{"url":"...","link":"...","title":"..."}]', tip: 'JSON 数组，每项含 url(必填)、link(可选)、title(可选)' },
    { key: 'autoPlay', label: '自动播放', type: 'switch', default: true },
    { key: 'interval', label: '切换间隔(ms)', type: 'number', default: 4000, placeholder: '4000' },
    { key: 'height', label: '高度(px)', type: 'number', default: 400, placeholder: '400' },
  ]},
  { key: 'video_player', label: '视频播放', icon: 'VideoCameraFilled', color: '#ef4444', multiInstance: true, desc: '嵌入视频播放器。可多次添加', configFields: [
    { key: 'url', label: '视频 URL', type: 'text', default: 'https://www.youtube.com/embed/dQw4w9WgXcQ', placeholder: '支持 YouTube/Bilibili/MP4 直链' },
    { key: 'aspectRatio', label: '宽高比', type: 'select', default: '16/9', options: [{ label: '16:9', value: '16/9' }, { label: '4:3', value: '4/3' }, { label: '21:9', value: '21/9' }, { label: '1:1', value: '1/1' }] },
    { key: 'autoplay', label: '自动播放', type: 'switch', default: false },
  ]},
  { key: 'custom', label: '自定义区域', icon: 'EditPen', color: '#6b7280', multiInstance: true, desc: '自定义 HTML 内容。可多次添加', configFields: [
    { key: 'html', label: 'HTML 内容', type: 'textarea', default: '<div style="text-align:center;padding:20px"><h3>自定义区域</h3><p>在这里填写你的自定义 HTML 内容</p></div>', placeholder: '<p>自定义 HTML</p>' },
  ]},
  { key: 'divider', label: '分割线', icon: 'Minus', color: '#9ca3af', multiInstance: true, desc: '模块间分隔线。可多次添加', configFields: [
    { key: 'style', label: '样式', type: 'select', default: 'solid', options: [{ label: '实线', value: 'solid' }, { label: '虚线', value: 'dashed' }, { label: '点线', value: 'dotted' }] },
    { key: 'margin', label: '上下间距(px)', type: 'number', default: 32, placeholder: '32' },
  ]},
  { key: 'spacer', label: '间距占位', icon: 'FullScreen', color: '#d1d5db', multiInstance: true, desc: '空白间距。可多次添加', configFields: [
    { key: 'height', label: '高度(px)', type: 'number', default: 40, placeholder: '40', tip: '空白区域的高度' },
  ]},
  { key: 'announcement_banner', label: '公告横幅', icon: 'BellFilled', color: '#f59e0b', multiInstance: true, desc: '醒目通知条，支持多种样式。可多次添加', configFields: [
    { key: 'text', label: '公告文字', type: 'text', default: '📢 欢迎访问我的博客！', placeholder: '显示的公告内容' },
    { key: 'style', label: '样式', type: 'select', default: 'info', options: [{ label: '信息蓝', value: 'info' }, { label: '成功绿', value: 'success' }, { label: '警告橙', value: 'warning' }, { label: '错误红', value: 'danger' }, { label: '主色', value: 'primary' }] },
    { key: 'dismissible', label: '可关闭', type: 'switch', default: true },
    { key: 'fontSize', label: '字体大小(px)', type: 'number', default: 15, placeholder: '15' },
  ]},
  { key: 'quote_block', label: '引用块', icon: 'Quote', color: '#8b5cf6', multiInstance: true, desc: '带样式的引用/引言', configFields: [
    { key: 'quote', label: '引用文字', type: 'textarea', default: '这是引用或引言的内容。', placeholder: '引用内容' },
    { key: 'author', label: '作者', type: 'text', default: '', placeholder: '引用的作者（可选）' },
    { key: 'source', label: '来源', type: 'text', default: '', placeholder: '文章/书名（可选）' },
    { key: 'style', label: '样式', type: 'select', default: 'default', options: [{ label: '默认', value: 'default' }, { label: '粗体', value: 'bold' }, { label: '边框', value: 'border' }] },
  ]},
  { key: 'icon_list', label: '图标列表', icon: 'Grid', color: '#14b8a6', multiInstance: true, desc: '带图标的特性/服务列表', configFields: [
    { key: 'items', label: '列表项 JSON', type: 'textarea', default: '[{"icon":"⚡","title":"快速","desc":"加载速度极快"},{"icon":"🛡","title":"安全","desc":"数据安全可靠"},{"icon":"🎨","title":"美观","desc":"界面优雅简洁"}]', placeholder: '[{"icon":"⚡","title":"快速","desc":"描述"}]', tip: 'JSON 数组，每项含 icon、title、desc' },
    { key: 'columns', label: '列数', type: 'number', default: 3, placeholder: '3', tip: '每行显示数量' },
  ]},
  { key: 'card_banner', label: '卡片横幅', icon: 'Collection', color: '#dc2626', multiInstance: true, desc: '醒目促销/引导卡片', configFields: [
    { key: 'title', label: '标题', type: 'text', default: '特别推荐', placeholder: '卡片标题' },
    { key: 'description', label: '描述', type: 'text', default: '这里填写描述内容', placeholder: '卡片描述' },
    { key: 'buttonText', label: '按钮文字', type: 'text', default: '了解更多', placeholder: '按钮文字' },
    { key: 'buttonLink', label: '按钮链接', type: 'text', default: '/', placeholder: '点击后跳转链接' },
    { key: 'style', label: '卡片样式', type: 'select', default: 'gradient', options: [{ label: '渐变', value: 'gradient' }, { label: '纯色', value: 'solid' }, { label: '描边', value: 'outline' }] },
    { key: 'fontSize', label: '字体大小(px)', type: 'number', default: 16, placeholder: '16', tip: '描述文字的字体大小' },
    { key: 'titleSize', label: '标题字体大小(px)', type: 'number', default: 22, placeholder: '22', tip: '标题文字的字体大小' },
  ]},
  { key: 'markdown', label: 'Markdown 区域', icon: 'Notebook', color: '#10b981', multiInstance: true, desc: '完整的 Markdown 内容区域。可多次添加', configFields: [
    { key: 'content', label: 'Markdown 内容', type: 'textarea', default: '# 标题\n\n这是 Markdown 内容，支持 **加粗**、*斜体*、`代码`、列表、图片等语法。\n\n- 列表项 1\n- 列表项 2\n\n> 引用内容', placeholder: '输入 Markdown 内容', tip: '支持 Markdown 语法，可上传图片或从媒体库插入' },
  ]},
]

/** ===== 所有可用的模块定义 ===== */
interface ConfigField {
  key: string
  label: string
  type: 'number' | 'switch' | 'select' | 'textarea' | 'text' | 'radio' | 'multi-select'
  default: any
  options?: { label: string; value: any }[]
  placeholder?: string
  tip?: string
}

interface ModuleDef {
  key: string
  label: string
  icon: string
  color: string
  needCategory?: boolean
  multiInstance?: boolean
  desc: string
  configFields: ConfigField[]
}

/** 带 page_hero 标题自定义的页面 key 列表 */
const pageHeroKeys = new Set(['articles', 'archive', 'search', 'projects', 'categories', 'category_detail', 'tags', 'tag_detail', 'about', 'friends', 'guestbook'])

/** 构建 allModuleOptions：每个页面的 = 页面特有模块 + 通用模块 */
function buildAllModuleOptions(): Record<string, ModuleDef[]> {
  const pageHeroWithConfig: ModuleDef = {
    key: 'page_hero', label: '页面标题区', icon: 'Tickets', color: '#6366f1', desc: '标题、描述、页面统计。标题和子标题可自定义', configFields: [
      { key: 'customSubtitle', label: '自定义子标题', type: 'text', default: '', placeholder: '留空使用默认子标题' },
    ]
  }

  const pageSpecific: Record<string, ModuleDef[]> = {
    home: [
      { key: 'hero', label: '首屏横幅', icon: 'Promotion', color: '#6366f1', desc: '站点名称、描述、统计方块、CTA按钮。支持自定义文字、多按钮和背景', configFields: [
        { key: 'subtitle', label: '第一行文字', type: 'text', default: 'INDEPENDENT CREATOR', placeholder: '如：INDEPENDENT CREATOR', tip: '横幅第一行小字' },
        { key: 'title', label: '主标题', type: 'text', default: '', placeholder: '留空使用站点名称', tip: '横幅大标题，留空则显示站点名称+Logo' },
        { key: 'description', label: '说明文字', type: 'text', default: '', placeholder: '留空使用站点描述', tip: '主标题下的说明' },
        { key: 'subDescription', label: '副说明文字', type: 'text', default: '', placeholder: '留空不显示', tip: '说明下方的第二行描述' },
        { key: 'buttons', label: '按钮列表 JSON', type: 'textarea', default: '[{"text":"开始阅读","link":"/articles","style":"solid"},{"text":"联系我","link":"/about","style":"outline"}]', placeholder: '[{"text":"按钮文字","link":"/url","style":"solid|outline"}]', tip: 'JSON 数组，支持多个按钮横排' },
        { key: 'backgroundMode', label: '背景模式', type: 'select', default: 'none', options: [{ label: '无背景', value: 'none' }, { label: '渐变', value: 'gradient' }, { label: '轮播', value: 'carousel' }], tip: '选择背景样式' },
        { key: 'gradientColors', label: '渐变色系', type: 'select', default: 'primary-to-accent', options: [
          { label: '主色→强调色', value: 'primary-to-accent' },
          { label: '紫色→蓝色', value: 'purple-to-blue' },
          { label: '蓝色→青色', value: 'blue-to-cyan' },
          { label: '蓝色→紫色', value: 'blue-to-purple' },
          { label: '青色→绿色', value: 'cyan-to-green' },
          { label: '绿色→青色', value: 'green-to-teal' },
          { label: '橙色→粉色', value: 'orange-to-pink' },
          { label: '粉红→紫色', value: 'pink-to-purple' },
          { label: '红色→橙色', value: 'red-to-orange' },
          { label: '暗色系', value: 'dark' },
          { label: '自定义', value: 'custom' },
        ], tip: '渐变背景色系，仅在渐变模式下生效' },
        { key: 'customGradientColor1', label: '自定义颜色 1', type: 'color', default: '#6366f1', tip: '选择第一种颜色，渐变色系为"自定义"时生效' },
        { key: 'customGradientColor2', label: '自定义颜色 2', type: 'color', default: '#a855f7', tip: '选择第二种颜色，渐变色系为"自定义"时生效' },
        { key: 'carouselImages', label: '轮播图片 JSON', type: 'textarea', default: '[{"url":"https://picsum.photos/1200/600?random=1"},{"url":"https://picsum.photos/1200/600?random=2"},{"url":"https://picsum.photos/1200/600?random=3"}]', placeholder: '[{"url":"..."}]', tip: 'JSON 数组，每项含 url。仅在轮播模式下生效' },
        { key: 'carouselInterval', label: '轮播间隔(ms)', type: 'number', default: 5000, placeholder: '5000', tip: '背景图片切换间隔' },
      ]},
      { key: 'hot_tags', label: '热门标签', icon: 'PriceTag', color: '#ec4899', desc: '首页标签行', configFields: [
        { key: 'count', label: '标签数量', type: 'number', default: 10, placeholder: '10', tip: '显示的标签数量上限' },
        { key: 'displayStyle', label: '显示样式', type: 'radio', default: 'inline', options: [{ label: '内联', value: 'inline' }, { label: '卡片', value: 'card' }] },
      ]},
      { key: 'pinned_posts', label: '置顶文章', icon: 'Top', color: '#f59e0b', desc: '置顶文章卡片/列表展示', configFields: [
        { key: 'displayStyle', label: '显示样式', type: 'radio', default: 'card', options: [{ label: '卡片', value: 'card' }, { label: '列表', value: 'list' }] },
        { key: 'count', label: '显示数量', type: 'number', default: 5, placeholder: '5', tip: '最多显示的置顶文章数' },
        { key: 'columns', label: '列数（卡片模式）', type: 'number', default: 3, placeholder: '3', min: 2, max: 4, tip: '卡片排列列数，最小2列，最大4列' },
      ]},
      { key: 'featured_posts', label: '精选文章', icon: 'StarFilled', color: '#f59e0b', desc: '推荐文章展示', configFields: [
        { key: 'count', label: '文章数量', type: 'number', default: 3, placeholder: '3', tip: '显示的精选文章数量' },
        { key: 'displayStyle', label: '显示样式', type: 'radio', default: 'card', options: [{ label: '卡片', value: 'card' }, { label: '列表', value: 'list' }] },
        { key: 'columns', label: '列数（卡片模式）', type: 'number', default: 3, placeholder: '2-3', min: 2, max: 3, tip: '卡片排列列数（右侧边栏打开时自动减1），最小值2，最大值3' },
      ]},
      { key: 'recent_posts', label: '最新文章', icon: 'List', color: '#10b981', desc: '文章列表（封面+标题+分类+标签）', configFields: [
        { key: 'count', label: '文章数量', type: 'number', default: 5, placeholder: '5', tip: '显示的最新文章数量' },
        { key: 'displayStyle', label: '显示样式', type: 'radio', default: 'list', options: [{ label: '列表', value: 'list' }, { label: '卡片', value: 'card' }] },
        { key: 'columns', label: '列数（卡片模式）', type: 'number', default: 3, placeholder: '2-4', min: 2, max: 4, tip: '卡片排列列数，最小值2，最大值4' },
      ]},
      { key: 'projects', label: '项目展示', icon: 'Monitor', color: '#8b5cf6', desc: '2 列项目卡片', configFields: [
        { key: 'count', label: '项目数量', type: 'number', default: 4, placeholder: '4', tip: '显示的项目数量' },
      ]},
      { key: 'subscription', label: '邮件订阅', icon: 'Message', color: '#06b6d4', desc: '订阅表单', configFields: [] },
      { key: 'statistics', label: '统计面板', icon: 'DataAnalysis', color: '#14b8a6', desc: '站点统计信息展示', configFields: [
        { key: 'showArticles', label: '显示文章数', type: 'switch', default: true },
        { key: 'showProjects', label: '显示项目数', type: 'switch', default: true },
        { key: 'showViews', label: '显示阅读量', type: 'switch', default: true },
        { key: 'showTags', label: '显示标签数', type: 'switch', default: true },
      ]},
    ],
    articles: [pageHeroWithConfig,
      { key: 'pinned_posts', label: '置顶文章', icon: 'Top', color: '#f59e0b', desc: '置顶文章卡片/列表展示', configFields: [
        { key: 'displayStyle', label: '显示样式', type: 'radio', default: 'card', options: [{ label: '卡片', value: 'card' }, { label: '列表', value: 'list' }] },
        { key: 'count', label: '显示数量', type: 'number', default: 5, placeholder: '5', tip: '最多显示的置顶文章数' },
        { key: 'columns', label: '列数（卡片模式）', type: 'number', default: 3, placeholder: '3', min: 2, max: 4, tip: '卡片排列列数，最小2列，最大4列' },
      ]},
      { key: 'search_bar', label: '搜索筛选条', icon: 'Search', color: '#06b6d4', desc: '搜索框、分类下拉、排序、视图切换', configFields: [] },
      { key: 'article_list', label: '文章列表', icon: 'List', color: '#10b981', desc: '卡片/列表双视图', configFields: [
        { key: 'defaultView', label: '默认视图', type: 'radio', default: 'card', options: [{ label: '卡片', value: 'card' }, { label: '列表', value: 'list' }] },
        { key: 'pageSize', label: '每页数量', type: 'number', default: 12, placeholder: '12', tip: '每页显示的文章数量' },
        { key: 'columns', label: '列数（卡片模式）', type: 'number', default: 3, placeholder: '2-4', min: 2, max: 4, tip: '卡片排列列数，最小值2，最大值4' },
      ]},
      { key: 'sidebar', label: '右侧边栏', icon: 'SetUp', color: '#8b5cf6', desc: '分类树 + 标签云', configFields: [] },
    ],
    article_detail: [
      { key: 'progress_bar', label: '阅读进度条', icon: 'Sort', color: '#6366f1', desc: '顶部 3px 进度条', configFields: [] },
      { key: 'article_hero', label: '文章标题区', icon: 'Tickets', color: '#f59e0b', desc: '分类标签、标题、元信息', configFields: [] },
      { key: 'content', label: '正文区', icon: 'Notebook', color: '#10b981', desc: 'Markdown 正文', configFields: [] },
      { key: 'sidebar', label: '右侧分享栏', icon: 'Share', color: '#8b5cf6', desc: '分享/收藏/复制 + 分类导航', configFields: [] },
      { key: 'toc', label: '浮动目录', icon: 'List', color: '#06b6d4', desc: '左侧浮动目录按钮', configFields: [] },
      { key: 'tags', label: '标签区', icon: 'PriceTag', color: '#ec4899', desc: '文章标签', configFields: [] },
      { key: 'prev_next', label: '上下篇文章', icon: 'ArrowLeft', color: '#14b8a6', desc: '上一篇/下一篇导航', configFields: [] },
      { key: 'related', label: '相关文章', icon: 'Connection', color: '#f97316', desc: '3 列相关文章卡片', configFields: [
        { key: 'count', label: '文章数量', type: 'number', default: 3, placeholder: '3' },
      ]},
      { key: 'comments', label: '评论区', icon: 'ChatDotSquare', color: '#8b5cf6', desc: '评论列表 + 提交表单', configFields: [] },
    ],
    archive: [pageHeroWithConfig,
      { key: 'timeline', label: '时间线列表', icon: 'Clock', color: '#10b981', desc: '按年份/月份归档', configFields: [] },
    ],
    search: [pageHeroWithConfig,
      { key: 'result_stats', label: '搜索结果统计', icon: 'DataBoard', color: '#06b6d4', desc: '共 X 篇 · 用时 Y 秒', configFields: [] },
      { key: 'search_results', label: '搜索结果列表', icon: 'List', color: '#10b981', desc: '搜索结果展示', configFields: [
        { key: 'pageSize', label: '每页数量', type: 'number', default: 12, placeholder: '12' },
      ]},
    ],
    projects: [pageHeroWithConfig,
      { key: 'project_grid', label: '项目卡片网格', icon: 'Grid', color: '#8b5cf6', desc: '2 列项目卡片', configFields: [
        { key: 'columns', label: '列数', type: 'number', default: 2, placeholder: '2' },
      ]},
    ],
    project_detail: [
      { key: 'project_hero', label: '项目 Hero', icon: 'Monitor', color: '#6366f1', desc: '标题、简介', configFields: [] },
      { key: 'cover', label: '封面图', icon: 'Picture', color: '#f59e0b', desc: '项目封面图', configFields: [] },
      { key: 'project_content', label: '项目介绍', icon: 'Notebook', color: '#10b981', desc: 'Markdown 内容', configFields: [] },
      { key: 'tech_stack', label: '技术栈', icon: 'Tools', color: '#06b6d4', desc: '技术标签', configFields: [] },
    ],
    categories: [pageHeroWithConfig,
      { key: 'category_grid', label: '分类卡片网格', icon: 'Grid', color: '#10b981', desc: '2 列分类卡片', configFields: [] },
    ],
    category_detail: [pageHeroWithConfig,
      { key: 'pinned_posts', label: '置顶文章', icon: 'Top', color: '#f59e0b', desc: '置顶文章卡片/列表展示', configFields: [
        { key: 'displayStyle', label: '显示样式', type: 'radio', default: 'card', options: [{ label: '卡片', value: 'card' }, { label: '列表', value: 'list' }] },
        { key: 'count', label: '显示数量', type: 'number', default: 5, placeholder: '5', tip: '最多显示的置顶文章数' },
        { key: 'columns', label: '列数（卡片模式）', type: 'number', default: 3, placeholder: '3', min: 2, max: 4, tip: '卡片排列列数，最小2列，最大4列' },
      ]},
      { key: 'article_list', label: '文章列表', icon: 'List', color: '#10b981', desc: '分类下文章列表', configFields: [
        { key: 'pageSize', label: '每页数量', type: 'number', default: 12, placeholder: '12' },
        { key: 'columns', label: '列数（卡片模式）', type: 'number', default: 3, placeholder: '2-4', min: 2, max: 4, tip: '卡片排列列数，最小值2，最大值4' },
      ]},
    ],
    tags: [pageHeroWithConfig,
      { key: 'tag_cloud', label: '标签云', icon: 'Cloudy', color: '#ec4899', desc: '彩色标签云', configFields: [
        { key: 'maxTags', label: '标签数量上限', type: 'number', default: 50, placeholder: '50' },
      ]},
    ],
    tag_detail: [pageHeroWithConfig,
      { key: 'article_list', label: '文章列表', icon: 'List', color: '#10b981', desc: '标签下文章列表', configFields: [
        { key: 'pageSize', label: '每页数量', type: 'number', default: 12, placeholder: '12' },
        { key: 'columns', label: '列数（卡片模式）', type: 'number', default: 3, placeholder: '2-4', min: 2, max: 4, tip: '卡片排列列数，最小值2，最大值4' },
      ]},
    ],
    about: [pageHeroWithConfig,
      { key: 'profile_sections', label: '个人模块区块', icon: 'UserFilled', color: '#10b981', desc: '由后台「个人页面」配置控制', configFields: [] },
    ],
    friends: [pageHeroWithConfig,
      { key: 'friend_grid', label: '友链卡片网格', icon: 'Link', color: '#8b5cf6', desc: '2 列友链卡片', configFields: [] },
    ],
    guestbook: [pageHeroWithConfig,
      { key: 'message_form', label: '留言表单', icon: 'Edit', color: '#10b981', desc: '昵称、邮箱、内容、验证码', configFields: [] },
      { key: 'message_list', label: '留言列表', icon: 'ChatLineSquare', color: '#8b5cf6', desc: '留言展示', configFields: [
        { key: 'pageSize', label: '每页数量', type: 'number', default: 20, placeholder: '20' },
      ]},
    ],
  }

  // 将通用模块合并到每个页面
  const result: Record<string, ModuleDef[]> = {}
  for (const [pageKey, specific] of Object.entries(pageSpecific)) {
    result[pageKey] = [...specific, ...universalModuleDefs]
  }
  return result
}

const allModuleOptions = buildAllModuleOptions()

const loading = ref(false)
const saving = ref(false)
const currentPageKey = ref('home')
const modules = ref<ModuleLayout[]>([])
const editModules = ref<{ moduleKey: string; title: string; isEnabled: boolean; config: string; configObj: Record<string, any> }[]>([])
const modulesListRef = ref<HTMLElement | null>(null)
let sortableInstance: Sortable | null = null

/** 分类列表 */
const flatCategories = ref<{ id: string; name: string; level: number }[]>([])

/** 添加模块对话框 */
const addDialogVisible = ref(false)
const selectedModuleKey = ref('')

/** 颜色选择器预定义色 */
const predefineColors = [
  '#6366f1', '#a855f7', '#7c3aed', '#3b82f6', '#06b6d4', '#059669', '#14b8a6',
  '#f59e0b', '#f97316', '#ef4444', '#ec4899', '#dc2626', '#10b981', '#1e293b',
]

/** 当前页面的可用模块列表 */
const moduleOptions = computed(() => allModuleOptions[currentPageKey.value] || [])

/** 已使用的模块 key 集合 */
const usedModuleKeys = computed(() => new Set(editModules.value.map(m => m.moduleKey)))

/** 可添加的模块（唯一模块去重，multiInstance 不限次数） */
const availableModules = computed(() =>
  moduleOptions.value.filter(m => m.multiInstance || !usedModuleKeys.value.has(m.key))
)

/** 获取当前页标签 */
function getCurrentPageLabel(): string {
  for (const group of pageGroups) {
    const page = group.pages.find(p => p.key === currentPageKey.value)
    if (page) return page.label
  }
  return currentPageKey.value
}

/** 解析 config JSON → configObj */
function parseConfig(config: string, fields: ConfigField[]): Record<string, any> {
  try {
    const parsed = JSON.parse(config)
    const obj: Record<string, any> = {}
    for (const f of fields) {
      obj[f.key] = parsed[f.key] !== undefined ? parsed[f.key] : f.default
    }
    return obj
  } catch {
    const obj: Record<string, any> = {}
    for (const f of fields) {
      obj[f.key] = f.default
    }
    return obj
  }
}

/** 合并 configObj → config JSON */
function serializeConfig(obj: Record<string, any>, fields: ConfigField[]): string {
  const json: Record<string, any> = {}
  for (const f of fields) {
    json[f.key] = obj[f.key] !== undefined ? obj[f.key] : f.default
  }
  return JSON.stringify(json)
}

/** 当 configObj 字段变化时同步到 config */
function syncConfig(index: number) {
  const mod = editModules.value[index]
  const def = getModuleDef(mod.moduleKey)
  if (!def) return
  mod.config = serializeConfig(mod.configObj, def.configFields)
}

/** Markdown 模块：上传图片 */
async function handleMarkdownUploadImg(raw: any, mod: any, index: number, callback?: Function) {
  const file = raw instanceof File ? raw : raw?.[0]
  if (!file) return
  const formData = new FormData()
  formData.append('file', file)
  try {
    const res = await request.post('/admin/upload/image', formData)
    const url = res.data?.url || res.data?.data?.url || ''
    if (!url) return
    if (callback) {
      // 编辑器内的 @on-upload-img 回调
      callback([{ url, alt: '' }])
    } else {
      // 外部上传按钮，直接追加 markdown 语法
      mod.configObj.content = (mod.configObj.content || '') + `\n![图片](${url})\n`
      syncConfig(index)
    }
  } catch { /* ignore */ }
}

function loadModules() {
  loading.value = true
  editModules.value = []
  getLayoutModulesApi(currentPageKey.value)
    .then(res => {
      modules.value = res.data.data || []
      syncEditModules()
    })
    .finally(() => { loading.value = false })
    .finally(() => { loading.value = false; nextTick(() => initSortable()) })
}

function syncEditModules() {
  editModules.value = modules.value.map(m => {
    const def = getModuleDef(m.moduleKey)
    return {
      moduleKey: m.moduleKey,
      title: m.title || '',
      isEnabled: m.isEnabled,
      config: m.config,
      configObj: def ? parseConfig(m.config, def.configFields) : {}
    }
  })
}

/** 初始化 SortableJS 拖拽排序 */
function initSortable() {
  if (sortableInstance) sortableInstance.destroy()
  const el = modulesListRef.value
  if (!el || editModules.value.length === 0) return

  sortableInstance = new Sortable(el, {
    handle: '.drag-handle',
    animation: 200,
    easing: 'cubic-bezier(0.25, 0.8, 0.25, 1)',
    ghostClass: 'ghost',
    onEnd: (evt) => {
      const { oldIndex, newIndex } = evt
      if (oldIndex === undefined || newIndex === undefined || oldIndex === newIndex) return
      const item = editModules.value.splice(oldIndex, 1)[0]
      editModules.value.splice(newIndex, 0, item)
    }
  })
}

function openAddDialog() {
  if (availableModules.value.length === 0) {
    ElMessage.warning('所有可用模块都已添加')
    return
  }
  selectedModuleKey.value = availableModules.value[0].key
  addDialogVisible.value = true
}

function confirmAddModule() {
  if (!selectedModuleKey.value) {
    ElMessage.warning('请选择要添加的模块')
    return
  }

  const modDef = moduleOptions.value.find(m => m.key === selectedModuleKey.value)
  if (!modDef) return

  const configObj: Record<string, any> = {}
  for (const f of modDef.configFields) {
    configObj[f.key] = f.default
  }
  let config = '{}'
  if (modDef.needCategory) {
    config = JSON.stringify({ categoryIds: [] })
  } else if (modDef.configFields.length > 0) {
    config = serializeConfig(configObj, modDef.configFields)
  }

  editModules.value.push({
    moduleKey: selectedModuleKey.value,
    title: '',
    isEnabled: true,
    config,
    configObj
  })
  addDialogVisible.value = false
}

function removeModule(index: number) {
  editModules.value.splice(index, 1)
}

function moveUp(index: number) {
  if (index <= 0) return
  const temp = editModules.value[index]
  editModules.value[index] = editModules.value[index - 1]
  editModules.value[index - 1] = temp
}

function moveDown(index: number) {
  if (index >= editModules.value.length - 1) return
  const temp = editModules.value[index]
  editModules.value[index] = editModules.value[index + 1]
  editModules.value[index + 1] = temp
}

function getModuleDef(moduleKey: string): ModuleDef | undefined {
  return allModuleOptions[currentPageKey.value]?.find(m => m.key === moduleKey)
}

function needCategoryConfig(moduleKey: string): boolean {
  return getModuleDef(moduleKey)?.needCategory ?? false
}

function getModuleCategoryIds(config: string): string[] {
  try {
    const parsed = JSON.parse(config)
    return Array.isArray(parsed.categoryIds) ? parsed.categoryIds : []
  } catch {
    return []
  }
}

function setModuleCategoryIds(mod: { config: string; configObj: Record<string, any> }, ids: string[]) {
  try {
    const parsed = JSON.parse(mod.config || '{}')
    parsed.categoryIds = ids
    mod.config = JSON.stringify(parsed)
    mod.configObj.categoryIds = ids
  } catch {
    mod.config = JSON.stringify({ categoryIds: ids })
    mod.configObj.categoryIds = ids
  }
}

async function handleSave() {
  saving.value = true
  try {
    await saveLayoutModulesApi({
      pageKey: currentPageKey.value,
      modules: editModules.value.map((m, i) => ({
        moduleKey: m.moduleKey,
        title: m.title || null,
        sortOrder: i,
        isEnabled: m.isEnabled,
        config: m.config
      }))
    })
    ElMessage.success('布局配置已更新')
    await loadModules()
  } finally {
    saving.value = false
  }
}

async function handleReset() {
  try {
    await ElMessageBox.confirm(
      `确定要恢复「${getCurrentPageLabel()}」的默认布局吗？此操作将清空所有已配置的模块。`,
      '确认重置',
      { confirmButtonText: '确定重置', cancelButtonText: '取消', type: 'warning' }
    )
  } catch {
    return
  }

  saving.value = true
  try {
    await resetLayoutModulesApi(currentPageKey.value)
    ElMessage.success('布局已恢复默认')
    await loadModules()
  } finally {
    saving.value = false
  }
}

onMounted(async () => {
  loadModules()
  try {
    const res = await getCategoryFlatListApi()
    const rawList: any[] = res.data.data || []
    // 树形深度优先排序：确保父级在前、子级在后，且子级紧跟父级
    const allMap = new Map<string, any>()
    for (const c of rawList) allMap.set(c.id, c)
    const childrenMap = new Map<string, any[]>()
    const topLevel: any[] = []
    for (const c of rawList) {
      if (c.parentId && allMap.has(c.parentId)) {
        if (!childrenMap.has(c.parentId)) childrenMap.set(c.parentId, [])
        childrenMap.get(c.parentId)!.push(c)
      } else {
        topLevel.push(c)
      }
    }
    const sortedList: any[] = []
    function dfs(list: any[], depth: number) {
      for (const item of list) {
        sortedList.push({ id: item.id, name: (depth > 0 ? '　'.repeat(depth) + '└ ' : '') + item.name, level: item.level, parentId: item.parentId })
        const children = childrenMap.get(item.id)
        if (children) dfs(children, depth + 1)
      }
    }
    dfs(topLevel, 0)
    flatCategories.value = sortedList
  } catch {
    // ignore
  }
  nextTick(() => initSortable())
})
</script>

<template>
  <div class="layout-page">
    <div class="page-header">
      <div class="page-header-left">
        <h3>页面模块布局配置</h3>
        <span class="page-header-tip">自定义各页面的模块顺序、启用状态和详细参数</span>
      </div>
      <div class="page-header-actions">
        <el-button :loading="saving" @click="handleReset">
          <el-icon><Refresh /></el-icon> 恢复默认
        </el-button>
        <el-button type="primary" :loading="saving" @click="handleSave">
          <el-icon><Check /></el-icon> 保存配置
        </el-button>
      </div>
    </div>

    <el-card shadow="never" class="layout-card">
      <!-- 页面分组选择器 -->
      <div class="page-selector">
        <div v-for="group in pageGroups" :key="group.label" class="page-group">
          <div class="page-group-label">
            <el-icon><component :is="group.icon" /></el-icon>
            {{ group.label }}
          </div>
          <div class="page-group-pages">
            <el-radio-group v-model="currentPageKey" @change="loadModules" size="small">
              <el-radio-button
                v-for="page in group.pages"
                :key="page.key"
                :value="page.key"
              >{{ page.label }}</el-radio-button>
            </el-radio-group>
          </div>
        </div>
      </div>

      <el-divider style="margin: 8px 0 20px" />

      <div v-loading="loading">
        <!-- 空状态 -->
        <el-empty v-if="editModules.length === 0 && !loading" description="暂无模块，点击下方添加" />

        <!-- 模块列表 -->
        <div class="modules-list" ref="modulesListRef">
          <div
            v-for="(mod, index) in editModules"
            :key="mod.moduleKey + index"
            class="module-card"
            :class="{ 'module-disabled': !mod.isEnabled }">
              <!-- 卡片头部 -->
              <div class="module-card-header">
                <div class="module-card-title-row">
                  <el-icon class="drag-handle" style="cursor:grab;color:#bbb;font-size:16px;">
                    <Rank />
                  </el-icon>
                  <span class="module-order-badge">#{{ index + 1 }}</span>
                  <el-tag
                    :color="getModuleDef(mod.moduleKey)?.color || '#6366f1'"
                    style="color:#fff;border:none"
                    size="small"
                  >
                    <el-icon style="margin-right:4px;vertical-align:middle">
                      <component :is="getModuleDef(mod.moduleKey)?.icon || 'Menu'" />
                    </el-icon>
                    {{ getModuleDef(mod.moduleKey)?.label || mod.moduleKey }}
                  </el-tag>
                  <span class="module-desc">{{ getModuleDef(mod.moduleKey)?.desc || '' }}</span>
                </div>
                <div class="module-card-actions">
                  <el-switch v-model="mod.isEnabled" size="small" active-text="启用" inactive-text="禁用" />
                  <el-button link @click="moveUp(index)" :disabled="index === 0">
                    <el-icon><ArrowUp /></el-icon>
                  </el-button>
                  <el-button link @click="moveDown(index)" :disabled="index >= editModules.length - 1">
                    <el-icon><ArrowDown /></el-icon>
                  </el-button>
                  <el-popconfirm title="确定移除该模块？" @confirm="removeModule(index)">
                    <template #reference>
                      <el-button link type="danger">
                        <el-icon><Delete /></el-icon>
                      </el-button>
                    </template>
                  </el-popconfirm>
                </div>
              </div>

              <!-- 卡片体 — 配置表单 -->
              <div class="module-card-body" v-if="getModuleDef(mod.moduleKey)">
                <div class="config-grid">
                  <!-- 显示标题 -->
                  <div class="config-item">
                    <label class="config-label">显示标题</label>
                    <el-input
                      v-model="mod.title"
                      placeholder="留空使用默认标题"
                      size="small"
                      clearable
                    />
                    <span class="config-tip">前端有标题区域时显示此文字</span>
                  </div>

                  <!-- 分类筛选 -->
                  <div class="config-item" v-if="needCategoryConfig(mod.moduleKey)">
                    <label class="config-label">关联分类</label>
                    <el-select
                      :model-value="getModuleCategoryIds(mod.config)"
                      @update:model-value="(val: string[]) => setModuleCategoryIds(mod, val)"
                      multiple
                      placeholder="不选则显示全部"
                      clearable
                      size="small"
                      style="width:100%"
                    >
                      <el-option v-for="c in flatCategories" :key="c.id" :label="c.name" :value="c.id" />
                    </el-select>
                    <span class="config-tip">选择后只显示这些分类下的内容</span>
                  </div>

                  <!-- 动态配置字段 -->
                  <div
                    v-for="field in (getModuleDef(mod.moduleKey)?.configFields || [])"
                    :key="field.key"
                    :class="['config-item', { 'config-item-full': field.type === 'textarea' }]"
                  >
                    <label class="config-label">{{ field.label }}</label>

                    <!-- number -->
                    <el-input-number
                      v-if="field.type === 'number'"
                      v-model="mod.configObj[field.key]"
                      :min="0"
                      :max="9999"
                      size="small"
                      controls-position="right"
                      style="width:160px"
                      @change="syncConfig(index)"
                    />

                    <!-- switch -->
                    <el-switch
                      v-else-if="field.type === 'switch'"
                      v-model="mod.configObj[field.key]"
                      size="small"
                      @change="syncConfig(index)"
                    />

                    <!-- select -->
                    <el-select
                      v-else-if="field.type === 'select'"
                      v-model="mod.configObj[field.key]"
                      size="small"
                      style="width:100%"
                      @change="syncConfig(index)"
                    >
                      <el-option
                        v-for="opt in (field.options || [])"
                        :key="opt.value"
                        :label="opt.label"
                        :value="opt.value"
                      />
                    </el-select>

                    <!-- radio -->
                    <el-radio-group
                      v-else-if="field.type === 'radio'"
                      v-model="mod.configObj[field.key]"
                      size="small"
                      @change="syncConfig(index)"
                    >
                      <el-radio-button
                        v-for="opt in (field.options || [])"
                        :key="opt.value"
                        :value="opt.value"
                      >{{ opt.label }}</el-radio-button>
                    </el-radio-group>

                    <!-- color picker -->
                    <el-color-picker
                      v-else-if="field.type === 'color'"
                      v-model="mod.configObj[field.key]"
                      size="small"
                      show-alpha
                      :predefine="predefineColors"
                      @change="syncConfig(index)"
                    />

                    <!-- textarea -->
                    <el-input
                      v-else-if="field.type === 'textarea' && mod.moduleKey !== 'markdown'"
                      v-model="mod.configObj[field.key]"
                      type="textarea"
                      :rows="4"
                      size="small"
                      :placeholder="field.placeholder"
                      @change="syncConfig(index)"
                    />

                    <!-- markdown 编辑器（markdown 模块专用） -->
                    <div v-else-if="field.type === 'textarea' && mod.moduleKey === 'markdown'" class="markdown-config-editor">
                      <div style="margin-bottom:8px;display:flex;gap:8px;justify-content:flex-end">
                        <el-upload
                          :show-file-list="false"
                          :auto-upload="false"
                          accept="image/*"
                          :on-change="(u: any) => handleMarkdownUploadImg(u.raw!, mod, index)"
                        >
                          <el-button size="small">上传图片</el-button>
                        </el-upload>
                      </div>
                      <MdEditor
                        :model-value="mod.configObj[field.key] || ''"
                        @update:model-value="mod.configObj[field.key] = $event ?? ''; syncConfig(index)"
                        :toolbars="[
                          'bold', 'italic', 'strikeThrough',
                          '-',
                          'title', 'quote', 'unorderedList', 'orderedList',
                          '-',
                          'code', 'codeRow', 'link', 'image', 'table',
                          '-',
                          'preview', 'fullscreen'
                        ]"
                        placeholder="输入 Markdown 内容..."
                        language="zh-CN"
                        style="min-height: 300px"
                        @on-upload-img="(files: any, callback: any) => handleMarkdownUploadImg(files, mod, index, callback)"
                      />
                    </div>

                    <!-- text -->
                    <el-input
                      v-else
                      v-model="mod.configObj[field.key]"
                      size="small"
                      :placeholder="field.placeholder"
                      @change="syncConfig(index)"
                    />

                    <span class="config-tip" v-if="field.tip">{{ field.tip }}</span>
                  </div>

                  <!-- 原始 JSON（调试用） -->
                  <div class="config-item config-item-full">
                    <label class="config-label">配置 JSON</label>
                    <el-input
                      v-model="mod.config"
                      type="textarea"
                      :rows="2"
                      size="small"
                      @change="() => { const def = getModuleDef(mod.moduleKey); if (def) mod.configObj = parseConfig(mod.config, def.configFields) }"
                    />
                    <span class="config-tip">直接编辑 JSON（修改上方表单会自动同步）</span>
                  </div>
                </div>
              </div>
            </div>
          </div>

        <!-- 添加按钮 -->
        <div class="add-module-bar">
          <el-button
            @click="openAddDialog"
            :disabled="availableModules.length === 0"
            style="width:100%;padding:16px;border:2px dashed #d9d9d9;color:#999"
          >
            <el-icon><Plus /></el-icon> 添加模块
          </el-button>
        </div>
      </div>
    </el-card>

    <!-- 添加模块选择对话框 -->
    <el-dialog v-model="addDialogVisible" title="选择要添加的模块" width="520px" :close-on-click-modal="false">
      <div class="module-picker-grid">
        <div
          v-for="m in availableModules"
          :key="m.key"
          class="module-picker-item"
          :class="{ selected: selectedModuleKey === m.key }"
          @click="selectedModuleKey = m.key"
        >
          <div class="mpi-icon" :style="{ backgroundColor: m.color + '20', color: m.color }">
            <el-icon><component :is="m.icon" /></el-icon>
          </div>
          <div class="mpi-info">
            <div class="mpi-title">{{ m.label }}</div>
            <div class="mpi-desc">{{ m.desc }}</div>
          </div>
        </div>
      </div>
      <template #footer>
        <el-button @click="addDialogVisible = false">取消</el-button>
        <el-button type="primary" @click="confirmAddModule">确认添加</el-button>
      </template>
    </el-dialog>
  </div>
</template>

<style scoped>
.layout-page { padding: 0; }

.page-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  margin-bottom: 20px;
  flex-wrap: wrap;
  gap: 12px;
}
.page-header-left {}
.page-header-left h3 { margin: 0; font-size: 20px; }
.page-header-tip { font-size: 12px; color: #909399; margin-top: 2px; display: block; }
.page-header-actions { display: flex; gap: 8px; }
.page-header-actions .el-button span { display: inline-flex; align-items: center; gap: 4px; }

.layout-card { border-radius: 12px; }

/* 页面分组选择器 */
.page-selector { margin-bottom: 4px; }
.page-group {
  display: flex;
  align-items: flex-start;
  gap: 8px;
  margin-bottom: 10px;
  padding: 8px 12px;
  border-radius: 8px;
  background: #f9fafb;
}
.page-group:last-child { margin-bottom: 0; }
.page-group-label {
  font-size: 13px;
  font-weight: 600;
  color: #374151;
  white-space: nowrap;
  display: flex;
  align-items: center;
  gap: 4px;
  min-width: 80px;
  padding-top: 2px;
}
.page-group-pages {
  display: flex;
  flex-wrap: wrap;
  gap: 4px;
}
.page-group-pages .el-radio-button {
  --el-radio-button-checked-bg-color: #6366f1;
  --el-radio-button-checked-border-color: #6366f1;
  --el-radio-button-checked-text-color: #fff;
}

/* 模块列表 */
.modules-list { display: flex; flex-direction: column; gap: 12px; }

.module-card {
  border: 1px solid #e5e7eb;
  border-radius: 10px;
  overflow: hidden;
  transition: box-shadow 0.2s, border-color 0.2s;
}
.module-card:hover {
  box-shadow: 0 2px 12px rgba(0,0,0,0.06);
  border-color: #c4b5fd;
}
.module-disabled { opacity: 0.7; }
.module-disabled .module-card-header {
  background: #f9fafb;
}

.module-card-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 10px 14px;
  background: #f3f4f6;
  border-bottom: 1px solid #e5e7eb;
  flex-wrap: wrap;
  gap: 8px;
}
.module-card-title-row {
  display: flex;
  align-items: center;
  gap: 8px;
  flex-wrap: wrap;
}
.module-order-badge {
  font-size: 11px;
  color: #999;
  font-weight: 500;
  min-width: 24px;
}
.module-desc {
  font-size: 11px;
  color: #9ca3af;
  max-width: 300px;
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
}
.module-card-actions {
  display: flex;
  align-items: center;
  gap: 4px;
}

.module-card-body { padding: 14px; }

.config-grid {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 12px;
}
.config-item {
  display: flex;
  align-items: center;
  gap: 8px;
  flex-wrap: wrap;
  min-height: 32px;
}
  .config-item .config-label,
  .config-item .el-switch,
  .config-item .el-select,
  .config-item .el-input-number,
  .config-item .el-radio-group,
  .config-item .el-input:not(.el-input--textarea) {
    flex-shrink: 0;
  }
  /* 让 select 和 input 在水平行中自动扩展 */
  .config-item:not(.config-item-full) .el-select,
  .config-item:not(.config-item-full) .el-input {
    flex: 1;
    min-width: 120px;
  }
  /* switch 不需要弹性 */
  .config-item .el-switch {
    flex: none;
  }

.config-item-full {
  grid-column: 1 / -1;
  flex-direction: column;
  align-items: stretch;
  min-height: auto;
}
.config-label {
  font-size: 12px;
  font-weight: 500;
  color: #374151;
}
.config-tip {
  font-size: 11px;
  color: #9ca3af;
  line-height: 1.3;
}

.ghost {
  opacity: 0.4;
  border: 2px dashed #6366f1;
}

.add-module-bar { margin-top: 16px; }

/* 模块选择器 */
.module-picker-grid {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 8px;
  max-height: 360px;
  overflow-y: auto;
}
.module-picker-item {
  display: flex;
  align-items: center;
  gap: 10px;
  padding: 10px;
  border: 1.5px solid #e5e7eb;
  border-radius: 8px;
  cursor: pointer;
  transition: all 0.15s;
}
.module-picker-item:hover { border-color: #a5b4fc; background: #f5f3ff; }
.module-picker-item.selected { border-color: #6366f1; background: #eef2ff; }
.mpi-icon {
  width: 36px;
  height: 36px;
  border-radius: 8px;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 18px;
  flex-shrink: 0;
}
.mpi-info { flex: 1; min-width: 0; }
.mpi-title { font-size: 13px; font-weight: 500; color: #1f2937; }
.mpi-desc { font-size: 11px; color: #9ca3af; margin-top: 1px; }

.form-tip { font-size: 12px; color: #909399; margin-top: 4px; }
</style>