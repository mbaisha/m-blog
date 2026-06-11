'use client'

import ReactMarkdown from 'react-markdown'
import remarkGfm from 'remark-gfm'
import rehypeHighlight from 'rehype-highlight'
import rehypeSlug from 'rehype-slug'
import rehypeRaw from 'rehype-raw'
import rehypeSanitize, { defaultSchema } from 'rehype-sanitize'

// 自定义 sanitize 方案，允许 callout div 结构
const sanitizeSchema = {
  ...defaultSchema,
  attributes: {
    ...defaultSchema.attributes,
    div: [
      ...(defaultSchema.attributes?.div || []),
      ['className'],
    ],
    span: [
      ...(defaultSchema.attributes?.span || []),
      ['className'],
    ],
  },
}

interface Props {
  content: string
}

// Pre-process callout blocks: :::tip / :::info / :::warning
function preprocessCallouts(markdown: string): string {
  return markdown.replace(
    /:::(\w+)\s*\n([\s\S]*?)\n:::/g,
    (_, type: string, content: string) => {
      const calloutType = type.toLowerCase()
      const icons: Record<string, string> = {
        tip: '💡',
        info: 'ℹ️',
        warning: '⚠️',
      }
      const icon = icons[calloutType] || 'ℹ️'
      return `<div class="callout callout-${calloutType}">
<div class="callout-icon">${icon}</div>
<div class="callout-content">${content.trim()}</div>
</div>`
    }
  )
}

function Callout({ type, children }: { type: string; children: React.ReactNode }) {
  const styles: Record<string, { borderColor: string; bg: string }> = {
    tip: { borderColor: '#22C55E', bg: '#F0FDF4' },
    info: { borderColor: '#6366F1', bg: '#EEEDFE' },
    warning: { borderColor: '#F59E0B', bg: '#FFFBEB' },
  }
  const s = styles[type] || styles.info
  const icons: Record<string, string> = {
    tip: '💡',
    info: 'ℹ️',
    warning: '⚠️',
  }

  return (
    <div
      className="callout"
      style={{
        borderRadius: '8px',
        padding: '16px',
        marginBottom: '1.2em',
        backgroundColor: s.bg,
        borderLeft: `4px solid ${s.borderColor}`,
      }}
    >
      <div className="flex gap-2 items-start">
        <span className="text-base flex-shrink-0 mt-0.5">{icons[type] || 'ℹ️'}</span>
        <div className="callout-content text-[14px] leading-relaxed" style={{ color: 'var(--color-text-primary)' }}>
          {children}
        </div>
      </div>
    </div>
  )
}

export default function MarkdownContent({ content }: Props) {
  const processedContent = preprocessCallouts(content)

  return (
    <ReactMarkdown
      remarkPlugins={[remarkGfm]}
      rehypePlugins={[rehypeHighlight, rehypeSlug, rehypeRaw, [rehypeSanitize, sanitizeSchema]]}
      components={{
        // Override callout div rendering
        div: ({ className, children, ...props }) => {
          if (className?.startsWith('callout')) {
            const type = className.replace('callout callout-', '')
            return <Callout type={type}>{children}</Callout>
          }
          return <div className={className} {...props}>{children}</div>
        },
        // Style headings for scroll offset
        h2: ({ children, id, ...props }) => (
          <h2 id={id} className="scroll-mt-20" {...props}>{children}</h2>
        ),
        h3: ({ children, id, ...props }) => (
          <h3 id={id} className="scroll-mt-20" {...props}>{children}</h3>
        ),
        h4: ({ children, id, ...props }) => (
          <h4 id={id} className="scroll-mt-20" {...props}>{children}</h4>
        ),
      }}
    >
      {processedContent}
    </ReactMarkdown>
  )
}