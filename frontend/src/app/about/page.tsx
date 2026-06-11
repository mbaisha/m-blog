import type { Metadata } from "next"
import { apiClient, fetchSeoSettings, fetchPageLayout } from "@/lib/api"
import type { ApiResponse, PublicProfileSection, PublicModuleLayout } from "@/types"
import VisitTracker from "@/components/VisitTracker"
import LayoutUniversalModules from "@/components/LayoutUniversalModules"

/** 生成元数据（从后台 SEO 配置获取） */
export async function generateMetadata(): Promise<Metadata> {
  const seo = await fetchSeoSettings("about")
  return {
    title: seo?.title || "关于我",
    description: seo?.description || "了解更多关于我的信息",
    keywords: seo?.keywords || undefined,
  }
}

/** 渲染模块内容 */
function renderSection(section: PublicProfileSection) {
  const sectionStyle = {
    padding: '20px',
    borderRadius: 'var(--radius-lg)',
    border: '0.5px solid var(--color-border)',
    backgroundColor: 'var(--card-bg)',
  }

  switch (section.sectionType) {
    case "hero":
      return (
        <section className="text-center py-16 max-w-2xl mx-auto">
          <h1
            className="text-[24px] md:text-[28px] font-semibold mb-4"
            style={{ color: 'var(--color-text-primary)' }}
          >
            {section.title || "关于我"}
          </h1>
          {section.content && (
            <div
              className="text-[14px] leading-relaxed whitespace-pre-wrap"
              style={{ color: 'var(--color-text-secondary)' }}
            >
              {section.content}
            </div>
          )}
        </section>
      )

    case "intro":
      return (
        <section className="mb-8" style={sectionStyle}>
          {section.title && (
            <h2
              className="text-[15px] font-medium mb-4"
              style={{ color: 'var(--color-text-primary)' }}
            >
              {section.title || "个人简介"}
            </h2>
          )}
          <div
            className="text-[14px] leading-relaxed whitespace-pre-wrap"
            style={{ color: 'var(--color-text-secondary)' }}
          >
            {section.content}
          </div>
        </section>
      )

    case "skills":
      return (
        <section className="mb-8" style={sectionStyle}>
          {section.title && (
            <h2
              className="text-[15px] font-medium mb-4"
              style={{ color: 'var(--color-text-primary)' }}
            >
              {section.title || "技能特长"}
            </h2>
          )}
          <div
            className="text-[14px] leading-relaxed whitespace-pre-wrap"
            style={{ color: 'var(--color-text-secondary)' }}
          >
            {section.content}
          </div>
        </section>
      )

    case "experience":
      return (
        <section className="mb-8" style={sectionStyle}>
          {section.title && (
            <h2
              className="text-[15px] font-medium mb-4"
              style={{ color: 'var(--color-text-primary)' }}
            >
              {section.title || "工作经历"}
            </h2>
          )}
          <div
            className="text-[14px] leading-relaxed whitespace-pre-wrap"
            style={{ color: 'var(--color-text-secondary)' }}
          >
            {section.content}
          </div>
        </section>
      )

    case "contact":
      return (
        <section className="mb-8" style={sectionStyle}>
          {section.title && (
            <h2
              className="text-[15px] font-medium mb-4"
              style={{ color: 'var(--color-text-primary)' }}
            >
              {section.title || "联系方式"}
            </h2>
          )}
          <div
            className="text-[14px] leading-relaxed whitespace-pre-wrap"
            style={{ color: 'var(--color-text-secondary)' }}
          >
            {section.content}
          </div>
        </section>
      )

    default:
      return (
        <section className="mb-8" style={sectionStyle}>
          {section.title && (
            <h2
              className="text-[15px] font-medium mb-4"
              style={{ color: 'var(--color-text-primary)' }}
            >
              {section.title}
            </h2>
          )}
          {section.content && (
            <div
              className="text-[14px] leading-relaxed whitespace-pre-wrap"
              style={{ color: 'var(--color-text-secondary)' }}
            >
              {section.content}
            </div>
          )}
        </section>
      )
  }
}

