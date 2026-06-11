'use client'

import Link from 'next/link'
import { usePathname } from 'next/navigation'

/**
 * 9.10 移动端底部导航栏
 * 在手机端固定显示在屏幕底部
 */

const iconPaths = {
  home: 'M3 12l2-2m0 0l7-7 7 7M5 10v10a1 1 0 001 1h3m10-11l2 2m-2-2v10a1 1 0 01-1 1h-3m-6 0a1 1 0 001-1v-4a1 1 0 011-1h2a1 1 0 011 1v4a1 1 0 001 1m-6 0h6',
  articles: 'M9 12h6m-6 4h6m2 5H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z',
  projects: 'M3 7v10a2 2 0 002 2h14a2 2 0 002-2V9a2 2 0 00-2-2h-6l-2-2H5a2 2 0 00-2 2z',
  search: 'M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0z',
  about: 'M16 7a4 4 0 11-8 0 4 4 0 018 0zM12 14a7 7 0 00-7 7h14a7 7 0 00-7-7z',
}

const bottomLinks = [
  { href: '/', label: '首页', icon: 'home' },
  { href: '/articles', label: '文章', icon: 'articles' },
  { href: '/projects', label: '项目', icon: 'projects' },
  { href: '/search', label: '搜索', icon: 'search' },
  { href: '/about', label: '关于', icon: 'about' },
]

export default function MobileBottomNav() {
  const pathname = usePathname()

  return (
    <nav
      className="fixed bottom-0 left-0 right-0 z-50 md:hidden safe-area-bottom"
      style={{
        borderTop: '0.5px solid var(--color-border)',
        backgroundColor: 'var(--color-surface)',
      }}
    >
      <div className="flex items-center justify-around h-14 px-2">
        {bottomLinks.map((link) => {
          const isActive = link.href === '/'
            ? pathname === '/'
            : pathname.startsWith(link.href)

          return (
            <Link
              key={link.href}
              href={link.href}
              className="flex flex-col items-center justify-center px-2 py-1 min-w-[48px] transition-colors rounded-lg"
              style={{
                color: isActive ? 'var(--color-primary)' : 'var(--color-text-tertiary)',
              }}
            >
              <svg className="w-5 h-5 mb-0.5" fill="none" stroke="currentColor" viewBox="0 0 24 24" strokeWidth={isActive ? 2.5 : 1.5}>
                <path strokeLinecap="round" strokeLinejoin="round" d={iconPaths[link.icon as keyof typeof iconPaths]} />
              </svg>
              <span className="text-[10px] leading-none font-medium">{link.label}</span>
            </Link>
          )
        })}
      </div>
    </nav>
  )
}