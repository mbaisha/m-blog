'use client'

import { useEffect, useState } from 'react'

/**
 * 回到顶部按钮
 * 滚动超过 400px 后出现，点击平滑回到顶部
 */
export default function BackToTop() {
  const [visible, setVisible] = useState(false)

  useEffect(() => {
    const onScroll = () => {
      setVisible(window.scrollY > 400)
    }
    window.addEventListener('scroll', onScroll, { passive: true })
    return () => window.removeEventListener('scroll', onScroll)
  }, [])

  const scrollToTop = () => {
    window.scrollTo({ top: 0, behavior: 'smooth' })
  }

  return (
    <button
      onClick={scrollToTop}
      aria-label="回到顶部"
      className="fixed z-40 flex items-center justify-center w-9 h-9 rounded-full transition-all duration-200"
      style={{
        bottom: '40px',
        right: '40px',
        backgroundColor: visible ? 'var(--navbar-active-bg, #F3F4F6)' : 'transparent',
        color: 'var(--color-text-secondary, #6B7280)',
        opacity: visible ? 1 : 0,
        pointerEvents: visible ? 'auto' : 'none',
        border: visible ? '0.5px solid var(--color-border, #E5E7EB)' : 'none',
      }}
      onMouseOver={(e) => {
        if (visible) {
          e.currentTarget.style.backgroundColor = 'var(--color-primary-light, #EEEDFE)'
          e.currentTarget.style.color = 'var(--color-primary, #6366F1)'
        }
      }}
      onMouseOut={(e) => {
        if (visible) {
          e.currentTarget.style.backgroundColor = 'var(--navbar-active-bg, #F3F4F6)'
          e.currentTarget.style.color = 'var(--color-text-secondary, #6B7280)'
        }
      }}
    >
      <svg className="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
        <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M5 15l7-7 7 7" />
      </svg>
    </button>
  )
}