import type { Metadata } from "next";
import Link from "next/link";
import { notFound } from "next/navigation";
import { apiClient } from "@/lib/api";
import type { ApiResponse, PublicCategoryInfo, PagedData, PublicArticleListItem, PublicModuleLayout } from "@/types";
import { fetchPageLayout } from "@/lib/api";
import VisitTracker from "@/components/VisitTracker"
import ArticleListView from "@/components/ArticleListView"
import { getServerSiteUrl } from "@/lib/runtimeConfig"

interface Props {
  params: Promise<{ slug: string }>
  searchParams: Promise<{ page?: string }>
}

export async function generateMetadata({ params }: Props): Promise<Metadata> {
  const { slug } = await params;
  try {
    const res = await apiClient.get<ApiResponse<PublicCategoryInfo[]>>("/categories");
    const cat = res.data.find((c) => c.slug === slug);
    if (!cat) return { title: "分类不存在" };
    return {
      title: `${cat.name} - 分类`,
      description: cat.description || `${cat.name}分类下的文章`,
      openGraph: {
        title: `${cat.name} - 分类`,
        description: cat.description || `${cat.name}分类下的文章`,
        type: 'website',
        url: `${getServerSiteUrl()}/categories/${cat.slug}`,
      },
      twitter: {
        card: 'summary_large_image',
        title: `${cat.name} - 分类`,
        description: cat.description || `${cat.name}分类下的文章`,
      },
    };
  } catch {
    return { title: "分类不存在" };
  }
}

export default async function CategoryDetailPage({ params, searchParams }: Props) {
  const { slug } = await params;
  const { page: pageStr } = await searchParams;
  const page = parseInt(pageStr || "1");

  let category: PublicCategoryInfo | null = null;
  let articles: PublicArticleListItem[] = [];
  let articlesTotal = 0;
  let totalPages = 1;
  let layoutModules: PublicModuleLayout[] = [];

  try {
    const [categoriesRes, layoutRes] = await Promise.all([
      apiClient.get<ApiResponse<PublicCategoryInfo[]>>("/categories"),
      fetchPageLayout("category_detail").catch(() => null),
    ]);
    category = categoriesRes.data.find((c) => c.slug === slug) || null;
    if (!category) notFound();
    layoutModules = layoutRes?.data || [];

    const articlesRes = await apiClient.get<ApiResponse<PagedData<PublicArticleListItem>>>("/articles", {
      page,
      pageSize: 12,
      categorySlug: slug,
    });
    articles = articlesRes.data.items;
    articlesTotal = articlesRes.data.totalCount;
    totalPages = articlesRes.data.totalPages;
  } catch {
    notFound();
  }

  const moduleMap = new Map(layoutModules.map(m => [m.moduleKey, m]))
  const isEnabled = (key: string) => {
    const mod = moduleMap.get(key)
    return mod ? mod.isEnabled : true
  }

  // 文章列表卡片列数
  const articleListMod = moduleMap.get('article_list')
  const articleListCfg = articleListMod ? (() => { try { return JSON.parse(articleListMod.config || '{}') } catch { return {} } })() : {}
  const articleListColumns = Math.min(Math.max(articleListCfg.columns || 3, 2), 4)

  return (
    <div className="content-container py-12">
      {/* page_hero */}
      {isEnabled('page_hero') && (
      <section className="mb-8">
        <Link
          href="/categories"
          className="group inline-flex items-center gap-1.5 px-3 py-1.5 rounded-full text-[12px] font-medium transition-all hover:scale-105 mb-4"
          style={{
            color: 'var(--color-text-secondary)',
            backgroundColor: 'var(--color-surface)',
            border: '0.5px solid var(--color-border)',
          }}
        >
          <svg className="w-3.5 h-3.5 transition-transform group-hover:-translate-x-0.5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M15 19l-7-7 7-7" />
          </svg>
          返回分类
        </Link>
        <h1 className="text-[24px] font-semibold mb-2" style={{ color: 'var(--color-text-primary)' }}>{category.name}</h1>
        {category.description && (
          <p className="text-[14px]" style={{ color: 'var(--color-text-secondary)' }}>{category.description}</p>
        )}
      </section>
      )}

      {/* article_list */}
      {isEnabled('article_list') && articles.length > 0 && (
        <ArticleListView articles={articles} showCategories={false} showTags={true} columns={articleListColumns} totalCount={articlesTotal} />
      )}

      {isEnabled('article_list') && articles.length === 0 && (
        <div className="empty-state">
          <p className="text-[#6b7280]">该分类下暂无文章</p>
        </div>
      )}

      {totalPages > 1 && (
        <div className="flex items-center justify-center gap-2 mt-12">
          {Array.from({ length: totalPages }, (_, i) => i + 1).map((p) => (
            <Link
              key={p}
              href={`/categories/${slug}?page=${p}`}
              className={`pagination-btn ${p === page ? "active" : ""}`}
            >
              {p}
            </Link>
          ))}
        </div>
      )}
      <VisitTracker pagePath={`/categories/${slug}`} />
    </div>
  );
}