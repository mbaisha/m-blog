'use client'

import type { PublicModuleLayout } from '@/types'
import ScrollReveal from './ScrollReveal'
import dynamic from 'next/dynamic'
import Link from 'next/link'
import MarkdownContent from '@/app/articles/[slug]/MarkdownContent'

const ImageCarousel = dynamic(() => import('@/components/ImageCarousel'), { ssr: false })
const VideoPlayer = dynamic(() => import('@/components/VideoPlayer'), { ssr: false })
const SubscribeForm = dynamic(() => import('@/components/SubscribeForm'), { ssr: false })

/**
 * 按后台布局配置渲染所有模块
 * @param modules  后台排序后的模块列表
 * @param pageRenderers  页面特有模块的渲染函数映射（moduleKey → 组件）
 */
export function RenderLayoutModules(
  modules: PublicModuleLayout[],
  pageRenderers: Record<string, (mod: PublicModuleLayout) => React.ReactNode>
): React.ReactNode[] {
  const enabled = modules.filter(m => m.isEnabled).sort((a, b) => a.sortOrder - b.sortOrder)
  const out: React.ReactNode[] = []

  enabled.forEach((mod, idx) => {
    // 1) 页面特有渲染器优先
    if (pageRenderers[mod.moduleKey]) {
      out.push(<div key={mod.id || idx} className="mb-8">{pageRenderers[mod.moduleKey](mod)}</div>)
      return
    }
    // 2) 通用模块渲染
    const rendered = renderUniversalModule(mod)
    if (rendered) {
      out.push(<div key={mod.id || idx} className="mb-8">{rendered}</div>)
    }
  })

  return out
}

