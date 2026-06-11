import type { Metadata } from "next";
import Link from "next/link";
import { notFound } from "next/navigation";
import { apiClient } from "@/lib/api";
import type { ApiResponse, PublicTagInfo, PagedData, PublicArticleListItem, PublicModuleLayout } from "@/types";
import { fetchPageLayout } from "@/lib/api";
import VisitTracker from "@/components/VisitTracker"
import ArticleListView from "@/components/ArticleListView"

interface Props {
  params: Promise<{ slug: string }>
  searchParams: Promise<{ page?: string }>
}

export async function generateMetadata({ params }: Props): Promise<Metadata> {
  const { slug } = await params;
  try {
    const res = await apiClient.get<ApiResponse<PublicTagInfo[]>>("/tags");
    const tag = res.data.find((t) => t.slug === slug);
    if (!tag) return { title: "标签不存在" };
    return {
      title: `${tag.name} - 标签`,
      description: `${tag.name}标签相关的文章`,
      openGraph: {
        title: `${tag.name} - 标签`,
        description: `${tag.name}标签相关的文章`,
        type: 'website',
        url: `${process.env.NEXT_PUBLIC_SITE_URL || 'http://localhost:3000'}/tags/${tag.slug}`,
      },
      twitter: {
        card: 'summary_large_image',
        title: `${tag.name} - 标签`,
        description: `${tag.name}标签相关的文章`,
      },
    };
  } catch {
    return { title: "标签不存在" };
  }
}

export default async function TagDetailPage({ params, searchParams }: Props) {
  const { slug } = await params;
  const { page: pageStr } = await searchParams;
  const page = parseInt(pageStr || "1");

  let tag: PublicTagInfo | null = null;
  let articles: PublicArticleListItem[] = [];
  let articlesTotal = 0;
  let totalPages = 1;
  let layoutModules: PublicModuleLayout[] = [];

  try {
    const [tagsRes, layoutRes] = await Promise.all([
      apiClient.get<ApiResponse<PublicTagInfo[]>>("/tags"),
      fetchPageLayout("tag_detail").catch(() => null),
    ]);
    tag = tagsRes.data.find((t) => t.slug === slug) || null;
    if (!tag) notFound();
    layoutModules = layoutRes?.data || [];

    const articlesRes = await apiClient.get<ApiResponse<PagedData<PublicArticleListItem>>>("/articles", {
      page,
      pageSize: 12,
      tagSlug: slug,
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
          href="/tags"
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
          返回标签
        </Link>
        <h1 className="text-[24px] font-semibold mb-2" style={{ color: 'var(--color-text-primary)' }}>
          <span
            className="inline-block px-4 py-1.5 rounded-[6px] text-[14px] font-medium"
            style={{
              backgroundColor: tag.bgColor || '#EEEDFE',
              color: tag.color || '#534AB7',
            }}
          >
            {tag.name}
          </span>
        </h1>
      </section>
      )}

      {/* article_list */}
      {isEnabled('article_list') && articles.length > 0 && (
        <ArticleListView articles={articles} showCategories={true} showTags={true} columns={articleListColumns} totalCount={articlesTotal} />
      )}

      {isEnabled('article_list') && articles.length === 0 && (
        <div className="empty-state">
          <p className="text-[#6b7280]">该标签下暂无文章</p>
        </div>
      )}

      {totalPages > 1 && (
        <div className="flex items-center justify-center gap-2 mt-12">
          {Array.from({ length: totalPages }, (_, i) => i + 1).map((p) => (
            <Link
              key={p}
              href={`/tags/${slug}?page=${p}`}
              className={`pagination-btn ${p === page ? "active" : ""}`}
            >
              {p}
            </Link>
          ))}
        </div>
      )}
      <VisitTracker pagePath={`/tags/${slug}`} />
    </div>
  );
}