import { memo, useEffect, useMemo, useRef } from 'react'
import type { RoundReplay } from '../../../services/replayApi'
import { drawReplay, type ReplayScene } from '../replay/drawReplay'
import { buildSeries, deathSeconds } from '../replay/replayMath'

interface ReplayCanvasProps {
  replay: RoundReplay
  mapName: string
  /** Registers a per-frame time listener (from useReplayPlayback); returns the unsubscribe function. */
  subscribe: (listener: (t: number) => void) => () => void
}

/** Radar image with a canvas overlay redrawn imperatively on every playback frame — no React re-render per frame. */
export const ReplayCanvas = memo(function ReplayCanvas({ replay, mapName, subscribe }: ReplayCanvasProps) {
  const containerRef = useRef<HTMLDivElement>(null)
  const canvasRef = useRef<HTMLCanvasElement>(null)
  const timeRef = useRef(0)
  // Lets the radar image re-measure the canvas once it has loaded — until then the container is only as tall as an
  // empty image, and a canvas sized then would be stretched (blurry, squashed dots) after the image appears.
  const resizeRef = useRef<() => void>(() => {})
  const scene = useMemo<ReplayScene>(
    () => ({ replay, series: buildSeries(replay), deaths: deathSeconds(replay) }),
    [replay],
  )

  useEffect(() => {
    const canvas = canvasRef.current
    const container = containerRef.current
    const ctx = canvas?.getContext('2d')
    if (!canvas || !container || !ctx) return

    let width = 0
    let height = 0
    const draw = () => {
      if (width > 0 && height > 0) drawReplay(ctx, width, height, { ...scene, t: timeRef.current })
    }

    const resize = () => {
      const ratio = window.devicePixelRatio || 1
      width = container.clientWidth
      height = container.clientHeight
      canvas.width = Math.round(width * ratio)
      canvas.height = Math.round(height * ratio)
      ctx.setTransform(ratio, 0, 0, ratio, 0, 0)
      draw()
    }

    resizeRef.current = resize
    const observer = new ResizeObserver(resize)
    observer.observe(container)
    resize()

    const unsubscribe = subscribe((t) => {
      timeRef.current = t
      draw()
    })

    return () => {
      observer.disconnect()
      unsubscribe()
    }
  }, [scene, subscribe])

  return (
    <div
      ref={containerRef}
      className="relative w-full select-none overflow-hidden rounded-md border border-neutral-800 bg-neutral-950"
    >
      <img
        src={`/maps/${mapName.toLowerCase()}.webp`}
        alt={`Radar mapy ${mapName}`}
        draggable={false}
        onLoad={() => resizeRef.current()}
        className="block h-auto w-full"
      />
      <canvas ref={canvasRef} className="pointer-events-none absolute inset-0 h-full w-full" />
    </div>
  )
})
