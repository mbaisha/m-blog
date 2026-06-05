# 08. 图片、视频、附件上传设计

## 1. 目标

媒体上传模块用于支撑：

- 文章封面图。
- Markdown 正文图片。
- 项目封面图。
- 个人头像。
- 友情链接 Logo。
- Open Graph 图片。
- 视频内容。
- 附件下载。

---

## 2. 技术栈

### 后端

- ASP.NET Core `IFormFile`
- 本地文件存储
- 后续可切换对象存储

### 前端

- Element Plus Upload
- Next.js 文件上传组件
- 上传进度条
- 文件预览

---

## 3. 文件类型规范

| 类型 | 允许扩展名 | 允许 MIME | 单文件限制 |
|---|---|---|---|
| 图片 | jpg, jpeg, png, webp, gif | image/jpeg, image/png, image/webp, image/gif | 10MB |
| 视频 | mp4, mov, webm | video/mp4, video/quicktime, video/webm | 500MB |
| 附件 | pdf, doc, docx, zip | application/pdf, application/msword, application/vnd.openxmlformats-officedocument.wordprocessingml.document, application/zip | 50MB |

---

## 4. 存储目录规范

```text
uploads/
├── images/
│   ├── 2026/
│   │   ├── 06/
│   │   │   └── 05/
│   └── avatars/
├── videos/
├── files/
└── thumbnails/
```

文件名重新生成：

```text
{uuid}.{extension}
```

示例：

```text
uploads/images/2026/06/05/8f3a2c1d-xxxx.jpg
```

---

## 5. 上传流程

```text
用户选择文件
 ↓
前端校验扩展名和大小
 ↓
上传到 /api/admin/upload/image
 ↓
后端校验 Token
 ↓
后端校验 MIME 和扩展名
 ↓
后端生成安全文件名
 ↓
后端保存到存储目录
 ↓
后端生成缩略图，图片可选
 ↓
后端写入 media 表
 ↓
返回 URL、文件名、MIME、大小
```

---

## 6. 图片处理

### 6.1 必选

- 格式校验。
- 大小限制。
- 安全文件名。
- 返回 URL。
- 写入 media 表。

### 6.2 推荐

- 自动生成缩略图。
- 自动压缩大图。
- WebP 转换。
- 记录宽高。

---

## 7. 视频处理

### 7.1 第一阶段

- 上传 MP4 / MOV / WebM。
- 保存原始文件。
- 前台播放器播放。
- 记录时长，可选。

### 7.2 后续增强

- 自动生成封面。
- 视频转码。
- HLS 切片。
- CDN 分发。
- 分片上传。
- 断点续传。

---

## 8. 安全要求

- 不允许上传可执行文件。
- 不允许上传脚本文件。
- 不允许用户控制保存路径。
- 不允许覆盖已有文件。
- 上传目录禁止执行脚本。
- 文件 MIME 与扩展名必须一致。
- 上传接口必须管理员登录。
- 上传失败必须返回明确错误。

---

## 9. 后台媒体库功能

- 图片上传。
- 视频上传。
- 附件上传。
- 文件预览。
- 文件删除。
- 文件搜索。
- 文件路径复制。
- 插入文章。
- 文件类型筛选。
- 存储空间统计。

---

## 10. 上传接口

```text
POST /api/admin/upload/image
POST /api/admin/upload/video
POST /api/admin/upload/file
GET  /api/admin/media
DELETE /api/admin/media/{id}
```

---

## 11. 验收标准

- 图片可以上传。
- 视频可以上传。
- 附件可以上传。
- 非法文件被拒绝。
- 超大文件被拒绝。
- 上传目录安全。
- 文件可被文章引用。
- 文件删除后数据库同步更新。
- 后台媒体库可管理。
