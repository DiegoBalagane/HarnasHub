import { useRef, useState, type PointerEvent } from 'react'
import type { MapPosition, MapSide, MapTextAnnotation } from '../../../services/mapStrategyApi'
import type { MapName } from '../../../services/nadesApi'
import { useAnnotationDrag } from '../hooks/useAnnotationDrag'
import { useMapZoom } from '../hooks/useMapZoom'
import { usePinDrag } from '../hooks/usePinDrag'
import { useUndoableNoteDelete } from '../hooks/useUndoableNoteDelete'
import { useRemovePlayerPosition, useRemoveTextAnnotation, useSetPlayerPosition, useUpdateTextAnnotation } from '../hooks/useMapStrategy'
import { PlayerPin } from './PlayerPin'
import { PositionNoteEditor } from './PositionNoteEditor'
import { TextAnnotationEditor } from './TextAnnotationEditor'
import { TextAnnotationPin } from './TextAnnotationPin'

interface MapRadarProps {
  mapName: MapName
  side: MapSide
  positions: MapPosition[]
  annotations: MapTextAnnotation[]
  /** Only Coach/Manager may drag pins/annotations around, edit annotations, or delete either. */
  canEdit: boolean
}

/** Radar image with the team's pins and text annotations on top; Coach/Manager can drag either, zoom/pan the image
 * with the scroll wheel, and edit or delete annotations. A drag's new spot is saved on pointer-up. */
export function MapRadar({ mapName, side, positions, annotations, canEdit }: MapRadarProps) {
  const viewportRef = useRef<HTMLDivElement>(null)
  const contentRef = useRef<HTMLDivElement>(null)
  const zoom = useMapZoom(viewportRef)
  const [editingNoteId, setEditingNoteId] = useState<string | null>(null)
  const [editingAnnotationId, setEditingAnnotationId] = useState<string | null>(null)
  const setPlayerPosition = useSetPlayerPosition()
  const removePlayerPosition = useRemovePlayerPosition()
  const updateTextAnnotation = useUpdateTextAnnotation()
  const removeTextAnnotation = useRemoveTextAnnotation()

  const pin = usePinDrag(positions, mapName, side, contentRef, (id) =>
    setEditingNoteId((current) => (current === id ? null : id)),
  )
  const annotation = useAnnotationDrag(annotations, contentRef, (id) =>
    setEditingAnnotationId((current) => (current === id ? null : id)),
  )
  const { deletedNote, deleteNote, undoDelete } = useUndoableNoteDelete((position, note) =>
    setPlayerPosition.mutate({ mapName, side, userId: position.userId, label: position.label, x: position.x, y: position.y, note }),
  )

  const editingPosition = positions.find((position) => position.id === editingNoteId) ?? null
  const editingAnnotation = annotations.find((candidate) => candidate.id === editingAnnotationId) ?? null

  function handlePointerMove(event: PointerEvent<HTMLDivElement>) {
    pin.move(event)
    annotation.move(event)
    zoom.handlePan(event)
  }

  function handlePointerUp(event: PointerEvent<HTMLDivElement>) {
    pin.end()
    annotation.end()
    zoom.endPan(event)
  }

  return (
    <div className="flex flex-col gap-2">
      <div
        ref={viewportRef}
        onWheel={zoom.handleWheel}
        onPointerMove={handlePointerMove}
        onPointerUp={handlePointerUp}
        onPointerCancel={handlePointerUp}
        className="relative w-full select-none overflow-hidden rounded-md border border-neutral-800 bg-neutral-950"
      >
        <div
          ref={contentRef}
          style={{ transform: `translate(${zoom.panX}px, ${zoom.panY}px) scale(${zoom.scale})`, transformOrigin: '0 0' }}
          className="relative w-full"
        >
          <img
            src={`/maps/${mapName.toLowerCase()}.webp`}
            alt={`Radar mapy ${mapName}`}
            draggable={false}
            onPointerDown={zoom.startPan}
            className={`block h-auto w-full ${zoom.isZoomed ? 'cursor-grab active:cursor-grabbing' : ''}`}
          />

          {positions.map((position) => (
            <PlayerPin
              key={position.id}
              position={position}
              x={pin.draft?.positionId === position.id ? pin.draft.x : position.x}
              y={pin.draft?.positionId === position.id ? pin.draft.y : position.y}
              canEdit={canEdit}
              isDragging={pin.draft?.positionId === position.id}
              onDragStart={pin.dragStart}
              onRemove={removePlayerPosition.mutate}
            />
          ))}

          {annotations.map((item) => (
            <TextAnnotationPin
              key={item.id}
              annotation={item}
              x={annotation.draft?.annotationId === item.id ? annotation.draft.x : item.x}
              y={annotation.draft?.annotationId === item.id ? annotation.draft.y : item.y}
              canEdit={canEdit}
              isDragging={annotation.draft?.annotationId === item.id}
              onDragStart={annotation.dragStart}
              onRemove={removeTextAnnotation.mutate}
            />
          ))}
        </div>

        {zoom.isZoomed && (
          <button
            type="button"
            onClick={zoom.resetZoom}
            className="absolute right-2 top-2 rounded-md border border-neutral-700 bg-neutral-950/80 px-2 py-1 text-xs text-neutral-200 hover:border-neutral-500"
          >
            Resetuj powiększenie
          </button>
        )}
      </div>

      {editingPosition && (
        <PositionNoteEditor
          key={editingPosition.id}
          position={editingPosition}
          onSave={(note) =>
            setPlayerPosition.mutate({
              mapName,
              side,
              userId: editingPosition.userId,
              label: editingPosition.label,
              x: editingPosition.x,
              y: editingPosition.y,
              note,
            })
          }
          onDelete={() => {
            deleteNote(editingPosition)
            setEditingNoteId(null)
          }}
          onClose={() => setEditingNoteId(null)}
          isSaving={setPlayerPosition.isPending}
        />
      )}

      {editingAnnotation && (
        <TextAnnotationEditor
          key={editingAnnotation.id}
          annotation={editingAnnotation}
          onSave={(text, color, fontSizePx) =>
            updateTextAnnotation.mutate({
              annotationId: editingAnnotation.id,
              text,
              color,
              fontSizePx,
              x: editingAnnotation.x,
              y: editingAnnotation.y,
            })
          }
          onDelete={() => {
            removeTextAnnotation.mutate(editingAnnotation.id)
            setEditingAnnotationId(null)
          }}
          onClose={() => setEditingAnnotationId(null)}
          isSaving={updateTextAnnotation.isPending}
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
