import { useCallback, useState, type PointerEvent, type RefObject } from 'react'
import type { MapTextAnnotation } from '../../../services/mapStrategyApi'
import { useUpdateTextAnnotation } from './useMapStrategy'

interface AnnotationDragDraft {
  annotationId: string
  x: number
  y: number
  /** Distinguishes a real drag from a plain click, so a tap doesn't trigger a pointless save. */
  moved: boolean
}

function clampFraction(value: number): number {
  return Math.min(1, Math.max(0, value))
}

/** Drag-to-reposition for text annotations on the radar — same shape as {@link usePinDrag}, kept separate since
 * an annotation's save payload (text/color/fontSize) differs from a pin's. */
export function useAnnotationDrag(
  annotations: MapTextAnnotation[],
  contentRef: RefObject<HTMLElement | null>,
  onTap: (annotationId: string) => void,
) {
  const [draft, setDraft] = useState<AnnotationDragDraft | null>(null)
  const updateTextAnnotation = useUpdateTextAnnotation()

  const dragStart = useCallback(
    (event: PointerEvent<HTMLElement>, annotationId: string) => {
      const annotation = annotations.find((candidate) => candidate.id === annotationId)
      if (!annotation) return

      event.preventDefault()
      event.stopPropagation()
      event.currentTarget.setPointerCapture(event.pointerId)
      setDraft({ annotationId, x: annotation.x, y: annotation.y, moved: false })
    },
    [annotations],
  )

  const move = useCallback(
    (event: PointerEvent<HTMLElement>) => {
      setDraft((current) => {
        if (!current) return current
        const rect = contentRef.current?.getBoundingClientRect()
        if (!rect || rect.width === 0 || rect.height === 0) return current

        return {
          annotationId: current.annotationId,
          x: clampFraction((event.clientX - rect.left) / rect.width),
          y: clampFraction((event.clientY - rect.top) / rect.height),
          moved: true,
        }
      })
    },
    [contentRef],
  )

  const end = useCallback(() => {
    if (!draft) return

    const annotation = annotations.find((candidate) => candidate.id === draft.annotationId)
    if (!annotation) {
      setDraft(null)
      return
    }

    if (!draft.moved) {
      onTap(annotation.id)
      setDraft(null)
      return
    }

    updateTextAnnotation.mutate(
      {
        annotationId: annotation.id,
        text: annotation.text,
        color: annotation.color,
        fontSizePx: annotation.fontSizePx,
        x: draft.x,
        y: draft.y,
      },
      // Held until the server answers so the label stays where it was dropped instead of snapping back.
      { onSettled: () => setDraft(null) },
    )
  }, [annotations, draft, onTap, updateTextAnnotation])

  return { draft, dragStart, move, end, isSaving: updateTextAnnotation.isPending }
}
