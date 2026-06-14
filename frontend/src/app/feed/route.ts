import { apiClient, fetchSiteSettings } from '@/lib/api'
import type { ApiResponse, PagedData, PublicArticleListItem } from '@/types'
import { getServerSiteUrl } from '@/lib/runtimeConfig'

const SITE_URL = getServerSiteUrl()

// 将相对路径图片补全为完整 URL
function toFullUrl(path: string | null | undefined): string | null {
  if (!path) return null
  if (path.startsWith('http://') || path.startsWith('https://')) return path
  return `${SITE_URL}${path.startsWith('/') ? '' : '/'}${path}`
}

export async function GET() {
  try {
    const [articlesRes, settings] = await Promise.all([
      apiClient.get<ApiResponse<PagedData<PublicArticleListItem>>>('/articles', {
        page: 1,
        pageSize: 50,
        sortBy: 'latest',
      }),
      fetchSiteSettings().catch(() => null),
    ])

    const articles = articlesRes.data.items || []
    const siteName = settings?.siteName || '个人博客'
    const siteDescription = settings?.siteDescription || ''

    const rssItems = articles
      .map((article) => {
        const publishedAt = article.publishedAt
          ? new Date(article.publishedAt).toUTCString()
          : new Date().toUTCString()
        const url = `${SITE_URL}/articles/${article.slug}`
        const description = article.summary || ''
        const categories = article.categories?.length > 0
          ? article.categories.map((c) => `<category>${c.name}</category>`).join('\n      ')
          : ''
        const fullCoverUrl = toFullUrl(article.coverImageUrl)
        const coverImage = fullCoverUrl
          ? `<enclosure url="${fullCoverUrl}" type="image/jpeg" />`
          : ''

        return `    <item>
      <title><![CDATA[${article.title}]]></title>
      <link>${url}</link>
      <guid isPermaLink="true">${url}</guid>
      <pubDate>${publishedAt}</pubDate>
      <description><![CDATA[${description}]]></description>
      ${categories}
      ${coverImage}
      <author>${article.authorName || 'Admin'}</author>
    </item>`
      })
      .join('\n')

    const rss = `<?xml version="1.0" encoding="UTF-8"?>
<rss version="2.0"
  xmlns:atom="http://www.w3.org/2005/Atom"
  xmlns:content="http://purl.org/rss/1.0/modules/content/">
  <channel>
    <title><![CDATA[${siteName}]]></title>
    <link>${SITE_URL}</link>
    <description><![CDATA[${siteDescription}]]></description>
    <language>zh-CN</language>
    <lastBuildDate>${new Date().toUTCString()}</lastBuildDate>
    <atom:link href="${SITE_URL}/feed" rel="self" type="application/rss+xml" />
    <generator>m-blog</generator>
${rssItems}
  </channel>
</rss>`

    return new Response(rss, {
      headers: {
        'Content-Type': 'application/rss+xml; charset=utf-8',
        'Cache-Control': 'public, max-age=3600, s-maxage=3600',
      },
    })
  } catch {
    // 降级：返回空 feed
    const rss = `<?xml version="1.0" encoding="UTF-8"?>
<rss version="2.0" xmlns:atom="http://www.w3.org/2005/Atom">
  <channel>
    <title>个人博客</title>
    <link>${SITE_URL}</link>
    <description>博客 RSS 订阅</description>
    <language>zh-CN</language>
    <atom:link href="${SITE_URL}/feed" rel="self" type="application/rss+xml" />
  </channel>
</rss>`

    return new Response(rss, {
      headers: {
        'Content-Type': 'application/rss+xml; charset=utf-8',
      },
    })
  }
}
