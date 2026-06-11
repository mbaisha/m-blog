interface CallToActionProps {
  config: string
}

export default function CallToAction({ config }: CallToActionProps) {
  let text = '联系我'
  let link = '/about'
  let description = ''
  let style = 'primary'

  try {
    const parsed = JSON.parse(config)
    text = parsed.text || '联系我'
    link = parsed.link || '/about'
    description = parsed.description || ''
    style = parsed.style || 'primary'
  } catch { /* ignore */ }

  const styleMap: Record<string, React.CSSProperties> = {
    primary: {
      backgroundColor: 'var(--color-primary, #6366f1)',
      color: '#fff',
    },
    outline: {
      border: '2px solid var(--color-primary, #6366f1)',
      color: 'var(--color-primary, #6366f1)',
      backgroundColor: 'transparent',
    },
    subtle: {
      backgroundColor: 'var(--color-primary-light, #eef2ff)',
      color: 'var(--color-primary, #6366f1)',
    },
  }

  return (
    <div className="text-center py-8 px-6 rounded-[12px]" style={{
      backgroundColor: 'var(--card-bg, #fff)',
      border: '0.5px solid var(--color-border, #e5e7eb)',
    }}>
      {description && (
        <p className="text-[14px] mb-4" style={{ color: 'var(--color-text-secondary, #6b7280)' }}>
          {description}
        </p>
      )}
      <a
        href={link}
        className="inline-block px-6 py-3 rounded-[8px] text-[14px] font-medium transition-all hover:opacity-90 hover:-translate-y-0.5"
        style={styleMap[style] || styleMap.primary}
      >
        {text}
      </a>
    </div>
  )
}