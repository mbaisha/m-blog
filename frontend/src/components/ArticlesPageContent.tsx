'use client'

import { useCallback, useEffect, useState } from 'react'
import { useRouter, useSearchParams } from 'next/navigation'
import Link from 'next/link'
import Image from 'next/image'
import type { PublicArticleListItem, PublicCategoryInfo, PublicTagInfo, PublicModuleLayout } from '@/types'
import LayoutUniversalModules from '@/components/LayoutUniversalModules'
import ArticleCard from '@/components/ArticleCard'
import { getClientApiBaseUrl } from '@/lib/runtimeConfig'

// ===== Types =====

interface ArticlesPageContentProps {
  initialArticles: PublicArticleListItem[]
  categories: PublicCategoryInfo[]
  tags: PublicTagInfo[]
  initialTotal: number
  initialTotalAll: number
  initialTotalPages: number
  initialCategorySlug: string
  initialTagSlug: string
  initialSortBy: string
  initialQuery: string
  initialPage: number
  layoutModules?: PublicModuleLayout[]
}

type ViewMode = 'card' | 'list'

// ===== Brand colors =====

const categoryBrandColors: Record<string, { bg: string; text: string; border: string; coverBg: string }> = {
  "前端":        { bg: "#EEEDFE", text: "#534AB7", border: "#CECBF6", coverBg: "#EEEDFE" },
  "后端":        { bg: "#E1F5EE", text: "#0F6E56", border: "#9FE1CB", coverBg: "#E1F5EE" },
  "产品":        { bg: "#FAECE7", text: "#B45A38", border: "#F5C4B3", coverBg: "#FAECE7" },
  "读书":        { bg: "#FBEAF0", text: "#B34776", border: "#F4C0D1", coverBg: "#FBEAF0" },
  "AI":          { bg: "#E7EEFB", text: "#2E5DA8", border: "#B3CCF4", coverBg: "#EAF3DE" },
  "其他":        { bg: "#F3F4F6", text: "#6B7280", border: "#E5E7EB", coverBg: "#F3F4F6" },
}

const tagColors = [
  { bg: "#EEEDFE", text: "#534AB7" },
  { bg: "#E1F5EE", text: "#0F6E56" },
  { bg: "#FAECE7", text: "#993C1D" },
  { bg: "#FBEAF0", text: "#993556" },
  { bg: "#E7EEFB", text: "#2E5DA8" },
  { bg: "#F3F4F6", text: "#6B7280" },
]

function getCategoryBrand(name: string) {
  return categoryBrandColors[name] || categoryBrandColors["其他"]
}

function getTagColor(index: number) {
  return tagColors[index % tagColors.length]
}

// ===== Main Component =====

/** 获取标题第一个有效字符（字母或汉字，字母转为大写，跳过标点） */
function getFirstValidChar(title: string): string {
  for (const ch of title) {
    if (/[a-zA-Z]/.test(ch)) return ch.toUpperCase()
    if (/[\u4e00-\u9fff]/.test(ch)) return ch
  }
  return '?'
}

