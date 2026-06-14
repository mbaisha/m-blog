'use client'

import React, { useState, useEffect, useMemo } from 'react'
import Link from 'next/link'
import Image from 'next/image'
import type { PublicArticleListItem, PublicCategoryInfo, PublicTagInfo, PublicProjectListItem, PublicSiteSettingResponse, PublicModuleLayout } from '@/types'
import ScrollReveal from './ScrollReveal'
import BackToTop from './BackToTop'
import VisitTracker from './VisitTracker'
import { renderUniversalModule } from './UniversalModuleComponents'
import { coverImageUrlWithVersion } from '@/lib/imageVersion'
import SubscribeForm from './SubscribeForm'

// ---- 页面特有导出函数 ----

/** 获取标题第一个有效字符（字母或汉字，字母转为大写，跳过标点） */
function getFirstValidChar(title: string): string {
  for (const ch of title) {
    if (/[a-zA-Z]/.test(ch)) return ch.toUpperCase()
    if (/[\u4e00-\u9fff]/.test(ch)) return ch
  }
  return '?'
}

// ---- Stat Block Colors ----
const statBlocks = [
  { label: "文章", bg: "#EEEDFE", border: "#CECBF6", text: "#534AB7" },
  { label: "项目", bg: "#E1F5EE", border: "#9FE1CB", text: "#0F6E56" },
  { label: "经验", bg: "#FAECE7", border: "#F5C4B3", text: "#B45A38" },
  { label: "阅读", bg: "#FBEAF0", border: "#F4C0D1", text: "#B34776" },
]

// ---- Gradient schemes for hero background ----
const gradientSchemes: Record<string, string[]> = {
  'primary-to-accent': ['#6366f1', '#a855f7', '#6366f1'],
  'purple-to-blue':   ['#7c3aed', '#3b82f6', '#7c3aed'],
  'blue-to-cyan':     ['#2563eb', '#06b6d4', '#2563eb'],
  'blue-to-purple':   ['#2563eb', '#7c3aed', '#2563eb'],
  'cyan-to-green':    ['#06b6d4', '#10b981', '#06b6d4'],
  'green-to-teal':    ['#059669', '#14b8a6', '#059669'],
  'orange-to-pink':   ['#f97316', '#ec4899', '#f97316'],
  'pink-to-purple':   ['#ec4899', '#8b5cf6', '#ec4899'],
  'red-to-orange':    ['#ef4444', '#f97316', '#ef4444'],
  'dark':             ['#1e293b', '#334155', '#1e293b'],
}

// ---- Category brand colors ----
const categoryBrandColors: Record<string, { bg: string; text: string; border: string }> = {
  "前端":        { bg: "#EEEDFE", text: "#534AB7", border: "#CECBF6" },
  "后端":        { bg: "#E1F5EE", text: "#0F6E56", border: "#9FE1CB" },
  "产品":        { bg: "#FAECE7", text: "#B45A38", border: "#F5C4B3" },
  "读书":        { bg: "#FBEAF0", text: "#B34776", border: "#F4C0D1" },
  "AI":          { bg: "#E7EEFB", text: "#2E5DA8", border: "#B3CCF4" },
  "其他":        { bg: "#F3F4F6", text: "#6B7280", border: "#E5E7EB" },
}

function getCategoryBrand(name: string) {
  return categoryBrandColors[name] || categoryBrandColors["其他"]
}

/** 根据背景色自动计算前景色 */
function autoTextColor(bgColor?: string | null): string {
  if (!bgColor) return 'var(--color-text-secondary)'
  const hex = bgColor.replace('#', '')
  const r = parseInt(hex.substring(0, 2), 16)
  const g = parseInt(hex.substring(2, 4), 16)
  const b = parseInt(hex.substring(4, 6), 16)
  const luminance = (0.299 * r + 0.587 * g + 0.114 * b) / 255
  return luminance > 0.6 ? '#1F2937' : '#FFFFFF'
}

/** 模块标题行 */
function SectionHeader({ title, href }: { title: string; href: string }) {
  return (
    <div className="flex items-center justify-between mb-5">
      <h2 className="text-[16px] font-medium" style={{ color: "var(--color-text-primary, #1F2937)" }}>{title}</h2>
      <Link href={href} className="group inline-flex items-center gap-1 px-4 py-1.5 rounded-full text-[11px] font-medium transition-all hover:scale-105" style={{ color: "var(--color-primary, #6366F1)", backgroundColor: "var(--color-primary-light, #EEEDFE)" }}>
        查看全部<span className="group-hover:translate-x-0.5 transition-transform inline-block">→</span>
      </Link>
    </div>
  )
}

// ---- Props ----

interface HomeContentProps {
  articles: PublicArticleListItem[]
  recommended: PublicArticleListItem[]
  latest: PublicArticleListItem[]
  projects: PublicProjectListItem[]
  categories: PublicCategoryInfo[]
  tags: PublicTagInfo[]
  totalArticles: number
  siteSettings: PublicSiteSettingResponse | null
  layoutModules: PublicModuleLayout[]
}

// ---- Component ----

