/**
 * 自定义 Next.js Image Loader
 *
 * 对于 /uploads/ 路径的本地图片（实际存储在后端服务器，通过 rewrites 代理），
 * 直接返回原路径，让浏览器通过 rewrites 代理请求后端。
 *
 * 对于外部 URL（http/https 开头），也直接返回原 URL。
 */
export default function imageLoader({
  src,
  width,
  quality,
}: {
  src: string
  width: number
  quality?: number
}): string {
  // 外部 URL：直接返回
  if (src.startsWith('http://') || src.startsWith('https://')) {
    return src
  }
  // 本地路径（/uploads/...）：直接返回，由 rewrites 代理到后端
  return src
}
