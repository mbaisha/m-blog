import type { Metadata } from "next"
import Link from "next/link"
import Image from "next/image"
import { notFound } from "next/navigation"
import { apiClient, fetchPageLayout } from "@/lib/api"
import type { ApiResponse, PublicProjectDetail, PublicModuleLayout } from "@/types"
import { coverImageUrlWithVersion } from "@/lib/imageVersion"
import VisitTracker from "@/components/VisitTracker"
import MarkdownContent from "../../articles/[slug]/MarkdownContent"

interface Props {
  params: Promise<{ slug: string }>
}

export async function generateMetadata({ params }: Props): Promise<Metadata> {
  const { slug } = await params
  try {
    const res = await apiClient.get<ApiResponse<PublicProjectDetail>>(`/projects/${slug}`)
    const project = res.data
    if (!project) return { title: "项目未找到" }
    return {
      title: project.title,
      description: project.summary || project.title,
      openGraph: {
        title: project.title,
        description: project.summary || project.title,
        type: 'article',
        url: `${process.env.NEXT_PUBLIC_SITE_URL || 'http://localhost:3000'}/projects/${project.slug}`,
      },
      twitter: {
        card: 'summary_large_image',
        title: project.title,
        description: project.summary || project.title,
      },
    }
  } catch {
    return { title: "项目未找到" }
  }
}

