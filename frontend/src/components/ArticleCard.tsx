'use client'

import Link from 'next/link'
import Image from 'next/image'
import type { PublicArticleListItem } from '@/types'

// ===== Brand colors =====
const categoryBrandColors: Record<string, { bg: string; text: string; border: string; coverBg: string }> = {
  "前端":        { bg: "#EEEDFE", text: "#534AB7", border: "#CECBF6", coverBg: "#EEEDFE" },
  "后端":        { bg: "#E1F5EE", text: "#0F6E56", border: "#9FE1CB", coverBg: "#E1F5EE" },
  "产品":        { bg: "#FAECE7", text: "#B45A38", border: "#F5C4B3", coverBg: "#FAECE7" },
  "读书":        { bg: "#FBEAF0", text: "#B34776", border: "#F4C0D1", coverBg: "#FBEAF0" },
  "AI":          { bg: "#E7EEFB", text: "#2E5DA8", border: "#B3CCF4", coverBg: "#EAF3DE" },
  "其他":        { bg: "#F3F4F6", text: "#6B7280", border: "#E5E7EB", coverBg: "#F3F4F6" },
}

const defaultBrand = categoryBrandColors["其他"]

function getCategoryBrand(name: string) {
  return categoryBrandColors[name] || defaultBrand
}

/** 获取标题第一个有效字符（字母或汉字，字母转为大写，跳过标点） */
function getFirstValidChar(title: string): string {
  for (const ch of title) {
    if (/[a-zA-Z]/.test(ch)) return ch.toUpperCase()
    if (/[\u4e00-\u9fff]/.test(ch)) return ch
  }
  return '?'
}

interface Props {
  article: PublicArticleListItem
  /** 视图模式 */
  viewMode: 'card' | 'list'
  /** 是否显示分类徽章（分类详情页隐藏） */
  showCategories?: boolean
  /** 是否显示标签（标签详情页隐藏） */
  showTags?: boolean
}

