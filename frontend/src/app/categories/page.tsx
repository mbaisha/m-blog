import type { Metadata } from "next"
import Link from "next/link"
import { apiClient, fetchSeoSettings, fetchPageLayout } from "@/lib/api"
import type { ApiResponse, PublicCategoryInfo, PublicModuleLayout } from "@/types"
import VisitTracker from "@/components/VisitTracker"
import LayoutUniversalModules from "@/components/LayoutUniversalModules"

/** 生成元数据（从后台 SEO 配置获取） */
export async function generateMetadata(): Promise<Metadata> {
  const seo = await fetchSeoSettings("category")
  return {
    title: seo?.title || "分类",
    description: seo?.description || "浏览所有文章分类，按主题发现内容",
    keywords: seo?.keywords || undefined,
  }
}

const categoryBrandColors: Record<string, { bg: string; text: string }> = {
  "前端":   { bg: "#EEEDFE", text: "#534AB7" },
  "后端":   { bg: "#E1F5EE", text: "#0F6E56" },
  "产品":   { bg: "#FAECE7", text: "#B45A38" },
  "读书":   { bg: "#FBEAF0", text: "#B34776" },
  "AI":     { bg: "#E7EEFB", text: "#2E5DA8" },
  "工具":   { bg: "#FAEEDA", text: "#9C6A1D" },
}

function getCategoryBrand(name: string) {
  return categoryBrandColors[name] || { bg: "#F3F4F6", text: "#6B7280" }
}

export default async function CategoriesPage() {
  let categories: PublicCategoryInfo[] = []
  let layoutModules: PublicModuleLayout[] = []
  try {
    const [res, layoutRes] = await Promise.all([
      apiClient.get<ApiResponse<PublicCategoryInfo[]>>("/categories"),
      fetchPageLayout("categories").catch(() => null),
    ])
    categories = res.data
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
  let heroTitle = '分类', heroSubtitle = `按主题浏览文章 · 共 ${categories.length} 个分类`
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

      {/* category_grid */}
      {isEnabled('category_grid') && (categories.length > 0 ? (
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-5">
          {categories.map((cat) => {
            const brand = getCategoryBrand(cat.name)
            return (
              <Link
                key={cat.id}
                href={`/categories/${cat.slug}`}
                className="flex items-stretch rounded-[12px] overflow-hidden transition-all hover:-translate-y-0.5 hover:shadow-md"
                style={{
                  border: '0.5px solid var(--color-border)',
                  backgroundColor: 'var(--card-bg)',
                }}
              >
                {/* Left brand color bar */}
                <div style={{ width: '6px', backgroundColor: brand.bg, flexShrink: 0 }} />
                <div className="flex-1 p-5">
                  <div className="flex items-start justify-between gap-3">
                    <div className="min-w-0 flex-1">
                      <h2
                        className="text-[16px] font-medium mb-1.5 truncate"
                        style={{ color: brand.text }}
                      >
                        {cat.name}
                      </h2>
                      {cat.description && (
                        <p className="text-[12px] line-clamp-2 mb-3" style={{ color: 'var(--color-text-secondary)' }}>
                          {cat.description}
                        </p>
                      )}
                    </div>
                    {/* Article count badge */}
                    <span
                      className="flex-shrink-0 inline-flex items-center px-2.5 py-1 rounded-full text-[10px] font-medium"
                      style={{ backgroundColor: brand.bg, color: brand.text }}
                    >
                      {cat.articleCount} 篇
                    </span>
                  </div>
                  {/* Bottom accent line */}
                  <div className="mt-3 h-[2px] rounded-full" style={{ backgroundColor: brand.bg, opacity: 0.5 }} />
                </div>
              </Link>
            )
          })}
        </div>
      ) : (
        <div className="flex flex-col items-center justify-center py-16">
          <div className="text-4xl mb-4 opacity-40">📂</div>
          <h3 className="text-[16px] font-medium mb-2" style={{ color: 'var(--color-text-primary)' }}>
            暂无分类
          </h3>
          <p className="text-[13px]" style={{ color: 'var(--color-text-secondary)' }}>
            分类正在准备中
          </p>
        </div>
      ))}
      <LayoutUniversalModules pageKey="categories" />
      <VisitTracker pagePath="/categories" />
    </div>
  )
}
