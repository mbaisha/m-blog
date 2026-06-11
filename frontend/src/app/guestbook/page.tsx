import type { Metadata } from "next"
import { fetchPageLayout } from "@/lib/api"
import type { PublicModuleLayout } from "@/types"
import VisitTracker from "@/components/VisitTracker"
import LayoutUniversalModules from "@/components/LayoutUniversalModules"
import GuestbookMessageForm from "@/components/GuestbookMessageForm"
import GuestbookMessageList from "@/components/GuestbookMessageList"

export async function generateMetadata(): Promise<Metadata> {
  return {
    title: "留言板",
    description: "欢迎在这里留下你的想法和建议",
  }
}

export default async function GuestbookPage() {
  let layoutModules: PublicModuleLayout[] = []
  try {
    const res = await fetchPageLayout("guestbook")
    layoutModules = res.data || []
  } catch { /* ignore */ }

  const moduleMap = new Map(layoutModules.map(m => [m.moduleKey, m]))
  const isEnabled = (key: string) => {
    const mod = moduleMap.get(key)
    return mod ? mod.isEnabled : true
  }

  // 从 page_hero 获取自定义标题
  const heroMod = layoutModules.find(m => m.moduleKey === 'page_hero' && m.isEnabled)
  let heroTitle = '留言板', heroSubtitle = '欢迎在这里留下你的想法、建议或任何想说的话'
  if (heroMod?.config) {
    try { const c = JSON.parse(heroMod.config); heroSubtitle = c.customSubtitle || heroSubtitle } catch { /* ignore */ }
  }
  if (heroMod?.title) heroTitle = heroMod.title

  // 读取 message_list 的 pageSize 配置
  let pageSize = 20
  try {
    const msgListMod = moduleMap.get('message_list')
    if (msgListMod) {
      const cfg = JSON.parse(msgListMod.config || '{}')
      pageSize = cfg.pageSize ?? 20
    }
  } catch { /* ignore */ }

  return (
    <div className="content-container pt-6 pb-24 md:pb-16">
      {/* ===== 页面标题区 ===== */}
      {isEnabled('page_hero') && (
        <section className="mb-8">
          <h1 className="text-[26px] font-bold mb-2" style={{ color: 'var(--color-text-primary, #1F2937)' }}>
            {heroTitle}
          </h1>
          <p className="text-[14px]" style={{ color: 'var(--color-text-secondary, #6B7280)' }}>
            {heroSubtitle}
          </p>
        </section>
      )}

      {/* ===== 留言表单 ===== */}
      {isEnabled('message_form') && (
        <section className="mb-10">
          <h2 className="text-[16px] font-medium mb-4" style={{ color: 'var(--color-text-primary, #1F2937)' }}>
            写留言
          </h2>
          <GuestbookMessageForm />
          <p className="text-[11px] mt-2" style={{ color: 'var(--color-text-tertiary, #9CA3AF)' }}>
            留言提交后需要审核才能显示，请耐心等待
          </p>
        </section>
      )}

      {/* ===== 留言列表 ===== */}
      {isEnabled('message_list') && (
        <section>
          <h2 className="text-[16px] font-medium mb-4" style={{ color: 'var(--color-text-primary, #1F2937)' }}>
            所有留言
          </h2>
          <GuestbookMessageList pageSize={pageSize} />
        </section>
      )}

      <LayoutUniversalModules pageKey="guestbook" />
      <VisitTracker pagePath="/guestbook" />
    </div>
  )
}