'use client'

import { useState, useEffect } from 'react'
import type { PublicModuleLayout } from '@/types'
import { apiClient } from '@/lib/api'
import { RenderLayoutModules } from './UniversalModuleComponents'

/**
 * 客户端异步加载通用模块（分割线/间距/自定义/轮播/视频/公告等）
 * 页面只需插入 <LayoutUniversalModules pageKey="articles" /> 即可
 */
export default function LayoutUniversalModules({ pageKey }: { pageKey: string }) {
  const [modules, setModules] = useState<PublicModuleLayout[]>([])

  useEffect(() => {
    apiClient.get<{ success: boolean; data: PublicModuleLayout[] }>(`/layout/${pageKey}`)
      .then(res => {
        if (res.success && Array.isArray(res.data)) {
          setModules(res.data)
        }
      })
      .catch(() => { /* ignore */ })
  }, [pageKey])

  const universalKeys = new Set([
    'divider', 'spacer', 'custom', 'image_carousel', 'video_player',
    'statistics', 'call_to_action', 'announcement_banner', 'quote_block',
    'icon_list', 'card_banner', 'markdown',
  ])

  const universalModules = modules.filter(m => m.isEnabled && universalKeys.has(m.moduleKey))

  if (universalModules.length === 0) return null

  return (
    <div className="mt-8 space-y-6">
      {RenderLayoutModules(universalModules, {})}
    </div>
  )
}

/**
 * 从 page_hero 模块配置中提取自定义子标题
 * 大标题使用模块的 title 字段（显示标题）
 */
export function getHeroTitleConfig(modules: PublicModuleLayout[]) {
  const heroMod = modules.find(m => m.moduleKey === 'page_hero' && m.isEnabled)
  if (!heroMod?.config) return { title: heroMod?.title || null, subtitle: null }
  try {
    const cfg = JSON.parse(heroMod.config)
    return {
      title: heroMod.title || null,
      subtitle: cfg.customSubtitle || null,
    }
  } catch {
    return { title: heroMod?.title || null, subtitle: null }
  }
}