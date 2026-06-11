import Link from "next/link"
import { apiClient } from "@/lib/api"
import type { ApiResponse, PagedData, PublicArticleListItem, PublicCategoryInfo, PublicTagInfo } from "@/types"

/**
 * 7.13 404 页面
 * 手绘风大号 404 + 友好提示 + 建议卡片（最新文章、热门标签、全部分类）
 */

interface CategoryColor {
  bg: string
  text: string
}

const categoryBrandColors: Record<string, CategoryColor> = {
  "前端":   { bg: "#EEEDFE", text: "#534AB7" },
  "后端":   { bg: "#E1F5EE", text: "#0F6E56" },
  "产品":   { bg: "#FAECE7", text: "#B45A38" },
  "读书":   { bg: "#FBEAF0", text: "#B34776" },
  "AI":     { bg: "#E7EEFB", text: "#2E5DA8" },
}

function getCategoryBrand(name: string): CategoryColor {
  return categoryBrandColors[name] || { bg: "#F3F4F6", text: "#6B7280" }
}

const tagColors = [
  { bg: "#EEEDFE", text: "#534AB7" },
  { bg: "#E1F5EE", text: "#0F6E56" },
  { bg: "#FAECE7", text: "#993C1D" },
  { bg: "#FBEAF0", text: "#993556" },
  { bg: "#E7EEFB", text: "#2E5DA8" },
  { bg: "#FAEEDA", text: "#9C6A1D" },
  { bg: "#EAF3DE", text: "#3D7A3E" },
  { bg: "#F3F4F6", text: "#6B7280" },
]

function getTagColor(index: number) {
  return tagColors[index % tagColors.length]
}

async function getSuggestions() {
  try {
    const [articlesRes, categoriesRes, tagsRes] = await Promise.all([
      apiClient.get<ApiResponse<PagedData<PublicArticleListItem>>>("/articles?pageSize=3&sortBy=publishedAt"),
      apiClient.get<ApiResponse<PublicCategoryInfo[]>>("/categories"),
      apiClient.get<ApiResponse<PublicTagInfo[]>>("/tags"),
    ])
    return {
      articles: articlesRes.data?.items || [],
      categories: categoriesRes.data || [],
      tags: tagsRes.data || [],
    }
  } catch {
    return { articles: [], categories: [], tags: [] }
  }
}

