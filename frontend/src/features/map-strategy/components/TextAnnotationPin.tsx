import { memo, type PointerEvent } from 'react'
import type { MapTextAnnotation } from '../../../services/mapStrategyApi'

interface TextAnnotationPinProps {
  annotation: MapTextAnnotation
  /** Radar-relative coordinates to render at — the live drag position, which may differ from the saved one. */
  x: number
  y: number
  canEdit: boolean
  isDragging: boolean
  onDragStart: (event: PointerEvent<HTMLElement>, annotationId: string) => void
  onRemove: (annotationId: string) => void
}

/** A free-floating text label on the radar; draggable, editable, and removable only for Coach/Manager. A plain tap
 * (vs. a drag) opens the editor — decided by the parent, same pattern as {@link PlayerPin}. */
export const TextAnnotationPin = memo(function TextAnnotationPin({
  annotation,
  x,
  y,
  canEdit,
  isDragging,
  onDragStart,
  onRemove,
}: TextAnnotationPinProps) {
  return (
    <div
      className="group absolute -translate-x-1/2 -translate-y-1/2"
      style={{ left: `${x * 100}%`, top: `${y * 100}%` }}
    >
      <span
        onPointerDown={canEdit ? (event) => onDragStart(event, annotation.id) : undefined}
        style={{ color: annotation.color, fontSize: `${annotation.fontSizePx}px` }}
        className={`inline-block whitespace-nowrap font-semibold [text-shadow:0_1px_2px_rgba(0,0,0,0.9)] ${
          canEdit ? 'cursor-grab touch-none active:cursor-grabbing' : 'cursor-default'
        } ${isDragging ? 'opacity-70' : ''}`}
      >
        {annotation.text}
      </span>

      {canEdit && !isDragging && (
        <button
          type="button"
          title="Usuń notatkę"
          onClick={() => onRemove(annotation.id)}
          className="absolute -right-3 -top-3 hidden h-4 w-4 items-center justify-center rounded-full bg-neutral-900 text-[10px] leading-none text-neutral-300 hover:text-red-400 group-hover:flex"
        >
          ×
        </button>
      )}
    </div>
  )
})
