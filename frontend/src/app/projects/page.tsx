import type { Metadata } from "next"
import Link from "next/link"
import Image from "next/image"
import { apiClient, fetchSeoSettings, fetchPageLayout } from "@/lib/api"
import type { ApiResponse, PublicProjectListItem, PublicModuleLayout } from "@/types"
import VisitTracker from "@/components/VisitTracker"
import LayoutUniversalModules from "@/components/LayoutUniversalModules"

/** 获取标题第一个有效字符（字母或汉字，字母转为大写，跳过标点） */
function getFirstValidChar(title: string): string {
  for (const ch of title) {
    if (/[a-zA-Z]/.test(ch)) return ch.toUpperCase()
    if (/[\u4e00-\u9fff]/.test(ch)) return ch
  }
  return '?'
}

/** 生成元数据（从后台 SEO 配置获取） */
export async function generateMetadata(): Promise<Metadata> {
  const seo = await fetchSeoSettings("project")
  return {
    title: seo?.title || "项目",
    description: seo?.description || "浏览我的项目作品集",
    keywords: seo?.keywords || undefined,
  }
}

export default async function ProjectsPage() {
  let projects: PublicProjectListItem[] = []
  let layoutModules: PublicModuleLayout[] = []

  try {
    const [res, layoutRes] = await Promise.all([
      apiClient.get<ApiResponse<PublicProjectListItem[]>>("/projects"),
      fetchPageLayout("projects").catch(() => null),
    ])
    projects = res.data || []
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
  let heroTitle = '项目', heroSubtitle = '这里展示了我参与和独立完成的一些项目'
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

      {/* project_grid */}
      {isEnabled('project_grid') && (projects.length === 0 ? (
        <div className="flex flex-col items-center justify-center py-16">
          <p className="text-[13px]" style={{ color: 'var(--color-text-tertiary)' }}>暂无项目展示</p>
        </div>
      ) : (
        <div className="grid gap-5 md:grid-cols-2 lg:grid-cols-3">
          {projects.map((project) => (
            <Link
              key={project.id}
              href={`/projects/${project.slug}`}
              className="group rounded-[12px] overflow-hidden transition-all hover:-translate-y-0.5"
              style={{
                border: '0.5px solid var(--color-border)',
                backgroundColor: 'var(--card-bg)',
              }}
            >
              {/* Cover - fixed 16:9 */}
              <div className="relative w-full overflow-hidden"
                style={{
                  aspectRatio: '16/9',
                  background: project.coverImageUrl ? undefined : 'linear-gradient(135deg, var(--color-primary-light), transparent)',
                }}
              >
                {project.coverImageUrl ? (
                  <Image src={project.coverImageUrl} alt={project.title} fill className="object-cover" sizes="(max-width: 767px) 100vw, (max-width: 1023px) 50vw, 33vw" />
                ) : (
                  <div className="absolute inset-0 flex items-center justify-center">
                    <span className="text-4xl font-bold" style={{ color: 'var(--color-primary)', opacity: 0.2 }}>
                      {getFirstValidChar(project.title)}
                    </span>
                  </div>
                )}
              </div>

              <div className="p-5">
                <h2 className="text-[15px] font-medium mb-2 transition-colors" style={{ color: 'var(--color-text-primary)' }}>
                  {project.title}
                </h2>
                {project.summary && (
                  <p className="text-[12px] line-clamp-2 mb-3" style={{ color: 'var(--color-text-secondary)' }}>
                    {project.summary}
                  </p>
                )}
                {project.techStack.length > 0 && (
                  <div className="flex flex-wrap gap-1.5 mb-3">
                    {project.techStack.slice(0, 4).map((tech) => (
                      <span
                        key={tech}
                        className="px-2 py-0.5 text-[10px] rounded-[4px]"
                        style={{
                          backgroundColor: 'var(--color-border-light)',
                          color: 'var(--color-text-secondary)',
                        }}
                      >
                        {tech}
                      </span>
                    ))}
                    {project.techStack.length > 4 && (
                      <span className="text-[10px]" style={{ color: 'var(--color-text-tertiary)' }}>
                        +{project.techStack.length - 4}
                      </span>
                    )}
                  </div>
                )}
                {/* Link badges */}
                <div className="flex flex-wrap gap-1.5 mt-auto">
                  {project.projectUrl && (
                    <span className="inline-flex items-center gap-1 px-2 py-0.5 text-[10px] rounded-[4px] font-medium"
                      style={{ backgroundColor: '#dbeafe', color: '#2563eb' }}>
                      <svg className="w-3 h-3" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                        <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M10 6H6a2 2 0 00-2 2v10a2 2 0 002 2h10a2 2 0 002-2v-4M14 4h6m0 0v6m0-6L10 14" />
                      </svg>
                      项目地址
                    </span>
                  )}
                  {project.githubUrl && (
                    <span className="inline-flex items-center gap-1 px-2 py-0.5 text-[10px] rounded-[4px] font-medium"
                      style={{ backgroundColor: '#e5e7eb', color: '#374151' }}>
                      <svg className="w-3 h-3" fill="currentColor" viewBox="0 0 24 24">
                        <path d="M12 0c-6.626 0-12 5.373-12 12 0 5.302 3.438 9.8 8.207 11.387.599.111.793-.261.793-.577v-2.234c-3.338.726-4.033-1.416-4.033-1.416-.546-1.387-1.333-1.756-1.333-1.756-1.089-.745.083-.729.083-.729 1.205.084 1.839 1.237 1.839 1.237 1.07 1.834 2.807 1.304 3.492.997.107-.775.418-1.305.762-1.604-2.665-.305-5.467-1.334-5.467-5.931 0-1.311.469-2.381 1.236-3.221-.124-.303-.535-1.524.117-3.176 0 0 1.008-.322 3.301 1.23.957-.266 1.983-.399 3.003-.404 1.02.005 2.047.138 3.006.404 2.291-1.552 3.297-1.23 3.297-1.23.653 1.653.242 2.874.118 3.176.77.84 1.235 1.911 1.235 3.221 0 4.609-2.807 5.624-5.479 5.921.43.372.823 1.102.823 2.222v3.293c0 .319.192.694.801.576 4.765-1.589 8.199-6.086 8.199-11.386 0-6.627-5.373-12-12-12z" />
                      </svg>
                      GitHub
                    </span>
                  )}
                  {project.demoUrl && (
                    <span className="inline-flex items-center gap-1 px-2 py-0.5 text-[10px] rounded-[4px] font-medium"
                      style={{ backgroundColor: '#d1fae5', color: '#059669' }}>
                      <svg className="w-3 h-3" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                        <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M15 10l4.553-2.276A1 1 0 0121 8.618v6.764a1 1 0 01-1.447.894L15 14M5 18h8a2 2 0 002-2V8a2 2 0 00-2-2H5a2 2 0 00-2 2v8a2 2 0 002 2z" />
                      </svg>
                      演示
                    </span>
                  )}
                </div>
              </div>
            </Link>
          ))}
        </div>
      ))}
      <LayoutUniversalModules pageKey="projects" />
      <VisitTracker pagePath="/projects" />
    </div>
  )
}