'use client'

import { useEffect, useState, useCallback, useRef } from 'react'

/**
 * 429 被拉黑时全屏覆盖组件
 * 行为：
 *   - 拦截全局 fetch 检测 429
 *   - 收到 429 后立即清空屏幕内容，显示全屏覆盖
 *   - 自第一次收到 429 开始倒计时，刷新页面只显示剩余时间，不重置
 *   - 不显示任何其他错误提示
 */
export default function IpBanOverlay() {
  const [banned, setBanned] = useState(false)
  const [retryAfterSeconds, setRetryAfterSeconds] = useState(0)
  const patchedRef = useRef(false)

  /** 计算剩余秒数（从 localStorage 读取拉黑开始时间） */
  const calcRemaining = useCallback(() => {
    try {
      const banStartStr = localStorage.getItem('ip_ban_start')
      const banDurationStr = localStorage.getItem('ip_ban_duration')
      if (!banStartStr || !banDurationStr) return 0

      const banStart = parseInt(banStartStr, 10)
      const banDuration = parseInt(banDurationStr, 10)
      const elapsed = Math.floor((Date.now() - banStart) / 1000)
      return Math.max(0, banDuration - elapsed)
    } catch {
      return 0
    }
  }, [])

  /** 收到 429 时执行的逻辑 */
  const handleBan = useCallback((retryAfter: number) => {
    // 记录拉黑开始时间和时长（只在首次记录）
    try {
      const existing = localStorage.getItem('ip_ban_start')
      if (!existing) {
        localStorage.setItem('ip_ban_start', String(Date.now()))
        localStorage.setItem('ip_ban_duration', String(retryAfter))
      }
    } catch {
      // localStorage 不可用时忽略
    }

    // 立即清空 body 内容，隐藏所有可能存在的错误提示
    document.body.innerHTML = ''
    document.body.style.margin = '0'
    document.body.style.overflow = 'hidden'

    setRetryAfterSeconds(calcRemaining() || retryAfter)
    setBanned(true)
  }, [calcRemaining])

  // 页面加载时检查 localStorage 中是否有未过期的拉黑
  useEffect(() => {
    const remaining = calcRemaining()
    if (remaining > 0) {
      handleBan(remaining)
    }
  }, [calcRemaining, handleBan])

  // 拦截全局 fetch 检测 429
  useEffect(() => {
    if (patchedRef.current) return
    patchedRef.current = true

    const originalFetch = window.fetch

    window.fetch = async (...args) => {
      const response = await originalFetch(...args)

      if (response.status === 429) {
        try {
          const data = await response.clone().json()
          const retryAfter = data.retryAfterSeconds || 300
          handleBan(retryAfter)
        } catch {
          handleBan(300)
        }
      }

      return response
    }

    return () => {
      window.fetch = originalFetch
      patchedRef.current = false
    }
  }, [handleBan])

  // 倒计时
  useEffect(() => {
    if (!banned) return

    const timer = setInterval(() => {
      setRetryAfterSeconds((prev) => {
        if (prev <= 1) {
          // 倒计时结束，清除 localStorage
          try {
            localStorage.removeItem('ip_ban_start')
            localStorage.removeItem('ip_ban_duration')
          } catch {
            // ignore
          }
          setBanned(false)
          return 0
        }
        return prev - 1
      })
    }, 1000)

    return () => clearInterval(timer)
  }, [banned])

  /** 格式化秒数为 X 分 X 秒 */
  function formatTime(seconds: number): string {
    const m = Math.floor(seconds / 60)
    const s = seconds % 60
    if (m > 0) return `${m} 分 ${s} 秒`
    return `${s} 秒`
  }

  /** 刷新重试 */
  function handleRetry() {
    try {
      localStorage.removeItem('ip_ban_start')
      localStorage.removeItem('ip_ban_duration')
    } catch {
      // ignore
    }
    window.location.reload()
  }

  if (!banned) return null

  return (
    <div
      id="ip-ban-overlay"
      style={{
        position: 'fixed',
        inset: 0,
        zIndex: 999999,
        backgroundColor: '#ffffff',
        display: 'flex',
        alignItems: 'center',
        justifyContent: 'center',
        fontFamily: 'system-ui, -apple-system, BlinkMacSystemFont, "Segoe UI", Roboto, sans-serif',
        color: '#333',
        padding: '24px',
      }}
    >
      <div style={{ maxWidth: '400px', width: '100%', textAlign: 'center' }}>
        {/* 警告标志 */}
        <div
          style={{
            width: '64px',
            height: '64px',
            margin: '0 auto 20px',
            borderRadius: '50%',
            backgroundColor: '#fef2f2',
            display: 'flex',
            alignItems: 'center',
            justifyContent: 'center',
            fontSize: '28px',
            color: '#dc2626',
            fontWeight: 700,
          }}
        >
          !
        </div>

        {/* 标题 */}
        <h1
          style={{
            fontSize: '22px',
            fontWeight: 700,
            margin: '0 0 10px',
            color: '#111827',
          }}
        >
          访问太频繁
        </h1>

        {/* 说明文字 */}
        <p
          style={{
            fontSize: '14px',
            color: '#6b7280',
            margin: '0 0 24px',
            lineHeight: 1.7,
          }}
        >
          当前 IP 请求过于频繁，已被暂时限制访问。
          <br />
          请稍后刷新页面重试。
        </p>

        {/* 倒计时 */}
        {retryAfterSeconds > 0 && (
          <div
            style={{
              fontSize: '40px',
              fontWeight: 700,
              color: '#dc2626',
              marginBottom: '28px',
              fontVariantNumeric: 'tabular-nums',
              letterSpacing: '2px',
            }}
          >
            {formatTime(retryAfterSeconds)}
          </div>
        )}

        {/* 刷新重试按钮 */}
        <button
          onClick={handleRetry}
          style={{
            padding: '12px 40px',
            fontSize: '15px',
            fontWeight: 600,
            color: '#fff',
            backgroundColor: '#2563eb',
            border: 'none',
            borderRadius: '8px',
            cursor: 'pointer',
            outline: 'none',
          }}
          onMouseOver={(e) => (e.currentTarget.style.backgroundColor = '#1d4ed8')}
          onMouseOut={(e) => (e.currentTarget.style.backgroundColor = '#2563eb')}
        >
          刷新重试
        </button>
      </div>
    </div>
  )
}
