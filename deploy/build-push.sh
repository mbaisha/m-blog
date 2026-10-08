#!/bin/bash
# ==========================================
# MBlog Docker 镜像构建并推送脚本
#
# 默认构建并推送全部 3 个服务：api / frontend / admin
# 镜像仓库：docker.cnb.cool/vxera/m-blog/<service>
#
# 使用示例：
#   bash deploy/build-push.sh                 # 构建并推送全部
#   bash deploy/build-push.sh api              # 只构建并推送 api
#   bash deploy/build-push.sh api admin       # 构建并推送 api + admin
#   bash deploy/build-push.sh --no-cache      # 不使用缓存重新构建全部
#   bash deploy/build-push.sh --no-push       # 只构建不推送
#   bash deploy/build-push.sh --login api     # 推送前先 docker login
#   bash deploy/build-push.sh --tag v1.2.0    # 自定义额外 tag
# ==========================================

set -euo pipefail

# ---------- 配置 ----------
REGISTRY="docker.cnb.cool"
NAMESPACE="vxera/m-blog"

# 服务定义：服务名 => 构建上下文目录
declare -A SERVICES=(
  ["api"]="./backend"
  ["frontend"]="./frontend"
  ["admin"]="./admin"
)

# ---------- 默认变量 ----------
DOCKER_PUSH="1"            # 1=推送, 0=不推送
NO_CACHE="0"              # 1=--no-cache
DO_LOGIN="0"              # 1=构建前 docker login
EXTRA_TAG=""              # 自定义额外 tag（如 v1.2.0）
TARGETS=()                # 要构建的服务列表

# 解析参数
while [[ $# -gt 0 ]]; do
  case "$1" in
    --no-cache)  NO_CACHE="1"; shift ;;
    --no-push)   DOCKER_PUSH="0"; shift ;;
    --login)     DO_LOGIN="1"; shift ;;
    --tag)       EXTRA_TAG="$2"; shift 2 ;;
    -h|--help)
      sed -n '2,20p' "$0" | sed 's/^# \{0,1\}//'
      exit 0 ;;
    *)
      TARGETS+=("$1"); shift ;;
  esac
done

# 默认构建全部
if [[ ${#TARGETS[@]} -eq 0 ]]; then
  TARGETS=("api" "frontend" "admin")
fi

# 校验服务名
for svc in "${TARGETS[@]}"; do
  if [[ -z "${SERVICES[$svc]:-}" ]]; then
    echo "错误：未知服务 '$svc'。可用服务：${!SERVICES[*]}"
    exit 1
  fi
done

# 时间戳 tag（用于追溯）
TIMESTAMP_TAG="$(date +%Y%m%d-%H%M%S)"
# git commit short sha（可选，更精确的版本追溯）
GIT_SHA="$(git rev-parse --short HEAD 2>/dev/null || echo 'nogit')"

# 构建参数
BUILD_ARGS=()
if [[ "$NO_CACHE" == "1" ]]; then
  BUILD_ARGS+=("--no-cache")
fi

# ---------- 登录 ----------
if [[ "$DO_LOGIN" == "1" && "$DOCKER_PUSH" == "1" ]]; then
  echo "==> 登录到 $REGISTRY"
  docker login "$REGISTRY"
fi

# ---------- 构建并推送 ----------
echo "=========================================="
echo "  目标服务：${TARGETS[*]}"
echo "  仓库：  $REGISTRY/$NAMESPACE"
echo "  tags：  latest, $TIMESTAMP_TAG, $GIT_SHA${EXTRA_TAG:+, $EXTRA_TAG}"
echo "  缓存：  $([[ "$NO_CACHE" == "1" ]] && echo '禁用' || echo '启用')"
echo "  推送：  $([[ "$DOCKER_PUSH" == "1" ]] && echo '是' || echo '否')"
echo "=========================================="

export DOCKER_BUILDKIT=1

failed=()
for svc in "${TARGETS[@]}"; do
  ctx="${SERVICES[$svc]}"
  base="$REGISTRY/$NAMESPACE/$svc"
  echo ""
  echo "==> [$svc] 构建上下文: $ctx"

  # 打标签：latest + 时间戳 + git sha + 可选自定义
  tags=( "$base:latest" "$base:$TIMESTAMP_TAG" "$base:$GIT_SHA" )
  [[ -n "$EXTRA_TAG" ]] && tags+=( "$base:$EXTRA_TAG" )

  build_cmd=( docker build )
  for t in "${tags[@]}"; do build_cmd+=( -t "$t" ); done
  # 兼容旧版 bash：仅在 BUILD_ARGS 非空时展开
  [[ ${#BUILD_ARGS[@]} -gt 0 ]] && build_cmd+=( "${BUILD_ARGS[@]}" )
  build_cmd+=( "$ctx" )

  echo "    运行: ${build_cmd[*]}"
  if ! "${build_cmd[@]}"; then
    echo "    [$svc] 构建失败！"
    failed+=("$svc")
    continue
  fi

  if [[ "$DOCKER_PUSH" == "1" ]]; then
    for t in "${tags[@]}"; do
      echo "    推送: $t"
      if ! docker push "$t"; then
        echo "    [$svc] 推送失败：$t"
        failed+=("$svc")
        break
      fi
    done
  fi

  echo "    [$svc] 完成 ✓"
done

# ---------- 汇总 ----------
echo ""
echo "=========================================="
if [[ ${#failed[@]} -eq 0 ]]; then
  echo "  全部成功 ✓"
  echo "  服务：${TARGETS[*]}"
  echo "  tag： latest / $TIMESTAMP_TAG / $GIT_SHA${EXTRA_TAG:+ / $EXTRA_TAG}"
else
  echo "  部分失败 ✗：${failed[*]}"
  exit 1
fi
echo "=========================================="
