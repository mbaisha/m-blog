import { notFound } from 'next/navigation'
import type { Metadata } from 'next'
import { fetchPageBySlug, fetchPublishedPages } from '@/lib/api'
import type { PublicPageItem } from '@/types'
import VisitTracker from '@/components/VisitTracker'

/** 页面缓存时间（秒），60 秒后重新验证 */
export const revalidate = 60

interface PageProps {
  params: Promise<{ slug: string }>
}

/** 生成静态路由参数 */
export async function generateStaticParams() {
  try {
    const res = await fetchPublishedPages()
    const pages = res.data || []
    return pages.map((page: PublicPageItem) => ({ slug: page.slug }))
  } catch {
    return []
  }
}

/** 动态生成 SEO 元数据 */
export async function generateMetadata({ params }: PageProps): Promise<Metadata> {
  const { slug } = await params
  try {
    const res = await fetchPageBySlug(slug)
    const page = res.data
    if (!page) return { title: '页面未找到' }
    return {
      title: page.seoTitle || page.title,
      description: page.seoDescription || page.summary || '',
      keywords: page.seoKeywords || undefined,
      openGraph: {
        title: page.seoTitle || page.title,
        description: page.seoDescription || page.summary || '',
        type: 'article',
        url: `${process.env.NEXT_PUBLIC_SITE_URL || 'http://localhost:3000'}/pages/${slug}`,
      },
      twitter: {
        card: 'summary_large_image',
        title: page.seoTitle || page.title,
        description: page.seoDescription || page.summary || '',
      },
    }
  } catch {
    return { title: '页面未找到' }
  }
}

export default async function Page({ params }: PageProps) {
  const { slug } = await params
  let page: PublicPageItem

  try {
    const res = await fetchPageBySlug(slug)
    page = res.data
  } catch {
    notFound()
  }

  if (!page) {
    notFound()
  }

  return (
    <div className="content-container py-12">
      <VisitTracker pagePath={`/pages/${slug}`} />
      <article className="mx-auto max-w-[800px]">
        <header className="mb-8">
          <h1 className="text-[24px] font-semibold" style={{ color: 'var(--color-text-primary)' }}>
            {page.title}
          </h1>
          {page.summary && (
            <p className="text-[14px] mt-2" style={{ color: 'var(--color-text-secondary)' }}>
              {page.summary}
            </p>
          )}
          <p className="text-[13px] mt-2" style={{ color: 'var(--color-text-tertiary)' }}>
            最后更新 {new Date(page.updatedAt).toISOString().slice(0, 10)}
          </p>
        </header>

        <div
          className="detail-article-body"
          style={{ borderTop: '0.5px solid var(--color-border)', paddingTop: '2em' }}
        >
          {page.content ? (
            <div className="whitespace-pre-wrap leading-relaxed" style={{ color: 'var(--color-text-primary)' }}>
              {page.content}
            </div>
          ) : (
            <p className="text-center py-16 text-[13px]" style={{ color: 'var(--color-text-tertiary)' }}>
              暂无内容
            </p>
          )}
        </div>

        {/* 7.8.3 可选评论区：后台配置控制 */}
        {page.enableComments && (
          <section
            className="mt-12 pt-8 text-center"
            style={{ borderTop: '0.5px solid var(--color-border)' }}
          >
            <h2 className="text-[15px] font-semibold mb-4" style={{ color: 'var(--color-text-primary)' }}>
              联系我们
            </h2>
            <p className="text-[13px] mb-6" style={{ color: 'var(--color-text-secondary)' }}>
              如有任何问题或建议，欢迎通过以下方式与我们联系
            </p>
            <div className="flex flex-wrap justify-center gap-4">
              <a
                href="mailto:contact@example.com"
                className="inline-flex items-center gap-2 px-4 py-2 rounded-[8px] text-[12px] font-medium transition-all hover:opacity-80"
                style={{
                  border: '0.5px solid var(--color-border)',
                  color: 'var(--color-text-secondary)',
                }}
              >
                <svg className="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M3 8l7.89 5.26a2 2 0 002.22 0L21 8M5 19h14a2 2 0 002-2V7a2 2 0 00-2-2H5a2 2 0 00-2 2v10a2 2 0 002 2z" />
                </svg>
                发送邮件
              </a>
            </div>
          </section>
        )}
      </article>
    </div>
  )
}
