import { useCallback, useState, type PointerEvent, type RefObject } from 'react'
import type { MapPosition, MapSide } from '../../../services/mapStrategyApi'
import type { MapName } from '../../../services/nadesApi'
import { useSetPlayerPosition } from './useMapStrategy'

interface PinDragDraft {
  positionId: string
  x: number
  y: number
  /** Distinguishes a real drag from a plain click, so a tap doesn't trigger a pointless save. */
  moved: boolean
}

function clampFraction(value: number): number {
  return Math.min(1, Math.max(0, value))
}

/** Drag-to-reposition for player pins on the radar. A plain tap (no movement) calls `onTap` instead of saving;
 * a real drag is saved once, on release, against `contentRef`'s on-screen box (so it stays correct under zoom/pan). */
export function usePinDrag(
  positions: MapPosition[],
  mapName: MapName,
  side: MapSide,
  contentRef: RefObject<HTMLElement | null>,
  onTap: (positionId: string) => void,
) {
  const [draft, setDraft] = useState<PinDragDraft | null>(null)
  const setPlayerPosition = useSetPlayerPosition()

  const dragStart = useCallback(
    (event: PointerEvent<HTMLDivElement>, positionId: string) => {
      const position = positions.find((candidate) => candidate.id === positionId)
      if (!position) return

      event.preventDefault()
      event.stopPropagation()
      event.currentTarget.setPointerCapture(event.pointerId)
      setDraft({ positionId, x: position.x, y: position.y, moved: false })
    },
    [positions],
  )

  const move = useCallback(
    (event: PointerEvent<HTMLElement>) => {
      setDraft((current) => {
        if (!current) return current
        const rect = contentRef.current?.getBoundingClientRect()
        if (!rect || rect.width === 0 || rect.height === 0) return current

        return {
          positionId: current.positionId,
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

    const position = positions.find((candidate) => candidate.id === draft.positionId)
    if (!position) {
      setDraft(null)
      return
    }

    if (!draft.moved) {
      onTap(position.id)
      setDraft(null)
      return
    }

    setPlayerPosition.mutate(
      { mapName, side, userId: position.userId, label: position.label, x: draft.x, y: draft.y, note: position.note },
      // Held until the server answers so the pin stays where it was dropped instead of snapping back.
      { onSettled: () => setDraft(null) },
    )
  }, [draft, mapName, onTap, positions, setPlayerPosition, side])

  return { draft, dragStart, move, end, isSaving: setPlayerPosition.isPending }
}
