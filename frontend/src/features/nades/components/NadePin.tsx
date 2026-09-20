import { memo, type PointerEvent } from 'react'
import type { NadeEntry } from '../../../services/nadesApi'
import { grenadeTypeColors, grenadeTypeMarks } from '../labels'

interface NadePinProps {
  nade: NadeEntry
  /** Radar-relative coordinates to render at — the live drag position, which may differ from the saved one. */
  x: number
  y: number
  canEdit: boolean
  isSelected: boolean
  isDragging: boolean
  onDragStart: (event: PointerEvent<HTMLDivElement>, nadeId: string) => void
}

/** A single grenade's landing spot on the radar; draggable by its author or Coach/Manager, always clickable to open its details. */
export const NadePin = memo(function NadePin({ nade, x, y, canEdit, isSelected, isDragging, onDragStart }: NadePinProps) {
  return (
    <div className="absolute -translate-x-1/2 -translate-y-1/2" style={{ left: `${x * 100}%`, top: `${y * 100}%` }}>
      <div
        title={nade.title}
        onPointerDown={(event) => onDragStart(event, nade.id)}
        className={`flex h-7 w-7 items-center justify-center rounded-full border border-black/40 text-[11px] font-bold text-neutral-950 shadow-md ${grenadeTypeColors[nade.type]} ${
          canEdit ? 'cursor-grab touch-none active:cursor-grabbing' : 'cursor-pointer'
        } ${isDragging ? 'ring-2 ring-white' : ''} ${isSelected ? 'ring-2 ring-amber-300' : ''}`}
      >
        {grenadeTypeMarks[nade.type]}
      </div>
    </div>
  )
})