export default function ArticlesPageContent({
  initialArticles,
  categories,
  tags,
  initialTotal,
  initialTotalAll,
  initialTotalPages,
  initialCategorySlug,
  initialTagSlug,
  initialSortBy,
  initialQuery,
  initialPage,
  layoutModules = [],
}: ArticlesPageContentProps) {
  const router = useRouter()
  const searchParams = useSearchParams()

  // 布局模块显隐
  const moduleMap = new Map(layoutModules.map(m => [m.moduleKey, m]))
  const isEnabled = (key: string) => {
    const mod = moduleMap.get(key)
    return mod ? mod.isEnabled : true
  }

  // 文章列表卡片列数
  const articleListMod = moduleMap.get('article_list')
  const articleListCfg = articleListMod ? (() => { try { return JSON.parse(articleListMod.config || '{}') } catch { return {} } })() : {}
  const articleListColumns = Math.min(Math.max(articleListCfg.columns || 3, 2), 4)
  const articlePageSize = articleListCfg.pageSize || 12

  // State
  const [articles, setArticles] = useState(initialArticles)
  const [total, setTotal] = useState(initialTotal)
  const [totalPages, setTotalPages] = useState(initialTotalPages)
  const [loading, setLoading] = useState(false)
  const [sidebarOpen, setSidebarOpen] = useState(true)
  const [viewMode, setViewMode] = useState<ViewMode>('card')

  // 置顶模块是否启用；不启用时置顶文章混入正常列表（优先排列 + 标记）
  const pinnedModuleEnabled = isEnabled('pinned_posts')
  const displayArticles = pinnedModuleEnabled ? articles.filter(a => !a.isTop) : articles

  // Current filters (from URL)
  const query = searchParams.get('q') || initialQuery
  const categorySlug = searchParams.get('categorySlug') || initialCategorySlug
  const tagSlug = searchParams.get('tagSlug') || initialTagSlug
  const sortBy = searchParams.get('sortBy') || initialSortBy
  const page = parseInt(searchParams.get('page') || '1')

  // Fetch data when filters change
  const fetchArticles = useCallback(async (params: Record<string, string>) => {
    setLoading(true)
    try {
      const queryStr = new URLSearchParams(params).toString()
      const apiBase = getClientApiBaseUrl()
      const res = await fetch(`${apiBase}/articles?${queryStr}`)
      const data = await res.json()
      if (data.success) {
        setArticles(data.data.items)
        setTotal(data.data.totalCount)
        setTotalPages(data.data.totalPages)
      }
    } catch {
      // Keep existing data on error
    } finally {
      setLoading(false)
    }
  }, [])

  // Update URL and fetch
  const updateFilter = useCallback((updates: Record<string, string | undefined>) => {
    const params = new URLSearchParams()
    const q = updates.q ?? query
    const cs = updates.categorySlug ?? categorySlug
    const ts = updates.tagSlug ?? tagSlug
    const sb = updates.sortBy ?? sortBy
    if (q) params.set('q', q)
    if (cs) params.set('categorySlug', cs)
    if (ts) params.set('tagSlug', ts)
    if (sb && sb !== 'latest') params.set('sortBy', sb)
    params.set('page', '1')
    const search = params.toString()
    router.push(`/articles${search ? `?${search}` : ''}`, { scroll: false })
  }, [query, categorySlug, tagSlug, sortBy, router])

  // Search handler
  const [searchInput, setSearchInput] = useState(query)

  const handleSearch = (e: React.FormEvent) => {
    e.preventDefault()
    // Directly fetch with search params, clearing category/tag filter
    const params: Record<string, string> = {}
    if (searchInput) params.q = searchInput
    if (sortBy && sortBy !== 'latest') params.sortBy = sortBy
    params.page = '1'
    params.pageSize = String(articlePageSize)
    fetchArticles(params)
    // Update URL for bookmarkability
    const urlParams = new URLSearchParams()
    if (searchInput) urlParams.set('q', searchInput)
    if (sortBy && sortBy !== 'latest') urlParams.set('sortBy', sortBy)
    urlParams.set('page', '1')
    const search = urlParams.toString()
    router.push(`/articles${search ? `?${search}` : ''}`, { scroll: false })
  }

  // Determine current category name
  const currentCategory = categories.find(c => c.slug === categorySlug)
  const currentTag = tags.find(t => t.slug === tagSlug)

  // Responsive sidebar (auto-collapse on tablet)
  useEffect(() => {
    const mq = window.matchMedia('(max-width: 1023px)')
    setSidebarOpen(!mq.matches)
    const handler = (e: MediaQueryListEvent) => setSidebarOpen(!e.matches)
    mq.addEventListener('change', handler)
    return () => mq.removeEventListener('change', handler)
  }, [])

  // 监听 URL 参数变化，侧栏分类/标签点击时自动加载对应文章
  useEffect(() => {
    const params: Record<string, string> = {}
    if (categorySlug) params.categorySlug = categorySlug
    if (tagSlug) params.tagSlug = tagSlug
    if (query) params.q = query
    if (sortBy && sortBy !== 'latest') params.sortBy = sortBy
    params.page = String(page)
    params.pageSize = String(articlePageSize)
    fetchArticles(params)
  }, [query, categorySlug, tagSlug, sortBy, page])

  // 从 page_hero 模块获取自定义标题
  const heroMod = layoutModules.find(m => m.moduleKey === 'page_hero' && m.isEnabled)
  let heroTitle = '文章', heroSubtitle = '记录技术实践、产品思考、读书笔记与项目复盘'
  if (heroMod?.config) {
    try {
      const cfg = JSON.parse(heroMod.config)
      if (cfg.customSubtitle) heroSubtitle = cfg.customSubtitle
    } catch { /* ignore */ }
  }
  if (heroMod?.title) heroTitle = heroMod.title
  const filterLink = (overrides: Record<string, string | undefined>) => {
    const params = new URLSearchParams()
    const q = overrides.q ?? query
    const cs = overrides.categorySlug ?? categorySlug
    const ts = overrides.tagSlug ?? tagSlug
    const sb = overrides.sortBy ?? sortBy
    if (q) params.set('q', q)
    if (cs) params.set('categorySlug', cs)
    if (ts) params.set('tagSlug', ts)
    if (sb && sb !== 'latest') params.set('sortBy', sb)
    if (overrides.page) params.set('page', overrides.page)
    const s = params.toString()
    return `/articles${s ? `?${s}` : ''}`
  }

  return (
    <div className="content-container py-12">
      {/* ===== Page Hero ===== */}
      {isEnabled('page_hero') && (
      <section className="mb-4">
        <h1 className="text-[24px] font-semibold" style={{ color: 'var(--color-text-primary)' }}>
          {heroTitle}
        </h1>
        <p className="text-[14px] mt-2" style={{ color: 'var(--color-text-secondary)' }}>
          {heroSubtitle}
        </p>
        <p className="text-[13px] mt-1" style={{ color: 'var(--color-text-tertiary)' }}>
          共 {initialTotalAll} 篇文章 · {categories.length} 个分类
          {categorySlug && currentCategory && (
            <span> · 在 &quot;{currentCategory.name}&quot; 分类中找到了 {total} 篇文章</span>
          )}
          {tagSlug && currentTag && (
            <span> · 在 &quot;{currentTag.name}&quot; 标签中找到了 {total} 篇文章</span>
          )}
          {query && (
            <span> · 搜索 &quot;{query}&quot; 共找到 {total} 篇文章</span>
          )}
        </p>
      </section>
      )}

      {/* ===== Main layout: content + sidebar ===== */}
      <div className="flex gap-6">
        {/* ===== Content area ===== */}
        <div className="flex-1 min-w-0">
          {/* ===== Search Bar (sticky) ===== */}
          {isEnabled('search_bar') && (
          <div
            className="sticky top-0 z-30 py-3 -mx-4 px-4"
            style={{ backgroundColor: 'var(--color-bg)' }}
          >
            <form onSubmit={handleSearch} className="flex items-center gap-2 flex-wrap">
              {/* Search input */}
              <div
                className="flex items-center gap-1.5 flex-1 min-w-[200px] max-w-[360px] h-[38px] px-3 rounded-[8px] transition-colors"
                style={{
                  border: '0.5px solid var(--color-border)',
                  backgroundColor: 'var(--color-surface)',
                }}
              >
                <svg className="w-4 h-4 flex-shrink-0" fill="none" stroke="currentColor" viewBox="0 0 24 24" style={{ color: 'var(--color-text-tertiary)' }}>
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0z" />
                </svg>
                <input
                  type="text"
                  value={searchInput}
                  onChange={(e) => setSearchInput(e.target.value)}
                  placeholder="搜索文章标题或内容..."
                  className="flex-1 text-[13px] bg-transparent outline-none min-w-0"
                  style={{ color: 'var(--color-text-primary)' }}
                />
              </div>

              {/* Category select */}
              <select
                value={categorySlug}
                onChange={(e) => {
                  setSearchInput('')
                  const params: Record<string, string> = {}
                  if (e.target.value) params.categorySlug = e.target.value
                  if (sortBy && sortBy !== 'latest') params.sortBy = sortBy
                  params.page = '1'
                  params.pageSize = String(articlePageSize)
                  fetchArticles(params)
                  const urlParams = new URLSearchParams()
                  if (e.target.value) urlParams.set('categorySlug', e.target.value)
                  if (sortBy && sortBy !== 'latest') urlParams.set('sortBy', sortBy)
                  urlParams.set('page', '1')
                  router.push(`/articles?${urlParams.toString()}`, { scroll: false })
                }}
                className="h-[38px] px-3 text-[13px] rounded-[8px] outline-none cursor-pointer"
                style={{
                  border: '0.5px solid var(--color-border)',
                  backgroundColor: 'var(--color-surface)',
                  color: 'var(--color-text-primary)',
                }}
              >
                <option value="">全部分类</option>
                {categories.map((cat) => (
                  <option key={cat.id} value={cat.slug}>{cat.name}</option>
                ))}
              </select>

              {/* Sort select */}
              <select
                value={sortBy}
                onChange={(e) => {
                  const params: Record<string, string> = {}
                  if (categorySlug) params.categorySlug = categorySlug
                  if (tagSlug) params.tagSlug = tagSlug
                  if (e.target.value !== 'latest') params.sortBy = e.target.value
                  params.page = '1'
                  params.pageSize = String(articlePageSize)
                  fetchArticles(params)
                  const urlParams = new URLSearchParams()
                  if (categorySlug) urlParams.set('categorySlug', categorySlug)
                  if (tagSlug) urlParams.set('tagSlug', tagSlug)
                  if (e.target.value !== 'latest') urlParams.set('sortBy', e.target.value)
                  urlParams.set('page', '1')
                  router.push(`/articles?${urlParams.toString()}`, { scroll: false })
                }}
                className="h-[38px] px-3 text-[13px] rounded-[8px] outline-none cursor-pointer"
                style={{
                  border: '0.5px solid var(--color-border)',
                  backgroundColor: 'var(--color-surface)',
                  color: 'var(--color-text-primary)',
                }}
              >
                <option value="latest">最新发布</option>
                <option value="views">最多阅读</option>
                <option value="comments">最多评论</option>
              </select>

              {/* Search button */}
              <button
                type="submit"
                className="h-[38px] px-5 rounded-[8px] text-[13px] font-medium text-white transition-opacity hover:opacity-90"
                style={{ backgroundColor: 'var(--color-primary)' }}
              >
                搜索
              </button>

              {/* View toggle */}
              <div className="flex items-center gap-0.5 ml-auto">
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
            </form>
          </div>
          )}

          {/* ===== Pinned Posts Module ===== */}
          {isEnabled('pinned_posts') && !loading && articles.filter(a => a.isTop).length > 0 && (
            <div className="mb-6">
              {(() => {
                const mod = moduleMap.get('pinned_posts')
                const cfg = (() => { try { return JSON.parse(mod?.config || '{}') } catch { return {} } })()
                const displayStyle = cfg.displayStyle || 'card'
                const count = cfg.count || 5
                const columns = Math.min(Math.max(cfg.columns || 3, 2), 4)
                const pinned = articles.filter(a => a.isTop).slice(0, count)
                if (pinned.length === 0) return null
                return (
                  <section>
                    <div className="flex items-center justify-between mb-4">
                      <h2 className="text-[16px] font-medium" style={{ color: 'var(--color-text-primary)' }}>置顶文章</h2>
                    </div>
                    {displayStyle === 'card' ? (
                      <div className="grid gap-4 article-grid-responsive" style={{ gridTemplateColumns: `repeat(${columns}, minmax(0, 1fr))` }}>
                        {pinned.map(article => (
                          <ArticleCard key={article.id} article={article} viewMode="card" showCategories={true} showTags={true} />
                        ))}
                      </div>
                    ) : (
                      <div className="flex flex-col gap-2">
                        {pinned.map(article => (
                          <ArticleCard key={article.id} article={article} viewMode="list" showCategories={true} showTags={true} />
                        ))}
                      </div>
                    )}
                  </section>
                )
              })()}
            </div>
          )}

          {/* ===== Articles Grid / List ===== */}
          {isEnabled('article_list') && loading && (
            <div
              className={viewMode === 'card' ? 'grid gap-4 article-grid-responsive' : 'flex flex-col gap-2'}
              style={viewMode === 'card' ? { gridTemplateColumns: `repeat(${articleListColumns}, minmax(0, 1fr))` } : undefined}
            >
              {Array.from({ length: 6 }).map((_, i) => (
                <div
                  key={i}
                  className={viewMode === 'card'
                    ? 'skeleton rounded-[10px]'
                    : 'skeleton h-[52px] rounded-[8px]'
                  }
                  style={viewMode === 'card' ? { aspectRatio: '4/3' } : {}}
                />
              ))}
            </div>
          )}

          {isEnabled('article_list') && !loading && displayArticles.length > 0 && (
            <>
              {/* Card View */}
              {viewMode === 'card' && (
                <div className="grid gap-5 article-grid-responsive" style={{ gridTemplateColumns: `repeat(${articleListColumns}, minmax(0, 1fr))` }}>
                  {displayArticles.map((article) => {
                    const catBrand = article.category ? getCategoryBrand(article.category.name) : categoryBrandColors["其他"]
                    const cats = article.categories?.length > 0 ? article.categories : (article.category ? [article.category] : [])
                    return (
                      <Link
                        key={article.id}
                        href={`/articles/${article.slug}`}
                        className="flex flex-col rounded-[10px] overflow-hidden transition-all hover:-translate-y-0.5"
                        style={{
                          border: '0.5px solid var(--color-border)',
                          backgroundColor: 'var(--card-bg)',
                          boxShadow: 'var(--card-shadow)',
                        }}
                      >
                        {/* Cover - fixed 16:9 */}
                        <div
                          className="relative w-full overflow-hidden"
                          style={{
                            aspectRatio: '16/9',
                            backgroundColor: article.coverImageUrl ? undefined : catBrand.coverBg,
                            color: catBrand.text,
                          }}
                        >
                          {/* 置顶标记 - 封面图右上角 */}
                          {!pinnedModuleEnabled && article.isTop && (
                            <span className="absolute top-2 right-2 z-10 inline-flex items-center px-2 py-1 rounded-[4px] text-[10px] font-medium backdrop-blur-sm"
                              style={{ backgroundColor: 'rgba(245, 158, 11, 0.88)', color: '#fff' }}>
                              <svg className="w-3 h-3 mr-0.5" viewBox="0 0 24 24" fill="currentColor"><path d="M15.5 2h-7L4 8.5l8 8.3 8-8.3L15.5 2zM8 8.5l3.5-4h1l3.5 4-4 4.15L8 8.5z"/></svg>
                              置顶
                            </span>
                          )}
                          {article.coverImageUrl ? (
                            <Image src={article.coverImageUrl} alt={article.title} fill className="object-cover" sizes="(max-width: 767px) 100vw, (max-width: 1023px) 50vw, 33vw" loading="lazy" />
                          ) : (
                            <div className="absolute inset-0 flex items-center justify-center text-[24px] font-bold">
                              <span>{getFirstValidChar(article.title)}</span>
                            </div>
                          )}
                        </div>
                        {/* Content */}
                        <div className="p-3 flex flex-col gap-1.5 flex-1">
                          {/* Title */}
                          <h3 className="text-[14px] font-semibold leading-snug line-clamp-2" style={{ color: 'var(--color-text-primary)' }}>
                            {article.title}
                          </h3>
                          {/* Categories - below title */}
                          {cats.length > 0 && (
                            <div className="flex flex-wrap gap-1">
                              {cats.map((cat) => {
                                const brand = getCategoryBrand(cat.name)
                                return (
                                  <span
                                    key={cat.id}
                                    className="px-1.5 py-0.5 rounded-[3px] text-[10px] font-medium"
                                    style={{ backgroundColor: brand.bg, color: brand.text }}
                                  >
                                    {cat.name}
                                  </span>
                                )
                              })}
                            </div>
                          )}
                          {/* Summary */}
                          {article.summary && (
                            <p className="text-[12px] leading-relaxed line-clamp-2" style={{ color: 'var(--color-text-secondary)' }}>
                              {article.summary}
                            </p>
                          )}
                          {/* Bottom meta */}
                          <div className="flex items-center gap-2 text-[11px] mt-auto pt-2" style={{ color: 'var(--color-text-tertiary)' }}>
                            {article.publishedAt && (
                              <time>{new Date(article.publishedAt).toLocaleDateString('zh-CN')}</time>
                            )}
                            <span>
                              <svg className="w-3.5 h-3.5 inline" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M15 12a3 3 0 11-6 0 3 3 0 016 0z" /><path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M2.458 12C3.732 7.943 7.523 5 12 5c4.478 0 8.268 2.943 9.542 7-1.274 4.057-5.064 7-9.542 7-4.477 0-8.268-2.943-9.542-7z" /></svg>
                              {article.viewCount || 0}
                            </span>
                            {article.tags && article.tags.length > 0 && (
                              <>
                                <span>·</span>
                                <div className="flex flex-wrap gap-1">
                                  {article.tags.slice(0, 3).map((tag) => (
                                    <span
                                      key={tag.id}
                                      className="px-1.5 py-0.5 rounded-[3px] text-[9px]"
                                      style={{
                                        backgroundColor: 'var(--color-border-light)',
                                        color: 'var(--color-text-secondary)',
                                      }}
                                    >
                                      {tag.name}
                                    </span>
                                  ))}
                                </div>
                              </>
                            )}
                          </div>
                        </div>
                      </Link>
                    )
                  })}
                </div>
              )}

              {/* List View - 与首页最新文章样式一致 */}
              {viewMode === 'list' && (
                <div className="flex flex-col gap-3">
                  {displayArticles.map((article) => {
                    const catBrand = article.category ? getCategoryBrand(article.category.name) : categoryBrandColors["其他"]
                    const cats = article.categories?.length > 0 ? article.categories : (article.category ? [article.category] : [])
                    const readTime = 5
                    return (
                      <Link
                        key={article.id}
                        href={`/articles/${article.slug}`}
                        className="flex items-center gap-3 rounded-[10px] p-3 transition-all hover:-translate-y-0.5"
                        style={{
                          border: '0.5px solid var(--color-border)',
                          backgroundColor: 'var(--card-bg)',
                          boxShadow: 'var(--card-shadow)',
                        }}
                      >
                        {/* 左侧小封面 - 无图时显示首字母 */}
                        <div
                          className="w-[56px] h-[56px] rounded-[10px] flex-shrink-0 relative overflow-hidden flex items-center justify-center text-[20px] font-bold"
                          style={{
                            backgroundColor: article.coverImageUrl
                              ? 'var(--color-border-light)'
                              : catBrand.coverBg || catBrand.bg,
                            color: catBrand.text,
                          }}
                        >
                          {/* 置顶标记 - 列表小图右上角 */}
                          {!pinnedModuleEnabled && article.isTop && (
                            <span className="absolute top-0.5 right-0.5 z-10 inline-flex items-center px-1 py-0.5 rounded-[3px] text-[8px] font-medium"
                              style={{ backgroundColor: 'rgba(245, 158, 11, 0.88)', color: '#fff' }}>
                              置顶
                            </span>
                          )}
                          {article.coverImageUrl ? (
                            <Image src={article.coverImageUrl} alt={article.title} fill className="object-cover" sizes="56px" loading="lazy" />
                          ) : (
                            <span className="text-[24px] font-bold block">{getFirstValidChar(article.title)}</span>
                          )}
                        </div>

                        {/* 中间内容 */}
                        <div className="flex-1 min-w-0">
                          <h3
                            className="text-[14px] font-medium leading-snug truncate"
                            style={{ color: 'var(--color-text-primary)' }}
                          >
                            {article.title}
                          </h3>
                          <div
                            className="flex items-center gap-2 mt-1 text-[11px] flex-wrap"
                            style={{ color: 'var(--color-text-secondary)' }}
                          >
                            {cats.length > 0 && cats.map((cat, idx) => (
                              <span key={cat.id}>
                                {idx > 0 && <span className="mr-1">·</span>}
                                <span
                                  className="px-1.5 py-0.5 rounded-[3px] text-[10px] font-medium"
                                  style={{ backgroundColor: getCategoryBrand(cat.name).bg, color: getCategoryBrand(cat.name).text }}
                                >
                                  {cat.name}
                                </span>
                              </span>
                            ))}
                            {article.publishedAt && (
                              <>
                                <span>·</span>
                                <time>
                                  {new Date(article.publishedAt).toLocaleDateString('zh-CN', { month: '2-digit', day: '2-digit' })}
                                </time>
                              </>
                            )}
                            <span>·</span>
                            <span className="flex items-center gap-1" style={{ color: 'var(--color-text-tertiary)' }}>
                              <svg className="w-3.5 h-3.5" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M15 12a3 3 0 11-6 0 3 3 0 016 0z" /><path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M2.458 12C3.732 7.943 7.523 5 12 5c4.478 0 8.268 2.943 9.542 7-1.274 4.057-5.064 7-9.542 7-4.477 0-8.268-2.943-9.542-7z" /></svg>
                              {article.viewCount || 0}
                            </span>
                            <span>·</span>
                            <span>{readTime} 分钟</span>
                          </div>
                        </div>

                        {/* 右侧标签块 */}
                        {article.tags && article.tags.length > 0 && (
                          <div className="hidden sm:flex flex-wrap gap-1.5 flex-shrink-0 max-w-[140px]">
                            {article.tags.slice(0, 2).map((tag) => {
                              const bgColor = tag.bgColor || 'var(--color-border-light)'
                              const textColor = tag.color || (() => {
                                if (!tag.bgColor) return 'var(--color-text-secondary)'
                                const hex = tag.bgColor.replace('#', '')
                                const r = parseInt(hex.substring(0, 2), 16)
                                const g = parseInt(hex.substring(2, 4), 16)
                                const b = parseInt(hex.substring(4, 6), 16)
                                const luminance = (0.299 * r + 0.587 * g + 0.114 * b) / 255
                                return luminance > 0.6 ? '#1F2937' : '#FFFFFF'
                              })()
                              return (
                                <span
                                  key={tag.id}
                                  className="px-2 py-0.5 rounded-[5px] text-[9px] font-medium whitespace-nowrap"
                                  style={{ backgroundColor: bgColor, color: textColor }}
                                >
                                  {tag.name}
                                </span>)})}
                          </div>
                        )}
                      </Link>
                    )
                  })}
                </div>
              )}
            </>
          )}

          {isEnabled('article_list') && !loading && displayArticles.length === 0 && (
            <div className="flex flex-col items-center justify-center py-16">
              <div className="text-4xl mb-4 opacity-40">🔍</div>
              <h3 className="text-[16px] font-medium mb-2" style={{ color: 'var(--color-text-primary)' }}>
                没有找到匹配的文章
              </h3>
              <p className="text-[13px] mb-6" style={{ color: 'var(--color-text-secondary)' }}>
                试试其他关键词或清空筛选条件以获得更多结果
              </p>
              <div className="flex gap-3">
                <Link
                  href="/articles"
                  className="px-4 py-2 rounded-[6px] text-[12px] font-medium transition-colors"
                  style={{
                    border: '0.5px solid var(--color-primary)',
                    color: 'var(--color-primary)',
                  }}
                >
                  清空筛选
                </Link>
                <Link
                  href="/articles"
                  className="px-4 py-2 rounded-[6px] text-[12px] font-medium text-white transition-opacity hover:opacity-90"
                  style={{ backgroundColor: 'var(--color-primary)' }}
                >
                  查看所有文章
                </Link>
              </div>
              <div className="flex gap-3 mt-6 text-[12px]" style={{ color: 'var(--color-primary)' }}>
                {categories.slice(0, 4).map((cat) => (
                  <Link key={cat.id} href={`/articles?categorySlug=${cat.slug}`}>{cat.name}</Link>
                ))}
              </div>
            </div>
          )}

          {/* ===== Pagination ===== */}
          {isEnabled('article_list') && totalPages > 1 && !loading && (
            <div className="flex items-center justify-center gap-1.5 mt-10">
              {page > 1 && (
                <Link
                  href={filterLink({ page: String(page - 1) })}
                  className="px-3 py-1.5 rounded-[6px] text-[12px] transition-colors"
                  style={{
                    border: '0.5px solid var(--color-border)',
                    color: 'var(--color-text-secondary)',
                  }}
                >
                  上一页
                </Link>
              )}
              {Array.from({ length: Math.min(totalPages, 7) }, (_, i) => {
                const p = i + 1
                const isCurrent = p === page
                return (
                  <Link
                    key={p}
                    href={filterLink({ page: String(p) })}
                    className="w-8 h-8 flex items-center justify-center rounded-[6px] text-[12px] transition-colors"
                    style={{
                      backgroundColor: isCurrent ? 'var(--color-primary-light)' : 'transparent',
                      color: isCurrent ? 'var(--color-primary)' : 'var(--color-text-secondary)',
                      fontWeight: isCurrent ? 500 : 400,
                    }}
                  >
                    {p}
                  </Link>
                )
              })}
              {page < totalPages && (
                <Link
                  href={filterLink({ page: String(page + 1) })}
                  className="px-3 py-1.5 rounded-[6px] text-[12px] transition-colors"
                  style={{
                    border: '0.5px solid var(--color-border)',
                    color: 'var(--color-text-secondary)',
                  }}
                >
                  下一页
                </Link>
              )}
            </div>
          )}
        </div>

        {/* ===== Sidebar ===== */}
        {isEnabled('sidebar') && (
        <aside
          className="sticky top-24 self-start transition-all duration-200"
          style={{ width: sidebarOpen ? '150px' : '28px', flexShrink: 0 }}
        >
          {/* Toggle button */}
          <button
            onClick={() => setSidebarOpen(!sidebarOpen)}
            className="absolute top-0 right-0 z-10 w-5 h-5 flex items-center justify-center rounded transition-colors hover:bg-[var(--color-border-light)]"
            style={{ color: 'var(--color-text-tertiary)' }}
            title={sidebarOpen ? '收起侧栏' : '展开侧栏'}
          >
            <svg className="w-3 h-3 transition-transform" fill="none" stroke="currentColor" viewBox="0 0 24 24" style={{ transform: sidebarOpen ? 'rotate(0)' : 'rotate(180)' }}>
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M15 19l-7-7 7-7" />
            </svg>
          </button>

          {/* Collapsed state */}
          {!sidebarOpen && (
            <div
              className="flex flex-col items-center pt-8 cursor-pointer"
              onClick={() => setSidebarOpen(true)}
              style={{ color: 'var(--color-text-tertiary)' }}
            >
              <svg className="w-4 h-4 mb-2" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={1.5} d="M4 6h16M4 12h16M4 18h16" />
              </svg>
              <span className="text-[9px] writing-vertical" style={{ writingMode: 'vertical-rl' }}>分类与标签</span>
            </div>
          )}

          {/* Expanded state */}
          {sidebarOpen && (
            <div className="space-y-5">
              {/* Categories */}
              <div>
                <h4 className="text-[14px] font-semibold mb-3" style={{ color: 'var(--color-text-primary)' }}>
                  分类
                </h4>
                <div className="space-y-0.5">
                  <Link
                    href="/articles"
                    onClick={() => setSearchInput('')}
                    className={`flex items-center justify-between px-3 py-1.5 rounded-[6px] text-[14px] transition-colors ${!categorySlug && !tagSlug && !query ? 'font-medium' : ''}`}
                    style={{
                      backgroundColor: !categorySlug && !tagSlug && !query ? 'var(--color-primary-light)' : 'transparent',
                      color: !categorySlug && !tagSlug && !query ? 'var(--color-primary)' : 'var(--color-text-secondary)',
                    }}
                  >
                    <span>全部分类</span>
                    <span className="text-[11px]" style={{ color: 'var(--color-text-tertiary)' }}>{initialTotalAll}</span>
                  </Link>
                  {categories.map((cat) => {
                    const isActive = categorySlug === cat.slug
                    const brand = getCategoryBrand(cat.name)
                    return (
                      <Link
                        key={cat.id}
                        href={`/articles?categorySlug=${cat.slug}`}
                        onClick={() => setSearchInput('')}
                        className={`flex items-center justify-between px-3 py-1.5 rounded-[6px] text-[14px] transition-colors ${isActive ? 'font-medium' : ''}`}
                        style={{
                          backgroundColor: isActive ? brand.bg : 'transparent',
                          color: isActive ? brand.text : 'var(--color-text-secondary)',
                        }}
                        onMouseOver={(e) => { if (!isActive) e.currentTarget.style.backgroundColor = 'var(--color-border-light)' }}
                        onMouseOut={(e) => { if (!isActive) e.currentTarget.style.backgroundColor = 'transparent' }}
                      >
                        <span>{cat.name}</span>
                        <span className="text-[11px]" style={{ color: 'var(--color-text-tertiary)' }}>{cat.articleCount}</span>
                      </Link>
                    )
                  })}
                </div>
              </div>

              {/* Divider */}
              <div style={{ borderTop: '0.5px solid var(--color-border)' }} />

              {/* Tag Cloud */}
              {tags.length > 0 && (
                <div>
                  <h4 className="text-[14px] font-semibold mb-3" style={{ color: 'var(--color-text-primary)' }}>
                    标签云
                  </h4>
                  <div className="flex flex-wrap gap-1.5">
                    {tags.map((tag, i) => {
                      const isActive = tagSlug === tag.slug
                      const color = getTagColor(i)
                      return (
                        <Link
                          key={tag.id}
                          href={isActive ? filterLink({ tagSlug: undefined }) : filterLink({ tagSlug: tag.slug })}
                          onClick={() => setSearchInput('')}
                          className="px-2.5 py-1 rounded-[5px] text-[12px] transition-all"
                          style={{
                            backgroundColor: isActive ? color.text : color.bg,
                            color: isActive ? '#fff' : color.text,
                          }}
                        >
                          {tag.name}
                        </Link>
                      )
                    })}
                  </div>
                </div>
              )}
            </div>
          )}
        </aside>
        )}
      </div>
      <LayoutUniversalModules pageKey="articles" />
    </div>
  )
}