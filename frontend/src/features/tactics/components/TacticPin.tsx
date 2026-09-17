import { memo, type PointerEvent } from 'react'

interface TacticPinProps {
  pointId: string
  /** 1-based position in the notes list, shown on the pin so it can be matched at a glance. */
  displayNumber: number
  x: number
  y: number
  hasDescription: boolean
  isSelected: boolean
  isDragging: boolean
  canEdit: boolean
  onDragStart: (event: PointerEvent<HTMLDivElement>, pointId: string) => void
}

/** A single numbered dot on a tactic's radar; draggable by Coach/Manager, always clickable to open its note. */
export const TacticPin = memo(function TacticPin({
  pointId,
  displayNumber,
  x,
  y,
  hasDescription,
  isSelected,
  isDragging,
  canEdit,
  onDragStart,
}: TacticPinProps) {
  return (
    <div className="absolute -translate-x-1/2 -translate-y-1/2" style={{ left: `${x * 100}%`, top: `${y * 100}%` }}>
      <div
        onPointerDown={(event) => onDragStart(event, pointId)}
        className={`flex h-7 w-7 items-center justify-center rounded-full border border-black/40 text-[11px] font-bold text-white shadow-md bg-red-600 ${
          canEdit ? 'cursor-grab touch-none active:cursor-grabbing' : 'cursor-pointer'
        } ${isDragging ? 'ring-2 ring-white' : ''} ${isSelected ? 'ring-2 ring-amber-300' : ''}`}
      >
        {displayNumber}
      </div>

      {hasDescription && (
        <span
          title="Ten punkt ma opis"
          className="absolute -right-0.5 -top-0.5 h-2 w-2 rounded-full border border-black/40 bg-amber-400"
        />
      )}
    </div>
  )
})
