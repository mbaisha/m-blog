import { ImageResponse } from 'next/og'
import { fetchSiteSettings } from '@/lib/api'

export const runtime = 'edge'
export const alt = '个人博客'
export const size = { width: 1200, height: 630 }
export const contentType = 'image/png'

export default async function Image() {
  const settings = await fetchSiteSettings().catch(() => null)
  const siteName = settings?.siteName || '个人博客'
  const description = settings?.siteDescription || '分享技术实践、产品思考与项目复盘'
  const logoUrl = settings?.logoImageUrl

  return new ImageResponse(
    (
      <div
        style={{
          width: '100%',
          height: '100%',
          display: 'flex',
          flexDirection: 'column',
          alignItems: 'center',
          justifyContent: 'center',
          background: 'linear-gradient(135deg, #6366f1 0%, #a855f7 50%, #ec4899 100%)',
          fontFamily: 'sans-serif',
          padding: 80,
        }}
      >
        {/* 顶部装饰线 */}
        <div
          style={{
            position: 'absolute',
            top: 0,
            left: 0,
            right: 0,
            height: 6,
            background: 'linear-gradient(90deg, #f59e0b, #ef4444, #a855f7, #3b82f6, #10b981)',
          }}
        />
        {/* 底部装饰线 */}
        <div
          style={{
            position: 'absolute',
            bottom: 0,
            left: 0,
            right: 0,
            height: 6,
            background: 'linear-gradient(90deg, #10b981, #3b82f6, #a855f7, #ef4444, #f59e0b)',
          }}
        />
        {/* Logo */}
        {logoUrl && (
          <img
            src={logoUrl}
            alt="Logo"
            width={120}
            height={120}
            style={{ borderRadius: 24, marginBottom: 40 }}
          />
        )}
        {/* 标题 */}
        <h1
          style={{
            fontSize: 72,
            fontWeight: 800,
            color: '#ffffff',
            textAlign: 'center',
            margin: 0,
            lineHeight: 1.1,
            textShadow: '0 2px 20px rgba(0,0,0,0.2)',
          }}
        >
          {siteName}
        </h1>
        {/* 描述 */}
        <p
          style={{
            fontSize: 32,
            color: 'rgba(255,255,255,0.85)',
            textAlign: 'center',
            marginTop: 24,
            marginBottom: 0,
            maxWidth: 900,
            lineHeight: 1.4,
          }}
        >
          {description}
        </p>
      </div>
    ),
    { width: 1200, height: 630 }
  )
}