import { memo, type PointerEvent } from 'react'
import type { MapPosition } from '../../../services/mapStrategyApi'
import { toInitials } from '../../availability/weekDates'
import { pinColorSwatch, teamRoleLabels } from '../../roster/labels'
import { pinColorFor } from '../pinColors'

interface PlayerPinProps {
  position: MapPosition
  /** Radar-relative coordinates to render at — the live drag position, which may differ from the saved one. */
  x: number
  y: number
  canEdit: boolean
  isDragging: boolean
  onDragStart: (event: PointerEvent<HTMLDivElement>, positionId: string) => void
  onRemove: (positionId: string) => void
}

/** A single player dot on the radar; draggable and removable only for Coach/Manager. */
export const PlayerPin = memo(function PlayerPin({
  position,
  x,
  y,
  canEdit,
  isDragging,
  onDragStart,
  onRemove,
}: PlayerPinProps) {
  const name = position.inGameNickname ?? position.displayName
  const role = position.teamRole ? teamRoleLabels[position.teamRole] : 'brak roli'
  const details = [position.label, position.note].filter(Boolean).join(' — ')
  const tooltip = `${name} (${role})${details ? ` · ${details}` : ''}`
  // A self-chosen colour (Main roster only) wins; everyone else keeps the automatic per-user colour.
  const colorClass = position.pinColor ? pinColorSwatch[position.pinColor] : pinColorFor(position.userId)

  return (
    <div
      className="group absolute -translate-x-1/2 -translate-y-1/2"
      style={{ left: `${x * 100}%`, top: `${y * 100}%` }}
    >
      <div
        title={tooltip}
        onPointerDown={canEdit ? (event) => onDragStart(event, position.id) : undefined}
        className={`flex h-7 w-7 items-center justify-center rounded-full border border-black/40 text-[10px] font-bold text-white shadow-md ${colorClass} ${
          canEdit ? 'cursor-grab touch-none active:cursor-grabbing' : 'cursor-default'
        } ${isDragging ? 'ring-2 ring-white' : ''}`}
      >
        {toInitials(name)}
      </div>

      {position.note && (
        <span
          title="Ta pozycja ma notatkę"
          className="absolute -right-0.5 -top-0.5 h-2 w-2 rounded-full border border-black/40 bg-amber-400"
        />
      )}

      {position.label && (
        <span className="pointer-events-none absolute left-1/2 top-full mt-0.5 -translate-x-1/2 whitespace-nowrap rounded bg-black/70 px-1 text-[9px] text-neutral-200">
          {position.label}
        </span>
      )}

      {canEdit && !isDragging && (
        <button
          type="button"
          title="Usuń pozycję"
          onClick={() => onRemove(position.id)}
          className="absolute -right-2 -top-2 hidden h-4 w-4 items-center justify-center rounded-full bg-neutral-900 text-[10px] leading-none text-neutral-300 hover:text-red-400 group-hover:flex"
        >
          ×
        </button>
      )}
    </div>
  )
})
