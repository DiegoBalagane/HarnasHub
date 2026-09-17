import { useCallback, useRef, useState, type PointerEvent } from 'react'
import type { MapPosition, MapSide } from '../../../services/mapStrategyApi'
import type { MapName } from '../../../services/nadesApi'
import { useRemovePlayerPosition, useSetPlayerPosition } from '../hooks/useMapStrategy'
import { PlayerPin } from './PlayerPin'
import { PositionNoteEditor } from './PositionNoteEditor'

interface MapRadarProps {
  mapName: MapName
  side: MapSide
  positions: MapPosition[]
  /** Only Coach/Manager may drag pins around or delete them. */
  canEdit: boolean
}

interface DragDraft {
  positionId: string
  x: number
  y: number
  /** Distinguishes a real drag from a plain click, so a tap doesn't trigger a pointless save. */
  moved: boolean
}

function clampFraction(value: number): number {
  return Math.min(1, Math.max(0, value))
}

interface DeletedNote {
  position: MapPosition
  previousNote: string
}

const undoWindowMs = 6000

/** Radar image with the team's pins on top; Coach/Manager can drag a pin and the new spot is saved on pointer-up. */
export function MapRadar({ mapName, side, positions, canEdit }: MapRadarProps) {
  const containerRef = useRef<HTMLDivElement>(null)
  const [draft, setDraft] = useState<DragDraft | null>(null)
  const [editingNoteId, setEditingNoteId] = useState<string | null>(null)
  const [deletedNote, setDeletedNote] = useState<DeletedNote | null>(null)
  const undoTimeoutRef = useRef<ReturnType<typeof setTimeout> | null>(null)
  const setPlayerPosition = useSetPlayerPosition()
  const removePlayerPosition = useRemovePlayerPosition()
  const editingPosition = positions.find((position) => position.id === editingNoteId) ?? null

  function saveNote(position: MapPosition, note: string | null) {
    setPlayerPosition.mutate({
      mapName,
      side,
      userId: position.userId,
      label: position.label,
      x: position.x,
      y: position.y,
      note,
    })
  }

  function deleteNote(position: MapPosition) {
    if (!position.note) return

    if (undoTimeoutRef.current) {
      clearTimeout(undoTimeoutRef.current)
    }

    saveNote(position, null)
    setEditingNoteId(null)
    setDeletedNote({ position, previousNote: position.note })
    undoTimeoutRef.current = setTimeout(() => setDeletedNote(null), undoWindowMs)
  }

  function undoDelete() {
    if (!deletedNote) return

    if (undoTimeoutRef.current) {
      clearTimeout(undoTimeoutRef.current)
    }

    saveNote(deletedNote.position, deletedNote.previousNote)
    setDeletedNote(null)
  }

  const handleDragStart = useCallback(
    (event: PointerEvent<HTMLDivElement>, positionId: string) => {
      const position = positions.find((candidate) => candidate.id === positionId)

      if (!position) {
        return
      }

      event.preventDefault()
      // Capturing on the pin keeps move/up events flowing (and bubbling to this container) outside the radar too.
      event.currentTarget.setPointerCapture(event.pointerId)
      setDraft({ positionId, x: position.x, y: position.y, moved: false })
    },
    [positions],
  )

  const handlePointerMove = useCallback(
    (event: PointerEvent<HTMLDivElement>) => {
      const rect = containerRef.current?.getBoundingClientRect()

      if (!draft || !rect || rect.width === 0 || rect.height === 0) {
        return
      }

      setDraft({
        positionId: draft.positionId,
        x: clampFraction((event.clientX - rect.left) / rect.width),
        y: clampFraction((event.clientY - rect.top) / rect.height),
        moved: true,
      })
    },
    [draft],
  )

  // Saving happens once, here — not on every pointermove — so a drag costs a single request.
  const handlePointerUp = useCallback(() => {
    if (!draft) {
      return
    }

    const position = positions.find((candidate) => candidate.id === draft.positionId)

    if (!position) {
      setDraft(null)
      return
    }

    if (!draft.moved) {
      // A plain tap/click, not a drag — open (or close) the note editor for this pin instead of moving it.
      if (canEdit) {
        setEditingNoteId((current) => (current === position.id ? null : position.id))
      }
      setDraft(null)
      return
    }

    setPlayerPosition.mutate(
      {
        mapName,
        side,
        userId: position.userId,
        label: position.label,
        x: draft.x,
        y: draft.y,
        note: position.note,
      },
      // Held until the server answers so the pin stays where it was dropped instead of snapping back.
      { onSettled: () => setDraft(null) },
    )
  }, [canEdit, draft, mapName, positions, setPlayerPosition, side])

  return (
    <div className="flex flex-col gap-2">
      <div
        ref={containerRef}
        onPointerMove={handlePointerMove}
        onPointerUp={handlePointerUp}
        onPointerCancel={() => setDraft(null)}
        className="relative w-full select-none overflow-hidden rounded-md border border-neutral-800 bg-neutral-950"
      >
        <img
          src={`/maps/${mapName.toLowerCase()}.webp`}
          alt={`Radar mapy ${mapName}`}
          draggable={false}
          className="block h-auto w-full"
        />

        {positions.map((position) => {
          const isDragging = draft?.positionId === position.id

          return (
            <PlayerPin
              key={position.id}
              position={position}
              x={isDragging ? draft.x : position.x}
              y={isDragging ? draft.y : position.y}
              canEdit={canEdit}
              isDragging={isDragging}
              onDragStart={handleDragStart}
              onRemove={removePlayerPosition.mutate}
            />
          )
        })}
      </div>

      {editingPosition && (
        <PositionNoteEditor
          key={editingPosition.id}
          position={editingPosition}
          onSave={(note) => saveNote(editingPosition, note)}
          onDelete={() => deleteNote(editingPosition)}
          onClose={() => setEditingNoteId(null)}
          isSaving={setPlayerPosition.isPending}
        />
      )}

      {deletedNote && (
        <div className="flex items-center justify-between gap-3 rounded-md border border-neutral-700 bg-neutral-950 px-3 py-2 text-xs text-neutral-300">
          <span>
            Usunięto notatkę dla {deletedNote.position.inGameNickname ?? deletedNote.position.displayName}.
          </span>
          <button
            type="button"
            onClick={undoDelete}
            className="shrink-0 rounded-md border border-neutral-600 px-2 py-1 font-medium text-neutral-100 transition hover:border-neutral-400"
          >
            Cofnij
          </button>
        </div>
      )}
    </div>
  )
}
