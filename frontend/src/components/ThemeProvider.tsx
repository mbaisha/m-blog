'use client'

import { useEffect, useState } from 'react'
import type { PublicThemeSetting } from '@/types'

interface ThemeProviderProps {
  children: React.ReactNode
}

/** 将 Hex 色值淡化指定比例（0-1），返回新的 Hex */
function lighten(hex: string, amount: number): string {
  const h = hex.replace('#', '')
  if (h.length < 6) return hex
  const r = parseInt(h.substring(0, 2), 16)
  const g = parseInt(h.substring(2, 4), 16)
  const b = parseInt(h.substring(4, 6), 16)
  const nr = Math.min(255, Math.round(r + (255 - r) * amount))
  const ng = Math.min(255, Math.round(g + (255 - g) * amount))
  const nb = Math.min(255, Math.round(b + (255 - b) * amount))
  return `#${nr.toString(16).padStart(2, '0')}${ng.toString(16).padStart(2, '0')}${nb.toString(16).padStart(2, '0')}`
}

/** 将 Hex 色值加深指定比例（0-1），返回新的 Hex */
function darken(hex: string, amount: number): string {
  const h = hex.replace('#', '')
  if (h.length < 6) return hex
  const r = parseInt(h.substring(0, 2), 16)
  const g = parseInt(h.substring(2, 4), 16)
  const b = parseInt(h.substring(4, 6), 16)
  const nr = Math.max(0, Math.round(r * (1 - amount)))
  const ng = Math.max(0, Math.round(g * (1 - amount)))
  const nb = Math.max(0, Math.round(b * (1 - amount)))
  return `#${nr.toString(16).padStart(2, '0')}${ng.toString(16).padStart(2, '0')}${nb.toString(16).padStart(2, '0')}`
}

/** 根据前景色判断是否为亮色背景 */
function isLightBg(hex: string): boolean {
  const h = hex.replace('#', '')
  const r = parseInt(h.substring(0, 2), 16)
  const g = parseInt(h.substring(2, 4), 16)
  const b = parseInt(h.substring(4, 6), 16)
  return (0.299 * r + 0.587 * g + 0.114 * b) / 255 > 0.55
}

export default function ThemeProvider({ children }: ThemeProviderProps) {
  const [theme, setTheme] = useState<PublicThemeSetting | null>(null)

  useEffect(() => {
    const baseUrl = process.env.NEXT_PUBLIC_API_BASE_URL || 'http://localhost:5092/api'
    fetch(`${baseUrl}/theme`)
      .then(res => res.json())
      .then((data: { success: boolean; data: PublicThemeSetting }) => {
        if (data.success && data.data?.primaryColor) {
          setTheme(data.data)
        }
      })
      .catch(() => {})
  }, [])

  const cssVars = theme ? computeCssVars(theme as any) : undefined

  return (
    <div style={cssVars as React.CSSProperties} className="theme-root">
      {children}
    </div>
  )
}

