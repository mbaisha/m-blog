import type { Metadata } from "next"
import Link from "next/link"
import Image from "next/image"
import { notFound } from "next/navigation"
import { apiClient, getSiteConfig } from "@/lib/api"
import type { ApiResponse, PublicArticleDetail, PublicArticleListItem, PublicCategoryInfo, PublicModuleLayout } from "@/types"
import { fetchPageLayout } from "@/lib/api"
import { extractHeadings } from "@/lib/headings"
import ReadingProgress from "@/components/ReadingProgress"
import FloatingToc from "@/components/FloatingToc"
import ShareTools from "@/components/ShareTools"
import MarkdownContent from "./MarkdownContent"
import CommentList from "@/components/CommentList"
import VisitTracker from "@/components/VisitTracker"

interface Props {
  params: Promise<{ slug: string }>
}

export async function generateMetadata({ params }: Props): Promise<Metadata> {
  try {
    const { slug } = await params
    const res = await apiClient.get<ApiResponse<PublicArticleDetail>>(`/articles/${slug}`)
    const article = res.data
    const { siteUrl } = getSiteConfig()

    return {
      title: article.seoTitle || article.title,
      description: article.seoDescription || article.summary || "",
      keywords: article.seoKeywords || undefined,
      openGraph: {
        title: article.seoTitle || article.title,
        description: article.seoDescription || article.summary || "",
        type: "article",
        publishedTime: article.publishedAt || undefined,
        modifiedTime: article.updatedAt,
        url: `${siteUrl}/articles/${article.slug}`,
      },
      twitter: {
        card: 'summary_large_image',
        title: article.seoTitle || article.title,
        description: article.seoDescription || article.summary || "",
      },
    }
  } catch {
    return { title: "文章不存在" }
  }
}

// Category brand colors
const categoryBrandColors: Record<string, { bg: string; text: string }> = {
  "前端":   { bg: "#EEEDFE", text: "#534AB7" },
  "后端":   { bg: "#E1F5EE", text: "#0F6E56" },
  "产品":   { bg: "#FAECE7", text: "#B45A38" },
  "读书":   { bg: "#FBEAF0", text: "#B34776" },
  "AI":     { bg: "#E7EEFB", text: "#2E5DA8" },
}

function getCategoryBrand(name: string) {
  return categoryBrandColors[name] || { bg: "#F3F4F6", text: "#6B7280" }
}

/** 根据背景色自动计算前景色 */
function autoTextColor(bgColor?: string | null): string {
  if (!bgColor) return 'var(--color-text-secondary)'
  const hex = bgColor.replace('#', '')
  const r = parseInt(hex.substring(0, 2), 16)
  const g = parseInt(hex.substring(2, 4), 16)
  const b = parseInt(hex.substring(4, 6), 16)
  const luminance = (0.299 * r + 0.587 * g + 0.114 * b) / 255
  return luminance > 0.6 ? '#1F2937' : '#FFFFFF'
}

