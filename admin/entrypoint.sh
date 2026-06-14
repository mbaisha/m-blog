#!/bin/sh
# ============================================================================
# 容器启动脚本：运行时根据环境变量生成 /usr/share/nginx/html/config.js
# - 构建产物不包含任何环境相关配置
# - 兼容 .env (通过 docker-compose 的 env_file) 或 docker-compose 直接传入环境变量
# ============================================================================
set -e

TEMPLATE="/usr/share/nginx/html/config.js.template"
OUTPUT="/usr/share/nginx/html/config.js"

# 兜底默认值（避免空值导致前端拿到 "__XXX__" 字面量）
: "${VITE_SITE_URL:=${SITE_URL:-}}"
: "${VITE_SITE_NAME:=${SITE_NAME:-个人博客后台管理}}"
: "${VITE_API_BASE_URL:=${API_BASE_URL:-/api}}"

export VITE_SITE_URL VITE_SITE_NAME VITE_API_BASE_URL

# 用 envsubst 替换占位符生成最终的 config.js
envsubst \
  '${VITE_API_BASE_URL} ${VITE_SITE_URL} ${VITE_SITE_NAME}' \
  < "$TEMPLATE" \
  > "$OUTPUT"

echo "[entrypoint] admin runtime config generated:"
echo "  VITE_API_BASE_URL=$VITE_API_BASE_URL"
echo "  VITE_SITE_URL=$VITE_SITE_URL"
echo "  VITE_SITE_NAME=$VITE_SITE_NAME"

# 启动 nginx
exec nginx -g "daemon off;"
