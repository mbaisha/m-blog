'use client'

import { useState, useRef, useCallback } from 'react'
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

/** 代码块复制按钮 */
function CodeBlock({ children, className }: { children: React.ReactNode; className?: string }) {
  const [copied, setCopied] = useState(false)
  const preRef = useRef<HTMLPreElement>(null)

  const handleCopy = useCallback(async () => {
    const codeEl = preRef.current?.querySelector('code')
    const text = codeEl?.textContent || ''
    try {
      await navigator.clipboard.writeText(text)
      setCopied(true)
      setTimeout(() => setCopied(false), 1500)
    } catch { /* clipboard API 不可用时静默失败 */ }
  }, [])

  return (
    <div style={{ position: 'relative' }}>
      <button
        onClick={handleCopy}
        aria-label={copied ? '已复制' : '复制代码'}
        style={{
          position: 'absolute',
          top: '8px',
          right: '8px',
          zIndex: 1,
          padding: '4px 10px',
          fontSize: '12px',
          lineHeight: '16px',
          color: copied ? '#98c379' : '#abb2bf',
          background: copied ? 'rgba(152, 195, 121, 0.15)' : 'rgba(171, 178, 191, 0.12)',
          border: `1px solid ${copied ? 'rgba(152, 195, 121, 0.3)' : 'rgba(171, 178, 191, 0.2)'}`,
          borderRadius: '6px',
          cursor: 'pointer',
          transition: 'all 0.15s ease',
          fontFamily: 'var(--font-mono, monospace)',
          userSelect: 'none',
        }}
        onMouseEnter={(e) => {
          if (!copied) {
            e.currentTarget.style.background = 'rgba(171, 178, 191, 0.2)'
            e.currentTarget.style.borderColor = 'rgba(171, 178, 191, 0.35)'
          }
        }}
        onMouseLeave={(e) => {
          if (!copied) {
            e.currentTarget.style.background = 'rgba(171, 178, 191, 0.12)'
            e.currentTarget.style.borderColor = 'rgba(171, 178, 191, 0.2)'
          }
        }}
      >
        {copied ? '✓ Copied!' : 'Copy'}
      </button>
      <pre ref={preRef} className={className}>
        {children}
      </pre>
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
        // 代码块：右上角添加复制按钮
        pre: ({ children, className, ...props }) => (
          <CodeBlock className={className}>{children}</CodeBlock>
        ),
      }}
    >
      {processedContent}
    </ReactMarkdown>
  )
}