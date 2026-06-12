'use client'

import { useState } from 'react'

interface Props {
  url: string
  title: string
}

export default function ShareTools({ url, title }: Props) {
  const [copied, setCopied] = useState(false)
  const [showQr, setShowQr] = useState(false)
  const [qrLoaded, setQrLoaded] = useState(false)

  const copyLink = async () => {
    try {
      await navigator.clipboard.writeText(url)
    } catch {
      // Fallback
      const textarea = document.createElement('textarea')
      textarea.value = url
      textarea.style.position = 'fixed'
      textarea.style.opacity = '0'
      document.body.appendChild(textarea)
      textarea.select()
      document.execCommand('copy')
      document.body.removeChild(textarea)
    }
    setCopied(true)
    setTimeout(() => setCopied(false), 2000)
  }

  const share = async () => {
    if (navigator.share) {
      try {
        await navigator.share({ title, url })
      } catch {
        // User cancelled
      }
    } else {
      copyLink()
    }
  }

  /** 添加到浏览器收藏夹 */
  const handleBookmark = () => {
    const u = url
    const t = title
    // Firefox
    if (typeof (window as any).sidebar !== 'undefined' && typeof (window as any).sidebar.addPanel === 'function') {
      (window as any).sidebar.addPanel(t, u, '')
    // IE / Edge legacy
    } else if (typeof (window as any).external !== 'undefined' && typeof (window as any).external.AddFavorite === 'function') {
      (window.external as any).AddFavorite(u, t)
    } else {
      // Chrome / Safari / modern browsers (Ctrl+D / Cmd+D)
      alert('请按 Ctrl+D (Windows) 或 ⌘+D (Mac) 将本页添加到浏览器收藏夹')
    }
  }

  /** 二维码图片 URL — 使用当前页面真实地址，不依赖构建时传入的 url prop */
  const qrUrl = `https://api.qrserver.com/v1/create-qr-code/?size=200x200&data=${encodeURIComponent(typeof window !== 'undefined' ? window.location.href : url)}`

  return (
    <div>
      <h4 className="text-[13px] font-medium mb-3" style={{ color: 'var(--color-text-primary)' }}>
        操作
      </h4>
      <div className="flex flex-wrap gap-2">
        {/* 分享 */}
        <button
          onClick={share}
          className="flex items-center justify-center w-[42px] h-[34px] rounded-[8px] text-[12px] transition-colors duration-150"
          style={{
            border: '0.5px solid var(--color-border)',
            color: 'var(--color-text-secondary)',
            backgroundColor: 'transparent',
          }}
          onMouseOver={(e) => { e.currentTarget.style.color = 'var(--color-primary)'; e.currentTarget.style.borderColor = 'var(--color-primary)' }}
          onMouseOut={(e) => { e.currentTarget.style.color = 'var(--color-text-secondary)'; e.currentTarget.style.borderColor = 'var(--color-border)' }}
          title="分享"
        >
          <svg className="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M8.684 13.342C8.886 12.938 9 12.482 9 12c0-.482-.114-.938-.316-1.342m0 2.684a3 3 0 110-2.684m0 2.684l6.632 3.316m-6.632-6l6.632-3.316m0 0a3 3 0 105.367-2.684 3 3 0 00-5.367 2.684zm0 9.316a3 3 0 105.368 2.684 3 3 0 00-5.368-2.684z" />
          </svg>
        </button>

        {/* 收藏 */}
        <button
          onClick={handleBookmark}
          className="flex items-center justify-center w-[42px] h-[34px] rounded-[8px] text-[12px] transition-colors duration-150"
          style={{
            border: '0.5px solid var(--color-border)',
            color: 'var(--color-text-secondary)',
            backgroundColor: 'transparent',
          }}
          onMouseOver={(e) => { e.currentTarget.style.color = 'var(--color-primary)'; e.currentTarget.style.borderColor = 'var(--color-primary)' }}
          onMouseOut={(e) => { e.currentTarget.style.color = 'var(--color-text-secondary)'; e.currentTarget.style.borderColor = 'var(--color-border)' }}
          title="收藏到浏览器"
        >
          <svg className="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M5 5a2 2 0 012-2h10a2 2 0 012 2v16l-7-3.5L5 21V5z" />
          </svg>
        </button>

        {/* 复制链接 */}
        <button
          onClick={copyLink}
          className="flex items-center justify-center w-[42px] h-[34px] rounded-[8px] text-[12px] transition-colors duration-150"
          style={{
            border: '0.5px solid var(--color-border)',
            color: copied ? 'var(--color-primary)' : 'var(--color-text-secondary)',
            backgroundColor: copied ? 'var(--color-primary-light)' : 'transparent',
          }}
          onMouseOver={(e) => { if (!copied) { e.currentTarget.style.color = 'var(--color-primary)'; e.currentTarget.style.borderColor = 'var(--color-primary)' } }}
          onMouseOut={(e) => { if (!copied) { e.currentTarget.style.color = 'var(--color-text-secondary)'; e.currentTarget.style.borderColor = 'var(--color-border)' } }}
          title="复制链接"
        >
          {copied ? (
            <svg className="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M5 13l4 4L19 7" />
            </svg>
          ) : (
            <svg className="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M13.828 10.172a4 4 0 00-5.656 0l-4 4a4 4 0 105.656 5.656l1.102-1.101m-.758-4.899a4 4 0 005.656 0l4-4a4 4 0 00-5.656-5.656l-1.1 1.1" />
            </svg>
          )}
        </button>

        {/* 二维码 */}
        <div
          className="relative"
          onMouseEnter={() => { setShowQr(true); setQrLoaded(false) }}
          onMouseLeave={() => { setShowQr(false); setQrLoaded(false) }}
        >
          <button
            className="flex items-center justify-center w-[42px] h-[34px] rounded-[8px] text-[12px] transition-colors duration-150"
            style={{
              border: '0.5px solid var(--color-border)',
              color: 'var(--color-text-secondary)',
              backgroundColor: 'transparent',
            }}
            onMouseOver={(e) => { e.currentTarget.style.color = 'var(--color-primary)'; e.currentTarget.style.borderColor = 'var(--color-primary)' }}
            onMouseOut={(e) => { e.currentTarget.style.color = 'var(--color-text-secondary)'; e.currentTarget.style.borderColor = 'var(--color-border)' }}
            title="二维码"
          >
            <svg className="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 4v1m6 11h2m-6 0h-2v4m0-11v3m0 0h.01M12 12h4.01M16 20h4M4 12h4m12 0h.01M5 8h2a1 1 0 001-1V5a1 1 0 00-1-1H5a1 1 0 00-1 1v2a1 1 0 001 1zm12 0h2a1 1 0 001-1V5a1 1 0 00-1-1h-2a1 1 0 00-1 1v2a1 1 0 001 1zM5 20h2a1 1 0 001-1v-2a1 1 0 00-1-1H5a1 1 0 00-1 1v2a1 1 0 001 1z" />
            </svg>
          </button>

          {/* QR 码弹出层 */}
          {showQr && (
            <div
              className="absolute z-50"
              style={{
                top: '100%',
                left: '50%',
                transform: 'translateX(-80%)',
                marginTop: '8px',
                padding: '12px',
                background: '#fff',
                borderRadius: '8px',
                boxShadow: '0 4px 20px rgba(0,0,0,0.15)',
              }}
            >
              {!qrLoaded && (
                <div
                  className="animate-pulse"
                  style={{
                    width: 200,
                    height: 200,
                    display: 'flex',
                    flexDirection: 'column',
                    alignItems: 'center',
                    justifyContent: 'center',
                    background: '#f5f5f5',
                    borderRadius: '4px',
                  }}
                >
                  <svg className="w-8 h-8 mb-2" fill="none" stroke="#ccc" viewBox="0 0 24 24">
                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={1.5} d="M12 4v1m6 11h2m-6 0h-2v4m0-11v3m0 0h.01M12 12h4.01M16 20h4M4 12h4m12 0h.01M5 8h2a1 1 0 001-1V5a1 1 0 00-1-1H5a1 1 0 00-1 1v2a1 1 0 001 1zm12 0h2a1 1 0 001-1V5a1 1 0 00-1-1h-2a1 1 0 00-1 1v2a1 1 0 001 1zM5 20h2a1 1 0 001-1v-2a1 1 0 00-1-1H5a1 1 0 00-1 1v2a1 1 0 001 1z" />
                  </svg>
                  <span style={{ fontSize: '11px', color: '#aaa' }}>加载中...</span>
                </div>
              )}
              <img
                src={qrUrl}
                alt="文章二维码"
                width={200}
                height={200}
                style={{ display: !qrLoaded ? 'none' : 'block', borderRadius: '4px', maxWidth: 'none' }}
                onLoad={() => setQrLoaded(true)}
              />
              <div style={{ fontSize: '10px', color: '#999', textAlign: 'center', marginTop: '4px' }}>
                扫码阅读
              </div>
            </div>
          )}
        </div>
      </div>
    </div>
  )
}