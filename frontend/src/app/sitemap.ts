import { MetadataRoute } from 'next'

const BASE_URL = process.env.NEXT_PUBLIC_SITE_URL || 'http://localhost:3000'

/**
 * 9.1 自动生成 Sitemap
 * 包含所有静态页面 + 动态内容页面（文章、分类、标签、项目、自定义页面）
 */
export default async function sitemap(): Promise<MetadataRoute.Sitemap> {
  const staticRoutes: MetadataRoute.Sitemap = [
    { url: BASE_URL, lastModified: new Date(), changeFrequency: 'daily', priority: 1.0 },
    { url: `${BASE_URL}/articles`, lastModified: new Date(), changeFrequency: 'daily', priority: 0.9 },
    { url: `${BASE_URL}/projects`, lastModified: new Date(), changeFrequency: 'weekly', priority: 0.7 },
    { url: `${BASE_URL}/categories`, lastModified: new Date(), changeFrequency: 'weekly', priority: 0.6 },
    { url: `${BASE_URL}/tags`, lastModified: new Date(), changeFrequency: 'weekly', priority: 0.5 },
    { url: `${BASE_URL}/archive`, lastModified: new Date(), changeFrequency: 'weekly', priority: 0.5 },
    { url: `${BASE_URL}/about`, lastModified: new Date(), changeFrequency: 'monthly', priority: 0.6 },
    { url: `${BASE_URL}/friends`, lastModified: new Date(), changeFrequency: 'monthly', priority: 0.4 },
    { url: `${BASE_URL}/search`, lastModified: new Date(), changeFrequency: 'monthly', priority: 0.3 },
  ]

  // 动态获取文章列表
  let articleRoutes: MetadataRoute.Sitemap = []
  let categoryRoutes: MetadataRoute.Sitemap = []
  let tagRoutes: MetadataRoute.Sitemap = []
  let projectRoutes: MetadataRoute.Sitemap = []
  let pageRoutes: MetadataRoute.Sitemap = []

  try {
    const API_BASE = process.env.INTERNAL_API_BASE_URL || process.env.NEXT_PUBLIC_API_BASE_URL || 'http://localhost:5092/api'

    // 5 秒超时，避免构建时因后端未启动而长时间挂起
    const controller = new AbortController()
    const timeout = setTimeout(() => controller.abort(), 5000)

    const fetchOptions = { signal: controller.signal }

    const [articlesRes, categoriesRes, tagsRes, projectsRes, pagesRes] = await Promise.all([
      fetch(`${API_BASE}/articles?pageSize=500`, fetchOptions).then(r => r.json()).catch(() => ({ data: { items: [] } })),
      fetch(`${API_BASE}/categories`, fetchOptions).then(r => r.json()).catch(() => ({ data: [] })),
      fetch(`${API_BASE}/tags`, fetchOptions).then(r => r.json()).catch(() => ({ data: [] })),
      fetch(`${API_BASE}/projects`, fetchOptions).then(r => r.json()).catch(() => ({ data: [] })),
      fetch(`${API_BASE}/pages/published`, fetchOptions).then(r => r.json()).catch(() => ({ data: [] })),
    ])

    clearTimeout(timeout)

    const items = articlesRes?.data?.items || []
    articleRoutes = items.map((article: any) => ({
      url: `${BASE_URL}/articles/${article.slug}`,
      lastModified: new Date(article.updatedAt || article.publishedAt || new Date()),
      changeFrequency: 'weekly' as const,
      priority: 0.8,
    }))

    const categories = categoriesRes?.data || []
    categoryRoutes = categories.map((cat: any) => ({
      url: `${BASE_URL}/categories/${cat.slug}`,
      lastModified: new Date(),
      changeFrequency: 'weekly' as const,
      priority: 0.6,
    }))

    const tags = tagsRes?.data || []
    tagRoutes = tags.map((tag: any) => ({
      url: `${BASE_URL}/tags/${tag.slug}`,
      lastModified: new Date(),
      changeFrequency: 'weekly' as const,
      priority: 0.5,
    }))

    const projects = projectsRes?.data?.items || projectsRes?.data || []
    projectRoutes = (Array.isArray(projects) ? projects : []).map((proj: any) => ({
      url: `${BASE_URL}/projects/${proj.slug}`,
      lastModified: new Date(proj.updatedAt || proj.createdAt || new Date()),
      changeFrequency: 'monthly' as const,
      priority: 0.6,
    }))

    const pages = pagesRes?.data || []
    pageRoutes = (Array.isArray(pages) ? pages : []).map((page: any) => ({
      url: `${BASE_URL}/${page.slug}`,
      lastModified: new Date(page.updatedAt || page.createdAt || new Date()),
      changeFrequency: 'monthly' as const,
      priority: 0.5,
    }))
  } catch {
    // 静默失败，只返回静态路由
  }

  return [
    ...staticRoutes,
    ...articleRoutes,
    ...categoryRoutes,
    ...tagRoutes,
    ...projectRoutes,
    ...pageRoutes,
  ]
}