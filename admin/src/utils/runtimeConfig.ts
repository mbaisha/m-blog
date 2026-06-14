/**
 * 运行时配置
 *
 * 优先级：
 *   1. window.__APP_CONFIG__   （由容器启动时根据环境变量注入；生产环境使用）
 *   2. import.meta.env.VITE_*  （本地开发时 Vite 构建时注入；生产镜像中通常为空/占位）
 *   3. 默认值                  （兜底）
 *
 * 设计目的：构建产物不依赖任何具体环境配置，部署时只需在 .env / docker-compose.yml
 * 设置 SITE_URL、API_BASE_URL、SITE_NAME，容器启动后会自动注入到前端。
 */

declare global {
  interface Window {
    __APP_CONFIG__?: {
      VITE_API_BASE_URL?: string
      VITE_SITE_URL?: string
      VITE_SITE_NAME?: string
    }
  }
}

function read(key: 'VITE_API_BASE_URL' | 'VITE_SITE_URL' | 'VITE_SITE_NAME'): string {
  // 优先读取运行时注入的配置（生产环境由 config.js 设置 window.__APP_CONFIG__）
  if (typeof window !== 'undefined' && window.__APP_CONFIG__) {
    const v = window.__APP_CONFIG__[key]
    if (v && v.trim() !== '' && !v.startsWith('__')) return v
  }
  // 本地开发：Vite 构建时注入
  const meta = (import.meta as any).env?.[key]
  if (meta && meta.trim() !== '' && !meta.startsWith('__')) return meta
  // 兜底
  return ''
}

export const runtimeConfig = {
  get API_BASE_URL(): string {
    return (
      read('VITE_API_BASE_URL') ||
      // 浏览器端默认走同源 /api（适配 nginx 反代），开发时由 Vite proxy 处理
      (typeof window !== 'undefined' ? `${window.location.origin}/api` : '/api')
    )
  },
  get SITE_URL(): string {
    return (
      read('VITE_SITE_URL') ||
      (typeof window !== 'undefined' ? `${window.location.origin}` : '')
    )
  },
  get SITE_NAME(): string {
    return read('VITE_SITE_NAME') || '后台管理'
  }
}