/** 可复用的文章卡片组件，与文章列表页保持一致的布局和样式 */
export default function ArticleCard({
  article,
  viewMode,
  showCategories = true,
  showTags = true,
}: Props) {
  const catBrand = article.category ? getCategoryBrand(article.category.name) : defaultBrand
  const cats = article.categories?.length > 0 ? article.categories : (article.category ? [article.category] : [])

  if (viewMode === 'card') {
    return (
      <Link
        href={`/articles/${article.slug}`}
        className="flex flex-col rounded-[10px] overflow-hidden transition-all hover:-translate-y-0.5"
        style={{
          border: '0.5px solid var(--color-border)',
          backgroundColor: 'var(--card-bg)',
          boxShadow: 'var(--card-shadow)',
        }}
      >
        {/* Cover - fixed aspect ratio 16:9 */}
        <div
          className="relative w-full overflow-hidden"
          style={{
            aspectRatio: '16/9',
            backgroundColor: article.coverImageUrl ? undefined : catBrand.coverBg,
            color: catBrand.text,
          }}
        >
          {article.coverImageUrl ? (
            <Image src={article.coverImageUrl} alt={article.title} fill className="object-cover" sizes="(max-width: 767px) 100vw, (max-width: 1023px) 50vw, 33vw" loading="lazy" />
          ) : (
            <div className="absolute inset-0 flex items-center justify-center text-[24px] font-bold">
              <span>{getFirstValidChar(article.title)}</span>
            </div>
          )}
          {article.isTop && (
            <div className="absolute top-1.5 right-1.5 z-10 flex items-center gap-1 px-1.5 py-0.5 rounded-[4px] text-[9px] font-medium bg-orange-500 text-white shadow-sm">
              <svg className="w-3 h-3" fill="currentColor" viewBox="0 0 24 24"><path d="M16 12h-2V6h2l4-4-4-4h-2V2h-2V0h-2v2H8V0H6L2 4l4 4h2v6H6v2l6 6 6-6z" transform="rotate(45 12 12)" /></svg>
              置顶
            </div>
          )}
        </div>
        {/* Content */}
        <div className="p-3 flex flex-col gap-1.5 flex-1">
          {/* Title */}
          <h3 className="text-[14px] font-semibold leading-snug line-clamp-2" style={{ color: 'var(--color-text-primary)' }}>
            {article.title}
          </h3>
          {/* Categories - 标签详情页显示，分类详情页不显示 */}
          {showCategories && cats.length > 0 && (
            <div className="flex flex-wrap gap-1">
              {cats.map((cat) => {
                const brand = getCategoryBrand(cat.name)
                return (
                  <span
                    key={cat.id}
                    className="px-1.5 py-0.5 rounded-[3px] text-[10px] font-medium"
                    style={{ backgroundColor: brand.bg, color: brand.text }}
                  >
                    {cat.name}
                  </span>
                )
              })}
            </div>
          )}
          {/* Summary */}
          {article.summary && (
            <p className="text-[12px] leading-relaxed line-clamp-2" style={{ color: 'var(--color-text-secondary)' }}>
              {article.summary}
            </p>
          )}
          {/* Bottom meta */}
          <div className="flex items-center gap-2 text-[11px] mt-auto pt-2" style={{ color: 'var(--color-text-tertiary)' }}>
            {article.publishedAt && (
              <time>{new Date(article.publishedAt).toLocaleDateString('zh-CN')}</time>
            )}
            <span>
              <svg className="w-3.5 h-3.5 inline" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M15 12a3 3 0 11-6 0 3 3 0 016 0z" /><path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M2.458 12C3.732 7.943 7.523 5 12 5c4.478 0 8.268 2.943 9.542 7-1.274 4.057-5.064 7-9.542 7-4.477 0-8.268-2.943-9.542-7z" /></svg>
              {article.viewCount || 0}
            </span>
            {showTags && article.tags && article.tags.length > 0 && (
              <>
                <span>·</span>
                <div className="flex flex-wrap gap-1">
                  {article.tags.slice(0, 3).map((tag) => (
                    <span
                      key={tag.id}
                      className="px-1.5 py-0.5 rounded-[3px] text-[9px]"
                      style={{
                        backgroundColor: 'var(--color-border-light)',
                        color: 'var(--color-text-secondary)',
                      }}
                    >
                      {tag.name}
                    </span>
                  ))}
                </div>
              </>
            )}
          </div>
        </div>
      </Link>
    )
  }

  // List View
  return (
    <Link
      key={article.id}
      href={`/articles/${article.slug}`}
      className="flex items-center gap-3 rounded-[10px] p-3 transition-all hover:-translate-y-0.5"
      style={{
        border: '0.5px solid var(--color-border)',
        backgroundColor: 'var(--card-bg)',
        boxShadow: 'var(--card-shadow)',
      }}
    >
      {/* 左侧小封面 */}
      <div
        className="w-[56px] h-[56px] rounded-[10px] flex-shrink-0 relative overflow-hidden flex items-center justify-center text-[20px] font-bold"
        style={{
          backgroundColor: article.coverImageUrl
            ? 'var(--color-border-light)'
            : catBrand.coverBg || catBrand.bg,
          color: catBrand.text,
        }}
      >
        {article.coverImageUrl ? (
          <Image src={article.coverImageUrl} alt={article.title} fill className="object-cover" sizes="56px" loading="lazy" />
        ) : (
          <span className="text-[24px] font-bold block">{getFirstValidChar(article.title)}</span>
        )}
      </div>

      {/* 中间内容 */}
      <div className="flex-1 min-w-0">
        <h3 className="text-[14px] font-medium leading-snug truncate" style={{ color: 'var(--color-text-primary)' }}>
          {article.isTop && <span className="inline-flex items-center mr-1 px-1 py-0.5 rounded-[3px] text-[9px] font-medium bg-orange-500 text-white align-middle">置顶</span>}
          {article.title}
        </h3>
        <div className="flex items-center gap-2 mt-1 text-[11px] flex-wrap" style={{ color: 'var(--color-text-secondary)' }}>
          {showCategories && cats.length > 0 && cats.map((cat, idx) => (
            <span key={cat.id}>
              {idx > 0 && <span className="mr-1">·</span>}
              <span
                className="px-1.5 py-0.5 rounded-[3px] text-[10px] font-medium"
                style={{ backgroundColor: getCategoryBrand(cat.name).bg, color: getCategoryBrand(cat.name).text }}
              >
                {cat.name}
              </span>
            </span>
          ))}
          {article.publishedAt && (
            <>
              {showCategories && cats.length > 0 && <span>·</span>}
              <time>
                {new Date(article.publishedAt).toLocaleDateString('zh-CN', { month: '2-digit', day: '2-digit' })}
              </time>
            </>
          )}
          <span>·</span>
          <span className="flex items-center gap-1" style={{ color: 'var(--color-text-tertiary)' }}>
            <svg className="w-3.5 h-3.5" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M15 12a3 3 0 11-6 0 3 3 0 016 0z" /><path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M2.458 12C3.732 7.943 7.523 5 12 5c4.478 0 8.268 2.943 9.542 7-1.274 4.057-5.064 7-9.542 7-4.477 0-8.268-2.943-9.542-7z" /></svg>
            {article.viewCount || 0}
          </span>
          <span>·</span>
          <span>5 分钟</span>
        </div>
      </div>

      {/* 右侧标签块 */}
      {showTags && article.tags && article.tags.length > 0 && (
        <div className="hidden sm:flex flex-wrap gap-1.5 flex-shrink-0 max-w-[140px]">
          {article.tags.slice(0, 2).map((tag) => (
            <span
              key={tag.id}
              className="px-1.5 py-0.5 rounded-[3px] text-[9px]"
              style={{
                backgroundColor: 'var(--color-border-light)',
                color: 'var(--color-text-secondary)',
              }}
            >
              {tag.name}
            </span>
          ))}
        </div>
      )}
    </Link>
  )
}
