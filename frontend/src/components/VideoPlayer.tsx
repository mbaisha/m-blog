"use client"

interface VideoPlayerProps {
  config: string
}

export default function VideoPlayer({ config }: VideoPlayerProps) {
  let url = ''
  let aspectRatio = '16/9'
  let autoplay = false

  try {
    const parsed = JSON.parse(config)
    url = parsed.url || ''
    aspectRatio = parsed.aspectRatio || '16/9'
    autoplay = parsed.autoplay || false
  } catch { /* ignore */ }

  if (!url) return null

  // Determine embed URL
  let embedUrl = url
  if (url.includes('youtube.com/watch') || url.includes('youtu.be')) {
    const id = url.match(/(?:youtube\.com\/watch\?v=|youtu\.be\/)([a-zA-Z0-9_-]+)/)?.[1]
    if (id) embedUrl = `https://www.youtube.com/embed/${id}${autoplay ? '?autoplay=1' : ''}`
  } else if (url.includes('bilibili.com')) {
    const id = url.match(/video\/([a-zA-Z0-9]+)/)?.[1]
    if (id) embedUrl = `https://player.bilibili.com/player.html?bvid=${id}&autoplay=${autoplay ? 1 : 0}`
  }

  return (
    <div className="w-full overflow-hidden rounded-[10px]" style={{ aspectRatio }}>
      <iframe
        src={embedUrl}
        className="w-full h-full"
        allow={`accelerometer; autoplay; clipboard-write; encrypted-media; gyroscope; picture-in-picture${autoplay ? '; autoplay' : ''}`}
        allowFullScreen
        title="Embedded video"
      />
    </div>
  )
}