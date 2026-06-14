FROM node:22-alpine AS build
WORKDIR /app

# 复制依赖文件
COPY package.json package-lock.json ./
RUN npm ci

# 复制源码并构建
# 注意：客户端相关的环境变量（SITE_URL/API_BASE_URL/SITE_NAME）在运行时由 docker-compose 注入，
# 构建时统一置空，避免构建产物与运行环境耦合。
# INTERNAL_API_BASE_URL 传给 next.config.ts（构建时）用于服务端 rewrites 目标。
ARG INTERNAL_API_BASE_URL=http://api:5000/api
ENV INTERNAL_API_BASE_URL=$INTERNAL_API_BASE_URL
COPY . .
RUN NEXT_PUBLIC_SITE_URL="" NEXT_PUBLIC_API_BASE_URL="" NEXT_PUBLIC_SITE_NAME="" \
    npm run build

# 运行镜像
FROM node:22-alpine AS runtime
WORKDIR /app

ENV NODE_ENV=production
ENV NEXT_TELEMETRY_DISABLED=1

# 从构建阶段复制
COPY --from=build /app/.next ./.next
COPY --from=build /app/public ./public
COPY --from=build /app/package.json ./package.json
COPY --from=build /app/node_modules ./node_modules
COPY --from=build /app/next.config.ts ./next.config.ts

EXPOSE 3000

CMD ["node_modules/.bin/next", "start"]
