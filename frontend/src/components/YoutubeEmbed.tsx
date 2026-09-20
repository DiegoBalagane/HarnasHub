import { useState } from 'react'

interface ParsedYoutubeUrl {
  videoId: string
  startSeconds: number | null
}

/** Extracts a video id and optional start time from a watch/share/shorts/embed YouTube URL, or null if unrecognized. */
function parseYoutubeUrl(url: string): ParsedYoutubeUrl | null {
  const idMatch = url.match(/(?:youtube\.com\/(?:watch\?v=|embed\/|shorts\/)|youtu\.be\/)([\w-]{11})/)
  if (!idMatch) return null

  const timeMatch = url.match(/[?&](?:t|start)=(\d+)/)
  return { videoId: idMatch[1], startSeconds: timeMatch ? Number(timeMatch[1]) : null }
}

interface YoutubeEmbedProps {
  url: string
  title: string
  className?: string
}

/**
 * A lineup clip embedded via a click-to-load thumbnail facade — the privacy-enhanced youtube-nocookie.com
 * iframe only mounts (and starts pulling Google's tracking/JS) once the viewer actually presses play,
 * which matters here since a single map's radar can list dozens of these at once.
 */
export function YoutubeEmbed({ url, title, className = '' }: YoutubeEmbedProps) {
  const [isPlaying, setIsPlaying] = useState(false)
  const parsed = parseYoutubeUrl(url)

  if (!parsed) {
    return (
      <a
        href={url}
        target="_blank"
        rel="noopener noreferrer"
        className={`text-sm text-red-400 hover:underline ${className}`}
      >
        Otwórz wideo ↗
      </a>
    )
  }

  if (isPlaying) {
    const startParam = parsed.startSeconds ? `&start=${parsed.startSeconds}` : ''
    return (
      <iframe
        className={`aspect-video w-full rounded-md ${className}`}
        src={`https://www.youtube-nocookie.com/embed/${parsed.videoId}?autoplay=1${startParam}`}
        title={title}
        loading="lazy"
        allow="autoplay; encrypted-media"
        allowFullScreen
      />
    )
  }

  return (
    <button
      type="button"
      onClick={() => setIsPlaying(true)}
      title="Odtwórz wideo"
      className={`group relative aspect-video w-full overflow-hidden rounded-md bg-neutral-900 ${className}`}
    >
      <img
        src={`https://img.youtube.com/vi/${parsed.videoId}/hqdefault.jpg`}
        alt={title}
        loading="lazy"
        className="h-full w-full object-cover"
      />
      <span className="absolute inset-0 flex items-center justify-center bg-black/30 transition group-hover:bg-black/50">
        <span className="flex h-12 w-12 items-center justify-center rounded-full bg-red-600 text-white shadow-lg">
          ▶
        </span>
      </span>
    </button>
  )
}
