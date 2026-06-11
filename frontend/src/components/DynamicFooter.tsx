'use client'

import { useState, useEffect } from 'react'
import Link from 'next/link'
import type { PublicFooterConfig, PublicNavigationItem } from '@/types'
import { apiClient } from '@/lib/api'

interface SocialLinkItem {
  platform: string
  url: string
  icon: string
}

interface FooterBlockLink {
  label: string
  url: string
}

interface FooterBlock {
  title: string
  links: FooterBlockLink[]
}

interface FooterStyleConfig {
  backgroundColor?: string
  textColor?: string
  linkColor?: string
  borderColor?: string
}

// 常用社交平台 SVG 图标
const platformIcons: Record<string, string> = {
  github: 'M12 0c-6.626 0-12 5.373-12 12 0 5.302 3.438 9.8 8.207 11.387.599.111.793-.261.793-.577v-2.234c-3.338.726-4.033-1.416-4.033-1.416-.546-1.387-1.333-1.756-1.333-1.756-1.089-.745.083-.729.083-.729 1.205.084 1.839 1.237 1.839 1.237 1.07 1.834 2.807 1.304 3.492.997.107-.775.418-1.305.762-1.604-2.665-.305-5.467-1.334-5.467-5.931 0-1.311.469-2.381 1.236-3.221-.124-.303-.535-1.524.117-3.176 0 0 1.008-.322 3.301 1.23.957-.266 1.983-.399 3.003-.404 1.02.005 2.047.138 3.006.404 2.291-1.552 3.297-1.23 3.297-1.23.653 1.653.242 2.874.118 3.176.77.84 1.235 1.911 1.235 3.221 0 4.609-2.807 5.624-5.479 5.921.43.372.823 1.102.823 2.222v3.293c0 .319.192.694.801.576 4.765-1.589 8.199-6.086 8.199-11.386 0-6.627-5.373-12-12-12z',
  email: 'M20 4H4c-1.1 0-2 .9-2 2v12c0 1.1.9 2 2 2h16c1.1 0 2-.9 2-2V6c0-1.1-.9-2-2-2zm0 4l-8 5-8-5V6l8 5 8-5v2z',
  rss: 'M5 21c-1.657 0-3-1.343-3-3s1.343-3 3-3 3 1.343 3 3-1.343 3-3 3zm5-2c0-4.418-3.582-8-8-8v-3c6.075 0 11 4.925 11 11h-3zm4 0c0-6.627-5.373-12-12-12v-3c8.284 0 15 6.716 15 15h-3z',
  twitter: 'M18.244 2.25h3.308l-7.227 8.26 8.502 11.24H16.17l-5.214-6.817L4.99 21.75H1.68l7.73-8.835L1.254 2.25H8.08l4.713 6.231zm-1.161 17.52h1.833L7.084 4.126H5.117z',
  weibo: 'M20.194 14.197c0 3.248-4.145 7.065-10.59 7.065-5.156 0-10.59-2.437-10.59-6.326 0-1.54 1.094-3.035 2.498-3.035.632 0 1.272.201 1.272.787 0 .355-.233.656-.52.854-.32.223-.496.55-.496.882 0 1.332 1.763 2.394 3.822 2.394 3.051 0 5.404-1.991 5.404-4.385 0-1.349-.678-2.591-1.863-3.358.096-.136.497-.678.614-.856.948-1.504 2.042-2.977 3.08-3.938.684-.633 1.632-.945 2.621-.945 1.407 0 2.952.977 2.952 2.702 0 .706-.348 2.794-1.361 4.514-.012.023-.083.163-.083.25 0 .268.208.442.553.442 1.915 0 3.907-1.01 3.907-3.983 0-4.598-5.097-7.69-10.89-7.69-5.065 0-8.804 2.479-8.804 5.469 0 1.607 1.216 3.052 3.12 3.052.625 0 1.392-.292 1.727-.658.121-.133.253-.414.403-.414.205 0 .305.262.305.515 0 .42-.145.782-.254.979-.472.852-1.628 1.601-2.866 1.601-2.135 0-4.115-1.728-4.115-4.782 0-3.598 3.458-6.696 8.358-6.696 5.27 0 8.803 2.486 8.803 6.186 0 2.01-1.335 3.772-3.12 3.772-1.524 0-2.688-1.003-2.688-2.382 0-1.387.855-1.932 1.562-1.932.612 0 .922.454.922.959 0 .756-.282 1.134-.282 1.628 0 .584.495.806.84.806.63 0 1.264-.99 1.264-2.067 0-1.371-1.141-3.074-3.896-3.074-.768 0-2.404.538-2.404 3.258 0 2.162 1.64 3.764 3.822 3.764 1.036 0 1.951-.382 2.694-1.058-.447.68-2.592 2.714-5.169 2.714-1.614 0-3.867-.99-3.867-3.536 0-1.919 1.42-3.441 3.516-4.149-.186-.082-.344-.125-.528-.125-1.396 0-2.338 1.385-2.338 2.926 0 1.57.88 2.769 2.302 2.769.542 0 1.006-.204 1.373-.575-.342.754-1.048 1.415-1.923 1.415-1.476 0-2.578-1.375-2.578-3.143 0-1.647 1.131-3.221 2.794-3.221.862 0 1.566.363 2.104.978.957-1.074 2.358-1.806 3.958-1.806 2.279 0 3.814 1.525 3.814 3.583 0 1.507-.813 2.831-1.98 3.562'.replace(/\s+/g, ' '),
  zhihu: 'M11.267 1.27a.81.81 0 0 0-.242-.228.618.618 0 0 0-.32-.092h-6.78a.618.618 0 0 0-.32.092.81.81 0 0 0-.242.228L.179 5.869a.619.619 0 0 0-.105.367c0 .142.045.28.134.392a.62.62 0 0 0 .34.208l3.304.58-.433 8.28a.62.62 0 0 0 .125.405.618.618 0 0 0 .346.213l3.137.542a.619.619 0 0 0 .392-.064.62.62 0 0 0 .249-.254l1.948-3.676-.59 7.87a.619.619 0 0 0 .125.404.618.618 0 0 0 .346.214l3.137.541a.619.619 0 0 0 .392-.064.62.62 0 0 0 .249-.254l4.084-8.536c.28-.347.47-.77.543-1.224a2.888 2.888 0 0 0-.069-1.086c-.078-.33-.205-.645-.376-.939.034-.04.065-.082.093-.125l4.44-6.6a.81.81 0 0 0 .24-.227.618.618 0 0 0 .092-.32V1.542a.618.618 0 0 0-.092-.32.81.81 0 0 0-.24-.228.618.618 0 0 0-.32-.092h-6.78a.618.618 0 0 0-.32.092.81.81 0 0 0-.242.228l-2.972 4.416c-.08.119-.14.25-.179.386a5.168 5.168 0 0 0-.436-.167l-6.332-1.88a.619.619 0 0 0-.392-.065.62.62 0 0 0-.323.19l-2.043 2.58a.618.618 0 0 0-.13.391c0 .143.046.282.131.393l1.08 1.386.04.05a.618.618 0 0 0 .38.221l2.964.52.003.001.002.001c.07.013.14.02.212.02a.618.618 0 0 0 .41-.156l.007-.007.006-.006c.188-.168.32-.39.38-.634l.503-1.783.003-.01a.3.3 0 0 1 .046-.098l.002-.002c.057-.075.14-.127.237-.147.082-.017.167.001.237.052l.272.196v.001c.07.051.12.125.14.208l.052.212.004.016V19.08l-2.719-1.686a.618.618 0 0 0-.345-.107.62.62 0 0 0-.349.11l-3.482 2.164a.618.618 0 0 0-.24.228.618.618 0 0 0-.093.32v3.105c0 .113.032.224.092.32a.618.618 0 0 0 .24.228l3.482 2.163a.62.62 0 0 0 .35.11c.126 0 .25-.038.348-.11l2.72-1.687v2.112c0 .113.03.223.092.32a.618.618 0 0 0 .24.228l3.482 2.163a.62.62 0 0 0 .35.11c.126 0 .25-.038.348-.11l2.72-1.687',
  bilibili: 'M17.813 4.653h.854c1.51.054 2.769.578 3.773 1.574 1.004.995 1.524 2.249 1.56 3.76v7.36c-.036 1.51-.556 2.769-1.56 3.773s-2.262 1.524-3.773 1.56H5.333c-1.51-.036-2.769-.556-3.773-1.56S.036 18.858 0 17.347v-7.36c.036-1.511.556-2.77 1.56-3.774S3.823 4.69 5.333 4.653h.774L4.687 3.115a.89.89 0 0 1-.28-.746.89.89 0 0 1 .373-.614.89.89 0 0 1 .72-.186c.254.037.473.158.659.36l1.954 2.18v-.027l.026-.026h2.027v.053l1.946-2.18a.774.774 0 0 1 .68-.347c.284.018.516.139.694.36.178.222.264.465.256.734a.89.89 0 0 1-.277.72l-1.333 1.52zm-5.706 2.24c-.746 0-1.387.213-1.92.64s-.833.955-.853 1.587v.746c0 .32.117.592.35.816s.514.346.843.346c.33 0 .607-.115.832-.346.226-.231.339-.497.339-.796v-.8c0-.329.107-.591.32-.789.213-.197.491-.296.835-.296.328 0 .606.102.835.307.229.204.344.466.344.789v1.386c0 .308.108.573.323.795.215.222.493.333.832.333.34 0 .625-.111.854-.333.23-.222.344-.487.344-.795V9.16c0-.618-.224-1.159-.672-1.622-.448-.464-1.013-.696-1.696-.696h-.17l.011.053v-.053zm-8.373 1.76a.75.75 0 0 0-.507.896l.96 3.873v6.027c0 .422.148.78.443 1.072.295.293.652.44 1.07.44h11.84c.42 0 .777-.147 1.072-.44.295-.293.443-.65.443-1.072v-6.027l.96-3.873a.75.75 0 0 0-.507-.896.76.76 0 0 0-.903.507l-.96 3.873h-2.11v-4.24c0-.35-.12-.648-.363-.894a1.23 1.23 0 0 0-.885-.363c-.354 0-.66.121-.917.363a1.21 1.21 0 0 0-.373.894v4.24h-3.067v-4.24c0-.35-.12-.648-.373-.894a1.23 1.23 0 0 0-.918-.363c-.353 0-.66.121-.917.363a1.21 1.21 0 0 0-.363.894v4.24H8.32l-.96-3.873a.75.75 0 0 0-.726-.53Z',
  linkedin: 'M20.447 20.452h-3.554v-5.569c0-1.328-.027-3.037-1.852-3.037-1.853 0-2.136 1.445-2.136 2.939v5.667H9.351V9h3.414v1.561h.046c.477-.9 1.637-1.85 3.37-1.85 3.601 0 4.267 2.37 4.267 5.455v6.286zM5.337 7.433c-1.144 0-2.063-.926-2.063-2.065 0-1.138.92-2.063 2.063-2.063 1.14 0 2.064.925 2.064 2.063 0 1.139-.925 2.065-2.064 2.065zm1.782 13.019H3.555V9h3.564v11.452zM22.225 0H1.771C.792 0 0 .774 0 1.729v20.542C0 23.227.792 24 1.771 24h20.451C23.2 24 24 23.227 24 22.271V1.729C24 .774 23.2 0 22.222 0h.003z',
  facebook: 'M24 12.073c0-6.627-5.373-12-12-12s-12 5.373-12 12c0 5.99 4.388 10.954 10.125 11.854v-8.385H7.078v-3.47h3.047V9.43c0-3.007 1.792-4.669 4.533-4.669 1.312 0 2.686.235 2.686.235v2.953H15.83c-1.491 0-1.956.925-1.956 1.874v2.25h3.328l-.532 3.47h-2.796v8.385C19.612 23.027 24 18.062 24 12.073z',
  youtube: 'M23.498 6.186a3.016 3.016 0 0 0-2.122-2.136C19.505 3.545 12 3.545 12 3.545s-7.505 0-9.377.505A3.017 3.017 0 0 0 .502 6.186C0 8.07 0 12 0 12s0 3.93.502 5.814a3.016 3.016 0 0 0 2.122 2.136c1.871.505 9.376.505 9.376.505s7.505 0 9.377-.505a3.015 3.015 0 0 0 2.122-2.136C24 15.93 24 12 24 12s0-3.93-.502-5.814zM9.545 15.568V8.432L15.818 12l-6.273 3.568z',
}

