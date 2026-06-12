import { ImageResponse } from 'next/og'
import { apiClient } from '@/lib/api'
import type { ApiResponse, PublicArticleDetail } from '@/types'

export const runtime = 'edge'
export const alt = '文章'
export const size = { width: 1200, height: 630 }
export const contentType = 'image/png'

export default async function Image({ params }: { params: Promise<{ slug: string }> }) {
  const { slug } = await params

  let title = '文章详情'
  let author = ''
  let publishedAt = ''
  let categoryName = ''

  try {
    const res = await apiClient.get<ApiResponse<PublicArticleDetail>>(`/articles/${slug}`)
    const article = res.data
    title = article.title
    author = article.author?.username || ''
    publishedAt = article.publishedAt
      ? new Date(article.publishedAt).toLocaleDateString('zh-CN', { year: 'numeric', month: 'long', day: 'numeric' })
      : ''
    categoryName = article.category?.name || article.categories?.[0]?.name || ''
  } catch {
    // 降级使用默认信息
  }

  return new ImageResponse(
    (
      <div
        style={{
          width: '100%',
          height: '100%',
          display: 'flex',
          flexDirection: 'column',
          justifyContent: 'center',
          background: 'linear-gradient(135deg, #1e293b 0%, #334155 50%, #1e293b 100%)',
          fontFamily: 'sans-serif',
          padding: 80,
          position: 'relative',
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
            background: 'linear-gradient(90deg, #6366f1, #a855f7, #ec4899)',
          }}
        />
        {/* 分类标签 */}
        {categoryName && (
          <div
            style={{
              display: 'flex',
              marginBottom: 32,
            }}
          >
            <span
              style={{
                fontSize: 24,
                color: '#a78bfa',
                background: 'rgba(167,139,250,0.15)',
                padding: '8px 24px',
                borderRadius: 12,
                fontWeight: 600,
              }}
            >
              {categoryName}
            </span>
          </div>
        )}
        {/* 标题 */}
        <h1
          style={{
            fontSize: 56,
            fontWeight: 800,
            color: '#f1f5f9',
            margin: 0,
            lineHeight: 1.2,
            maxWidth: 1000,
          }}
        >
          {title}
        </h1>
        {/* 底部信息 */}
        <div
          style={{
            display: 'flex',
            alignItems: 'center',
            gap: 24,
            marginTop: 48,
            position: 'absolute',
            bottom: 80,
            left: 80,
          }}
        >
          {/* 作者头像占位 */}
          <div
            style={{
              width: 48,
              height: 48,
              borderRadius: '50%',
              background: 'linear-gradient(135deg, #6366f1, #a855f7)',
              display: 'flex',
              alignItems: 'center',
              justifyContent: 'center',
              fontSize: 24,
              color: '#fff',
              fontWeight: 700,
            }}
          >
            {author?.charAt(0)?.toUpperCase() || 'A'}
          </div>
          <div style={{ display: 'flex', flexDirection: 'column', gap: 4 }}>
            <span style={{ fontSize: 22, color: '#e2e8f0', fontWeight: 600 }}>
              {author || 'Admin'}
            </span>
            {publishedAt && (
              <span style={{ fontSize: 18, color: '#94a3b8' }}>
                {publishedAt}
              </span>
            )}
          </div>
        </div>
      </div>
    ),
    { width: 1200, height: 630 }
  )
}