export default async function NotFound() {
  const { articles, categories, tags } = await getSuggestions()

  return (
    <div className="content-container py-16">
      {/* 404 主区域 */}
      <div className="flex flex-col items-center justify-center text-center mb-16">
        <div className="text-[80px] md:text-[120px] font-bold leading-none mb-4 select-none"
          style={{ color: "rgba(99, 102, 241, 0.15)" }}>
          404
        </div>
        <h1 className="text-[20px] font-medium mb-2" style={{ color: 'var(--color-text-primary)' }}>
          页面未找到
        </h1>
        <p className="text-[13px] mb-8 max-w-[400px]" style={{ color: 'var(--color-text-secondary)' }}>
          你访问的页面不存在，可能已被移除或链接错误
        </p>
        <div className="flex gap-3">
          <Link
            href="/"
            className="px-5 py-2 rounded-[8px] text-[13px] font-medium text-white transition-opacity hover:opacity-90"
            style={{ backgroundColor: 'var(--color-primary)' }}
          >
            返回首页
          </Link>
          <Link
            href="/articles"
            className="px-5 py-2 rounded-[8px] text-[13px] font-medium transition-all"
            style={{
              border: '0.5px solid var(--color-border)',
              color: 'var(--color-text-secondary)',
            }}
          >
            查看文章
          </Link>
        </div>
      </div>

      {/* 建议卡片 */}
      <div className="max-w-[800px] mx-auto space-y-10">
        {/* 最新文章 */}
        {articles.length > 0 && (
          <section>
            <h2 className="text-[15px] font-semibold mb-4" style={{ color: 'var(--color-text-primary)' }}>
              最新文章
            </h2>
            <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
              {articles.map((article) => {
                const cats = article.categories?.length > 0 ? article.categories : (article.category ? [article.category] : [])
                return (
                  <Link
                    key={article.id}
                    href={`/articles/${article.slug}`}
                    className="block p-4 rounded-[12px] transition-all hover:opacity-80"
                    style={{
                      border: '0.5px solid var(--color-border)',
                      backgroundColor: 'var(--color-surface)',
                    }}
                  >
                    {cats.length > 0 && cats.slice(0, 1).map((cat) => (
                      <span
                        key={cat.id}
                        className="inline-block px-2 py-0.5 rounded-[4px] text-[10px] font-medium mb-2"
                        style={{
                          backgroundColor: getCategoryBrand(cat.name).bg,
                          color: getCategoryBrand(cat.name).text,
                        }}
                      >
                        {cat.name}
                      </span>
                    ))}
                  <h3 className="text-[13px] font-medium line-clamp-2" style={{ color: 'var(--color-text-primary)' }}>
                    {article.title}
                  </h3>
                </Link>
              );
            })}
            </div>
          </section>
        )}

        {/* 热门标签 */}
        {tags.length > 0 && (
          <section>
            <h2 className="text-[15px] font-semibold mb-4" style={{ color: 'var(--color-text-primary)' }}>
              热门标签
            </h2>
            <div className="flex flex-wrap gap-2">
              {tags.slice(0, 12).map((tag) => {
                const bgColor = tag.bgColor || '#EEEDFE'
                const textColor = tag.color || (() => {
                  if (!tag.bgColor) return '#6B7280'
                  const hex = tag.bgColor.replace('#', '')
                  const r = parseInt(hex.substring(0, 2), 16)
                  const g = parseInt(hex.substring(2, 4), 16)
                  const b = parseInt(hex.substring(4, 6), 16)
                  const luminance = (0.299 * r + 0.587 * g + 0.114 * b) / 255
                  return luminance > 0.6 ? '#1F2937' : '#FFFFFF'
                })()
                return (
                  <Link
                    key={tag.id}
                    href={`/tags/${tag.slug}`}
                    className="px-3 py-1.5 rounded-[6px] text-[12px] font-medium transition-all hover:opacity-80"
                    style={{
                      backgroundColor: bgColor,
                      color: textColor,
                    }}
                  >
                    {tag.name}
                    {tag.articleCount > 0 && (
                      <span className="ml-1 opacity-60">({tag.articleCount})</span>
                    )}
                  </Link>)})}
            </div>
          </section>
        )}

        {/* 全部分类 */}
        {categories.length > 0 && (
          <section>
            <h2 className="text-[15px] font-semibold mb-4" style={{ color: 'var(--color-text-primary)' }}>
              全部分类
            </h2>
            <div className="grid grid-cols-2 md:grid-cols-3 gap-3">
              {categories.map((cat) => {
                const brand = getCategoryBrand(cat.name)
                return (
                  <Link
                    key={cat.id}
                    href={`/categories/${cat.slug}`}
                    className="flex items-center gap-3 p-3 rounded-[10px] transition-all hover:opacity-80"
                    style={{
                      border: '0.5px solid var(--color-border)',
                      backgroundColor: 'var(--color-surface)',
                    }}
                  >
                    {/* 品牌色竖线 */}
                    <div
                      className="w-[4px] h-[24px] rounded-full flex-shrink-0"
                      style={{ backgroundColor: brand.bg }}
                    />
                    <div className="min-w-0 flex-1">
                      <div className="text-[12px] font-medium truncate" style={{ color: brand.text }}>
                        {cat.name}
                      </div>
                      {cat.articleCount > 0 && (
                        <div className="text-[10px] mt-0.5" style={{ color: 'var(--color-text-tertiary)' }}>
                          {cat.articleCount} 篇文章
                        </div>
                      )}
                    </div>
                  </Link>
                )
              })}
            </div>
          </section>
        )}
      </div>
    </div>
  )
}