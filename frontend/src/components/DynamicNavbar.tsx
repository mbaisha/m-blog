'use client'

import Link from 'next/link'
import { useState, useEffect, useRef } from 'react'
import type { PublicNavigationItem } from '@/types'
import { apiClient } from '@/lib/api'
import { useDarkMode } from './DarkModeProvider'
import { usePathname, useRouter } from 'next/navigation'

// ===== 默认导航链接（API 无数据时使用） =====
const defaultLinks = [
  { href: '/', label: '首页' },
  { href: '/articles', label: '文章' },
  { href: '/categories', label: '分类' },
  { href: '/tags', label: '标签' },
  { href: '/archive', label: '归档' },
  { href: '/projects', label: '项目' },
  { href: '/about', label: '关于' },
  { href: '/friends', label: '友链' },
  { href: '/search', label: '搜索' },
]

interface FlattenedLink {
  href: string
  label: string
  children?: { href: string; label: string; openInNewTab: boolean }[]
  openInNewTab?: boolean
}

export default function DynamicNavbar() {
  const [navItems, setNavItems] = useState<PublicNavigationItem[]>([])
  const [mobileOpen, setMobileOpen] = useState(false)
  const [searchOpen, setSearchOpen] = useState(false)
  const [searchQuery, setSearchQuery] = useState('')
  const [openDropdown, setOpenDropdown] = useState<string | null>(null)
  const [siteName, setSiteName] = useState('')
  const [logoUrl, setLogoUrl] = useState('')
  const [faviconUrl, setFaviconUrl] = useState('')
  const { isDark, toggle: toggleDark } = useDarkMode()
  const router = useRouter()
  const pathname = usePathname()
  const searchRef = useRef<HTMLFormElement>(null)
  const dropdownTimer = useRef<ReturnType<typeof setTimeout> | undefined>(undefined)

  // ----- 获取后台导航菜单 -----
  useEffect(() => {
    apiClient.get<{ success: boolean; data: PublicNavigationItem[] }>('/navigation/header')
      .then(data => {
        if (data.success && data.data?.length > 0) {
          setNavItems(data.data)
        }
      })
      .catch(() => {})
  }, [])

  // ----- 获取后台站点设置（站点名、Logo、Favicon） -----
  useEffect(() => {
    apiClient.get<{ success: boolean; data: { siteName: string; logoImageUrl: string | null; faviconImageUrl: string | null } }>('/settings')
      .then(data => {
        if (data.success && data.data?.siteName) {
          setSiteName(data.data.siteName)
          setLogoUrl(data.data.logoImageUrl || '')
          setFaviconUrl(data.data.faviconImageUrl || '')
        }
      })
      .catch(() => {})
  }, [])

  // ----- 动态更新 Favicon -----
  useEffect(() => {
    if (!faviconUrl) return
    let link = document.querySelector<HTMLLinkElement>('link[rel="icon"]')
    if (!link) {
      link = document.createElement('link')
      link.rel = 'icon'
      document.head.appendChild(link)
    }
    link.href = faviconUrl
    // 也更新 shortcut icon
    let shortcut = document.querySelector<HTMLLinkElement>('link[rel="shortcut icon"]')
    if (!shortcut) {
      shortcut = document.createElement('link')
      shortcut.rel = 'shortcut icon'
      document.head.appendChild(shortcut)
    }
    shortcut.href = faviconUrl
  }, [faviconUrl])

  // ----- 计算显示名（有站点名用站点名，否则回退） -----

  // ----- 扁平化导航数据（按后端 sortOrder 排序） -----
  const allLinks: FlattenedLink[] = navItems.length > 0
    ? [...navItems].map(item => ({
        href: item.url,
        label: item.title,
        openInNewTab: item.openInNewTab,
        children: item.children?.length > 0
          ? [...item.children].map(c => ({
              href: c.url,
              label: c.title,
              openInNewTab: c.openInNewTab,
            }))
          : undefined,
      }))
    : defaultLinks

  // 桌面端（≥1024px）显示全部，平板端（768-1023px）折叠多余到"更多"
  const desktopLinks = allLinks
  const tabletVisibleCount = 5
  const tabletVisible = allLinks.slice(0, tabletVisibleCount)
  const tabletMore = allLinks.slice(tabletVisibleCount)

  // ----- 搜索框外部点击关闭 -----
  useEffect(() => {
    const handleClickOutside = (e: MouseEvent) => {
      if (searchRef.current && !searchRef.current.contains(e.target as Node)) {
        setSearchOpen(false)
      }
    }
    document.addEventListener('mousedown', handleClickOutside)
    return () => document.removeEventListener('mousedown', handleClickOutside)
  }, [])

  // ----- 搜索提交 -----
  const handleSearch = (e: React.FormEvent) => {
    e.preventDefault()
    if (searchQuery.trim()) {
      router.push(`/search?keywords=${encodeURIComponent(searchQuery.trim())}`)
      setSearchOpen(false)
      setSearchQuery('')
      setMobileOpen(false)
    }
  }

  // ----- 判断当前路由是否激活 -----
  const isActive = (href: string) => {
    if (href === '/') return pathname === '/'
    return pathname.startsWith(href)
  }

  // ----- 下拉菜单 hover 延迟控制 -----
  const handleMouseEnter = (label: string) => {
    clearTimeout(dropdownTimer.current)
    setOpenDropdown(label)
  }
  const handleMouseLeave = () => {
    dropdownTimer.current = setTimeout(() => setOpenDropdown(null), 150)
  }

  // ----- 渲染导航链接（通用） -----
  const renderNavLink = (link: FlattenedLink, isMobile = false) => {
    const hasChildren = link.children && link.children.length > 0
    const active = isActive(link.href)

    if (hasChildren) {
      return (
        <div
          key={link.label}
          className={isMobile ? 'border-b border-[var(--color-border-light)] last:border-0' : 'relative'}
          onMouseEnter={isMobile ? undefined : () => handleMouseEnter(link.label)}
          onMouseLeave={isMobile ? undefined : handleMouseLeave}
        >
          <button
            onClick={() => {
              if (isMobile) {
                setOpenDropdown(openDropdown === link.label ? null : link.label)
              }
            }}
            className={`
              flex items-center gap-1 w-full text-left
              ${isMobile
                ? 'px-4 py-3 text-[15px] font-medium'
                : 'px-4 py-2 text-[15px] font-medium rounded-lg transition-colors'
              }
              ${active
                ? 'font-semibold'
                : ''
              }
            `}
            style={{
              color: active ? 'var(--navbar-active-text)' : 'var(--navbar-text)',
              backgroundColor: active && !isMobile ? 'var(--navbar-active-bg)' : 'transparent',
            }}
            onMouseOver={(e) => {
              if (!isMobile && !active) {
                e.currentTarget.style.backgroundColor = 'var(--navbar-active-bg)'
                e.currentTarget.style.color = 'var(--navbar-active-text)'
              }
            }}
            onMouseOut={(e) => {
              if (!isMobile && !active) {
                e.currentTarget.style.backgroundColor = 'transparent'
                e.currentTarget.style.color = 'var(--navbar-text)'
              }
            }}
          >
            {link.label}
            <svg className={`w-3.5 h-3.5 transition-transform ${openDropdown === link.label ? 'rotate-180' : ''}`}
              fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M19 9l-7 7-7-7" />
            </svg>
          </button>

          {/* 子菜单 */}
          {openDropdown === link.label && (
            <div
              className={isMobile
                ? 'pl-4 pb-2'
                : 'absolute top-full left-0 mt-1 min-w-[160px] rounded-lg shadow-lg border z-50'
              }
              style={{
                backgroundColor: isMobile ? 'transparent' : 'var(--color-surface)',
                borderColor: 'var(--color-border)',
                boxShadow: isMobile ? 'none' : 'var(--shadow-lg)',
              }}
              onMouseEnter={() => clearTimeout(dropdownTimer.current)}
              onMouseLeave={isMobile ? undefined : () => handleMouseLeave()}
            >
              {link.children?.map((child) => (
                <Link
                  key={child.label}
                  href={child.href}
                  target={child.openInNewTab ? '_blank' : undefined}
                  rel={child.openInNewTab ? 'noopener noreferrer' : undefined}
                  className={isMobile
                    ? 'block px-4 py-2.5 text-[15px] rounded-lg transition-colors'
                    : 'block px-4 py-2.5 text-[15px] rounded-lg transition-colors'
                  }
                  style={{
                    color: 'var(--navbar-text)',
                  }}
                  onMouseOver={(e) => {
                    e.currentTarget.style.backgroundColor = 'var(--navbar-active-bg)'
                    e.currentTarget.style.color = 'var(--navbar-active-text)'
                  }}
                  onMouseOut={(e) => {
                    e.currentTarget.style.backgroundColor = 'transparent'
                    e.currentTarget.style.color = 'var(--navbar-text)'
                  }}
                  onClick={() => { setMobileOpen(false); setOpenDropdown(null) }}
                >
                  {child.label}
                </Link>
              ))}
            </div>
          )}
        </div>
      )
    }

    return (
      <Link
        key={link.label}
        href={link.href}
        target={link.openInNewTab ? '_blank' : undefined}
        rel={link.openInNewTab ? 'noopener noreferrer' : undefined}
        className={isMobile
          ? 'block px-4 py-3 text-[15px] font-medium border-b border-[var(--color-border-light)] last:border-0'
          : 'px-4 py-2 text-[15px] font-medium rounded-lg transition-colors'
        }
        style={{
          color: active ? 'var(--navbar-active-text)' : 'var(--navbar-text)',
          backgroundColor: active && !isMobile ? 'var(--navbar-active-bg)' : 'transparent',
        }}
        onMouseOver={(e) => {
          if (!isMobile && !active) {
            e.currentTarget.style.backgroundColor = 'var(--navbar-active-bg)'
            e.currentTarget.style.color = 'var(--navbar-active-text)'
          }
        }}
        onMouseOut={(e) => {
          if (!isMobile && !active) {
            e.currentTarget.style.backgroundColor = 'transparent'
            e.currentTarget.style.color = 'var(--navbar-text)'
          }
        }}
        onClick={() => { setMobileOpen(false); setOpenDropdown(null) }}
      >
        {link.label}
      </Link>
    )
  }

  return (
    <header
      className="sticky top-0 z-50"
      style={{ backgroundColor: 'var(--navbar-bg)', borderBottom: 'var(--navbar-border)' }}
    >
      {/* ===== 布局容器 ===== */}
      <div className="content-container">
        {/* ===== 标题行（桌面 ≥768px 显示） ===== */}
        <div className="hidden md:flex items-center justify-between h-[40px]">
          <Link
            href="/"
            className="hidden md:flex items-center gap-2 text-[18px] font-bold tracking-tight transition-colors"
            style={{ color: 'var(--color-text-primary)' }}
          >
            {logoUrl ? (
              <img src={logoUrl} alt={siteName || 'logo'} className="h-[30px] w-auto object-contain" />
            ) : (
              siteName ? (
                <><span style={{ color: 'var(--color-primary)' }}>{siteName.charAt(0)}</span>{siteName.slice(1)}</>
              ) : (
                <><span style={{ color: 'var(--color-primary)' }}>Zhu</span>&apos;s Blog</>
              )
            )}
          </Link>

          {/* 右侧：搜索 + Dark Mode */}
          <div className="flex items-center gap-1">
            {/* 搜索 */}
            <div className="flex items-center">
              {searchOpen ? (
                <form ref={searchRef} onSubmit={handleSearch} className="flex items-center">
                  <input
                    type="text"
                    value={searchQuery}
                    onChange={(e) => setSearchQuery(e.target.value)}
                    placeholder="搜索文章..."
                    className="w-[220px] h-9 px-3 text-[14px] rounded-lg border outline-none transition-all"
                    style={{
                      backgroundColor: 'var(--color-surface)',
                      borderColor: 'var(--color-border-active)',
                      color: 'var(--color-text-primary)',
                    }}
                    autoFocus
                    onKeyDown={(e) => {
                      if (e.key === 'Escape') setSearchOpen(false)
                    }}
                  />
                  <button
                    type="submit"
                    className="ml-1.5 p-2 rounded-md transition-colors"
                    style={{ color: 'var(--color-primary)' }}
                    aria-label="提交搜索"
                  >
                    <svg className="w-[18px] h-[18px]" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                      <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2}
                        d="M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0z" />
                    </svg>
                  </button>
                </form>
              ) : (
                <button
                  onClick={() => setSearchOpen(true)}
                  className="p-2 rounded-md transition-colors"
                  style={{ color: 'var(--navbar-text)' }}
                  onMouseOver={(e) => { e.currentTarget.style.backgroundColor = 'var(--navbar-active-bg)' }}
                  onMouseOut={(e) => { e.currentTarget.style.backgroundColor = 'transparent' }}
                  aria-label="搜索"
                >
                  <svg className="w-[18px] h-[18px]" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2}
                      d="M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0z" />
                  </svg>
                </button>
              )}
            </div>

            {/* Dark Mode 切换 */}
            <button
              onClick={toggleDark}
              className="p-2 rounded-md transition-colors"
              style={{ color: 'var(--navbar-text)' }}
              onMouseOver={(e) => { e.currentTarget.style.backgroundColor = 'var(--navbar-active-bg)' }}
              onMouseOut={(e) => { e.currentTarget.style.backgroundColor = 'transparent' }}
              aria-label={isDark ? '切换亮色模式' : '切换暗黑模式'}
            >
              {isDark ? (
                <svg className="w-[18px] h-[18px]" fill="currentColor" viewBox="0 0 24 24" style={{ color: '#FBBF24' }}>
                  <path d="M12 3v2m0 14v2m9-9h-2M5 12H3m15.364-6.364l-1.414 1.414M7.05 16.95l-1.414 1.414M21 12a9 9 0 11-18 0 9 9 0 0118 0z" />
                </svg>
              ) : (
                <svg className="w-[18px] h-[18px]" fill="currentColor" viewBox="0 0 24 24">
                  <path d="M20.354 15.354A9 9 0 018.646 3.646 9.003 9.003 0 0012 21a9.003 9.003 0 008.354-5.646z" />
                </svg>
              )}
            </button>
          </div>
        </div>

        {/* ===== 导航栏 ===== */}
        <nav
          className="flex items-center justify-between h-[52px]"
          style={{ borderTop: '0.5px solid var(--color-border)' }}
        >
          {/* 左侧：移动端 Logo + 桌面端菜单 */}
          <div className="flex items-center gap-1 flex-1 min-w-0">
            {/* 移动端 Logo */}
            {logoUrl ? (
              <img src={logoUrl} alt={siteName || 'logo'} className="h-[26px] w-auto object-contain mr-4" />
            ) : (
              <span className="md:hidden text-[17px] font-bold tracking-tight whitespace-nowrap mr-4"
                style={{ color: 'var(--color-text-primary)' }}>
                {siteName ? (
                  <><span style={{ color: 'var(--color-primary)' }}>{siteName.charAt(0)}</span>{siteName.slice(1)}</>
                ) : (
                  <><span style={{ color: 'var(--color-primary)' }}>M</span>Blog</>
                )}
              </span>
            )}

            {/* 桌面端菜单（≥1024px 全显示） */}
            <div className="hidden lg:flex items-center gap-1">
              {desktopLinks.map(link => renderNavLink(link))}
            </div>

            {/* 平板端菜单（768-1023px，前 5 个 + "更多"） */}
            <div className="hidden md:flex lg:hidden items-center gap-1">
              {tabletVisible.map(link => renderNavLink(link))}
              {tabletMore.length > 0 && (
                <div className="relative">
                  <button
                    onClick={() => setOpenDropdown(openDropdown === '__more__' ? null : '__more__')}
                    className="px-4 py-2 text-[15px] font-medium rounded-lg transition-colors flex items-center gap-1"
                    style={{ color: 'var(--navbar-text)' }}
                  >
                    更多
                    <svg className="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                      <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M6 9l6 6 6-6" />
                    </svg>
                  </button>
                  {openDropdown === '__more__' && (
                    <div
                      className="absolute top-full left-0 mt-1 min-w-[150px] rounded-lg shadow-lg border z-50 py-1"
                      style={{
                        backgroundColor: 'var(--color-surface)',
                        borderColor: 'var(--color-border)',
                        boxShadow: 'var(--shadow-lg)',
                      }}
                    >
                      {tabletMore.map(link => (
                        <Link
                          key={link.label}
                          href={link.href}
                          className="block px-4 py-2.5 text-[15px] rounded-lg transition-colors"
                          style={{ color: 'var(--navbar-text)' }}
                          onMouseOver={(e) => {
                            e.currentTarget.style.backgroundColor = 'var(--navbar-active-bg)'
                            e.currentTarget.style.color = 'var(--navbar-active-text)'
                          }}
                          onMouseOut={(e) => {
                            e.currentTarget.style.backgroundColor = 'transparent'
                            e.currentTarget.style.color = 'var(--navbar-text)'
                          }}
                          onClick={() => { setMobileOpen(false); setOpenDropdown(null) }}
                        >
                          {link.label}
                        </Link>
                      ))}
                    </div>
                  )}
                </div>
              )}
            </div>
          </div>

          {/* 移动端：搜索 + 汉堡菜单 */}
          <div className="flex items-center gap-1.5 md:hidden">
            <button
              onClick={() => setSearchOpen(!searchOpen)}
              className="p-2 rounded-md transition-colors"
              style={{ color: 'var(--navbar-text)' }}
              aria-label="搜索"
            >
              <svg className="w-[18px] h-[18px]" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2}
                  d="M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0z" />
              </svg>
            </button>
            <button
              className="p-2 rounded-md transition-colors"
              onClick={() => setMobileOpen(!mobileOpen)}
              aria-label="切换菜单"
              style={{ color: 'var(--navbar-text)' }}
            >
              <svg className="w-[18px] h-[18px]" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                {mobileOpen ? (
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M6 18L18 6M6 6l12 12" />
                ) : (
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M4 6h16M4 12h16M4 18h16" />
                )}
              </svg>
            </button>
          </div>
        </nav>
      </div>

      {/* ===== 移动端搜索框（紧贴导航栏下方） ===== */}
      {searchOpen && (
        <div className="md:hidden px-4 pb-3" style={{ backgroundColor: 'var(--color-surface)' }}>
          <form onSubmit={handleSearch} className="flex items-center gap-2">
            <input
              type="text"
              value={searchQuery}
              onChange={(e) => setSearchQuery(e.target.value)}
              placeholder="搜索文章..."
              className="flex-1 h-10 px-3 text-[14px] rounded-lg border outline-none"
              style={{
                backgroundColor: 'var(--color-bg)',
                borderColor: 'var(--color-border-active)',
                color: 'var(--color-text-primary)',
              }}
              autoFocus
            />
            <button
              type="submit"
              className="px-5 h-10 text-[14px] font-medium rounded-lg transition-colors"
              style={{
                backgroundColor: 'var(--color-primary)',
                color: '#FFFFFF',
              }}
            >
              搜索
            </button>
          </form>
        </div>
      )}

      {/* ===== 移动端抽屉导航 ===== */}
      {mobileOpen && (
        <div
          className="md:hidden fixed inset-0 z-40"
          onClick={() => setMobileOpen(false)}
          style={{ backgroundColor: 'rgba(0,0,0,0.3)' }}
        >
          <div
            className="absolute top-0 right-0 h-full w-[280px] shadow-xl overflow-y-auto"
            style={{ backgroundColor: 'var(--color-surface)' }}
            onClick={(e) => e.stopPropagation()}
          >
            <div className="flex items-center justify-between px-4 h-[44px] border-b"
              style={{ borderColor: 'var(--color-border)' }}>
              <span className="text-[15px] font-semibold" style={{ color: 'var(--color-text-primary)' }}>菜单</span>
              <button
                onClick={() => setMobileOpen(false)}
                className="p-1 rounded-md"
                style={{ color: 'var(--navbar-text)' }}
                aria-label="关闭菜单"
              >
                <svg className="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M6 18L18 6M6 6l12 12" />
                </svg>
              </button>
            </div>
            <div className="py-2">
              {allLinks.map(link => renderNavLink(link, true))}
            </div>
          </div>
        </div>
      )}
    </header>
  )
}