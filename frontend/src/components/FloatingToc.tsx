'use client'

import { useEffect, useState, useRef } from 'react'
import type { TocItem } from '@/lib/headings'

interface Props {
  headings: TocItem[]
}

export default function FloatingToc({ headings }: Props) {
  const [open, setOpen] = useState(false)
  const [activeId, setActiveId] = useState('')
  const panelRef = useRef<HTMLDivElement>(null)
  const scrollRAF = useRef<number | undefined>(undefined)

  // Determine the topmost heading level for correct indentation
  const minLevel = headings.length > 0 ? Math.min(...headings.map(h => h.level)) : 2

  // Track visible heading via IntersectionObserver
  useEffect(() => {
    if (headings.length === 0) return

    const observer = new IntersectionObserver(
      (entries) => {
        entries.forEach((entry) => {
          if (entry.isIntersecting) {
            setActiveId(entry.target.id)
          }
        })
      },
      { rootMargin: '-80px 0px -75% 0px' }
    )

    const elements: Element[] = []
    headings.forEach((h) => {
      const el = document.getElementById(h.id)
      if (el) {
        observer.observe(el)
        elements.push(el)
      }
    })

    const onScroll = () => {
      if (scrollRAF.current) return
      scrollRAF.current = requestAnimationFrame(() => {
        for (let i = elements.length - 1; i >= 0; i--) {
          const el = elements[i]
          const rect = el.getBoundingClientRect()
          if (rect.top <= 120) {
            setActiveId(el.id)
            break
          }
        }
        scrollRAF.current = undefined
      })
    }

    window.addEventListener('scroll', onScroll, { passive: true })
    return () => {
      observer.disconnect()
      window.removeEventListener('scroll', onScroll)
      if (scrollRAF.current) cancelAnimationFrame(scrollRAF.current)
    }
  }, [headings])

  // Close on click outside / Escape
  useEffect(() => {
    if (!open) return
    const handleClick = (e: MouseEvent) => {
      if (panelRef.current && !panelRef.current.contains(e.target as Node)) {
        setOpen(false)
      }
    }
    const handleEsc = (e: KeyboardEvent) => {
      if (e.key === 'Escape') setOpen(false)
    }
    // Delay to avoid the same click that opened it
    const timer = setTimeout(() => {
      document.addEventListener('click', handleClick, { capture: true })
      document.addEventListener('keydown', handleEsc)
    }, 0)
    return () => {
      clearTimeout(timer)
      document.removeEventListener('click', handleClick, { capture: true })
      document.removeEventListener('keydown', handleEsc)
    }
  }, [open])

  const scrollTo = (text: string) => {
    const allHeadings = document.querySelectorAll('h1, h2, h3, h4')
    for (const heading of allHeadings) {
      if (heading.textContent?.trim() === text) {
        const top = heading.getBoundingClientRect().top + window.scrollY - 80
        window.scrollTo({ top, behavior: 'smooth' })
        setOpen(false)
        return
      }
    }
    // Fallback: try by id
    const el = document.getElementById(text)
    if (el) {
      const top = el.getBoundingClientRect().top + window.scrollY - 80
      window.scrollTo({ top, behavior: 'smooth' })
    }
    setOpen(false)
  }

  if (headings.length === 0) return null

  return (
    <div ref={panelRef} className="hidden lg:block">
      {/* Toggle button - subtle, elegant */}
      <button
        onClick={() => setOpen(!open)}
        className="fixed z-40 flex items-center justify-center transition-all duration-300 ease-out"
        style={{
          left: open
            ? `max(284px, calc((100% - 1200px) / 2 + 268px))`
            : 'max(8px, calc((100% - 1200px) / 2 - 54px))',
          top: '50%',
          transform: 'translateY(-50%)',
          width: open ? '32px' : '36px',
          height: open ? '32px' : '36px',
          borderRadius: '50%',
          backgroundColor: open ? 'var(--color-surface)' : 'var(--color-primary-light)',
          color: open ? 'var(--color-primary)' : 'var(--color-primary)',
          border: open ? '0.5px solid var(--color-border)' : 'none',
          boxShadow: open ? 'none' : '0 2px 12px rgba(99,102,241,0.2)',
          opacity: 0.85,
        }}
        onMouseOver={(e) => { e.currentTarget.style.opacity = '1'; e.currentTarget.style.boxShadow = '0 2px 16px rgba(99,102,241,0.35)' }}
        onMouseOut={(e) => { e.currentTarget.style.opacity = '0.85'; if (!open) e.currentTarget.style.boxShadow = '0 2px 12px rgba(99,102,241,0.2)' }}
        title={open ? '关闭目录' : '显示目录'}
      >
        <svg
          className="transition-transform duration-200"
          style={{ transform: open ? 'rotate(0deg)' : 'rotate(0deg)', width: open ? '14px' : '16px', height: open ? '14px' : '16px' }}
          fill="none" stroke="currentColor" viewBox="0 0 24 24"
        >
          {open ? (
            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M6 18L18 6M6 6l12 12" />
          ) : (
            <>
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M4 6h16" />
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M4 12h16" />
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M4 18h16" />
            </>
          )}
        </svg>
      </button>

      {/* Floating panel - glassmorphism, elegant */}
      {open && (
        <div
          className="fixed z-40 rounded-xl overflow-hidden animate-in"
          style={{
            left: 'max(52px, calc((100% - 1200px) / 2 + 12px))',
            top: '50%',
            transform: 'translateY(-50%)',
            width: '228px',
            maxHeight: '70vh',
            backgroundColor: 'var(--color-surface)',
            border: '0.5px solid var(--color-border)',
            boxShadow: '0 8px 32px rgba(0,0,0,0.10), 0 2px 8px rgba(0,0,0,0.04)',
            backdropFilter: 'blur(12px)',
            WebkitBackdropFilter: 'blur(12px)',
          }}
        >
          {/* Header with decorative accent bar */}
          <div
            className="h-[3px] w-full flex-shrink-0"
            style={{ background: 'linear-gradient(90deg, var(--color-primary), transparent)' }}
          />
          <div className="px-4 pt-3 pb-2 flex items-center gap-2">
            <svg className="w-[14px] h-[14px]" fill="none" stroke="currentColor" viewBox="0 0 24 24" style={{ color: 'var(--color-primary)' }}>
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M4 6h16M4 12h16M4 18h16" />
            </svg>
            <span className="text-[13px] font-semibold tracking-wide" style={{ color: 'var(--color-text-primary)' }}>
              目录
            </span>
            <span className="text-[10px] ml-auto" style={{ color: 'var(--color-text-tertiary)' }}>
              {headings.length} 项
            </span>
          </div>
          <div className="border-t" style={{ borderColor: 'var(--color-border)' }} />

          {/* Scrollable content */}
          <nav className="overflow-y-auto" style={{ maxHeight: 'calc(70vh - 60px)' }}>
            {headings.map((h) => {
              const isActive = activeId === h.id
              const depth = h.level - minLevel
              return (
                <button
                  key={h.id}
                  onClick={() => scrollTo(h.text)}
                  className="block w-full text-left transition-all duration-150 ease-out group"
                  style={{
                    padding: '7px 16px',
                    paddingLeft: `${16 + depth * 18}px`,
                    backgroundColor: isActive ? 'var(--color-primary-light)' : 'transparent',
                  }}
                >
                  <div className="flex items-start gap-2">
                    {/* Active dot indicator */}
                    <span
                      className="flex-shrink-0 mt-[5px] rounded-full transition-all duration-200"
                      style={{
                        width: isActive ? '6px' : '4px',
                        height: isActive ? '6px' : '4px',
                        backgroundColor: isActive ? 'var(--color-primary)' : 'var(--color-text-tertiary)',
                        opacity: isActive ? 1 : 0.35,
                      }}
                    />
                    {/* Text */}
                    <span
                      className="text-[13px] leading-relaxed transition-colors duration-150 group-hover:opacity-100"
                      style={{
                        color: isActive ? 'var(--color-primary)' : 'var(--color-text-secondary)',
                        fontWeight: isActive ? 500 : 400,
                        opacity: isActive ? 1 : 0.75,
                      }}
                    >
                      {h.text}
                    </span>
                  </div>
                </button>
              )
            })}
          </nav>
        </div>
      )}

      {/* Global animation keyframes injected once */}
      <style jsx global>{`
        @keyframes tocFadeIn {
          from { opacity: 0; transform: translateY(-50%) translateX(-8px); }
          to   { opacity: 1; transform: translateY(-50%) translateX(0); }
        }
        .animate-in {
          animation: tocFadeIn 0.2s ease-out;
        }
      `}</style>
    </div>
  )
}
