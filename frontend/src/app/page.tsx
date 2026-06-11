import type { Metadata } from "next"
import { apiClient, fetchSeoSettings, fetchSiteSettings } from "@/lib/api"
import type { ApiResponse, PagedData, PublicArticleListItem, PublicCategoryInfo, PublicTagInfo, PublicProjectListItem, PublicSiteSettingResponse, PublicModuleLayout } from "@/types"
import { fetchPageLayout } from "@/lib/api"
import HomeContent from "@/components/HomeContent"

/** 生成首页元数据（从后台 SEO 配置获取） */
export async function generateMetadata(): Promise<Metadata> {
  const seo = await fetchSeoSettings("home")
  return {
    title: seo?.title || undefined,
    description: seo?.description || undefined,
    keywords: seo?.keywords || undefined,
  }
}

// ---- Data Fetching ----

async function getHomeData() {
  try {
    const [articlesRes, projectsRes, categoriesRes, tagsRes, siteSettingsRes, layoutRes] = await Promise.all([
      apiClient.get<ApiResponse<PagedData<PublicArticleListItem>>>("/articles", { page: 1, pageSize: 50, sortBy: "latest" }),
      apiClient.get<ApiResponse<PublicProjectListItem[]>>("/projects"),
      apiClient.get<ApiResponse<PublicCategoryInfo[]>>("/categories"),
      apiClient.get<ApiResponse<PublicTagInfo[]>>("/tags"),
      apiClient.get<ApiResponse<PublicSiteSettingResponse>>("/settings").catch(() => null),
      fetchPageLayout("home").catch(() => null),
    ])
    const allArticles = articlesRes.data.items
    const layoutModules = layoutRes?.data || []
    // 构建模块配置查找表
    const moduleMap = new Map(layoutModules.map((m: PublicModuleLayout) => [m.moduleKey, m]))

    // 读取配置的工具函数
    const getCount = (key: string, fallback: number) => {
      const mod = moduleMap.get(key)
      if (!mod || !mod.isEnabled) return 0
      try { const c = JSON.parse(mod.config || '{}'); return c.count ?? fallback } catch { return fallback }
    }
    const getCategoryIds = (key: string) => {
      const mod = moduleMap.get(key)
      if (!mod) return []
      try { const c = JSON.parse(mod.config || '{}'); return c.categoryIds ?? [] } catch { return [] }
    }

    // 热门标签 — 按配置动态截取
    const tagCount = getCount('hot_tags', 10)
    const tags = (tagsRes.data || []).slice(0, tagCount)

    // 精选文章 — 按配置动态截取 + 分类筛选
    const fpCount = getCount('featured_posts', 3)
    const fpCategoryIds = getCategoryIds('featured_posts')
    let recommended = allArticles.filter((a) => a.isRecommend)
    if (fpCategoryIds.length > 0) {
      recommended = recommended.filter((a) => {
        const cats = a.categories?.length > 0 ? a.categories : (a.category ? [a.category] : [])
        return cats.some((c) => fpCategoryIds.includes(c.id))
      })
    }
    recommended = recommended.slice(0, fpCount)

    // 最新文章 — 按发布时间全局倒序（不管是否置顶）
    const rpCount = getCount('recent_posts', 5)
    const sortedByPublishedAt = [...allArticles].sort((a, b) => {
      const dateA = a.publishedAt ? new Date(a.publishedAt).getTime() : 0
      const dateB = b.publishedAt ? new Date(b.publishedAt).getTime() : 0
      return dateB - dateA
    })
    const latest = sortedByPublishedAt.slice(0, rpCount)

    // 项目展示 — 按配置动态截取
    const projCount = getCount('projects', 4)
    const projects = (projectsRes.data || []).slice(0, projCount)

    return {
      articles: allArticles,
      recommended,
      latest,
      projects,
      categories: categoriesRes.data || [],
      tags,
      totalArticles: articlesRes.data.totalCount || allArticles.length,
      siteSettings: siteSettingsRes?.data || null,
      layoutModules,
    }
  } catch {
    return { articles: [], recommended: [], latest: [], projects: [], categories: [], tags: [], totalArticles: 0, siteSettings: null, layoutModules: [] }
  }
}

// ---- Home Page Component ----

export default async function HomePage() {
  const data = await getHomeData()
  return <HomeContent {...data} />
}
