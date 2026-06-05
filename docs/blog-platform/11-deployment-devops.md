# 11. 部署、运维与备份设计

## 1. 目标

保证系统可以稳定部署、持续运行、方便恢复。

---

## 2. 部署架构

### 2.1 第一阶段

```text
Nginx
 ├── 前台 Next.js
 ├── 后台 Vue Admin
 └── 后端 ASP.NET Core API

PostgreSQL
本地文件存储
```

---

### 2.2 后续增强

```text
CDN
 ├── 静态资源
 ├── 图片
 └── 视频

对象存储
 ├── 图片
 ├── 视频
 └── 附件

应用服务器
 ├── Next.js
 ├── Vue Admin
 └── ASP.NET Core API

PostgreSQL
Redis，可选
日志服务，可选
监控服务，可选
```

---

## 3. 环境变量

### 3.1 后端

```env
ConnectionStrings__DefaultConnection=...
Jwt__Key=...
Jwt__Issuer=...
Jwt__Audience=...
Storage__RootPath=./uploads
Cors__AllowedOrigins=...
```

### 3.2 前台

```env
NEXT_PUBLIC_API_BASE_URL=...
```

### 3.3 后台

```env
VITE_API_BASE_URL=...
```

---

## 4. Docker Compose

建议包含：

- postgres
- api
- web
- admin
- nginx

---

## 5. 备份策略

### 5.1 数据库备份

- 每日备份。
- 保留 7 天。
- 每周保留 4 份。
- 每月保留 12 份。

### 5.2 文件备份

- 图片备份。
- 视频备份。
- 附件备份。
- 与数据库备份分开存储。

---

## 6. 日志策略

记录：

- 请求日志。
- 错误日志。
- 登录日志。
- 上传日志。
- 评论审核日志。
- 权限拒绝日志。

---

## 7. 监控指标

- API 响应时间。
- 错误率。
- 数据库连接数。
- 磁盘空间。
- 上传文件大小。
- 评论提交频率。
- 登录失败次数。

---

## 8. 运维验收

- 可通过 Docker Compose 启动。
- 数据库可迁移。
- 上传目录可持久化。
- 日志可查看。
- 数据库可备份。
- 文件可备份。
- 服务异常可恢复。
