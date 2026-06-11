'use client'

import { useEffect, useRef, type ReactNode } from 'react'

interface ScrollRevealProps {
  children: ReactNode
  className?: string
  /** 每个子元素之间的延迟（ms），默认 100 */
  staggerDelay?: number
  /** 是否只执行一次，默认 true */
  once?: boolean
}

/**
 * 滚动渐入动画容器
 * 使用 IntersectionObserver，子元素依次向下淡入
 */
export default function ScrollReveal({
  children,
  className = '',
  staggerDelay = 100,
  once = true,
}: ScrollRevealProps) {
  const containerRef = useRef<HTMLDivElement>(null)

  useEffect(() => {
    const container = containerRef.current
    if (!container) return

    const items = container.children
    if (items.length === 0) return

    // 检查 prefers-reduced-motion
    const mediaQuery = window.matchMedia('(prefers-reduced-motion: reduce)')
    if (mediaQuery.matches) {
      Array.from(items).forEach((el) => {
        const htmlEl = el as HTMLElement
        htmlEl.style.opacity = '1'
        htmlEl.style.transform = 'none'
        htmlEl.style.transition = 'none'
      })
      return
    }

    // 先全部设为透明
    Array.from(items).forEach((el) => {
      const htmlEl = el as HTMLElement
      htmlEl.style.opacity = '0'
      htmlEl.style.transform = 'translateY(20px)'
      htmlEl.style.transition = 'opacity 400ms ease-out, transform 400ms ease-out'
    })

    const observer = new IntersectionObserver(
      (entries) => {
        entries.forEach((entry) => {
          if (entry.isIntersecting) {
            const index = Array.from(items).indexOf(entry.target)
            const el = entry.target as HTMLElement

            requestAnimationFrame(() => {
              el.style.transitionDelay = `${index * staggerDelay}ms`
              el.style.opacity = '1'
              el.style.transform = 'translateY(0)'

              if (once) {
                // 动画完成后清理 style，然后取消观察
                setTimeout(() => {
                  el.style.transitionDelay = ''
                  el.style.opacity = ''
                  el.style.transform = ''
                }, 400 + index * staggerDelay)
              }
            })

            if (once) {
              observer.unobserve(entry.target)
            }
          } else if (!once) {
            const el = entry.target as HTMLElement
            el.style.opacity = '0'
            el.style.transform = 'translateY(20px)'
            el.style.transitionDelay = '0ms'
          }
        })
      },
      { threshold: 0.01, rootMargin: '0px 0px -40px 0px' }
    )

    Array.from(items).forEach((el) => observer.observe(el))

    return () => observer.disconnect()
  }, [children, staggerDelay, once])

  return (
    <div ref={containerRef} className={className}>
      {children}
    </div>
  )
}