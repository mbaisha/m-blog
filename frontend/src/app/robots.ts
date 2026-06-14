import { MetadataRoute } from 'next'

import { getServerSiteUrl } from '@/lib/runtimeConfig'

const BASE_URL = getServerSiteUrl()

/**
 * 9.2 自动生成 Robots.txt
 */
export default function robots(): MetadataRoute.Robots {
  return {
    rules: [
      {
        userAgent: '*',
        allow: '/',
        disallow: ['/api/', '/_next/'],
      },
    ],
    sitemap: `${BASE_URL}/sitemap.xml`,
  }
}