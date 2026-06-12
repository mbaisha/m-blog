import type { Metadata } from "next"
import Link from "next/link"
import Image from "next/image"
import { apiClient, fetchSeoSettings, fetchPageLayout } from "@/lib/api"
import type { ApiResponse, PublicFriendItem, PublicModuleLayout } from "@/types"
import VisitTracker from "@/components/VisitTracker"
import LayoutUniversalModules from "@/components/LayoutUniversalModules"

/** 获取标题第一个有效字符（字母或汉字，字母转为大写，跳过标点） */
function getFirstValidChar(name: string): string {
  for (const ch of name) {
    if (/[a-zA-Z]/.test(ch)) return ch.toUpperCase()
    if (/[\u4e00-\u9fff]/.test(ch)) return ch
  }
  return '?'
}

/** 生成元数据（从后台 SEO 配置获取） */
export async function generateMetadata(): Promise<Metadata> {
  const seo = await fetchSeoSettings("friends")
  return {
    title: seo?.title || "友情链接",
    description: seo?.description || "我的朋友们",
    keywords: seo?.keywords || undefined,
  }
}

export default async function FriendsPage() {
  let friends: PublicFriendItem[] = []
  let layoutModules: PublicModuleLayout[] = []

  try {
    const [res, layoutRes] = await Promise.all([
      apiClient.get<ApiResponse<PublicFriendItem[]>>("/friends"),
      fetchPageLayout("friends").catch(() => null),
    ])
    friends = res.data || []
    layoutModules = layoutRes?.data || []
  } catch {
    // ignore
  }

  const moduleMap = new Map(layoutModules.map(m => [m.moduleKey, m]))
  const isEnabled = (key: string) => {
    const mod = moduleMap.get(key)
    return mod ? mod.isEnabled : true
  }

  // 从 page_hero 获取自定义标题
  const heroMod = layoutModules.find(m => m.moduleKey === 'page_hero' && m.isEnabled)
  let heroTitle = '友情链接', heroSubtitle = '这里是我的一些朋友们'
  if (heroMod?.config) {
    try { const c = JSON.parse(heroMod.config); heroSubtitle = c.customSubtitle || heroSubtitle } catch { /* ignore */ }
  }
  if (heroMod?.title) heroTitle = heroMod.title

  return (
    <div className="content-container py-12">
      {/* page_hero */}
      {isEnabled('page_hero') && (
      <section className="mb-8">
        <h1 className="text-[24px] font-semibold" style={{ color: 'var(--color-text-primary)' }}>
          {heroTitle}
        </h1>
        <p className="text-[14px] mt-2" style={{ color: 'var(--color-text-secondary)' }}>
          {heroSubtitle}
        </p>
      </section>
      )}

      {/* friend_grid */}
      {isEnabled('friend_grid') && (friends.length === 0 ? (
        <div className="flex flex-col items-center justify-center py-16">
          <p className="text-[13px]" style={{ color: 'var(--color-text-tertiary)' }}>暂无友情链接</p>
        </div>
      ) : (
        <div className="grid gap-4 sm:grid-cols-2">
          {friends.map((friend) => (
            <a
              key={friend.id}
              href={friend.url}
              target="_blank"
              rel="noopener noreferrer"
              className="flex items-center gap-4 p-5 rounded-[12px] transition-all hover:-translate-y-0.5"
              style={{
                border: '0.5px solid var(--color-border)',
                backgroundColor: 'var(--card-bg)',
              }}
            >
              {/* Logo */}
              <div
                className="flex-shrink-0 w-11 h-11 rounded-[8px] flex items-center justify-center overflow-hidden"
                style={{ background: 'linear-gradient(135deg, var(--color-primary-light), transparent)' }}
              >
                {friend.logoImageUrl ? (
                  <Image src={friend.logoImageUrl} alt={friend.name} fill className="object-cover" sizes="64px" />
                ) : (
                  <span className="text-lg font-bold" style={{ color: 'var(--color-primary)', opacity: 0.4 }}>
                    {getFirstValidChar(friend.name)}
                  </span>
                )}
              </div>

              {/* Info */}
              <div className="min-w-0 flex-1">
                <h3
                  className="text-[13px] font-medium truncate transition-colors"
                  style={{ color: 'var(--color-text-primary)' }}
                >
                  {friend.name}
                </h3>
                {friend.description && (
                  <p className="text-[11px] mt-0.5 line-clamp-1" style={{ color: 'var(--color-text-tertiary)' }}>
                    {friend.description}
                  </p>
                )}
              </div>

              {/* Arrow */}
              <svg className="w-4 h-4 flex-shrink-0" style={{ color: 'var(--color-text-tertiary)' }} fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M10 6H6a2 2 0 00-2 2v10a2 2 0 002 2h10a2 2 0 002-2v-4M14 4h6m0 0v6m0-6L10 14" />
              </svg>
            </a>
          ))}
        </div>
      ))}
      <LayoutUniversalModules pageKey="friends" />
      <VisitTracker pagePath="/friends" />
    </div>
  )
}