export default async function AboutPage() {
  let sections: PublicProfileSection[] = []
  let layoutModules: PublicModuleLayout[] = []

  try {
    const [res, layoutRes] = await Promise.all([
      apiClient.get<ApiResponse<PublicProfileSection[]>>("/profile"),
      fetchPageLayout("about").catch(() => null),
    ])
    sections = res.data || []
    layoutModules = layoutRes?.data || []
  } catch {
    // 后端未部署或个人页面未配置时使用默认内容
  }

  const hasContent = sections.length > 0
  const moduleMap = new Map(layoutModules.map(m => [m.moduleKey, m]))
  const isEnabled = (key: string) => {
    const mod = moduleMap.get(key)
    return mod ? mod.isEnabled : true
  }

  // 从 page_hero 获取自定义标题
  const heroMod = layoutModules.find(m => m.moduleKey === 'page_hero' && m.isEnabled)
  let heroTitle = '关于我', heroSubtitle = '了解更多关于我的信息'
  if (heroMod?.config) {
    try { const c = JSON.parse(heroMod.config); heroSubtitle = c.customSubtitle || heroSubtitle } catch { /* ignore */ }
  }
  if (heroMod?.title) heroTitle = heroMod.title

  return (
    <div className="content-container py-12">
      {/* page_hero */}
      {isEnabled('page_hero') && (
      <section className="mb-8">
        <h1 className="text-[20px] font-medium" style={{ color: 'var(--color-text-primary)' }}>
          {heroTitle}
        </h1>
        <p className="text-[12px] mt-1" style={{ color: 'var(--color-text-secondary)' }}>
          {heroSubtitle}
        </p>
      </section>
      )}

      {/* profile_sections */}
      {isEnabled('profile_sections') && (

      <div className="max-w-[800px] mx-auto" style={{ borderTop: '0.5px solid var(--color-border)', paddingTop: '2em' }}>
        {hasContent ? (
          sections
            .filter((s) => s.isEnabled)
            .sort((a, b) => a.sortOrder - b.sortOrder)
            .map((section) => (
              <div key={section.id}>
                {renderSection(section)}
              </div>
            ))
        ) : (
          <>
            {/* 默认 Hero */}
            <section className="text-center py-12 max-w-2xl mx-auto">
              <h1 className="text-[24px] font-semibold mb-4" style={{ color: 'var(--color-text-primary)' }}>
                关于我
              </h1>
              <p className="text-[14px]" style={{ color: 'var(--color-text-secondary)' }}>
                你好！欢迎来到我的个人博客。
              </p>
            </section>

            {/* 默认简介 */}
            <section
              className="mb-8 p-5 rounded-[12px]"
              style={{
                border: '0.5px solid var(--color-border)',
                backgroundColor: 'var(--card-bg)',
              }}
            >
              <h2 className="text-[15px] font-medium mb-4" style={{ color: 'var(--color-text-primary)' }}>
                个人简介
              </h2>
              <p className="text-[14px] leading-relaxed" style={{ color: 'var(--color-text-secondary)' }}>
                我是一名全栈开发者，热衷于技术探索和知识分享。这个博客记录了我在技术路上的学习心得、项目经验和个人思考。
              </p>
            </section>

            {/* 默认技能 */}
            <section
              className="mb-8 p-5 rounded-[12px]"
              style={{
                border: '0.5px solid var(--color-border)',
                backgroundColor: 'var(--card-bg)',
              }}
            >
              <h2 className="text-[15px] font-medium mb-4" style={{ color: 'var(--color-text-primary)' }}>
                技能特长
              </h2>
              <div className="flex flex-wrap gap-2">
                {["TypeScript", "React", "Vue", "Node.js", "ASP.NET Core", "PostgreSQL", "Docker", "Git"].map((skill) => (
                  <span
                    key={skill}
                    className="px-3 py-1 rounded-[6px] text-[12px] font-medium"
                    style={{
                      backgroundColor: 'var(--color-border-light)',
                      color: 'var(--color-text-secondary)',
                    }}
                  >
                    {skill}
                  </span>
                ))}
              </div>
            </section>

            {/* 默认联系 */}
            <section
              className="mb-8 p-5 rounded-[12px]"
              style={{
                border: '0.5px solid var(--color-border)',
                backgroundColor: 'var(--card-bg)',
              }}
            >
              <h2 className="text-[15px] font-medium mb-4" style={{ color: 'var(--color-text-primary)' }}>
                联系我
              </h2>
              <p className="text-[14px]" style={{ color: 'var(--color-text-secondary)' }}>
                如果你对我的项目或文章有任何想法，欢迎通过评论区与我交流。
              </p>
            </section>
          </>
        )}
      </div>
      )}
      <LayoutUniversalModules pageKey="about" />
      <VisitTracker pagePath="/about" />
    </div>
  )
}