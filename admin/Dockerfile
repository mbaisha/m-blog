FROM node:22-alpine AS build
WORKDIR /app

# 构建参数（Vite 在构建时 inlined 到客户端 JS 中）
ARG VITE_API_BASE_URL
ARG VITE_SITE_NAME
ARG VITE_SITE_URL
ENV VITE_API_BASE_URL=$VITE_API_BASE_URL
ENV VITE_SITE_NAME=$VITE_SITE_NAME
ENV VITE_SITE_URL=$VITE_SITE_URL

# 复制依赖文件
COPY package.json package-lock.json ./
RUN npm ci

# 复制源码并构建
COPY . .
RUN npm run build

# 运行镜像 - 使用 Nginx 托管生产构建
FROM nginx:stable-alpine AS runtime
WORKDIR /usr/share/nginx/html

# 从构建阶段复制 dist
COPY --from=build /app/dist .

# 复制 Nginx 配置
COPY deploy/nginx-admin.conf /etc/nginx/conf.d/default.conf

EXPOSE 80

CMD ["nginx", "-g", "daemon off;"]