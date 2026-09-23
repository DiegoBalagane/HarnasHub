import { useCallback, useRef, useState, type PointerEvent, type RefObject, type WheelEvent } from 'react'

const MIN_SCALE = 1
const MAX_SCALE = 4
const WHEEL_ZOOM_FACTOR = 1.2

function clamp(value: number, min: number, max: number) {
  return Math.min(max, Math.max(min, value))
}

interface PanDraft {
  pointerId: number
  startClientX: number
  startClientY: number
  startPanX: number
  startPanY: number
}

/** Scroll-to-zoom and drag-to-pan for the map radar. Keeps the point under the cursor fixed while zooming and never
 * pans the content past its own edges. `viewportRef` must point at the fixed-size, `overflow-hidden` outer element —
 * the returned `scale`/`panX`/`panY` go into a CSS transform on the inner element that actually holds the image/pins. */
export function useMapZoom(viewportRef: RefObject<HTMLElement | null>) {
  const [scale, setScale] = useState(MIN_SCALE)
  const [pan, setPan] = useState({ x: 0, y: 0 })
  const panDraftRef = useRef<PanDraft | null>(null)

  const clampPan = useCallback(
    (panX: number, panY: number, atScale: number) => {
      const rect = viewportRef.current?.getBoundingClientRect()
      if (!rect || rect.width === 0 || rect.height === 0) {
        return { x: panX, y: panY }
      }
      const minPanX = rect.width - rect.width * atScale
      const minPanY = rect.height - rect.height * atScale
      return { x: clamp(panX, minPanX, 0), y: clamp(panY, minPanY, 0) }
    },
    [viewportRef],
  )

  const handleWheel = useCallback(
    (event: WheelEvent<HTMLElement>) => {
      event.preventDefault()
      const rect = viewportRef.current?.getBoundingClientRect()
      if (!rect || rect.width === 0 || rect.height === 0) {
        return
      }

      const cursorX = event.clientX - rect.left
      const cursorY = event.clientY - rect.top
      const zoomFactor = event.deltaY < 0 ? WHEEL_ZOOM_FACTOR : 1 / WHEEL_ZOOM_FACTOR

      setScale((currentScale) => {
        const nextScale = clamp(currentScale * zoomFactor, MIN_SCALE, MAX_SCALE)
        if (nextScale === currentScale) {
          return currentScale
        }

        setPan((currentPan) => {
          const contentX = (cursorX - currentPan.x) / currentScale
          const contentY = (cursorY - currentPan.y) / currentScale
          return clampPan(cursorX - contentX * nextScale, cursorY - contentY * nextScale, nextScale)
        })

        return nextScale
      })
    },
    [clampPan, viewportRef],
  )

  /** Attach to `onPointerDown` on the radar image itself (not on pins/annotations) — panning only engages when zoomed in. */
  const startPan = useCallback(
    (event: PointerEvent<HTMLElement>) => {
      if (scale <= MIN_SCALE) {
        return
      }

      event.preventDefault()
      event.currentTarget.setPointerCapture(event.pointerId)
      panDraftRef.current = {
        pointerId: event.pointerId,
        startClientX: event.clientX,
        startClientY: event.clientY,
        startPanX: pan.x,
        startPanY: pan.y,
      }
    },
    [pan, scale],
  )

  const handlePan = useCallback(
    (event: PointerEvent<HTMLElement>) => {
      const draft = panDraftRef.current
      if (!draft || draft.pointerId !== event.pointerId) {
        return
      }

      const rawPanX = draft.startPanX + (event.clientX - draft.startClientX)
      const rawPanY = draft.startPanY + (event.clientY - draft.startClientY)
      setPan(clampPan(rawPanX, rawPanY, scale))
    },
    [clampPan, scale],
  )

  const endPan = useCallback((event: PointerEvent<HTMLElement>) => {
    if (panDraftRef.current?.pointerId === event.pointerId) {
      panDraftRef.current = null
    }
  }, [])

  const resetZoom = useCallback(() => {
    setScale(MIN_SCALE)
    setPan({ x: 0, y: 0 })
  }, [])

  return {
    scale,
    panX: pan.x,
    panY: pan.y,
    isZoomed: scale > MIN_SCALE,
    handleWheel,
    startPan,
    handlePan,
    endPan,
    resetZoom,
  }
}
