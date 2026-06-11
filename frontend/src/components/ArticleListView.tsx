'use client'

import { useState } from 'react'
import ArticleCard from './ArticleCard'
import type { PublicArticleListItem } from '@/types'

interface Props {
  articles: PublicArticleListItem[]
  /** 是否显示分类徽章 */
  showCategories?: boolean
  /** 是否显示标签 */
  showTags?: boolean
  /** 卡片模式列数（默认3） */
  columns?: number
  /** 文章总数（显示在视图切换栏左侧） */
  totalCount?: number
}

export default function ArticleListView({ articles, showCategories = true, showTags = true, columns = 3, totalCount }: Props) {
  const [viewMode, setViewMode] = useState<'card' | 'list'>('card')

  return (
    <>
      {/* View toggle */}
      <div className="flex items-center gap-1 mb-4">
        {totalCount !== undefined && (
          <span className="text-[13px] font-medium mr-auto" style={{ color: 'var(--color-text-secondary)' }}>
            共找到 {totalCount} 篇文章
          </span>
        )}
        <button
          type="button"
          onClick={() => setViewMode('card')}
          className="w-7 h-7 flex items-center justify-center rounded-[4px] transition-colors"
          style={{
            backgroundColor: viewMode === 'card' ? 'var(--color-primary-light)' : 'transparent',
            color: viewMode === 'card' ? 'var(--color-primary)' : 'var(--color-text-tertiary)',
          }}
          title="卡片视图"
        >
          <svg className="w-3.5 h-3.5" fill="currentColor" viewBox="0 0 16 16">
            <rect x="1" y="1" width="6" height="6" rx="1" />
            <rect x="9" y="1" width="6" height="6" rx="1" />
            <rect x="1" y="9" width="6" height="6" rx="1" />
            <rect x="9" y="9" width="6" height="6" rx="1" />
          </svg>
        </button>
        <button
          type="button"
          onClick={() => setViewMode('list')}
          className="w-7 h-7 flex items-center justify-center rounded-[4px] transition-colors"
          style={{
            backgroundColor: viewMode === 'list' ? 'var(--color-primary-light)' : 'transparent',
            color: viewMode === 'list' ? 'var(--color-primary)' : 'var(--color-text-tertiary)',
          }}
          title="列表视图"
        >
          <svg className="w-3.5 h-3.5" fill="currentColor" viewBox="0 0 16 16">
            <rect x="1" y="2" width="14" height="2.5" rx="1" />
            <rect x="1" y="6.75" width="14" height="2.5" rx="1" />
            <rect x="1" y="11.5" width="14" height="2.5" rx="1" />
          </svg>
        </button>
      </div>

      {/* Card View */}
      {viewMode === 'card' && (
        <div className="grid gap-4" style={{ gridTemplateColumns: `repeat(${Math.min(Math.max(columns, 2), 4)}, minmax(0, 1fr))` }}>
          {articles.map((article) => (
            <ArticleCard
              key={article.id}
              article={article}
              viewMode="card"
              showCategories={showCategories}
              showTags={showTags}
            />
          ))}
        </div>
      )}

      {/* List View */}
      {viewMode === 'list' && (
        <div className="flex flex-col gap-3">
          {articles.map((article) => (
            <ArticleCard
              key={article.id}
              article={article}
              viewMode="list"
              showCategories={showCategories}
              showTags={showTags}
            />
          ))}
        </div>
      )}
    </>
  )
}