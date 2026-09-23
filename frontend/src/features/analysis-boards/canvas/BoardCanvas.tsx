import { useEffect, useRef, useState } from 'react'
import { REFERENCE_SIZE, type Stroke, type StrokePoint } from './types'

interface BoardCanvasProps {
  backgroundSrc: string
  strokes: Stroke[]
  /** Present only in the interactive editor — a completed drag is appended to the board's strokes via this callback.
   * Omitted for a read-only gallery thumbnail, which just renders the given strokes and ignores pointer input. */
  onStrokeComplete?: (stroke: Stroke) => void
  currentColor?: string
  currentWidth?: number
  className?: string
}

/** Renders a map/screenshot with freehand strokes drawn on top, and — when `onStrokeComplete` is given — lets the
 * user draw new ones with the pointer. Coordinates are radar-relative fractions, so strokes stay aligned regardless
 * of how large the canvas is rendered. */
export function BoardCanvas({
  backgroundSrc,
  strokes,
  onStrokeComplete,
  currentColor = '#ff2d2d',
  currentWidth = 6,
  className,
}: BoardCanvasProps) {
  const canvasRef = useRef<HTMLCanvasElement>(null)
  const imageRef = useRef<HTMLImageElement | null>(null)
  const [imageLoaded, setImageLoaded] = useState(false)
  const drawingRef = useRef<StrokePoint[] | null>(null)
  const [livePoints, setLivePoints] = useState<StrokePoint[] | null>(null)

  useEffect(() => {
    setImageLoaded(false)
    const image = new Image()
    image.crossOrigin = 'anonymous'
    image.onload = () => {
      imageRef.current = image
      const canvas = canvasRef.current
      if (canvas) {
        canvas.width = image.naturalWidth
        canvas.height = image.naturalHeight
      }
      setImageLoaded(true)
    }
    image.src = backgroundSrc
  }, [backgroundSrc])

  useEffect(() => {
    const canvas = canvasRef.current
    const image = imageRef.current
    const ctx = canvas?.getContext('2d')
    if (!canvas || !image || !ctx || !imageLoaded) return

    ctx.clearRect(0, 0, canvas.width, canvas.height)
    ctx.drawImage(image, 0, 0, canvas.width, canvas.height)

    const allStrokes = livePoints ? [...strokes, { color: currentColor, width: currentWidth, points: livePoints }] : strokes

    for (const stroke of allStrokes) {
      if (stroke.points.length === 0) continue

      ctx.strokeStyle = stroke.color
      ctx.lineWidth = (stroke.width / REFERENCE_SIZE) * canvas.width
      ctx.lineCap = 'round'
      ctx.lineJoin = 'round'
      ctx.beginPath()

      stroke.points.forEach((point, index) => {
        const x = point.x * canvas.width
        const y = point.y * canvas.height
        if (index === 0) {
          ctx.moveTo(x, y)
        } else {
          ctx.lineTo(x, y)
        }
      })

      ctx.stroke()
    }
  }, [imageLoaded, strokes, livePoints, currentColor, currentWidth])

  function toFraction(event: React.PointerEvent<HTMLCanvasElement>): StrokePoint {
    const rect = canvasRef.current!.getBoundingClientRect()
    return {
      x: Math.min(1, Math.max(0, (event.clientX - rect.left) / rect.width)),
      y: Math.min(1, Math.max(0, (event.clientY - rect.top) / rect.height)),
    }
  }

  function handlePointerDown(event: React.PointerEvent<HTMLCanvasElement>) {
    if (!onStrokeComplete) return
    event.preventDefault()
    event.currentTarget.setPointerCapture(event.pointerId)
    const point = toFraction(event)
    drawingRef.current = [point]
    setLivePoints([point])
  }

  function handlePointerMove(event: React.PointerEvent<HTMLCanvasElement>) {
    if (!drawingRef.current) return
    drawingRef.current.push(toFraction(event))
    setLivePoints([...drawingRef.current])
  }

  function handlePointerUp() {
    if (!drawingRef.current) return
    if (drawingRef.current.length > 1) {
      onStrokeComplete?.({ color: currentColor, width: currentWidth, points: drawingRef.current })
    }
    drawingRef.current = null
    setLivePoints(null)
  }

  return (
    <canvas
      ref={canvasRef}
      onPointerDown={handlePointerDown}
      onPointerMove={handlePointerMove}
      onPointerUp={handlePointerUp}
      onPointerCancel={handlePointerUp}
      className={
        className ??
        `block h-auto w-full touch-none select-none rounded-md border border-neutral-800 bg-neutral-950 ${onStrokeComplete ? 'cursor-crosshair' : ''}`
      }
    />
  )
}