export default async function ProjectDetailPage({ params }: Props) {
  const { slug } = await params
  let project: PublicProjectDetail | null = null
  let layoutModules: PublicModuleLayout[] = []

  try {
    const [res, layoutRes] = await Promise.all([
      apiClient.get<ApiResponse<PublicProjectDetail>>(`/projects/${slug}`),
      fetchPageLayout("project_detail").catch(() => null),
    ])
    project = res.data
    layoutModules = layoutRes?.data || []
  } catch {
    notFound()
  }

  if (!project) notFound()

  const moduleMap = new Map(layoutModules.map(m => [m.moduleKey, m]))
  const isEnabled = (key: string) => {
    const mod = moduleMap.get(key)
    return mod ? mod.isEnabled : true
  }

  return (
    <div className="content-container py-12 max-w-[800px]">
      {/* project_hero */}
      {isEnabled('project_hero') && (
      <>
      {/* Back link */}
      <nav className="mb-8">
        <Link
          href="/projects"
          className="inline-flex items-center gap-1.5 px-4 py-2 rounded-[8px] text-[13px] font-medium transition-all hover:-translate-x-0.5"
          style={{
            border: '0.5px solid var(--color-border)',
            color: 'var(--color-text-secondary)',
            backgroundColor: 'var(--card-bg)',
          }}
        >
          <svg className="w-3.5 h-3.5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M19 12H5m7-7l-7 7 7 7" />
          </svg>
          返回项目列表
        </Link>
      </nav>

      {/* Title */}
      <h1 className="text-[24px] md:text-[28px] font-semibold mb-3" style={{ color: 'var(--color-text-primary)' }}>
        {project.title}
      </h1>

      {/* Summary */}
      {project.summary && (
        <p className="text-[14px] mb-6" style={{ color: 'var(--color-text-secondary)' }}>
          {project.summary}
        </p>
      )}

      {/* Project action bar: circular icon buttons + tech stack */}
      <div className="flex flex-wrap items-center gap-3 mb-8">
        {project.projectUrl && (
          <a
            href={project.projectUrl}
            target="_blank"
            rel="noopener noreferrer"
            className="w-10 h-10 rounded-full flex items-center justify-center transition-all hover:scale-110 hover:shadow-md"
            style={{ backgroundColor: '#dbeafe', color: '#2563eb' }}
            title="项目地址"
          >
            <svg className="w-4.5 h-4.5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M10 6H6a2 2 0 00-2 2v10a2 2 0 002 2h10a2 2 0 002-2v-4M14 4h6m0 0v6m0-6L10 14" />
            </svg>
          </a>
        )}
        {project.githubUrl && (
          <a
            href={project.githubUrl}
            target="_blank"
            rel="noopener noreferrer"
            className="w-10 h-10 rounded-full flex items-center justify-center transition-all hover:scale-110 hover:shadow-md"
            style={{ backgroundColor: '#e5e7eb', color: '#374151' }}
            title="GitHub 地址"
          >
            <svg className="w-4.5 h-4.5" fill="currentColor" viewBox="0 0 24 24">
              <path d="M12 0c-6.626 0-12 5.373-12 12 0 5.302 3.438 9.8 8.207 11.387.599.111.793-.261.793-.577v-2.234c-3.338.726-4.033-1.416-4.033-1.416-.546-1.387-1.333-1.756-1.333-1.756-1.089-.745.083-.729.083-.729 1.205.084 1.839 1.237 1.839 1.237 1.07 1.834 2.807 1.304 3.492.997.107-.775.418-1.305.762-1.604-2.665-.305-5.467-1.334-5.467-5.931 0-1.311.469-2.381 1.236-3.221-.124-.303-.535-1.524.117-3.176 0 0 1.008-.322 3.301 1.23.957-.266 1.983-.399 3.003-.404 1.02.005 2.047.138 3.006.404 2.291-1.552 3.297-1.23 3.297-1.23.653 1.653.242 2.874.118 3.176.77.84 1.235 1.911 1.235 3.221 0 4.609-2.807 5.624-5.479 5.921.43.372.823 1.102.823 2.222v3.293c0 .319.192.694.801.576 4.765-1.589 8.199-6.086 8.199-11.386 0-6.627-5.373-12-12-12z" />
            </svg>
          </a>
        )}
        {project.demoUrl && (
          <a
            href={project.demoUrl}
            target="_blank"
            rel="noopener noreferrer"
            className="w-10 h-10 rounded-full flex items-center justify-center transition-all hover:scale-110 hover:shadow-md"
            style={{ backgroundColor: '#d1fae5', color: '#059669' }}
            title="演示地址"
          >
            <svg className="w-4.5 h-4.5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M15 10l4.553-2.276A1 1 0 0121 8.618v6.764a1 1 0 01-1.447.894L15 14M5 18h8a2 2 0 002-2V8a2 2 0 00-2-2H5a2 2 0 00-2 2v8a2 2 0 002 2z" />
            </svg>
          </a>
        )}

        {/* Separator dot */}
        {(project.projectUrl || project.githubUrl || project.demoUrl) && project.techStack.length > 0 && (
          <span className="w-1 h-1 rounded-full flex-shrink-0" style={{ backgroundColor: 'var(--color-text-tertiary)' }} />
        )}

        {/* Tech stack */}
        {project.techStack.length > 0 && (
          <div className="flex flex-wrap gap-1.5">
            {project.techStack.map((tech) => (
              <span
                key={tech}
                className="px-2.5 py-1 rounded-[6px] text-[12px] font-medium"
                style={{
                  backgroundColor: 'var(--color-border-light)',
                  color: 'var(--color-text-secondary)',
                }}
              >
                {tech}
              </span>
            ))}
          </div>
        )}
      </div>
      </>
      )}

      {/* cover */}
      {isEnabled('cover') && project.coverImageUrl && (
        <div className="aspect-video rounded-[16px] overflow-hidden mb-8 flex justify-center bg-[#F3F4F6] relative">
          <Image src={coverImageUrlWithVersion(project.coverImageUrl, project.updatedAt)} alt={project.title} fill className="object-cover" sizes="(max-width: 767px) 100vw, 720px" priority />
        </div>
      )}

      {/* project_content */}
      {isEnabled('project_content') && (
      <article className="detail-article-body" style={{ borderTop: '0.5px solid var(--color-border)', paddingTop: '2em' }}>
        <MarkdownContent content={project.content || ''} />
      </article>
      )}
      <VisitTracker pagePath={`/projects/${slug}`} />
    </div>
  )
}