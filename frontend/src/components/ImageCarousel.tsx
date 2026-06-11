"use client"

import { useState, useEffect, useCallback } from 'react'

interface CarouselImage {
  url: string
  link?: string
  title?: string
}

interface ImageCarouselProps {
  config: string
}

export default function ImageCarousel({ config }: ImageCarouselProps) {
  let images: CarouselImage[] = []
  let autoPlay = true
  let interval = 4000
  let height = 400

  try {
    const parsed = JSON.parse(config)
    images = parsed.images || []
    autoPlay = parsed.autoPlay !== false
    interval = parsed.interval || 4000
    height = parsed.height || 400
  } catch {
    images = [{ url: 'https://picsum.photos/800/400?random=1' }]
  }

  const [current, setCurrent] = useState(0)
  const [isPaused, setIsPaused] = useState(false)

  const next = useCallback(() => {
    setCurrent((prev) => (prev + 1) % images.length)
  }, [images.length])

  const prev = useCallback(() => {
    setCurrent((prev) => (prev - 1 + images.length) % images.length)
  }, [images.length])

  useEffect(() => {
    if (!autoPlay || isPaused || images.length <= 1) return
    const timer = setInterval(next, interval)
    return () => clearInterval(timer)
  }, [autoPlay, isPaused, interval, next, images.length])

  if (images.length === 0) return null

  const img = images[current]

  return (
    <div
      className="relative w-full overflow-hidden rounded-[12px] group"
      style={{ height }}
      onMouseEnter={() => setIsPaused(true)}
      onMouseLeave={() => setIsPaused(false)}
    >
      {/* Image */}
      <div
        className="w-full h-full bg-cover bg-center transition-all duration-700"
        style={{ backgroundImage: `url(${img.url})` }}
      >
        {img.link ? (
          <a href={img.link} className="block w-full h-full relative">
            {img.title && (
              <div className="absolute bottom-0 left-0 right-0 p-6 bg-gradient-to-t from-black/60 to-transparent">
                <h3 className="text-white text-lg font-medium">{img.title}</h3>
              </div>
            )}
          </a>
        ) : (
          img.title && (
            <div className="absolute bottom-0 left-0 right-0 p-6 bg-gradient-to-t from-black/60 to-transparent">
              <h3 className="text-white text-lg font-medium">{img.title}</h3>
            </div>
          )
        )}
      </div>

      {/* Navigation arrows */}
      {images.length > 1 && (
        <>
          <button
            onClick={prev}
            className="absolute left-3 top-1/2 -translate-y-1/2 w-9 h-9 rounded-full bg-white/80 hover:bg-white flex items-center justify-center opacity-0 group-hover:opacity-100 transition-opacity shadow"
            aria-label="上一张"
          >
            <svg className="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M15 19l-7-7 7-7" />
            </svg>
          </button>
          <button
            onClick={next}
            className="absolute right-3 top-1/2 -translate-y-1/2 w-9 h-9 rounded-full bg-white/80 hover:bg-white flex items-center justify-center opacity-0 group-hover:opacity-100 transition-opacity shadow"
            aria-label="下一张"
          >
            <svg className="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9 5l7 7-7 7" />
            </svg>
          </button>
        </>
      )}

      {/* Dots */}
      {images.length > 1 && (
        <div className="absolute bottom-3 left-1/2 -translate-x-1/2 flex gap-2">
          {images.map((_, i) => (
            <button
              key={i}
              onClick={() => setCurrent(i)}
              className={`w-2 h-2 rounded-full transition-all ${
                i === current ? 'bg-white w-5' : 'bg-white/50 hover:bg-white/80'
              }`}
              aria-label={`第 ${i + 1} 张`}
            />
          ))}
        </div>
      )}
    </div>
  )
}