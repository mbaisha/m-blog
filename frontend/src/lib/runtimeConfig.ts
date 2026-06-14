/**
 * 运行时配置
 *
 * Next.js 中 `NEXT_PUBLIC_*` 会在构建时内联到客户端 bundle，
 * 为支持「构建产物与运行环境完全解耦」，我们采用运行时注入：
 *
 * - 服务端：直接读取 process.env（Docker 运行时由 .env 注入）
 * - 客户端：读取 window.__APP_CONFIG__（由根 layout 的 <script> 注入）
 *
 * env 变量命名遵循 docker-compose.yml 中的约定：
 *   SITE_URL          前台站点 URL
 *   API_BASE_URL      浏览器端访问的后端 API 地址
 *   INTERNAL_API_BASE_URL  服务端直连后端地址（容器内部网络）
 *   SITE_NAME         站点名称
 */

declare global {
  interface Window {
    __APP_CONFIG__?: {
      SITE_URL?: string
      API_BASE_URL?: string
      SITE_NAME?: string
    }
  }
}

function readWindow(key: 'SITE_URL' | 'API_BASE_URL' | 'SITE_NAME'): string {
  if (typeof window === 'undefined') return ''
  const v = window.__APP_CONFIG__?.[key]
  if (v && v.trim() !== '' && !v.startsWith('__')) return v
  return ''
}

function readProcess(key: 'SITE_URL' | 'API_BASE_URL' | 'SITE_NAME' | 'INTERNAL_API_BASE_URL'): string {
  if (typeof process === 'undefined' || !process.env) return ''
  const v = process.env[key]
  if (v && v.trim() !== '' && !v.startsWith('__')) return v
  return ''
}

/** 浏览器端 API 地址：window.__APP_CONFIG__ 优先 */
export function getClientApiBaseUrl(): string {
  return readWindow('API_BASE_URL') || '/api'
}

/** 服务端 API 地址：直连后端（不走 Nginx 代理） */
export function getServerApiBaseUrl(): string {
  return (
    readProcess('INTERNAL_API_BASE_URL') ||
    readProcess('API_BASE_URL') ||
    'http://localhost:5092/api'
  )
}

/** 浏览器端站点 URL */
export function getClientSiteUrl(): string {
  if (typeof window !== 'undefined') {
    const win = readWindow('SITE_URL')
    if (win) return win
    return window.location.origin
  }
  return ''
}

/** 服务端站点 URL（用于 sitemap/feed/og 等 SSR 场景） */
export function getServerSiteUrl(): string {
  return readProcess('SITE_URL') || 'http://localhost:3000'
}

/** 浏览器端站点名称 */
export function getClientSiteName(): string {
  return readWindow('SITE_NAME') || '个人博客'
}

/** 服务端站点名称 */
export function getServerSiteName(): string {
  return readProcess('SITE_NAME') || '个人博客'
}
