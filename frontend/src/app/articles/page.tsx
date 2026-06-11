import type { Metadata } from "next"
import { apiClient, fetchSeoSettings, fetchPageLayout } from "@/lib/api"
import type { ApiResponse, PagedData, PublicArticleListItem, PublicCategoryInfo, PublicTagInfo, PublicModuleLayout } from "@/types"
import ArticlesPageContent from "@/components/ArticlesPageContent"
import VisitTracker from "@/components/VisitTracker"

/** 生成元数据（从后台 SEO 配置获取） */
export async function generateMetadata(): Promise<Metadata> {
  const seo = await fetchSeoSettings("articles")
  return {
    title: seo?.title || "文章",
    description: seo?.description || "浏览所有文章列表，可按分类、标签筛选",
    keywords: seo?.keywords || undefined,
  }
}

interface Props {
  searchParams: Promise<{
    page?: string
    categorySlug?: string
    tagSlug?: string
    sortBy?: string
    q?: string
  }>
}

export default async function ArticlesPage({ searchParams }: Props) {
  const params = await searchParams
  const page = parseInt(params.page || "1")
  const categorySlug = params.categorySlug || ""
  const tagSlug = params.tagSlug || ""
  const sortBy = (params.sortBy || "latest") as "latest" | "views" | "comments"
  const query = params.q || ""

  const [articlesRes, categoriesRes, tagsRes, layoutRes, totalAllRes] = await Promise.all([
    apiClient.get<ApiResponse<PagedData<PublicArticleListItem>>>("/articles", {
      page,
      pageSize: 12,
      categorySlug: categorySlug || undefined,
      tagSlug: tagSlug || undefined,
      sortBy,
      q: query || undefined,
    }),
    apiClient.get<ApiResponse<PublicCategoryInfo[]>>("/categories"),
    apiClient.get<ApiResponse<PublicTagInfo[]>>("/tags"),
    fetchPageLayout("articles").catch(() => null),
    // 无筛选条件获取全量总数
    apiClient.get<ApiResponse<PagedData<PublicArticleListItem>>>("/articles", { page: 1, pageSize: 1 }).catch(() => null),
  ])
  const layoutModules: PublicModuleLayout[] = layoutRes?.data || []
  const initialTotalAll = totalAllRes?.data?.totalCount ?? articlesRes.data.totalCount

  return (
    <>
      <ArticlesPageContent
        initialArticles={articlesRes.data.items}
        categories={categoriesRes.data}
        tags={tagsRes.data}
        initialTotal={articlesRes.data.totalCount}
        initialTotalAll={initialTotalAll}
        initialTotalPages={articlesRes.data.totalPages}
        initialCategorySlug={categorySlug}
        initialTagSlug={tagSlug}
        initialSortBy={sortBy}
        initialQuery={query}
        initialPage={page}
        layoutModules={layoutModules}
      />
      <VisitTracker pagePath="/articles" />
    </>
  )
}