'use client'

import { createContext, useContext, useEffect, useState, type ReactNode } from 'react'

interface DarkModeContextType {
  isDark: boolean
  toggle: () => void
}

const DarkModeContext = createContext<DarkModeContextType>({
  isDark: false,
  toggle: () => {},
})

export function useDarkMode() {
  return useContext(DarkModeContext)
}

export function DarkModeProvider({ children }: { children: ReactNode }) {
  const [isDark, setIsDark] = useState(false)

  useEffect(() => {
    // 优先读取 localStorage，避免闪烁
    const stored = localStorage.getItem('darkMode')
    if (stored !== null) {
      setIsDark(stored === 'true')
      document.documentElement.classList.toggle('dark', stored === 'true')
      return
    }
    // 否则跟随系统偏好
    const mq = window.matchMedia('(prefers-color-scheme: dark)')
    setIsDark(mq.matches)
    document.documentElement.classList.toggle('dark', mq.matches)

    const handler = (e: MediaQueryListEvent) => {
      if (localStorage.getItem('darkMode') === null) {
        setIsDark(e.matches)
        document.documentElement.classList.toggle('dark', e.matches)
      }
    }
    mq.addEventListener('change', handler)
    return () => mq.removeEventListener('change', handler)
  }, [])

  const toggle = () => {
    setIsDark(prev => {
      const next = !prev
      localStorage.setItem('darkMode', String(next))
      document.documentElement.classList.toggle('dark', next)
      return next
    })
  }

  return (
    <DarkModeContext.Provider value={{ isDark, toggle }}>
      {children}
    </DarkModeContext.Provider>
  )
}