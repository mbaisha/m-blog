import type { Metadata } from "next"
import Link from "next/link"
import { apiClient, fetchSeoSettings, fetchPageLayout } from "@/lib/api"
import type { ApiResponse, ArchiveItem, PublicModuleLayout } from "@/types"
import VisitTracker from "@/components/VisitTracker"
import LayoutUniversalModules from "@/components/LayoutUniversalModules"

/** 生成元数据（从后台 SEO 配置获取） */
export async function generateMetadata(): Promise<Metadata> {
  const seo = await fetchSeoSettings("archive")
  return {
    title: seo?.title || "归档",
    description: seo?.description || "按时间轴浏览所有文章",
    keywords: seo?.keywords || undefined,
  }
}

const MONTH_NAMES = [
  "", "一月", "二月", "三月", "四月", "五月", "六月",
  "七月", "八月", "九月", "十月", "十一月", "十二月",
]

export default async function ArchivePage() {
  let archives: ArchiveItem[] = []
  let layoutModules: PublicModuleLayout[] = []
  try {
    const [res, layoutRes] = await Promise.all([
      apiClient.get<ApiResponse<ArchiveItem[]>>("/archive"),
      fetchPageLayout("archive").catch(() => null),
    ])
    archives = res.data
    layoutModules = layoutRes?.data || []
  } catch {
    // ignore
  }

  const totalArticles = archives.reduce((sum, a) => sum + a.count, 0)
  const moduleMap = new Map(layoutModules.map(m => [m.moduleKey, m]))
  const isEnabled = (key: string) => {
    const mod = moduleMap.get(key)
    return mod ? mod.isEnabled : true
  }

  // 从 page_hero 模块获取自定义标题
  const heroMod = layoutModules.find(m => m.moduleKey === 'page_hero' && m.isEnabled)
  let heroTitle = '归档', heroSubtitle = `共 ${totalArticles} 篇文章`
  if (heroMod?.config) {
    try { const c = JSON.parse(heroMod.config); heroSubtitle = c.customSubtitle || heroSubtitle } catch { /* ignore */ }
  }
  if (heroMod?.title) heroTitle = heroMod.title

  return (
    <div className="content-container py-12">
      {/* page_hero */}
      {isEnabled('page_hero') && (
      <section className="mb-8">
        <h1 className="text-[24px] font-semibold" style={{ color: 'var(--color-text-primary)' }}>
          {heroTitle}
        </h1>
        <p className="text-[14px] mt-2" style={{ color: 'var(--color-text-secondary)' }}>
          {heroSubtitle}
        </p>
      </section>
      )}

      {/* timeline */}
      {isEnabled('timeline') && (archives.length > 0 ? (
        <div className="space-y-10 max-w-[800px]">
          {archives.map((archive) => (
            <div key={`${archive.year}-${archive.month}`}>
              <h2
                className="text-[18px] font-medium mb-4 sticky top-[60px] py-2 z-10"
                style={{
                  color: 'var(--color-text-primary)',
                  backgroundColor: 'var(--color-bg)',
                }}
              >
                {archive.year} 年 {MONTH_NAMES[archive.month]}
                <span className="text-[12px] font-normal ml-2" style={{ color: 'var(--color-text-tertiary)' }}>
                  ({archive.count} 篇)
                </span>
              </h2>

              <div className="relative pl-8" style={{ borderLeft: '2px solid var(--color-border)' }}>
                <div className="space-y-5">
                  {archive.articles.map((article) => (
                    <div key={article.id} className="relative">
                      {/* Timeline dot */}
                      <div
                        className="absolute -left-[33px] top-1.5 w-3 h-3 rounded-full"
                        style={{
                          backgroundColor: 'var(--color-bg)',
                          border: '2px solid var(--color-primary)',
                        }}
                      />

                      <Link
                        href={`/articles/${article.slug}`}
                        className="block group p-4 rounded-[8px] transition-colors hover:bg-[var(--color-border-light)] -mx-4 px-4"
                      >
                        <div className="flex items-center gap-2 mb-1">
                          <time className="text-[12px]" style={{ color: 'var(--color-text-tertiary)' }}>
                            {new Date(article.publishedAt).toLocaleDateString("zh-CN")}
                          </time>
                        </div>
                        <h3
                          className="hover-text-primary text-[14px] font-medium transition-colors"
                          style={{ color: 'var(--color-text-primary)' }}
                        >
                          {article.title}
                        </h3>
                        {article.summary && (
                          <p className="text-[12px] mt-1 line-clamp-1" style={{ color: 'var(--color-text-secondary)' }}>
                            {article.summary}
                          </p>
                        )}
                      </Link>
                    </div>
                  ))}
                </div>
              </div>
            </div>
          ))}
        </div>
      ) : (
        <div className="flex flex-col items-center justify-center py-16">
          <div className="text-4xl mb-4 opacity-40">📅</div>
          <h3 className="text-[16px] font-medium mb-2" style={{ color: 'var(--color-text-primary)' }}>
            暂无文章
          </h3>
          <p className="text-[13px]" style={{ color: 'var(--color-text-secondary)' }}>
            文章正在准备中
          </p>
        </div>
      ))}

      <LayoutUniversalModules pageKey="archive" />
      <VisitTracker pagePath="/archive" />
    </div>
  )
}