function computeCssVars(theme: any): Record<string, string> {
  const primary = theme.primaryColor || '#6366f1'
  const accent = theme.accentColor || '#a855f7'
  const bg = theme.backgroundColor || '#f8fafc'
  const text = theme.textColor || '#111827'
  const link = theme.linkColor || '#6366f1'
  const navBg = theme.navbarBackground || ''
  const navText = theme.navbarTextColor || ''

  const light = isLightBg(bg)

  // 优先使用用户配置的值，否则自动计算
  const surface = theme.surfaceColor || (light ? '#ffffff' : '#1e293b')
  const surfaceSecondary = light ? '#f8fafc' : '#1e293b'
  const textSecondary = theme.textSecondaryColor || (light ? '#6b7280' : '#94a3b8')
  const textTertiary = light ? '#9ca3af' : '#64748b'
  const border = theme.borderColor || (light ? '#e5e7eb' : '#334155')
  const borderLight = light ? '#f3f4f6' : '#1e293b'
  const borderActive = light ? lighten(primary, 0.7) : primary

  const navbarBg = navBg || surface
  const navbarText = navText || textSecondary
  const navbarActiveBg = light ? lighten(primary, 0.85) : `${primary}1f`
  const navbarActiveText = light ? darken(primary, 0.1) : primary

  // 按钮/状态色
  const success = theme.successColor || '#10b981'
  const danger = theme.dangerColor || '#ef4444'
  const warning = theme.warningColor || '#f59e0b'
  const info = '#3b82f6'

  // 页脚（未设置时留空，由各组件自行 fallback）
  const footerBg = theme.footerBackground || ''
  const footerText = theme.footerTextColor || ''

  // Hero
  const heroBg = theme.heroBackground || lighten(primary, 0.9)

  // 代码块
  const codeBg = theme.codeBackground || (light ? '#f8f9fa' : '#1a1a2e')

  // 字体
  const fontFamilyMap: Record<string, string> = {
    'sans-serif': 'ui-sans-serif, system-ui, -apple-system, sans-serif',
    'serif': 'ui-serif, Georgia, Cambria, "Times New Roman", Times, serif',
    'monospace': 'ui-monospace, SFMono-Regular, Menlo, Monaco, Consolas, monospace',
  }
  const fontFamily = theme.fontFamily ? (fontFamilyMap[theme.fontFamily as string] || theme.fontFamily) : ''

  const borderRadiusMap: Record<string, string> = {
    none: '0px',
    small: '4px',
    medium: '8px',
    large: '16px',
  }
  const radius = borderRadiusMap[theme.borderRadius || 'medium'] || '8px'

  const shadowSm = light ? '0 1px 2px rgba(0,0,0,0.04)' : '0 1px 2px rgba(0,0,0,0.2)'
  const shadowMd = light ? '0 2px 6px rgba(0,0,0,0.04)' : '0 2px 6px rgba(0,0,0,0.25)'
  const shadowLg = light ? '0 4px 12px rgba(0,0,0,0.06)' : '0 4px 12px rgba(0,0,0,0.3)'

  const radiusMd = radius
  const radiusLg = radius === '0px' ? '8px' : `calc(${radius} * 1.5)`
  const radiusXl = radius === '0px' ? '12px' : `calc(${radius} * 2)`

  const cardBg = surface
  const cardBorder = `0.5px solid ${border}`
  const cardRadius = radiusLg
  const cardShadow = shadowMd
  const cardShadowHover = shadowLg
  const cardTransition = 'all 0.2s ease'

  const tagBg = light ? '#f3f4f6' : 'rgba(255,255,255,0.06)'

  const progressBg = `linear-gradient(90deg, ${primary}, ${lighten(primary, 0.3)})`

  return {
    // ========== 品牌 & 中性色 ==========
    '--color-primary': primary,
    '--color-primary-light': lighten(primary, 0.85),
    '--color-primary-dark': darken(primary, 0.1),
    '--color-accent': accent,
    '--color-bg': bg,
    '--color-surface': surface,
    '--color-surface-secondary': surfaceSecondary,
    '--color-bg-alt': surfaceSecondary,

    // ========== 文字 ==========
    '--color-text-primary': text,
    '--color-text-secondary': textSecondary,
    '--color-text-tertiary': textTertiary,

    // ========== 链接 & 边框 ==========
    '--color-link': link,
    '--color-border': border,
    '--color-border-light': borderLight,
    '--color-border-active': borderActive,

    // ========== 状态色 ==========
    '--color-success': success,
    '--color-success-light': lighten(success, 0.8),
    '--color-danger': danger,
    '--color-danger-light': lighten(danger, 0.8),
    '--color-warning': warning,
    '--color-warning-light': lighten(warning, 0.8),
    '--color-info': info,
    '--color-info-light': lighten(info, 0.8),

    // ========== 字体 ==========
    '--font-sans': fontFamily || 'ui-sans-serif, system-ui, -apple-system, sans-serif',

    // ========== 页脚 ==========
    '--footer-bg': footerBg,
    '--footer-text': footerText,
    '--footer-border': `0.5px solid ${light ? 'rgba(255,255,255,0.1)' : 'rgba(255,255,255,0.05)'}`,

    // ========== Hero ==========
    '--hero-bg': heroBg,

    // ========== 代码块 ==========
    '--code-bg': codeBg,

    // ========== 阴影 ==========
    '--shadow-sm': shadowSm,
    '--shadow-md': shadowMd,
    '--shadow-lg': shadowLg,

    // ========== 圆角 ==========
    '--radius-sm': '4px',
    '--radius-md': radiusMd,
    '--radius-lg': radiusLg,
    '--radius-xl': radiusXl,
    '--theme-radius': radius,
    '--app-radius': radius,

    // ========== 导航栏 ==========
    '--navbar-bg': navbarBg,
    '--navbar-text': navbarText,
    '--navbar-active-bg': navbarActiveBg,
    '--navbar-active-text': navbarActiveText,
    '--navbar-border': `0.5px solid ${border}`,

    // ========== 卡片 ==========
    '--card-bg': cardBg,
    '--card-border': cardBorder,
    '--card-radius': cardRadius,
    '--card-shadow': cardShadow,
    '--card-shadow-hover': cardShadowHover,
    '--card-transition': cardTransition,

    // ========== 标签 ==========
    '--tag-bg': tagBg,

    // ========== 阅读进度条 ==========
    '--progress-height': '3px',
    '--progress-bg': progressBg,

    // ========== Legacy 别名 ==========
    '--theme-primary': primary,
    '--theme-accent': accent,
    '--theme-bg': bg,
    '--theme-text': text,
    '--theme-link': link,
    '--theme-navbar-bg': navbarBg,
    '--theme-navbar-text': navbarText,
    '--theme-navbar': navbarBg,
    '--theme-border-radius': radius,
  }
}