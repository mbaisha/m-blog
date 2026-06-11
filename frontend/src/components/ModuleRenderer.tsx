import type { PublicModuleLayout } from '@/types'
import Link from 'next/link'

// 延迟加载新组件（仅在需要时导入）
import dynamic from 'next/dynamic'

const ImageCarousel = dynamic(() => import('@/components/ImageCarousel'), { ssr: false })
const VideoPlayer = dynamic(() => import('@/components/VideoPlayer'), { ssr: false })
const Statistics = dynamic(() => import('@/components/Statistics'), { ssr: false })
const CallToAction = dynamic(() => import('@/components/CallToAction'), { ssr: false })

interface ModuleRendererProps {
  modules: PublicModuleLayout[]
  pageModules?: Record<string, React.ReactNode>
}

/**
 * 模块引擎 — 根据 module_key 匹配渲染组件
 */
export default function ModuleRenderer({ modules, pageModules = {} }: ModuleRendererProps) {
  const enabledModules = modules.filter(m => m.isEnabled).sort((a, b) => a.sortOrder - b.sortOrder)

  if (enabledModules.length === 0) return null

  return (
    <>
      {enabledModules.map((mod) => (
        <ModuleSection key={mod.id} module={mod}>
          {renderModuleContent(mod, pageModules[mod.moduleKey])}
        </ModuleSection>
      ))}
    </>
  )
}

function ModuleSection({ module, children }: { module: PublicModuleLayout; children: React.ReactNode }) {
  if (!module.isEnabled) return null

  return (
    <section className="mb-10">
      {module.title && (
        <h2 className="text-2xl font-bold mb-6" style={{ color: 'var(--theme-text, #111827)' }}>
          {module.title}
        </h2>
      )}
      {children}
    </section>
  )
}

function renderModuleContent(module: PublicModuleLayout, customContent?: React.ReactNode) {
  // 如果有外部传入的自定义内容（如 About 页面的 hero/intro 等），优先使用
  if (customContent) return customContent

  // 根据 module_key 渲染默认模块
  switch (module.moduleKey) {
    case 'hero':
      return <HeroModule config={module.config} />

    case 'featured_posts':
    case 'recent_posts':
      return <PostListModule moduleKey={module.moduleKey} config={module.config} />

    case 'categories':
      return <CategoriesModule config={module.config} />

    case 'tags':
      return <TagsModule config={module.config} />

    case 'projects':
      return <ProjectsModule config={module.config} />

    case 'about_me':
      return <AboutMeModule config={module.config} />

    case 'custom':
      return <CustomModule config={module.config} />

    case 'image_carousel':
    case 'video_player':
    case 'statistics':
    case 'call_to_action':
      return <GenericModule moduleKey={module.moduleKey} config={module.config} />

    default:
      return null
  }
}

// ===== 内置模块组件 =====

function HeroModule({ config: _config }: { config: string }) {
  return (
    <div className="text-center py-16">
      <h1 className="text-4xl font-bold mb-4" style={{ color: 'var(--theme-text, #111827)' }}>
        欢迎来到我的博客
      </h1>
      <p className="text-lg max-w-2xl mx-auto leading-relaxed" style={{ color: 'var(--theme-text, #6b7280)' }}>
        分享技术实践、产品思考与项目复盘
      </p>
    </div>
  )
}

function PostListModule({ moduleKey, config: _config }: { moduleKey: string; config: string }) {
  const title = moduleKey === 'featured_posts' ? '精选文章' : '最新文章'
  return (
    <div className="text-center py-8" style={{ color: 'var(--theme-text, #9ca3af)' }}>
      <p>{title}模块 — 文章列表由 API 动态加载</p>
    </div>
  )
}

function CategoriesModule({ config: _config }: { config: string }) {
  return (
    <div className="flex flex-wrap gap-3">
      <Link href="/categories" className="px-4 py-2 rounded-lg text-sm"
        style={{ backgroundColor: 'var(--theme-primary, #6366f1)', color: '#fff' }}>
        查看全部分类 →
      </Link>
    </div>
  )
}

function TagsModule({ config: _config }: { config: string }) {
  return (
    <div className="flex flex-wrap gap-2">
      <Link href="/tags" className="px-3 py-1 rounded-full text-sm border"
        style={{
          borderColor: 'var(--theme-primary, #6366f1)',
          color: 'var(--theme-primary, #6366f1)'
        }}>
        查看全部标签 →
      </Link>
    </div>
  )
}

function ProjectsModule({ config: _config }: { config: string }) {
  return (
    <div>
      <Link href="/projects" className="inline-flex items-center px-4 py-2 rounded-lg text-sm"
        style={{ backgroundColor: 'var(--theme-primary, #6366f1)', color: '#fff' }}>
        查看项目 →
      </Link>
    </div>
  )
}

function AboutMeModule({ config: _config }: { config: string }) {
  return (
    <div className="p-6 rounded-lg border" style={{
      borderColor: 'var(--theme-border, #e5e7eb)',
      backgroundColor: 'var(--theme-bg, #f8fafc)'
    }}>
      <Link href="/about" style={{ color: 'var(--theme-link, #6366f1)' }}>
        了解更多关于我 →
      </Link>
    </div>
  )
}

function CustomModule({ config }: { config: string }) {
  let parsed: Record<string, string> = {}
  try { parsed = JSON.parse(config) } catch { /* ignore */ }

  if (parsed.html) {
    return <div dangerouslySetInnerHTML={{ __html: parsed.html }} />
  }

  return (
    <div className="p-6 rounded-lg border" style={{
      borderColor: 'var(--theme-border, #e5e7eb)'
    }}>
      <p style={{ color: 'var(--theme-text, #6b7280)' }}>自定义区域</p>
    </div>
  )
}

/** 通用模块渲染（使用动态导入的组件） */
function GenericModule({ moduleKey, config }: { moduleKey: string; config: string }) {
  switch (moduleKey) {
    case 'image_carousel':
      return <ImageCarousel config={config} />
    case 'video_player':
      return <VideoPlayer config={config} />
    case 'statistics':
      return <Statistics config={config} />
    case 'call_to_action':
      return <CallToAction config={config} />
    default:
      return null
  }
}