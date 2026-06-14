FROM node:22-alpine AS build
WORKDIR /app

# 复制依赖文件
COPY package.json package-lock.json ./
RUN npm ci

# 复制源码并构建
COPY . .
RUN npm run build

# 运行镜像 - 使用 Nginx 托管生产构建
FROM nginx:stable-alpine AS runtime
WORKDIR /usr/share/nginx/html

# 安装 envsubst (gettext) 用于运行时配置注入
RUN apk add --no-cache gettext

# 从构建阶段复制 dist
COPY --from=build /app/dist .

# 复制 entrypoint 脚本和 nginx 配置
COPY entrypoint.sh /entrypoint.sh
COPY deploy/nginx-admin.conf /etc/nginx/conf.d/default.conf
RUN chmod +x /entrypoint.sh

EXPOSE 80

ENTRYPOINT ["/entrypoint.sh"]
