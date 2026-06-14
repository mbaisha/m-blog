'use client'

import { useEffect } from 'react'
import { getClientApiBaseUrl } from '@/lib/runtimeConfig'

interface VisitTrackerProps {
  /** 页面路径，如 /articles/my-post */
  pagePath: string
  /** 文章 ID（文章页时传入） */
  articleId?: string
  /** 文章 Slug */
  articleSlug?: string
}

/**
 * 访问上报 Client Component
 * 在页面挂载时自动上报访问记录
 */
export default function VisitTracker({ pagePath, articleId, articleSlug }: VisitTrackerProps) {
  useEffect(() => {
    const API_BASE = getClientApiBaseUrl()

    fetch(`${API_BASE}/visits/track`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({
        pagePath,
        articleId: articleId || null,
        articleSlug: articleSlug || null,
      }),
      // 使用 keepalive 确保页面关闭时请求仍能发送
      keepalive: true,
    }).catch(() => {
      // 静默失败，不影响用户体验
    })
  }, [pagePath, articleId, articleSlug])

  // 纯行为组件，不渲染任何 UI
  return null
}