export default function HomeContent(props: HomeContentProps) {
  const { articles, recommended, latest, projects, tags, totalArticles, siteSettings, layoutModules } = props
  const totalViews = articles.reduce((sum, a) => sum + (a.viewCount || 0), 0)
  const siteName = siteSettings?.siteName || "Zhu's Blog"
  const logoUrl = siteSettings?.logoImageUrl

  // 布局模块显隐查找表
  const moduleMap = new Map(layoutModules.map(m => [m.moduleKey, m]))

  // 构建按 sortOrder 排序的已启用模块列表
  const enabledModules = layoutModules
    .filter(m => m.isEnabled)
    .sort((a, b) => a.sortOrder - b.sortOrder)

  const stats = [
    { value: totalArticles, ...statBlocks[0] },
    { value: projects.length, ...statBlocks[1] },
    { value: "3", ...statBlocks[2] },
    { value: totalViews >= 1000 ? `${Math.round(totalViews / 1000)}K` : totalViews.toString(), ...statBlocks[3] },
  ]

  // ---- Hero Section Component ----
  function HeroSection({
    heroMod,
    logoUrl,
    siteName,
    siteDescription,
    stats,
  }: {
    heroMod: PublicModuleLayout | undefined
    logoUrl?: string | null
    siteName: string
    siteDescription?: string | null
    stats: { label: string; value: string | number; bg: string; border: string; text: string }[]
  }) {
    const cfg = useMemo(() => {
      try { return JSON.parse(heroMod?.config || '{}') } catch { return {} }
    }, [heroMod?.config])

    const subtitle = cfg.subtitle || ''
    const title = cfg.title || ''
    const description = cfg.description || ''
    const subDescription = cfg.subDescription || ''
    const buttons: { text: string; link: string; style: string }[] = Array.isArray(cfg.buttons) ? cfg.buttons : []
    const backgroundMode: string = cfg.backgroundMode || 'none'
    const gradientColors: string = cfg.gradientColors || 'primary-to-accent'
    const carouselImages: { url: string }[] = Array.isArray(cfg.carouselImages) ? cfg.carouselImages : []
    const carouselInterval = cfg.carouselInterval || 5000

    // Carousel state
    const [carouselIndex, setCarouselIndex] = useState(0)
    useEffect(() => {
      if (backgroundMode !== 'carousel' || carouselImages.length < 2) return
      const timer = setInterval(() => setCarouselIndex(i => (i + 1) % carouselImages.length), carouselInterval)
      return () => clearInterval(timer)
    }, [backgroundMode, carouselImages.length, carouselInterval])

    // Gradient animation keyframes — injected once
    useEffect(() => {
      if (backgroundMode !== 'gradient') return
      const styleId = 'hero-gradient-keyframes'
      if (document.getElementById(styleId)) return
      const style = document.createElement('style')
      style.id = styleId
      style.textContent = `
        @keyframes heroGradient {
          0%   { background-position: 0% 50%; }
          50%  { background-position: 100% 50%; }
          100% { background-position: 0% 50%; }
        }
      `
      document.head.appendChild(style)
      return () => { const el = document.getElementById(styleId); if (el) el.remove() }
    }, [backgroundMode])

    // Compute background style
    const bgStyle = (): React.CSSProperties => {
      if (backgroundMode === 'gradient') {
        let colors: string[]
        if (gradientColors === 'custom' && cfg.customGradientColor1 && cfg.customGradientColor2) {
          colors = [cfg.customGradientColor1, cfg.customGradientColor2, cfg.customGradientColor1]
        } else {
          colors = gradientSchemes[gradientColors] || gradientSchemes['primary-to-accent']
        }
        return {
          background: `linear-gradient(135deg, ${colors[0]}, ${colors[1]}, ${colors[2] || colors[0]})`,
          backgroundSize: '400% 400%',
          animation: 'heroGradient 8s ease infinite',
          color: '#fff',
        }
      }
      if (backgroundMode === 'carousel' && carouselImages.length > 0) {
        return {
          backgroundImage: `url(${carouselImages[carouselIndex].url})`,
          backgroundSize: 'cover',
          backgroundPosition: 'center',
          transition: 'background-image 0.6s ease-in-out',
          color: '#fff',
        }
      }
      return {
        backgroundColor: 'var(--hero-bg)',
        border: '0.5px solid var(--color-border, #E5E7EB)',
      }
    }

    const showOverlay = backgroundMode !== 'none'
    const resolvedTitle = title || siteName
    const resolvedDesc = description || (siteDescription || '记录思考，分享创造')

    return (
      <ScrollReveal key="hero">
        <section
          className="rounded-[18px] p-6 md:p-8 flex flex-col md:flex-row gap-6 md:gap-8 items-center relative overflow-hidden"
          style={bgStyle()}
        >
          {showOverlay && (
            <div
              className="absolute inset-0"
              style={{
                background: backgroundMode === 'gradient'
                  ? 'rgba(0,0,0,0.15)'
                  : 'linear-gradient(to right, rgba(0,0,0,0.55), rgba(0,0,0,0.3))',
                pointerEvents: 'none',
              }}
            />
          )}
          <div className="flex-1 text-center md:text-left relative z-10">
            {subtitle && (
              <p
                className="text-[11px] tracking-[3px] font-medium mb-3"
                style={{ color: showOverlay ? 'rgba(255,255,255,0.8)' : 'var(--color-primary, #6366F1)' }}
              >
                {subtitle}
              </p>
            )}
            <h1
              className="text-[30px] md:text-[30px] font-medium leading-tight mb-2 flex items-center gap-3 justify-center md:justify-start"
              style={{ color: showOverlay ? '#fff' : 'var(--color-text-primary, #1F2937)' }}
            >
              {logoUrl && !title ? (
                <Image src={logoUrl} alt={siteName} width={120} height={36} className="h-[36px] w-auto object-contain inline-block" />
              ) : null}
              {resolvedTitle}
            </h1>
            <p
              className="text-[14px] mb-1"
              style={{ color: showOverlay ? 'rgba(255,255,255,0.85)' : 'var(--color-text-secondary, #6B7280)' }}
            >
              {resolvedDesc}
            </p>
            {subDescription && (
              <p
                className="text-[13px] mb-5"
                style={{ color: showOverlay ? 'rgba(255,255,255,0.7)' : 'var(--color-text-tertiary, #9CA3AF)' }}
              >
                {subDescription}
              </p>
            )}
            {buttons.length > 0 && (
              <div className="flex flex-wrap gap-3 justify-center md:justify-start">
                {buttons.map((btn, i) => (
                  <Link
                    key={i}
                    href={btn.link}
                    className="inline-flex items-center px-5 py-2 rounded-lg text-[12px] font-medium transition-all hover:opacity-90"
                    style={
                      btn.style === 'outline'
                        ? {
                            backgroundColor: 'transparent',
                            color: showOverlay ? '#fff' : 'var(--color-text-secondary, #6B7280)',
                            border: showOverlay ? '1px solid rgba(255,255,255,0.6)' : '0.5px solid var(--color-border, #E5E7EB)',
                          }
                        : {
                            backgroundColor: showOverlay ? 'rgba(255,255,255,0.95)' : 'var(--color-text-primary, #1F2937)',
                            color: showOverlay ? '#1F2937' : '#fff',
                          }
                    }
                  >
                    {btn.text}
                  </Link>
                ))}
              </div>
            )}
          </div>
          <div className="flex-shrink-0 grid grid-cols-2 gap-3 relative z-10">
            {stats.map((stat) => (
              <div
                key={stat.label}
                className="flex flex-col items-center justify-center rounded-[10px] w-[52px] h-[52px]"
                style={{
                  backgroundColor: showOverlay ? 'rgba(255,255,255,0.2)' : stat.bg,
                  border: showOverlay ? '0.5px solid rgba(255,255,255,0.3)' : `0.5px solid ${stat.border}`,
                  backdropFilter: showOverlay ? 'blur(4px)' : undefined,
                }}
              >
                <span className="text-[16px] font-medium leading-none" style={{ color: showOverlay ? '#fff' : stat.text }}>
                  {stat.value}
                </span>
                <span className="text-[9px] leading-none mt-0.5" style={{ color: showOverlay ? 'rgba(255,255,255,0.8)' : stat.text }}>
                  {stat.label}
                </span>
              </div>
            ))}
          </div>
        </section>
      </ScrollReveal>
    )
  }

  // 页面特有模块渲染表
  const moduleRenderers: Record<string, () => React.ReactNode> = {
    hero: () => (
      <HeroSection
        heroMod={moduleMap.get('hero')}
        logoUrl={logoUrl}
        siteName={siteName}
        siteDescription={siteSettings?.siteDescription}
        stats={stats}
      />
    ),

    pinned_posts: () => {
      const mod = moduleMap.get('pinned_posts')
      if (!mod || !mod.isEnabled) return null
      const cfg = (() => { try { return JSON.parse(mod.config || '{}') } catch { return {} } })()
      const displayStyle = cfg.displayStyle || 'card'
      const count = cfg.count || 5
      const columns = Math.min(Math.max(cfg.columns || 3, 2), 4)
      const pinnedArticles = articles.filter(a => a.isTop).slice(0, count)
      if (pinnedArticles.length === 0) return null
      return (
        <ScrollReveal key="pinned_posts">
          <section className="mt-8">
            <SectionHeader title="置顶文章" href="/articles" />
            {displayStyle === 'card' ? (
              <div className="grid grid-cols-1 sm:grid-cols-2 gap-4 article-grid-responsive" style={{ gridTemplateColumns: `repeat(${columns}, minmax(0, 1fr))` }}>
                {pinnedArticles.map((article) => {
                  const cats = article.categories?.length > 0 ? article.categories : (article.category ? [article.category] : [])
                  const catBrand = cats.length > 0 ? getCategoryBrand(cats[0].name) : categoryBrandColors['其他']
                  return (
                    <Link key={article.id} href={`/articles/${article.slug}`} className="flex flex-col rounded-[12px] overflow-hidden transition-all hover:-translate-y-0.5" style={{ border: '0.5px solid var(--color-border, #E5E7EB)', backgroundColor: 'var(--card-bg, #fff)', boxShadow: 'var(--card-shadow, none)' }}>
                      <div className="relative w-full overflow-hidden bg-[#F3F4F6]" style={{ aspectRatio: '16/9' }}>
                        {article.coverImageUrl ? <Image src={coverImageUrlWithVersion(article.coverImageUrl, article.updatedAt)} alt={article.title} fill className="object-cover" sizes="(max-width: 767px) 100vw, (max-width: 1023px) 50vw, 33vw" loading="lazy" /> : null}
                        {cats.length > 0 && (
                          <div className="absolute top-2 left-2 flex flex-wrap gap-1">
                            {cats.map((cat) => <span key={cat.id} className="px-2 py-0.5 rounded-[4px] text-[8px] font-medium" style={{ backgroundColor: "rgba(255,255,255,0.9)", color: catBrand.text }}>{cat.name}</span>)}
                          </div>
                        )}
                        <div className="absolute top-1.5 right-1.5 z-10 flex items-center gap-1 px-1.5 py-0.5 rounded-[4px] text-[9px] font-medium bg-orange-500 text-white shadow-sm">
                          <svg className="w-3 h-3" fill="currentColor" viewBox="0 0 24 24"><path d="M16 12h-2V6h2l4-4-4-4h-2V2h-2V0h-2v2H8V0H6L2 4l4 4h2v6H6v2l6 6 6-6z" transform="rotate(45 12 12)" /></svg>
                          置顶
                        </div>
                      </div>
                      <div className="p-3 flex flex-col gap-1.5 flex-1">
                        <h3 className="text-[13px] font-medium leading-snug line-clamp-2" style={{ color: 'var(--color-text-primary, #1F2937)' }}>{article.title}</h3>
                        <div className="flex items-center gap-2 text-[10px]" style={{ color: 'var(--color-text-tertiary, #9CA3AF)' }}>
                          {article.publishedAt && <time>{new Date(article.publishedAt).toLocaleDateString('zh-CN')}</time>}
                          <span className="flex items-center gap-1">
                            <svg className="w-3 h-3" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M15 12a3 3 0 11-6 0 3 3 0 016 0z" /><path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M2.458 12C3.732 7.943 7.523 5 12 5c4.478 0 8.268 2.943 9.542 7-1.274 4.057-5.064 7-9.542 7-4.477 0-8.268-2.943-9.542-7z" /></svg>
                            {article.viewCount || 0}
                          </span>
                          <span>·</span>
                          <span>5 分钟</span>
                        </div>
                        {article.tags && article.tags.length > 0 && (
                          <div className="flex flex-wrap gap-1 mt-auto pt-1">
                            {article.tags.slice(0, 3).map((tag: any) => <span key={tag.id} className="px-1.5 py-0.5 rounded-[3px] text-[7px]" style={{ backgroundColor: tag.bgColor || 'var(--color-border-light)', color: tag.color || autoTextColor(tag.bgColor) }}>{tag.name}</span>)}
                          </div>
                        )}
                      </div>
                    </Link>
                  )
                })}
              </div>
            ) : (
              <div className="flex flex-col gap-3">
                {pinnedArticles.map((article) => {
                  const cats = article.categories?.length > 0 ? article.categories : (article.category ? [article.category] : [])
                  const catBrand = cats.length > 0 ? getCategoryBrand(cats[0].name) : categoryBrandColors['其他']
                  return (
                    <Link key={article.id} href={`/articles/${article.slug}`} className="flex items-center gap-3 rounded-[10px] p-3 transition-all hover:-translate-y-0.5" style={{ border: '0.5px solid var(--color-border, #E5E7EB)', backgroundColor: 'var(--card-bg, #fff)', boxShadow: 'var(--card-shadow, none)' }}>
                      <div className="w-[56px] h-[56px] rounded-[10px] flex-shrink-0 relative overflow-hidden flex items-center justify-center text-[20px] font-bold" style={{ backgroundColor: article.coverImageUrl ? 'var(--color-border-light, #F3F4F6)' : catBrand.bg || '#EEEDFE', color: catBrand.text || '#534AB7' }}>
                        {article.coverImageUrl ? <Image src={coverImageUrlWithVersion(article.coverImageUrl, article.updatedAt)} alt={article.title} fill className="object-cover" sizes="56px" loading="lazy" /> : <span className="text-[24px] font-bold block">{getFirstValidChar(article.title)}</span>}
                      </div>
                      <div className="flex-1 min-w-0">
                        <h3 className="text-[14px] font-medium leading-snug truncate" style={{ color: 'var(--color-text-primary, #1F2937)' }}>
                          <span className="inline-flex items-center mr-1 px-1 py-0.5 rounded-[3px] text-[9px] font-medium bg-orange-500 text-white align-middle">置顶</span>
                          {article.title}
                        </h3>
                        <div className="flex items-center gap-2 mt-1 text-[11px] flex-wrap" style={{ color: 'var(--color-text-secondary, #6B7280)' }}>
                          {cats.map((cat, idx) => (<span key={cat.id}>{idx > 0 && <span className="mr-1">·</span>}<span className="px-1.5 py-0.5 rounded-[3px] text-[10px] font-medium" style={{ backgroundColor: getCategoryBrand(cat.name).bg, color: getCategoryBrand(cat.name).text }}>{cat.name}</span></span>))}
                          {article.publishedAt && <><span>·</span><time>{new Date(article.publishedAt).toLocaleDateString('zh-CN', { month: '2-digit', day: '2-digit' })}</time></>}
                          <span>·</span>
                          <span className="flex items-center gap-1">
                            <svg className="w-3 h-3" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M15 12a3 3 0 11-6 0 3 3 0 016 0z" /><path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M2.458 12C3.732 7.943 7.523 5 12 5c4.478 0 8.268 2.943 9.542 7-1.274 4.057-5.064 7-9.542 7-4.477 0-8.268-2.943-9.542-7z" /></svg>
                            {article.viewCount || 0}
                          </span><span>·</span><span>5 分钟</span>
                        </div>
                      </div>
                      {article.tags && article.tags.length > 0 && (
                        <div className="hidden sm:flex flex-wrap gap-1.5 flex-shrink-0 max-w-[140px]">
                          {article.tags.slice(0, 2).map((tag: any) => <span key={tag.id} className="px-2 py-0.5 rounded-[5px] text-[9px] font-medium whitespace-nowrap" style={{ backgroundColor: tag.bgColor || 'var(--color-border-light)', color: tag.color || autoTextColor(tag.bgColor) }}>{tag.name}</span>)}
                        </div>
                      )}
                    </Link>
                  )
                })}
              </div>
            )}
          </section>
        </ScrollReveal>
      )
    },

    hot_tags: () => {
      if (tags.length === 0) return null
      return (
        <ScrollReveal key="hot_tags">
          <section className="mt-8">
            <div className="flex flex-wrap gap-2 justify-center">
              {tags.map((tag) => (
                <Link key={tag.id} href={`/tags/${tag.slug}`} className="text-sm px-4 py-1.5 rounded-full font-medium transition-all hover:opacity-80" style={{ backgroundColor: tag.bgColor || 'var(--color-border-light)', color: tag.color || autoTextColor(tag.bgColor) }}>
                  {tag.name}
                </Link>
              ))}
            </div>
          </section>
        </ScrollReveal>
      )
    },

    featured_posts: () => {
      if (recommended.length === 0) return null
      const mod = moduleMap.get('featured_posts')
      const cfg = mod ? (() => { try { return JSON.parse(mod.config || '{}') } catch { return {} } })() : {}
      const displayStyle = cfg.displayStyle || 'card'
      const columns = Math.min(Math.max(cfg.columns || 3, 2), 4)
      const count = cfg.count || 3
      const items = recommended.slice(0, count)
      return (
        <ScrollReveal key="featured_posts">
          <section className="mt-10">
            <SectionHeader title="精选文章" href="/articles" />
            {displayStyle === 'list' ? (
              <div className="flex flex-col gap-3">
                {items.map((article) => {
                  const cats = article.categories?.length > 0 ? article.categories : (article.category ? [article.category] : [])
                  const catBrand = cats.length > 0 ? getCategoryBrand(cats[0].name) : categoryBrandColors["其他"]
                  return (
                    <Link key={article.id} href={`/articles/${article.slug}`} className="flex items-center gap-3 rounded-[10px] p-3 transition-all hover:-translate-y-0.5" style={{ border: "0.5px solid var(--color-border, #E5E7EB)", backgroundColor: "var(--card-bg, #fff)", boxShadow: "var(--card-shadow, none)" }}>
                      <div className="w-[56px] h-[56px] rounded-[10px] flex-shrink-0 relative overflow-hidden flex items-center justify-center text-[20px] font-bold" style={{ backgroundColor: article.coverImageUrl ? 'var(--color-border-light, #F3F4F6)' : catBrand.bg || '#EEEDFE', color: catBrand.text || '#534AB7' }}>
                        {article.coverImageUrl ? <Image src={coverImageUrlWithVersion(article.coverImageUrl, article.updatedAt)} alt={article.title} fill className="object-cover" sizes="56px" loading="lazy" /> : <span className="text-[24px] font-bold block">{getFirstValidChar(article.title)}</span>}
                      </div>
                      <div className="flex-1 min-w-0">
                        <h3 className="text-[14px] font-medium leading-snug truncate" style={{ color: "var(--color-text-primary, #1F2937)" }}>{article.title}</h3>
                        <div className="flex items-center gap-2 mt-1 text-[11px] flex-wrap" style={{ color: "var(--color-text-secondary, #6B7280)" }}>
                          {cats.map((cat, idx) => (<span key={cat.id}>{idx > 0 && <span className="mr-1">·</span>}<span className="px-1.5 py-0.5 rounded-[3px] text-[10px] font-medium" style={{ backgroundColor: getCategoryBrand(cat.name).bg, color: getCategoryBrand(cat.name).text }}>{cat.name}</span></span>))}
                          {article.publishedAt && <><span>·</span><time>{new Date(article.publishedAt).toLocaleDateString("zh-CN", { month: "2-digit", day: "2-digit" })}</time></>}
                          <span>·</span>
                          <span className="flex items-center gap-1">
                            <svg className="w-3 h-3" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M15 12a3 3 0 11-6 0 3 3 0 016 0z" /><path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M2.458 12C3.732 7.943 7.523 5 12 5c4.478 0 8.268 2.943 9.542 7-1.274 4.057-5.064 7-9.542 7-4.477 0-8.268-2.943-9.542-7z" /></svg>
                            {article.viewCount || 0}
                          </span><span>·</span><span>5 分钟</span>
                        </div>
                      </div>
                      {article.tags && article.tags.length > 0 && (
                        <div className="hidden sm:flex flex-wrap gap-1.5 flex-shrink-0 max-w-[140px]">
                          {article.tags.slice(0, 2).map((tag) => <span key={tag.id} className="px-2 py-0.5 rounded-[5px] text-[9px] font-medium whitespace-nowrap" style={{ backgroundColor: tag.bgColor || 'var(--color-border-light)', color: tag.color || autoTextColor(tag.bgColor) }}>{tag.name}</span>)}
                        </div>
                      )}
                    </Link>
                  )
                })}
              </div>
            ) : (
              <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-4 article-grid-responsive" style={{ gridTemplateColumns: `repeat(${Math.min(columns, 4)}, minmax(0, 1fr))` }}>
                {items.map((article) => {
                  const cats = article.categories?.length > 0 ? article.categories : (article.category ? [article.category] : [])
                  const catBrand = cats.length > 0 ? getCategoryBrand(cats[0].name) : categoryBrandColors["其他"]
                  return (
                    <Link key={article.id} href={`/articles/${article.slug}`} className="flex flex-col rounded-[12px] overflow-hidden transition-all hover:-translate-y-0.5" style={{ border: "0.5px solid var(--color-border, #E5E7EB)", backgroundColor: "var(--card-bg, #fff)", boxShadow: "var(--card-shadow, none)" }}>
                      <div className="relative w-full overflow-hidden bg-[#F3F4F6]" style={{ aspectRatio: '16/9' }}>
                        {article.coverImageUrl ? <Image src={coverImageUrlWithVersion(article.coverImageUrl, article.updatedAt)} alt={article.title} fill className="object-cover" sizes="(max-width: 767px) 100vw, (max-width: 1023px) 50vw, 33vw" loading="lazy" /> : null}
                        {cats.length > 0 && (
                          <div className="absolute top-2 left-2 flex flex-wrap gap-1">
                            {cats.map((cat) => <span key={cat.id} className="px-2 py-0.5 rounded-[4px] text-[8px] font-medium" style={{ backgroundColor: "rgba(255,255,255,0.9)", color: catBrand.text }}>{cat.name}</span>)}
                          </div>
                        )}
                      </div>
                      <div className="p-3 flex flex-col gap-1.5 flex-1">
                        <h3 className="text-[13px] font-medium leading-snug line-clamp-2" style={{ color: "var(--color-text-primary, #1F2937)" }}>{article.title}</h3>
                        <div className="flex items-center gap-2 text-[10px]" style={{ color: "var(--color-text-tertiary, #9CA3AF)" }}>
                          {article.publishedAt && <time>{new Date(article.publishedAt).toLocaleDateString("zh-CN")}</time>}
                          <span className="flex items-center gap-1">
                            <svg className="w-3 h-3" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M15 12a3 3 0 11-6 0 3 3 0 016 0z" /><path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M2.458 12C3.732 7.943 7.523 5 12 5c4.478 0 8.268 2.943 9.542 7-1.274 4.057-5.064 7-9.542 7-4.477 0-8.268-2.943-9.542-7z" /></svg>
                            {article.viewCount || 0}
                          </span>
                          <span>·</span>
                          <span>5 分钟</span>
                        </div>
                        {article.tags && article.tags.length > 0 && (
                          <div className="flex flex-wrap gap-1 mt-auto pt-1">
                            {article.tags.slice(0, 3).map((tag) => <span key={tag.id} className="px-1.5 py-0.5 rounded-[3px] text-[7px]" style={{ backgroundColor: tag.bgColor || 'var(--color-border-light)', color: tag.color || autoTextColor(tag.bgColor) }}>{tag.name}</span>)}
                          </div>
                        )}
                      </div>
                    </Link>
                  )
                })}
              </div>
            )}
          </section>
        </ScrollReveal>
      )
    },

    recent_posts: () => {
      if (latest.length === 0) return null
      const mod = moduleMap.get('recent_posts')
      const cfg = mod ? (() => { try { return JSON.parse(mod.config || '{}') } catch { return {} } })() : {}
      const displayStyle = cfg.displayStyle || 'list'
      const count = cfg.count || 5
      const columns = Math.min(Math.max(cfg.columns || 3, 2), 4)
      const items = latest.slice(0, count)
      return (
        <ScrollReveal key="recent_posts">
          <section className="mt-10">
            <SectionHeader title="最新文章" href="/articles" />
            {displayStyle === 'card' ? (
              <div className="grid grid-cols-1 sm:grid-cols-2 gap-4 article-grid-responsive" style={{ gridTemplateColumns: `repeat(${columns}, minmax(0, 1fr))` }}>
                {items.map((article) => {
                  const cats = article.categories?.length > 0 ? article.categories : (article.category ? [article.category] : [])
                  const catBrand = cats.length > 0 ? getCategoryBrand(cats[0].name) : categoryBrandColors["其他"]
                  return (
                    <Link key={article.id} href={`/articles/${article.slug}`} className="flex flex-col rounded-[12px] overflow-hidden transition-all hover:-translate-y-0.5" style={{ border: "0.5px solid var(--color-border, #E5E7EB)", backgroundColor: "var(--card-bg, #fff)", boxShadow: "var(--card-shadow, none)" }}>
                      <div className="relative w-full overflow-hidden bg-[#F3F4F6]" style={{ aspectRatio: '16/9' }}>
                        {article.coverImageUrl ? <Image src={coverImageUrlWithVersion(article.coverImageUrl, article.updatedAt)} alt={article.title} fill className="object-cover" sizes="(max-width: 767px) 100vw, (max-width: 1023px) 50vw, 33vw" loading="lazy" /> : null}
                        {cats.length > 0 && (
                          <div className="absolute top-2 left-2 flex flex-wrap gap-1">
                            {cats.map((cat) => <span key={cat.id} className="px-2 py-0.5 rounded-[4px] text-[8px] font-medium" style={{ backgroundColor: "rgba(255,255,255,0.9)", color: catBrand.text }}>{cat.name}</span>)}
                          </div>
                        )}
                      </div>
                      <div className="p-3 flex flex-col gap-1.5 flex-1">
                        <h3 className="text-[13px] font-medium leading-snug line-clamp-2" style={{ color: "var(--color-text-primary, #1F2937)" }}>{article.title}</h3>
                        <div className="flex items-center gap-2 text-[10px]" style={{ color: "var(--color-text-tertiary, #9CA3AF)" }}>
                          {article.publishedAt && <time>{new Date(article.publishedAt).toLocaleDateString("zh-CN")}</time>}
                          <span className="flex items-center gap-1">
                            <svg className="w-3 h-3" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M15 12a3 3 0 11-6 0 3 3 0 016 0z" /><path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M2.458 12C3.732 7.943 7.523 5 12 5c4.478 0 8.268 2.943 9.542 7-1.274 4.057-5.064 7-9.542 7-4.477 0-8.268-2.943-9.542-7z" /></svg>
                            {article.viewCount || 0}
                          </span>
                          <span>·</span>
                          <span>5 分钟</span>
                        </div>
                        {article.tags && article.tags.length > 0 && (
                          <div className="flex flex-wrap gap-1 mt-auto pt-1">
                            {article.tags.slice(0, 3).map((tag) => <span key={tag.id} className="px-1.5 py-0.5 rounded-[3px] text-[7px]" style={{ backgroundColor: tag.bgColor || 'var(--color-border-light)', color: tag.color || autoTextColor(tag.bgColor) }}>{tag.name}</span>)}
                          </div>
                        )}
                      </div>
                    </Link>
                  )
                })}
              </div>
            ) : (
              <div className="flex flex-col gap-3">
                {items.map((article) => {
                  const cats = article.categories?.length > 0 ? article.categories : (article.category ? [article.category] : [])
                  const catBrand = cats.length > 0 ? getCategoryBrand(cats[0].name) : categoryBrandColors["其他"]
                  return (
                    <Link key={article.id} href={`/articles/${article.slug}`} className="flex items-center gap-3 rounded-[10px] p-3 transition-all hover:-translate-y-0.5" style={{ border: "0.5px solid var(--color-border, #E5E7EB)", backgroundColor: "var(--card-bg, #fff)", boxShadow: "var(--card-shadow, none)" }}>
                      <div className="w-[56px] h-[56px] rounded-[10px] flex-shrink-0 relative overflow-hidden flex items-center justify-center text-[20px] font-bold" style={{ backgroundColor: article.coverImageUrl ? 'var(--color-border-light, #F3F4F6)' : catBrand.bg || '#EEEDFE', color: catBrand.text || '#534AB7' }}>
                        {article.coverImageUrl ? <Image src={coverImageUrlWithVersion(article.coverImageUrl, article.updatedAt)} alt={article.title} fill className="object-cover" sizes="56px" loading="lazy" /> : <span className="text-[24px] font-bold block">{getFirstValidChar(article.title)}</span>}
                      </div>
                      <div className="flex-1 min-w-0">
                        <h3 className="text-[14px] font-medium leading-snug truncate" style={{ color: "var(--color-text-primary, #1F2937)" }}>{article.title}</h3>
                        <div className="flex items-center gap-2 mt-1 text-[11px] flex-wrap" style={{ color: "var(--color-text-secondary, #6B7280)" }}>
                          {cats.map((cat, idx) => (
                            <span key={cat.id}>{idx > 0 && <span className="mr-1">·</span>}<span className="px-1.5 py-0.5 rounded-[3px] text-[10px] font-medium" style={{ backgroundColor: getCategoryBrand(cat.name).bg, color: getCategoryBrand(cat.name).text }}>{cat.name}</span></span>
                          ))}
                          {article.publishedAt && <><span>·</span><time>{new Date(article.publishedAt).toLocaleDateString("zh-CN", { month: "2-digit", day: "2-digit" })}</time></>}
                          <span>·</span>
                          <span className="flex items-center gap-1">
                            <svg className="w-3 h-3" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M15 12a3 3 0 11-6 0 3 3 0 016 0z" /><path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M2.458 12C3.732 7.943 7.523 5 12 5c4.478 0 8.268 2.943 9.542 7-1.274 4.057-5.064 7-9.542 7-4.477 0-8.268-2.943-9.542-7z" /></svg>
                            {article.viewCount || 0}
                          </span><span>·</span><span>5 分钟</span>
                        </div>
                      </div>
                      {article.tags && article.tags.length > 0 && (
                        <div className="hidden sm:flex flex-wrap gap-1.5 flex-shrink-0 max-w-[140px]">
                          {article.tags.slice(0, 2).map((tag) => <span key={tag.id} className="px-2 py-0.5 rounded-[5px] text-[9px] font-medium whitespace-nowrap" style={{ backgroundColor: tag.bgColor || 'var(--color-border-light)', color: tag.color || autoTextColor(tag.bgColor) }}>{tag.name}</span>)}
                        </div>
                      )}
                    </Link>
                  )
                })}
              </div>
            )}
          </section>
        </ScrollReveal>
      )
    },

    projects: () => {
      if (projects.length === 0) return null
      return (
        <ScrollReveal key="projects">
          <section className="mt-10">
            <SectionHeader title="项目" href="/projects" />
            <div className="grid grid-cols-1 sm:grid-cols-2 gap-3">
              {projects.map((project) => (
                <Link key={project.id} href={`/projects/${project.slug}`} className="flex items-center gap-3 rounded-[10px] p-3 transition-all hover:-translate-y-0.5" style={{ border: "0.5px solid var(--color-border, #E5E7EB)", backgroundColor: "var(--card-bg, #fff)", boxShadow: "var(--card-shadow, none)" }}>
                  {project.coverImageUrl ? (
                    <div className="w-[48px] h-[48px] rounded-[8px] overflow-hidden flex-shrink-0 relative"><Image src={coverImageUrlWithVersion(project.coverImageUrl, project.createdAt)} alt={project.title} fill className="object-cover" sizes="48px" loading="lazy" /></div>
                  ) : (
                    <div className="w-[48px] h-[48px] rounded-[8px] flex items-center justify-center text-[18px] font-bold flex-shrink-0" style={{ backgroundColor: "var(--color-primary-light, #EEEDFE)", color: "var(--color-primary, #6366F1)" }}>{getFirstValidChar(project.title)}</div>
                  )}
                  <div className="flex-1 min-w-0">
                    <h3 className="text-[13px] font-medium truncate" style={{ color: "var(--color-text-primary, #1F2937)" }}>{project.title}</h3>
                    {project.techStack && project.techStack.length > 0 && <p className="text-[10px] mt-0.5 truncate" style={{ color: "var(--color-text-secondary, #6B7280)" }}>{project.techStack.join(" · ")}</p>}
                  </div>
                </Link>
              ))}
            </div>
          </section>
        </ScrollReveal>
      )
    },

    subscription: () => <SubscribeForm key="subscription" />,

    statistics: () => {
      const totalViews = articles.reduce((sum, a) => sum + (a.viewCount || 0), 0)
      const tagsCount = tags.length
      const projCount = projects.length
      return (
        <ScrollReveal key="statistics">
          <section className="mt-8">
            <div className="grid grid-cols-2 sm:grid-cols-4 gap-3">
              {[
                { label: '文章', value: totalArticles },
                { label: '标签', value: tagsCount },
                { label: '项目', value: projCount },
                { label: '访问', value: totalViews >= 1000 ? `${Math.round(totalViews / 1000)}K` : totalViews.toString() },
              ].map(s => (
                <div key={s.label} className="rounded-[10px] p-4 text-center" style={{ border: '0.5px solid var(--color-border)', backgroundColor: 'var(--card-bg)' }}>
                  <div className="text-xl font-bold" style={{ color: 'var(--color-primary)' }}>{s.value}</div>
                  <div className="text-xs mt-1" style={{ color: 'var(--color-text-secondary)' }}>{s.label}</div>
                </div>
              ))}
            </div>
          </section>
        </ScrollReveal>
      )
    },
  }

  return (
    <div className="content-container pt-6 pb-24 md:pb-16">
      {/* 按后台配置的排序依次渲染已启用的模块 */}
      {enabledModules.map(mod => {
        const renderer = moduleRenderers[mod.moduleKey]
        if (renderer) return <div key={mod.id || mod.moduleKey} className="mb-8">{renderer()}</div>
        // 通用模块 fallback
        const universal = renderUniversalModule(mod)
        if (universal) return <div key={mod.id || mod.moduleKey} className="mb-8">{universal}</div>
        return null
      })}

      {/* 空状态：只在没有任何内容时显示 */}
      {articles.length === 0 && (
        <div className="flex flex-col items-center justify-center py-20">
          <div className="text-4xl mb-4 opacity-50">📝</div>
          <h2 className="text-xl font-semibold mb-2" style={{ color: "var(--color-text-primary, #1F2937)" }}>暂无内容</h2>
          <p style={{ color: "var(--color-text-secondary, #6B7280)" }}>内容正在准备中，请稍后再来</p>
        </div>
      )}

      <BackToTop />
      <VisitTracker pagePath="/" />
    </div>
  )
}
