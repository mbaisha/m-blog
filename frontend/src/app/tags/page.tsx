import type { Metadata } from "next"
import Link from "next/link"
import { apiClient, fetchSeoSettings, fetchPageLayout } from "@/lib/api"
import type { ApiResponse, PublicTagInfo, PublicModuleLayout } from "@/types"
import VisitTracker from "@/components/VisitTracker"
import LayoutUniversalModules from "@/components/LayoutUniversalModules"

/** 生成元数据（从后台 SEO 配置获取） */
export async function generateMetadata(): Promise<Metadata> {
  const seo = await fetchSeoSettings("tag")
  return {
    title: seo?.title || "标签",
    description: seo?.description || "通过标签快速发现相关内容",
    keywords: seo?.keywords || undefined,
  }
}

/** 根据背景色自动计算前景色（黑或白） */
function autoTextColor(bgColor: string): string {
  const hex = bgColor.replace('#', '')
  const r = parseInt(hex.substring(0, 2), 16)
  const g = parseInt(hex.substring(2, 4), 16)
  const b = parseInt(hex.substring(4, 6), 16)
  const luminance = (0.299 * r + 0.587 * g + 0.114 * b) / 255
  return luminance > 0.6 ? '#1F2937' : '#FFFFFF'
}

export default async function TagsPage() {
  let tags: PublicTagInfo[] = []
  let layoutModules: PublicModuleLayout[] = []
  try {
    const [res, layoutRes] = await Promise.all([
      apiClient.get<ApiResponse<PublicTagInfo[]>>("/tags"),
      fetchPageLayout("tags").catch(() => null),
    ])
    tags = res.data
    layoutModules = layoutRes?.data || []
  } catch {
    // ignore
  }

  const moduleMap = new Map(layoutModules.map(m => [m.moduleKey, m]))
  const isEnabled = (key: string) => {
    const mod = moduleMap.get(key)
    return mod ? mod.isEnabled : true
  }

  // 从 page_hero 获取自定义标题
  const heroMod = layoutModules.find(m => m.moduleKey === 'page_hero' && m.isEnabled)
  let heroTitle = '标签', heroSubtitle = `通过标签快速发现相关内容 · 共 ${tags.length} 个标签`
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

      {/* tag_cloud */}
      {isEnabled('tag_cloud') && (tags.length > 0 ? (
        <div className="flex flex-wrap gap-3 max-w-3xl">
          {tags.map((tag) => {
            // 使用 API 返回的 BgColor + Color，未设置时自动生成
            const bgColor = tag.bgColor || '#EEEDFE'
            const textColor = tag.color || autoTextColor(bgColor)
            // 根据文章数决定标签大小
            const maxCount = Math.max(...tags.map(t => t.articleCount || 1))
            const ratio = (tag.articleCount || 1) / maxCount
            const fontSize = 12 + Math.round(ratio * 6) // 12px ~ 18px
            const paddingX = 10 + Math.round(ratio * 8)
            const paddingY = 4 + Math.round(ratio * 4)
            return (
              <Link
                key={tag.id}
                href={`/tags/${tag.slug}`}
                className="inline-flex items-center gap-1.5 rounded-[6px] font-medium transition-all hover:-translate-y-0.5 hover:shadow-sm"
                style={{
                  backgroundColor: bgColor,
                  color: textColor,
                  fontSize: `${fontSize}px`,
                  padding: `${paddingY}px ${paddingX}px`,
                }}
              >
                {tag.name}
                <span className="opacity-60" style={{ fontSize: `${Math.max(fontSize - 2, 10)}px` }}>
                  {tag.articleCount}
                </span>
              </Link>
            )
          })}
        </div>
      ) : (
        <div className="flex flex-col items-center justify-center py-16">
          <div className="text-4xl mb-4 opacity-40">🏷️</div>
          <h3 className="text-[16px] font-medium mb-2" style={{ color: 'var(--color-text-primary)' }}>
            暂无标签
          </h3>
          <p className="text-[13px]" style={{ color: 'var(--color-text-secondary)' }}>
            标签正在准备中
          </p>
        </div>
      ))}
      <LayoutUniversalModules pageKey="tags" />
      <VisitTracker pagePath="/tags" />
    </div>
  )
}