export default async function ArticleDetailPage({ params }: Props) {
  const { slug } = await params

  let article: PublicArticleDetail
  let categories: PublicCategoryInfo[] = []
  let layoutModules: PublicModuleLayout[] = []
  try {
    const [articleRes, categoriesRes, layoutRes] = await Promise.all([
      apiClient.get<ApiResponse<PublicArticleDetail>>(`/articles/${slug}`),
      apiClient.get<ApiResponse<PublicCategoryInfo[]>>("/categories").catch(() => null),
      fetchPageLayout("article_detail").catch(() => null),
    ])
    article = articleRes.data
    categories = categoriesRes?.data || []
    layoutModules = layoutRes?.data || []
  } catch {
    notFound()
  }

  // Related articles
  let related: PublicArticleListItem[] = []
  let prev: PublicArticleListItem | null = null
  let next: PublicArticleListItem | null = null
  try {
    const [relatedRes, prevRes, nextRes] = await Promise.all([
      apiClient.get<ApiResponse<PublicArticleListItem[]>>(`/articles/${slug}/related?count=3`).catch(() => null),
      apiClient.get<ApiResponse<PublicArticleListItem | null>>(`/articles/${slug}/prev`).catch(() => null),
      apiClient.get<ApiResponse<PublicArticleListItem | null>>(`/articles/${slug}/next`).catch(() => null),
    ])
    related = relatedRes?.data || []
    prev = prevRes?.data || null
    next = nextRes?.data || null
  } catch {
    // ignore
  }

  // Track view (fire-and-forget)
  apiClient.post(`/articles/${slug}/view`).catch(() => {})

  // Extract headings for TOC (exclude article title which is already in Hero)
  const headings = extractHeadings(article.content, [article.title])

  const catBrand = article.category ? getCategoryBrand(article.category.name) : { bg: "#F3F4F6", text: "#6B7280" }
  const articleUrl = `${getSiteConfig().siteUrl}/articles/${article.slug}`
  const readTime = Math.max(1, Math.ceil((article.content.length / 300) || 5))

  // 检查文章状态 — 非已发布状态显示友好提示
  if (article.status !== 'published') {
    return (
      <div className="content-container py-20">
        <div className="max-w-[600px] mx-auto text-center">
          <div className="text-6xl mb-6 opacity-30">
            {article.status === 'archived' ? '📦' : '📝'}
          </div>
          <h1 className="text-2xl font-semibold mb-3" style={{ color: 'var(--color-text-primary)' }}>
            {article.status === 'archived' ? '该文章已下架' : '该文章暂未发布'}
          </h1>
          <p className="mb-8" style={{ color: 'var(--color-text-secondary)' }}>
            {article.status === 'archived'
              ? '作者已将该文章下架，暂时无法访问。'
              : '该文章目前为草稿状态，暂未公开发布。'}
          </p>
          <Link
            href="/articles"
            className="inline-flex items-center gap-2 px-5 py-2.5 rounded-full text-[13px] font-medium transition-all hover:scale-105"
            style={{ backgroundColor: 'var(--color-primary)', color: '#fff' }}
          >
            返回文章列表
          </Link>
        </div>
      </div>
    )
  }

  const moduleMap = new Map(layoutModules.map(m => [m.moduleKey, m]))
  const isEnabled = (key: string) => {
    const mod = moduleMap.get(key)
    return mod ? mod.isEnabled : true
  }

  return (
    <>
      <script
        type="application/ld+json"
        dangerouslySetInnerHTML={{
          __html: JSON.stringify({
            '@context': 'https://schema.org',
            '@type': 'Article',
            headline: article.title,
            description: article.summary || '',
            author: { '@type': 'Person', name: article.author.username },
            datePublished: article.publishedAt,
            dateModified: article.updatedAt,
            image: article.coverImageUrl || undefined,
            mainEntityOfPage: { '@type': 'WebPage', '@id': articleUrl },
          }),
        }}
      />
      <VisitTracker pagePath={`/articles/${slug}`} articleSlug={slug} />

      {/* Reading Progress Bar */}
      {isEnabled('progress_bar') && <ReadingProgress />}

      {/* ===== Article Hero ===== */}
      {isEnabled('article_hero') && (
      <section
        className="content-container pt-12 pb-4"
        style={{ backgroundColor: 'var(--color-bg-alt)' }}
      >
        <div className="flex flex-col md:flex-row gap-8 items-start">
          <div className="flex-1 min-w-0">
            {/* Title - 类别移到了下面的元信息行 */}
            <h1 className="text-[24px] md:text-[28px] font-semibold leading-tight mb-3" style={{ color: 'var(--color-text-primary)' }}>
              {article.title}
            </h1>

            {/* Summary */}
            {article.summary && (
              <p className="text-[14px] mb-4" style={{ color: 'var(--color-text-secondary)' }}>
                {article.summary}
              </p>
            )}

            {/* Categories - 单独一行 */}
            {((article.categories && article.categories.length > 0) || article.category) && (
              <div className="flex flex-wrap gap-2 mb-3">
                {(article.categories && article.categories.length > 0 ? article.categories : (article.category ? [article.category] : [])).map((cat) => {
                  const brand = getCategoryBrand(cat.name)
                  return (
                    <Link
                      key={cat.id}
                      href={`/categories/${cat.slug}`}
                      className="px-2.5 py-0.5 rounded-[4px] text-[12px] font-medium transition-opacity hover:opacity-80"
                      style={{ backgroundColor: brand.bg, color: brand.text }}
                    >
                      {cat.name}
                    </Link>
                  )
                })}
              </div>
            )}

            {/* Meta info - 作者、时间等 */}
            <div className="flex flex-wrap items-center gap-x-2 gap-y-2 text-[13px]" style={{ color: 'var(--color-text-tertiary)' }}>
              <span>作者：{article.author.username}</span>
              {article.publishedAt && (
                <>
                  <span>·</span>
                  <time>发布于 {new Date(article.publishedAt).toLocaleDateString('zh-CN')}</time>
                </>
              )}
              {article.updatedAt !== article.publishedAt && article.updatedAt && (
                <>
                  <span>·</span>
                  <time>更新于 {new Date(article.updatedAt).toLocaleDateString('zh-CN')}</time>
                </>
              )}
              <span>·</span>
              <span>预计阅读 {readTime} 分钟</span>
              <span>·</span>
              <span>{Intl.NumberFormat('zh-CN').format(article.viewCount)} 阅读</span>
              <span>·</span>
              <span>{article.commentCount} 评论</span>
            </div>

          </div>

          {/* Cover image (optional) */}
          {article.coverImageUrl && (
            <div className="flex-shrink-0 w-full md:w-[360px] rounded-2xl overflow-hidden relative aspect-video">
              <Image
                src={article.coverImageUrl}
                alt={article.title}
                fill
                className="object-cover"
                sizes="(max-width: 767px) 100vw, 360px"
                priority
              />
            </div>
          )}
        </div>
      </section>
      )}

      {/* ===== Main layout: content + sidebar ===== */}
      <div
        className="content-container flex gap-6 py-8"
        style={{ backgroundColor: 'var(--color-bg)' }}
      >
        {/* Floating TOC */}
        {isEnabled('toc') && <FloatingToc headings={headings} />}

        {/* Main content area */}
        <main className="flex-1 min-w-0 max-w-[900px] mx-auto">
          {/* Article body with markdown styling */}
          {isEnabled('content') && (
          <article className="detail-article-body">
            <MarkdownContent content={article.content} />
          </article>
          )}

          {/* ===== Article Nav (prev / next) ===== */}
          {isEnabled('prev_next') && (prev || next) && (
            <div className="mt-8 flex gap-4">
              <div className="flex-1">
                {prev ? (
                  <Link
                    href={`/articles/${prev.slug}`}
                    className="hover-nav-bg flex flex-col p-4 rounded-[8px] transition-colors"
                  >
                    <span className="text-[10px] mb-1" style={{ color: 'var(--color-text-tertiary)' }}>← 上一篇</span>
                    <span className="text-[13px] truncate" style={{ color: 'var(--color-text-primary)' }}>{prev.title}</span>
                  </Link>
                ) : (
                  <div className="flex flex-col p-4 rounded-[8px]" style={{ border: '0.5px solid var(--color-border)', opacity: 0.4 }}>
                    <span className="text-[10px]" style={{ color: 'var(--color-text-tertiary)' }}>← 已是第一篇</span>
                  </div>
                )}
              </div>
              <div className="flex-1">
                {next ? (
                  <Link
                    href={`/articles/${next.slug}`}
                    className="hover-nav-bg flex flex-col p-4 rounded-[8px] text-right transition-colors"
                  >
                    <span className="text-[10px] mb-1" style={{ color: 'var(--color-text-tertiary)' }}>下一篇 →</span>
                    <span className="text-[13px] truncate" style={{ color: 'var(--color-text-primary)' }}>{next.title}</span>
                  </Link>
                ) : (
                  <div className="flex flex-col p-4 rounded-[8px] text-right" style={{ border: '0.5px solid var(--color-border)', opacity: 0.4 }}>
                    <span className="text-[10px]" style={{ color: 'var(--color-text-tertiary)' }}>已是最后一篇 →</span>
                  </div>
                )}
              </div>
            </div>
          )}

          {/* ===== Related Articles ===== */}
          {isEnabled('related') && related.length > 0 && (
            <section className="mt-12">
              <h2 className="text-[16px] font-medium mb-4" style={{ color: 'var(--color-text-primary)' }}>
                相关文章
              </h2>
              <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
                {related.map((r) => {
                    const rCats = r.categories?.length > 0 ? r.categories : (r.category ? [r.category] : [])
                    return (
                    <Link
                      key={r.id}
                      href={`/articles/${r.slug}`}
                      className="p-4 rounded-[10px] transition-all hover:-translate-y-0.5"
                      style={{
                        border: '0.5px solid var(--color-border)',
                        backgroundColor: 'var(--card-bg)',
                      }}
                    >
                      {rCats.length > 0 && rCats.slice(0, 1).map((cat) => (
                        <span
                          key={cat.id}
                          className="inline-block px-2 py-0.5 rounded-[4px] text-[9px] font-medium mb-2"
                          style={{
                            backgroundColor: getCategoryBrand(cat.name).bg,
                            color: getCategoryBrand(cat.name).text,
                          }}
                        >
                          {cat.name}
                        </span>
                      ))}
                    <h3
                      className="text-[13px] font-medium leading-snug line-clamp-2 mb-2"
                      style={{ color: 'var(--color-text-primary)' }}
                    >
                      {r.title}
                    </h3>
                    <div className="text-[10px]" style={{ color: 'var(--color-text-tertiary)' }}>
                      {r.viewCount} 阅读
                    </div>
                  </Link>
                )
              })}
              </div>
            </section>
          )}

          {/* ===== Comment Section ===== */}
          {isEnabled('comments') && (
          <section className="mt-12 pt-8" style={{ borderTop: '0.5px solid var(--color-border)' }}>
            <CommentList articleId={article.id} />
          </section>
          )}
        </main>

        {/* ===== Right Sidebar (220px) ===== */}
        {isEnabled('sidebar') && (
        <aside
          className="hidden lg:block flex-shrink-0 sticky self-start overflow-y-auto"
          style={{ width: '220px', top: '100px', maxHeight: 'calc(100vh - 100px)' }}
        >
          <div className="space-y-6">
            {/* Share / Copy / Bookmark */}
            <ShareTools url={articleUrl} title={article.title} />

            {/* Divider */}
            <div style={{ borderTop: '0.5px solid var(--color-border)' }} />

            {/* Category Navigation */}
            {categories.length > 0 && (
              <div>
                <h4 className="text-[14px] font-medium mb-3" style={{ color: 'var(--color-text-primary)' }}>
                  分类导航
                </h4>
                <div className="space-y-1">
                  {categories
                    .sort((a, b) => (b.articleCount || 0) - (a.articleCount || 0))
                    .slice(0, 6)
                    .map((cat) => {
                      const articleCatIds = (article.categories?.length > 0 ? article.categories : (article.category ? [article.category] : [])).map(c => c.id)
                      const isActive = articleCatIds.includes(cat.id)
                      const brand = getCategoryBrand(cat.name)
                      return (
                        <Link
                          key={cat.id}
                          href={`/categories/${cat.slug}`}
                          className="flex items-center justify-between px-2 py-2 rounded-[6px] text-[14px] transition-colors"
                          style={{
                            backgroundColor: isActive ? brand.bg : 'transparent',
                            color: isActive ? brand.text : 'var(--color-text-secondary)',
                            fontWeight: isActive ? 500 : 400,
                          }}
                        >
                          <span>{cat.name}</span>
                          <span className="text-[11px]" style={{ color: 'var(--color-text-tertiary)' }}>{cat.articleCount}</span>
                        </Link>
                      )
                    })}
                </div>
                {categories.length > 6 && (
                  <Link
                    href="/categories"
                    className="hover-opacity-80 block mt-2 text-[12px] transition-colors"
                    style={{ color: 'var(--color-primary)' }}
                  >
                    查看全部 →
                  </Link>
                )}
              </div>
            )}

            {/* Tags */}
            {isEnabled('tags') && article.tags.length > 0 && (
              <div>
                <div style={{ borderTop: '0.5px solid var(--color-border)' }} className="mb-3" />
                <h4 className="text-[14px] font-medium mb-3" style={{ color: 'var(--color-text-primary)' }}>
                  标签
                </h4>
                <div className="flex flex-wrap gap-2">
                  {article.tags.map((tag) => {
                    const bgColor = tag.bgColor || 'var(--color-border-light)'
                    const textColor = tag.color || autoTextColor(tag.bgColor)
                    return (
                      <Link
                        key={tag.id}
                        href={`/tags/${tag.slug}`}
                        className="px-2.5 py-1 rounded-[4px] text-[12px] font-medium transition-colors hover:opacity-80"
                        style={{ backgroundColor: bgColor, color: textColor }}
                      >
                        {tag.name}
                      </Link>
                    )
                  })}
                </div>
              </div>
            )}
          </div>
        </aside>
        )}
      </div>
    </>
  )
}