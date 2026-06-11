"use client"

import { useEffect, useState } from 'react'
import { apiClient } from '@/lib/api'
import type { ApiResponse } from '@/types'

interface StatisticsData {
  totalArticles: number
  totalProjects: number
  totalViews: number
  totalTags: number
}

interface StatisticsProps {
  config: string
}

export default function Statistics({ config }: StatisticsProps) {
  const [data, setData] = useState<StatisticsData>({
    totalArticles: 0,
    totalProjects: 0,
    totalViews: 0,
    totalTags: 0,
  })

  let showArticles = true
  let showProjects = true
  let showViews = true
  let showTags = true

  try {
    const parsed = JSON.parse(config)
    showArticles = parsed.showArticles !== false
    showProjects = parsed.showProjects !== false
    showViews = parsed.showViews !== false
    showTags = parsed.showTags !== false
  } catch { /* ignore */ }

  useEffect(() => {
    Promise.all([
      apiClient.get<ApiResponse<any>>('/articles?page=1&pageSize=1').catch(() => null),
      apiClient.get<ApiResponse<any[]>>('/projects').catch(() => null),
      apiClient.get<ApiResponse<any[]>>('/tags').catch(() => null),
    ]).then(([articlesRes, projectsRes, tagsRes]) => {
      setData({
        totalArticles: articlesRes?.data?.totalCount ?? 0,
        totalProjects: (projectsRes?.data as any[])?.length ?? 0,
        totalViews: 0,
        totalTags: (tagsRes?.data as any[])?.length ?? 0,
      })
    })
  }, [])

  const items: { label: string; value: number | string; show: boolean }[] = [
    { label: '文章', value: data.totalArticles, show: showArticles },
    { label: '项目', value: data.totalProjects, show: showProjects },
    { label: '阅读量', value: data.totalViews > 0 ? data.totalViews.toLocaleString() : '--', show: showViews },
    { label: '标签', value: data.totalTags, show: showTags },
  ]

  const visibleItems = items.filter(item => item.show)
  if (visibleItems.length === 0) return null

  return (
    <div className="grid gap-4" style={{
      gridTemplateColumns: `repeat(${Math.min(visibleItems.length, 4)}, 1fr)`,
    }}>
      {visibleItems.map((item) => (
        <div
          key={item.label}
          className="text-center p-4 rounded-[10px]"
          style={{
            backgroundColor: 'var(--card-bg, #fff)',
            border: '0.5px solid var(--color-border, #e5e7eb)',
          }}
        >
          <div className="text-[24px] font-bold" style={{ color: 'var(--color-primary, #6366f1)' }}>
            {item.value}
          </div>
          <div className="text-[12px] mt-1" style={{ color: 'var(--color-text-secondary, #6b7280)' }}>
            {item.label}
          </div>
        </div>
      ))}
    </div>
  )
}