export default function DynamicFooter() {
  const currentYear = new Date().getFullYear()
  const [config, setConfig] = useState<PublicFooterConfig | null>(null)
  const [parsedBlocks, setParsedBlocks] = useState<FooterBlock[]>([])
  const [parsedSocialLinks, setParsedSocialLinks] = useState<SocialLinkItem[]>([])
  const [footerNavItems, setFooterNavItems] = useState<PublicNavigationItem[]>([])

  useEffect(() => {
    apiClient.get<{ success: boolean; data: PublicFooterConfig }>('/footer')
      .then(data => {
        if (data.success && data.data) {
          setConfig(data.data)
          // Parse blocks JSON
          try {
            const blocks = JSON.parse(data.data.blocks || '[]')
            if (Array.isArray(blocks)) setParsedBlocks(blocks)
          } catch { /* ignore */ }
          // Parse socialLinks JSON
          try {
            const links = JSON.parse(data.data.socialLinks || '[]')
            if (Array.isArray(links)) setParsedSocialLinks(links)
          } catch { /* ignore */ }
        }
      })
      .catch(() => {})
  }, [])

  // ----- 获取底部导航菜单 -----
  useEffect(() => {
    apiClient.get<{ success: boolean; data: PublicNavigationItem[] }>('/navigation/footer')
      .then(data => {
        if (data.success && data.data?.length > 0) {
          setFooterNavItems(data.data)
        }
      })
      .catch(() => {})
  }, [])

  // Parse style config
  let styleConfig: FooterStyleConfig = {}
  try {
    if (config?.style) {
      const parsed = JSON.parse(config.style)
      if (typeof parsed === 'object' && parsed !== null) {
        styleConfig = parsed
      }
    }
  } catch {
    // style 可能是 simple/detailed/centered 字符串
  }

  const footerStyle = config?.style || 'detailed'
  const useSimpleStyle = footerStyle === 'simple' || footerStyle === '"simple"'
  const useCenteredStyle = footerStyle === 'centered' || footerStyle === '"centered"'

  const copyright = config?.copyright || `© ${currentYear} MBlog. All rights reserved.`
  const icpNumber = config?.icpNumber
  const icpUrl = config?.icpUrl
  const policeNumber = config?.policeNumber
  const policeUrl = config?.policeUrl

  if (config && !config.isVisible) return null

  // Build footer inline styles from styleConfig
  const footerInlineStyle: React.CSSProperties = {
    borderTop: '0.5px solid var(--color-border)',
    color: 'var(--footer-text, var(--color-text-secondary))',
  }
  if (styleConfig.backgroundColor) footerInlineStyle.backgroundColor = styleConfig.backgroundColor
  if (styleConfig.textColor) footerInlineStyle.color = styleConfig.textColor
  if (styleConfig.borderColor) footerInlineStyle.borderTop = `0.5px solid ${styleConfig.borderColor}`

  const getLinkStyle = (): React.CSSProperties => ({
    color: styleConfig.linkColor || 'var(--color-primary)',
  })

  const hasBlocks = parsedBlocks.length > 0
  const hasSocialLinks = parsedSocialLinks.length > 0
  const hasFooterNav = footerNavItems.length > 0

  return (
    <footer className="mt-auto" style={footerInlineStyle}>
      <div className="content-container py-8">
        {/* 简洁模式：仅版权信息 */}
        {useSimpleStyle && (
          <div className="flex flex-col md:flex-row md:justify-between items-center gap-4">
            <p className="text-lg" style={{ color: styleConfig.textColor || 'var(--color-text-secondary)' }}>
              {copyright}
            </p>
            <div className="flex flex-wrap gap-x-4 gap-y-1 text-base" style={{ color: styleConfig.textColor || 'var(--color-text-tertiary)' }}>
              {icpNumber && (
                icpUrl
                  ? <a href={icpUrl} target="_blank" rel="noopener noreferrer" style={getLinkStyle()}>{icpNumber}</a>
                  : <span>{icpNumber}</span>
              )}
              {policeNumber && (
                policeUrl
                  ? <a href={policeUrl} target="_blank" rel="noopener noreferrer" style={getLinkStyle()}>{policeNumber}</a>
                  : <span>{policeNumber}</span>
              )}
            </div>
          </div>
        )}

        {/* 居中模式：所有内容居中 */}
        {useCenteredStyle && (
          <div className="flex flex-col items-center text-center gap-4">
            <p className="text-lg" style={{ color: styleConfig.textColor || 'var(--color-text-secondary)' }}>
              {copyright}
            </p>
            {(icpNumber || policeNumber) && (
              <div className="flex flex-wrap gap-x-4 gap-y-1 text-base justify-center" style={{ color: styleConfig.textColor || 'var(--color-text-tertiary)' }}>
                {icpNumber && (
                  icpUrl
                    ? <a href={icpUrl} target="_blank" rel="noopener noreferrer" style={getLinkStyle()}>{icpNumber}</a>
                    : <span>{icpNumber}</span>
                )}
                {policeNumber && (
                  policeUrl
                    ? <a href={policeUrl} target="_blank" rel="noopener noreferrer" style={getLinkStyle()}>{policeNumber}</a>
                    : <span>{policeNumber}</span>
                )}
              </div>
            )}
            {hasSocialLinks && (
              <div className="flex items-center gap-3">
                {parsedSocialLinks.map((link, i) => (
                  <a
                    key={i}
                    href={link.url}
                    target="_blank"
                    rel="noopener noreferrer"
                    className="p-2 rounded-lg transition-all"
                    style={{ color: styleConfig.textColor || 'var(--color-text-tertiary)' }}
                    onMouseOver={(e) => {
                      e.currentTarget.style.color = styleConfig.linkColor || 'var(--color-primary)'
                      e.currentTarget.style.backgroundColor = 'var(--color-primary-light)'
                    }}
                    onMouseOut={(e) => {
                      e.currentTarget.style.color = styleConfig.textColor || 'var(--color-text-tertiary)'
                      e.currentTarget.style.backgroundColor = 'transparent'
                    }}
                    aria-label={link.platform}
                  >
                    {platformIcons[link.platform] ? (
                      <svg className="w-6 h-6" fill="currentColor" viewBox="0 0 24 24">
                        <path d={platformIcons[link.platform]} />
                      </svg>
                    ) : (
                      <svg className="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                        <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M10 6H6a2 2 0 00-2 2v10a2 2 0 002 2h10a2 2 0 002-2v-4M14 4h6m0 0v6m0-6L10 14" />
                      </svg>
                    )}
                  </a>
                ))}
              </div>
            )}
          </div>
        )}

        {/* 详细模式（默认）：完整布局 */}
        {!useSimpleStyle && !useCenteredStyle && (
          <div className="flex flex-col gap-8">
            {/* 区块链接行 + 底部导航 */}
            {(hasBlocks || hasFooterNav) && (
              <div className="grid grid-cols-2 md:grid-cols-4 gap-6">
                {parsedBlocks.map((block, i) => (
                  <div key={i}>
                    <h4 className="text-base font-semibold mb-3" style={{ color: styleConfig.textColor || 'var(--color-text-primary)' }}>
                      {block.title}
                    </h4>
                    <ul className="flex flex-col gap-2">
                      {block.links.map((link, j) => (
                        <li key={j}>
                          <a
                            href={link.url}
                            target="_blank"
                            rel="noopener noreferrer"
                            className="text-base transition-colors hover:opacity-80"
                            style={{ color: styleConfig.textColor || 'var(--color-text-tertiary)' }}
                          >
                            {link.label}
                          </a>
                        </li>
                      ))}
                    </ul>
                  </div>
                ))}
                {hasFooterNav && (
                  <div>
                    <h4 className="text-base font-semibold mb-3" style={{ color: styleConfig.textColor || 'var(--color-text-primary)' }}>
                      快速链接
                    </h4>
                    <ul className="flex flex-col gap-2">
                      {footerNavItems.map((item, i) => (
                        <li key={i}>
                          {item.children?.length > 0 ? (
                            <details className="group">
                              <summary className="cursor-pointer text-base transition-colors hover:opacity-80 list-none flex items-center gap-1"
                                style={{ color: styleConfig.textColor || 'var(--color-text-tertiary)' }}>
                                {item.title}
                                <svg className="w-3 h-3 transition-transform group-open:rotate-90" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9 5l7 7-7 7" />
                                </svg>
                              </summary>
                              <ul className="pl-3 mt-1 flex flex-col gap-1">
                                {item.children.map((child, ci) => (
                                  <li key={ci}>
                                    <Link
                                      href={child.url}
                                      target={child.openInNewTab ? '_blank' : undefined}
                                      rel={child.openInNewTab ? 'noopener noreferrer' : undefined}
                                      className="text-base transition-colors hover:opacity-80 block py-0.5"
                                      style={{ color: styleConfig.textColor || 'var(--color-text-tertiary)' }}
                                    >
                                      {child.title}
                                    </Link>
                                  </li>
                                ))}
                              </ul>
                            </details>
                          ) : (
                            <Link
                              href={item.url}
                              target={item.openInNewTab ? '_blank' : undefined}
                              rel={item.openInNewTab ? 'noopener noreferrer' : undefined}
                              className="text-base transition-colors hover:opacity-80"
                              style={{ color: styleConfig.textColor || 'var(--color-text-tertiary)' }}
                            >
                              {item.title}
                            </Link>
                          )}
                        </li>
                      ))}
                    </ul>
                  </div>
                )}
              </div>
            )}

            {/* 底部行：版权 + 社交 + 备案 */}
            <div className="flex flex-col md:flex-row md:justify-between gap-4 items-start md:items-center border-t pt-6" style={{ borderColor: styleConfig.borderColor || 'var(--color-border)' }}>
              <div className="flex flex-col gap-2">
                <p className="text-lg" style={{ color: styleConfig.textColor || 'var(--color-text-secondary)' }}>
                  {copyright}
                </p>
                {(icpNumber || policeNumber) && (
                  <div className="flex flex-wrap gap-x-4 gap-y-1 text-base" style={{ color: styleConfig.textColor || 'var(--color-text-tertiary)' }}>
                    {icpNumber && (
                      icpUrl
                        ? <a href={icpUrl} target="_blank" rel="noopener noreferrer" style={getLinkStyle()}>{icpNumber}</a>
                        : <span>{icpNumber}</span>
                    )}
                    {policeNumber && (
                      policeUrl
                        ? <a href={policeUrl} target="_blank" rel="noopener noreferrer" style={getLinkStyle()}>{policeNumber}</a>
                        : <span>{policeNumber}</span>
                    )}
                  </div>
                )}
              </div>

              {/* 社交链接 + 联系方式 */}
              <div className="flex flex-col items-start md:items-end gap-2">
                {hasSocialLinks && (
                  <div className="flex items-center gap-3">
                    {parsedSocialLinks.map((link, i) => (
                      <a
                        key={i}
                        href={link.url}
                        target="_blank"
                        rel="noopener noreferrer"
                        className="p-2 rounded-lg transition-all"
                        style={{ color: styleConfig.textColor || 'var(--color-text-tertiary)' }}
                        onMouseOver={(e) => {
                          e.currentTarget.style.color = styleConfig.linkColor || 'var(--color-primary)'
                          e.currentTarget.style.backgroundColor = 'var(--color-primary-light)'
                        }}
                        onMouseOut={(e) => {
                          e.currentTarget.style.color = styleConfig.textColor || 'var(--color-text-tertiary)'
                          e.currentTarget.style.backgroundColor = 'transparent'
                        }}
                        aria-label={link.platform}
                      >
                        {platformIcons[link.platform] ? (
                          <svg className="w-6 h-6" fill="currentColor" viewBox="0 0 24 24">
                            <path d={platformIcons[link.platform]} />
                          </svg>
                        ) : (
                          <svg className="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M10 6H6a2 2 0 00-2 2v10a2 2 0 002 2h10a2 2 0 002-2v-4M14 4h6m0 0v6m0-6L10 14" />
                          </svg>
                        )}
                      </a>
                    ))}
                  </div>
                )}
                {/* 联系方式 */}
                {(config?.contactEmail || config?.contactWeChat) && (
                  <div className="flex flex-wrap gap-x-3 gap-y-1 text-base" style={{ color: styleConfig.textColor || 'var(--color-text-tertiary)' }}>
                    {config.contactEmail && <span>邮箱：{config.contactEmail}</span>}
                    {config.contactWeChat && <span>微信：{config.contactWeChat}</span>}
                  </div>
                )}
              </div>
            </div>
          </div>
        )}
      </div>
    </footer>
  )
}