/** 渲染通用模块 */
function renderUniversalModule(mod: PublicModuleLayout): React.ReactNode {
  const cfg = tryParse(mod.config)

  switch (mod.moduleKey) {
    // ====== 已有通用模块 ======
    case 'image_carousel':
      return <ScrollReveal><ImageCarousel config={mod.config || '{}'} /></ScrollReveal>

    case 'video_player':
      return <ScrollReveal><VideoPlayer config={mod.config || '{}'} /></ScrollReveal>

    case 'custom':
      if (!cfg?.html) return null
      return (
        <ScrollReveal>
          <section className="rounded-[12px] p-6" style={{ border: '0.5px solid var(--color-border)', backgroundColor: 'var(--card-bg)' }}>
            {mod.title && <h2 className="text-[16px] font-medium mb-4" style={{ color: 'var(--color-text-primary)' }}>{mod.title}</h2>}
            <div dangerouslySetInnerHTML={{ __html: cfg.html }} />
          </section>
        </ScrollReveal>
      )

    case 'divider': {
      const margin = cfg?.margin ?? 32
      const style = cfg?.style || 'solid'
      return <hr style={{ borderStyle: style, margin: `${margin}px 0`, borderColor: 'var(--color-border)' }} />
    }

    case 'spacer': {
      const height = cfg?.height ?? 40
      return <div style={{ height }} />
    }

    case 'statistics':
      return <ScrollReveal><StatisticsDefault /></ScrollReveal>

    case 'call_to_action': {
      const text = cfg?.text || '联系我'
      const link = cfg?.link || '/about'
      const desc = cfg?.description || ''
      const btnStyle = cfg?.style || 'primary'
      return (
        <ScrollReveal>
          <section className="text-center rounded-[12px] p-8" style={{ border: '0.5px solid var(--color-border)', backgroundColor: 'var(--color-surface)' }}>
            {desc && <p className="mb-3 text-sm" style={{ color: 'var(--color-text-secondary)' }}>{desc}</p>}
            <Link href={link} className="inline-flex items-center px-6 py-2.5 rounded-lg text-sm font-medium transition-opacity hover:opacity-85"
              style={{
                backgroundColor: btnStyle === 'primary' ? 'var(--color-primary)' : 'transparent',
                color: btnStyle === 'primary' ? '#fff' : 'var(--color-primary)',
                border: btnStyle === 'outline' ? '1.5px solid var(--color-primary)' : 'none',
              }}>
              {text}
            </Link>
          </section>
        </ScrollReveal>
      )
    }

    // ====== 新增通用模块 ======

    case 'announcement_banner': {
      const text = cfg?.text || ''
      const style = cfg?.style || 'info'
      const dismissible = cfg?.dismissible !== false
      const fontSize = cfg?.fontSize || 15
      if (!text) return null
      const colorMap: Record<string, { bg: string; text: string; border: string }> = {
        info: { bg: '#eff6ff', text: '#1e40af', border: '#bfdbfe' },
        success: { bg: '#f0fdf4', text: '#166534', border: '#bbf7d0' },
        warning: { bg: '#fffcf5', text: '#92400e', border: '#fde68a' },
        danger: { bg: '#fef2f2', text: '#991b1b', border: '#fecaca' },
        primary: { bg: 'var(--color-primary-light)', text: 'var(--color-primary)', border: 'var(--color-primary)' },
      }
      const c = colorMap[style] || colorMap.info
      return (
        <div className={`rounded-lg px-4 py-3 flex items-center justify-between gap-3 ${dismissible ? '' : ''}`}
          style={{ backgroundColor: c.bg, color: c.text, border: `0.5px solid ${c.border}`, fontSize }}>
          <span>{text}</span>
          {dismissible && <button onClick={(e) => (e.currentTarget.parentElement!.style.display = 'none')} className="flex-shrink-0 opacity-60 hover:opacity-100" style={{ fontSize: fontSize + 4 }}>&times;</button>}
        </div>
      )
    }

    case 'quote_block': {
      const quote = cfg?.quote || ''
      const author = cfg?.author || ''
      const source = cfg?.source || ''
      const qStyle = cfg?.style || 'default'
      if (!quote) return null
      return (
        <figure className={`rounded-[12px] p-6 ${qStyle === 'bold' ? 'font-semibold text-lg' : ''}`}
          style={{
            backgroundColor: 'var(--color-surface)',
            border: qStyle === 'border' ? 'none' : '0.5px solid var(--color-border)',
            borderLeft: qStyle === 'border' ? '4px solid var(--color-primary)' : '0.5px solid var(--color-border)',
            color: 'var(--color-text-primary)',
          }}>
          <blockquote className="italic mb-2" style={{ color: 'var(--color-text-primary)' }}>&ldquo;{quote}&rdquo;</blockquote>
          {(author || source) && (
            <figcaption className="text-sm" style={{ color: 'var(--color-text-secondary)' }}>
              {author && <span>{author}</span>}
              {source && <span> — <cite>{source}</cite></span>}
            </figcaption>
          )}
        </figure>
      )
    }

    case 'icon_list': {
      const items = cfg?.items ? (Array.isArray(cfg.items) ? cfg.items : tryParse(String(cfg.items))) : []
      const columns = cfg?.columns || 3
      if (!items?.length) return null
      return (
        <ScrollReveal>
          <div className={mod.title ? 'mt-6' : ''}>
            {mod.title && <h2 className="text-[16px] font-medium mb-5" style={{ color: 'var(--color-text-primary)' }}>{mod.title}</h2>}
            <div className="grid gap-4 article-grid-responsive" style={{ gridTemplateColumns: `repeat(${Math.min(columns, 4)}, 1fr)` }}>
              {items.map((item: any, i: number) => (
                <div key={i} className="flex gap-3 items-start rounded-[10px] p-4" style={{ border: '0.5px solid var(--color-border)', backgroundColor: 'var(--card-bg)' }}>
                  <span className="text-2xl flex-shrink-0">{item.icon || '📌'}</span>
                  <div>
                    <h3 className="text-sm font-medium" style={{ color: 'var(--color-text-primary)' }}>{item.title || ''}</h3>
                    {item.desc && <p className="text-xs mt-0.5" style={{ color: 'var(--color-text-secondary)' }}>{item.desc}</p>}
                  </div>
                </div>
              ))}
            </div>
          </div>
        </ScrollReveal>
      )
    }

    case 'card_banner': {
      const title = cfg?.title || ''
      const description = cfg?.description || ''
      const btnText = cfg?.buttonText || ''
      const btnLink = cfg?.buttonLink || '/'
      const cardStyle = cfg?.style || 'gradient'
      const fontSize = cfg?.fontSize || 16
      const titleSize = cfg?.titleSize || 22
      if (!title && !description) return null
      const gradBg = 'linear-gradient(135deg, var(--color-primary), var(--color-accent, var(--color-primary)))'
      return (
        <ScrollReveal>
          <div className="rounded-[16px] p-8 text-center"
            style={{
              background: cardStyle === 'gradient' ? gradBg : cardStyle === 'solid' ? 'var(--color-primary)' : 'transparent',
              border: cardStyle === 'outline' ? '1.5px solid var(--color-primary)' : 'none',
              color: cardStyle === 'outline' ? 'var(--color-text-primary)' : '#fff',
            }}>
            {title && <h3 style={{ fontSize: titleSize, fontWeight: 700, marginBottom: '0.5rem' }}>{title}</h3>}
            {description && <p style={{ fontSize, opacity: btnText ? 0.9 : 1, marginBottom: btnText ? '1.25rem' : 0 }}>{description}</p>}
            {btnText && (
              <Link href={btnLink} className="inline-flex items-center px-5 py-2 rounded-lg font-medium transition-all hover:scale-105"
                style={{
                  backgroundColor: cardStyle === 'outline' ? 'var(--color-primary)' : 'rgba(255,255,255,0.2)',
                  color: '#fff',
                  fontSize: Math.max(fontSize - 2, 13),
                }}>
                {btnText}
              </Link>
            )}
          </div>
        </ScrollReveal>
      )
    }

    // ====== Markdown 区域 ======
    case 'markdown': {
      const content = cfg?.content || ''
      if (!content) return null
      return (
        <ScrollReveal>
          <section className="rounded-[12px] p-6" style={{ border: '0.5px solid var(--color-border)', backgroundColor: 'var(--card-bg)' }}>
            {mod.title && <h2 className="text-[18px] font-semibold mb-4" style={{ color: 'var(--color-text-primary)' }}>{mod.title}</h2>}
            <article className="detail-article-body" style={{ fontSize: '15px', lineHeight: 1.75 }}>
              <MarkdownContent content={content} />
            </article>
          </section>
        </ScrollReveal>
      )
    }

    default:
      return null
  }
}

/** 导出渲染单个通用模块，供页面使用 */
export { renderUniversalModule }

function tryParse(str: string): Record<string, any> | null {
  if (!str) return null
  try { return JSON.parse(str) } catch { return null }
}

/** 默认统计面板（无配置时备用） */
function StatisticsDefault() {
  return (
    <div className="grid grid-cols-2 sm:grid-cols-4 gap-3 text-center">
      {[{ label: '文章', value: '-' }, { label: '标签', value: '-' }, { label: '项目', value: '-' }, { label: '访问', value: '-' }].map(s => (
        <div key={s.label} className="rounded-[10px] p-4" style={{ border: '0.5px solid var(--color-border)', backgroundColor: 'var(--card-bg)' }}>
          <div className="text-xl font-bold" style={{ color: 'var(--color-primary)' }}>{s.value}</div>
          <div className="text-xs mt-1" style={{ color: 'var(--color-text-secondary)' }}>{s.label}</div>
        </div>
      ))}
    </div>
  )
}