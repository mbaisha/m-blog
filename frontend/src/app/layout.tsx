import type { Metadata } from "next";
import { Geist, Geist_Mono } from "next/font/google";
import DynamicNavbar from "@/components/DynamicNavbar";
import DynamicFooter from "@/components/DynamicFooter";
import ThemeProvider from "@/components/ThemeProvider";
import { DarkModeProvider } from "@/components/DarkModeProvider";
import MobileBottomNav from "@/components/MobileBottomNav";
import PwaRegistration from "@/components/PwaRegistration";
import IpBanOverlay from "@/components/IpBanOverlay";
import { fetchSiteSettings } from "@/lib/api";
import { getServerSiteUrl } from "@/lib/runtimeConfig";
import "./globals.css";

const geistSans = Geist({
  variable: "--font-geist-sans",
  subsets: ["latin"],
});

const geistMono = Geist_Mono({
  variable: "--font-geist-mono",
  subsets: ["latin"],
});

/** 站点 URL（运行时从 .env / docker-compose 注入） */
const siteUrl = getServerSiteUrl();

const fallbackName = "个人博客";
const fallbackDesc = "一个现代个人博客，分享技术实践、产品思考与项目复盘";

/** 从后端加载站点设置生成元数据 */
export async function generateMetadata(): Promise<Metadata> {
  const settings = await fetchSiteSettings()
  const siteName = settings?.siteName || fallbackName
  const description = settings?.siteDescription || fallbackDesc

  return {
    title: {
      default: siteName,
      template: `%s | ${siteName}`,
    },
    description,
    metadataBase: new URL(siteUrl),
    manifest: "/manifest.json",
    icons: settings?.faviconImageUrl
      ? { icon: settings.faviconImageUrl, shortcut: settings.faviconImageUrl }
      : undefined,
    alternates: {
      types: {
        'application/rss+xml': `${siteUrl}/feed`,
      },
    },
    other: {
      'theme-color': '#6366f1',
      'apple-mobile-web-app-capable': 'yes',
      'apple-mobile-web-app-status-bar-style': 'default',
      'apple-mobile-web-app-title': siteName,
    },
    openGraph: {
      type: "website",
      siteName,
      title: siteName,
      description,
      images: settings?.logoImageUrl ? [{ url: settings.logoImageUrl }] : undefined,
    },
    twitter: {
      card: "summary_large_image",
      title: siteName,
      description,
      images: settings?.logoImageUrl ? [settings.logoImageUrl] : undefined,
    },
    robots: {
      index: true,
      follow: true,
    },
  };
}

export default async function RootLayout({
  children,
}: Readonly<{
  children: React.ReactNode;
}>) {
  const settings = await fetchSiteSettings()
  const siteName = settings?.siteName || fallbackName
  const description = settings?.siteDescription || fallbackDesc

  return (
    <html
      lang="zh-CN"
      className={`${geistSans.variable} ${geistMono.variable} h-full antialiased`}
    >
      <head>
        {/* 运行时配置：将服务端环境变量注入到 window.__APP_CONFIG__ 供客户端读取 */}
        <script
          // 由 .env / docker-compose 在容器启动时注入；构建时为空字符串
          dangerouslySetInnerHTML={{
            __html: `window.__APP_CONFIG__ = ${JSON.stringify({
              SITE_URL: process.env.SITE_URL || '',
              API_BASE_URL: process.env.API_BASE_URL || '',
              SITE_NAME: process.env.SITE_NAME || '',
            })};`,
          }}
        />
        {/* 9.3 JSON-LD 结构化数据 — Website */}
        <script
          type="application/ld+json"
          dangerouslySetInnerHTML={{
            __html: JSON.stringify({
              '@context': 'https://schema.org',
              '@type': 'WebSite',
              name: siteName,
              url: siteUrl,
              description,
              potentialAction: {
                '@type': 'SearchAction',
                target: `${siteUrl}/search?q={search_term_string}`,
                'query-input': 'required name=search_term_string',
              },
            }),
          }}
        />
      </head>
      <body className="min-h-full flex flex-col" style={{ backgroundColor: 'var(--theme-bg, #f8fafc)' }}>
        <DarkModeProvider>
          <ThemeProvider>
            <DynamicNavbar />
            <main className="flex-1">{children}</main>
            <DynamicFooter />
            <MobileBottomNav />
            <PwaRegistration />
            <IpBanOverlay />
          </ThemeProvider>
        </DarkModeProvider>
      </body>
    </html>
  );
}