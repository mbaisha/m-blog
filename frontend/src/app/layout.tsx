import type { Metadata } from "next";
import { Geist, Geist_Mono } from "next/font/google";
import DynamicNavbar from "@/components/DynamicNavbar";
import DynamicFooter from "@/components/DynamicFooter";
import ThemeProvider from "@/components/ThemeProvider";
import { DarkModeProvider } from "@/components/DarkModeProvider";
import MobileBottomNav from "@/components/MobileBottomNav";
import { fetchSiteSettings } from "@/lib/api";
import "./globals.css";

const geistSans = Geist({
  variable: "--font-geist-sans",
  subsets: ["latin"],
});

const geistMono = Geist_Mono({
  variable: "--font-geist-mono",
  subsets: ["latin"],
});

const siteUrl = process.env.NEXT_PUBLIC_SITE_URL || "http://localhost:3000";

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
    icons: settings?.faviconImageUrl
      ? { icon: settings.faviconImageUrl, shortcut: settings.faviconImageUrl }
      : undefined,
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
          </ThemeProvider>
        </DarkModeProvider>
      </body>
    </html>
  );
}