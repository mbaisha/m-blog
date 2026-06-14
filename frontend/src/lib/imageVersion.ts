/**
 * 为图片 URL 追加版本号参数（v=），用于绕过 next/image 缓存
 *
 * 场景：用户上传了新封面图但文件名相同，next/image 会复用旧的缓存二进制。
 * 通过在 URL 上拼接 updatedAt 时间戳，使 URL 变化，下游代理会重新拉取。
 *
 * 同时对外暴露 coverImageUrlWithVersion 包装函数，避免在组件中重复判断空值。
 */

/** 简单的 URL 末尾追加 v= 参数。空值原样返回。 */
export function withImageVersion(url: string | null | undefined, version?: string | null): string {
  if (!url) return ''
  // 已经是 data: / blob: 这种内联 URL，不需要处理
  if (url.startsWith('data:') || url.startsWith('blob:')) return url
  const v = version || new Date().toISOString()
  const sep = url.includes('?') ? '&' : '?'
  return `${url}${sep}v=${encodeURIComponent(v)}`
}

/** 业务封装：coverImageUrl + updatedAt → 缓存安全的图片 URL */
export function coverImageUrlWithVersion(
  coverImageUrl: string | null | undefined,
  updatedAt?: string | null,
): string {
  return withImageVersion(coverImageUrl, updatedAt)
}
