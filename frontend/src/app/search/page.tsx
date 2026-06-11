import type { Metadata } from "next"
import Link from "next/link"
import { apiClient, fetchSeoSettings, fetchPageLayout } from "@/lib/api"
import type { ApiResponse, PagedData, PublicArticleListItem, PublicModuleLayout } from "@/types"
import VisitTracker from "@/components/VisitTracker"
import LayoutUniversalModules from "@/components/LayoutUniversalModules"

/** 生成元数据（从后台 SEO 配置获取） */
export async function generateMetadata(): Promise<Metadata> {
  const seo = await fetchSeoSettings("search")
  return {
    title: seo?.title || "搜索",
    description: seo?.description || "搜索文章内容，快速找到你感兴趣的文章",
    keywords: seo?.keywords || undefined,
  }
}

interface Props {
  searchParams: Promise<{
    keyword?: string
    page?: string
  }>
}

export default async function SearchPage({ searchParams }: Props) {
  const params = await searchParams
  const keyword = params.keyword || ""
  const page = parseInt(params.page || "1")

  let layoutModules: PublicModuleLayout[] = []
  let articles: PublicArticleListItem[] = []
  let total = 0
  let totalPages = 1

  const [layoutRes] = await Promise.all([
    fetchPageLayout("search").catch(() => null),
  ])
  layoutModules = layoutRes?.data || []

  if (keyword) {
    try {
      const res = await apiClient.get<ApiResponse<PagedData<PublicArticleListItem>>>("/articles", {
        page,
        pageSize: 12,
        keyword,
      })
      articles = res.data.items
      total = res.data.totalCount
      totalPages = res.data.totalPages
    } catch {
      // ignore
    }
  }

  const moduleMap = new Map(layoutModules.map(m => [m.moduleKey, m]))
  const isEnabled = (key: string) => {
    const mod = moduleMap.get(key)
    return mod ? mod.isEnabled : true
  }

  // 从 page_hero 获取自定义标题
  const heroMod = layoutModules.find(m => m.moduleKey === 'page_hero' && m.isEnabled)
  let heroTitle = '搜索', heroSubtitle = ''
  if (heroMod?.config) {
    try { const c = JSON.parse(heroMod.config); heroSubtitle = c.customSubtitle || heroSubtitle } catch { /* ignore */ }
  }
  if (heroMod?.title) heroTitle = heroMod.title

  return (
    <div className="content-container py-12">
      {/* page_hero */}
      {isEnabled('page_hero') && (
      <section className="mb-8 text-center">
        <h1 className="text-[24px] font-semibold" style={{ color: 'var(--color-text-primary)' }}>
          {heroTitle}
        </h1>
        {heroSubtitle && <p className="text-[14px] mt-2" style={{ color: 'var(--color-text-secondary)' }}>{heroSubtitle}</p>}
      </section>
      )}

      {/* search_bar */}
      {isEnabled('search_bar') && (
      <form action="/search" method="GET" className="mb-10 max-w-xl mx-auto">
        <div className="relative">
          <input
            type="text"
            name="keyword"
            defaultValue={keyword}
            placeholder="搜索文章标题或摘要..."
            className="w-full h-12 px-5 pr-14 rounded-[10px] text-[14px] outline-none transition-colors"
            style={{
              border: '0.5px solid var(--color-border)',
              backgroundColor: 'var(--color-surface)',
              color: 'var(--color-text-primary)',
            }}
          />
          <button
            type="submit"
            className="absolute right-1.5 top-1/2 -translate-y-1/2 w-9 h-9 rounded-[8px] flex items-center justify-center transition-opacity hover:opacity-90"
            style={{ backgroundColor: 'var(--color-primary)', color: '#fff' }}
          >
            <svg className="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0z" />
            </svg>
          </button>
        </div>
      </form>
      )}

      {/* result_stats */}
      {isEnabled('result_stats') && keyword && (
        <div className="mb-6 text-[13px] max-w-[800px] mx-auto text-left" style={{ color: 'var(--color-text-secondary)' }}>
          搜索 &ldquo;<strong style={{ color: 'var(--color-text-primary)' }}>{keyword}</strong>&rdquo;
          {total > 0 ? `，找到 ${total} 个结果` : "，未找到相关结果"}
        </div>
        )}

      {/* search_results */}
      {isEnabled('search_results') && (articles.length > 0 ? (
        <div className="max-w-[800px] mx-auto space-y-3">
          {articles.map((article) => {
            const cats = article.categories?.length > 0 ? article.categories : (article.category ? [article.category] : [])
            return (
            <Link
              key={article.id}
              href={`/articles/${article.slug}`}
              className="hover-bg-light block p-5 rounded-[10px] transition-colors"
              style={{
                border: '0.5px solid var(--color-border)',
                backgroundColor: 'var(--card-bg)',
              }}
            >
              <h3 className="text-[15px] font-medium mb-1" style={{ color: 'var(--color-text-primary)' }}>
                {article.title}
              </h3>
              {article.summary && (
                <p className="text-[13px] line-clamp-2 mb-2" style={{ color: 'var(--color-text-secondary)' }}>
                  {article.summary}
                </p>
              )}
              <div className="flex items-center gap-3 text-[11px]" style={{ color: 'var(--color-text-tertiary)' }}>
                {article.publishedAt && (
                  <time>{new Date(article.publishedAt).toLocaleDateString("zh-CN")}</time>
                )}
                <span>{article.viewCount} 阅读</span>
                <div className="ml-auto flex items-center gap-2 flex-wrap">
                  {(() => {
                    const cats = article.categories?.length > 0 ? article.categories : (article.category ? [article.category] : [])
                    return cats.map((cat) => (
                      <span
                        key={cat.id}
                        className="text-[10px] font-medium px-2 py-0.5 rounded-[4px]"
                        style={{
                          backgroundColor: 'var(--color-primary-light)',
                          color: 'var(--color-primary)',
                        }}
                      >
                        {cat.name}
                      </span>
                    ))
                  })()}
                </div>
              </div>
            </Link>
          );
        })}
        </div>
      ) : keyword ? (
        <div className="flex flex-col items-center justify-center py-16">
          <div className="text-4xl mb-4 opacity-40">🔍</div>
          <h3 className="text-[16px] font-medium mb-2" style={{ color: 'var(--color-text-primary)' }}>
            未找到结果
          </h3>
          <p className="text-[13px]" style={{ color: 'var(--color-text-secondary)' }}>
            尝试使用其他关键词搜索
          </p>
        </div>
      ) : null)}

      {totalPages > 1 && (
        <div className="flex items-center justify-center gap-1.5 mt-10">
          {Array.from({ length: totalPages }, (_, i) => i + 1).map((p) => (
            <Link
              key={p}
              href={`/search?keyword=${keyword}&page=${p}`}
              className="w-8 h-8 flex items-center justify-center rounded-[6px] text-[12px] transition-colors"
              style={{
                backgroundColor: p === page ? 'var(--color-primary-light)' : 'transparent',
                color: p === page ? 'var(--color-primary)' : 'var(--color-text-secondary)',
                fontWeight: p === page ? 500 : 400,
              }}
            >
              {p}
            </Link>
          ))}
        </div>
      )}
      <LayoutUniversalModules pageKey="search" />
      <VisitTracker pagePath="/search" />
    </div>
  )
}