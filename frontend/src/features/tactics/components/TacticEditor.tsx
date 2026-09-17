import { useCallback, useEffect, useRef, useState, type MouseEvent, type PointerEvent } from 'react'
import { useIsCoachOrManager } from '../../auth/hooks/useIsCoachOrManager'
import { mapSideLabels } from '../../map-strategy/labels'
import type { EconomyType } from '../../../services/tacticsApi'
import { useDeleteTactic, useTacticDetail, useUpdateTactic } from '../hooks/useTactics'
import { economyLabels, economyTypes } from '../labels'
import { TacticPin } from './TacticPin'
import { TacticPointEditor } from './TacticPointEditor'

interface TacticEditorProps {
  tacticId: string
  onClose: () => void
}

interface WorkingPoint {
  id: string
  x: number
  y: number
  description: string | null
  nadeEntryId: string | null
}

interface DragDraft {
  pointId: string
  x: number
  y: number
  /** Distinguishes a real drag from a plain click, so a tap opens the note instead of "moving" the pin in place. */
  moved: boolean
}

function clampFraction(value: number): number {
  return Math.min(1, Math.max(0, value))
}

/** Radar-based editor for one tactic: drag pins into place, attach a note (and optionally a nade lineup) to each, save once. */
export function TacticEditor({ tacticId, onClose }: TacticEditorProps) {
  const { data: tactic, isLoading, isError } = useTacticDetail(tacticId)
  const canEdit = useIsCoachOrManager()
  const updateTactic = useUpdateTactic(tacticId)
  const deleteTactic = useDeleteTactic()

  const containerRef = useRef<HTMLDivElement>(null)
  const [name, setName] = useState('')
  const [economy, setEconomy] = useState<EconomyType>('Eco')
  const [note, setNote] = useState('')
  const [points, setPoints] = useState<WorkingPoint[]>([])
  const [selectedPointId, setSelectedPointId] = useState<string | null>(null)
  const [draft, setDraft] = useState<DragDraft | null>(null)

  useEffect(() => {
    if (!tactic) return
    setName(tactic.name)
    setEconomy(tactic.economy)
    setNote(tactic.note ?? '')
    setPoints(tactic.points.map((point) => ({ ...point })))
  }, [tactic])

  const handleDragStart = useCallback(
    (event: PointerEvent<HTMLDivElement>, pointId: string) => {
      const point = points.find((candidate) => candidate.id === pointId)
      if (!point) return

      event.preventDefault()
      event.stopPropagation()
      event.currentTarget.setPointerCapture(event.pointerId)
      setDraft({ pointId, x: point.x, y: point.y, moved: false })
    },
    [points],
  )

  const handlePointerMove = useCallback(
    (event: PointerEvent<HTMLDivElement>) => {
      const rect = containerRef.current?.getBoundingClientRect()
      if (!draft || !rect || rect.width === 0 || rect.height === 0) return

      setDraft({
        pointId: draft.pointId,
        x: clampFraction((event.clientX - rect.left) / rect.width),
        y: clampFraction((event.clientY - rect.top) / rect.height),
        moved: true,
      })
    },
    [draft],
  )

  const handlePointerUp = useCallback(() => {
    if (!draft) return

    if (!draft.moved) {
      setSelectedPointId((current) => (current === draft.pointId ? null : draft.pointId))
      setDraft(null)
      return
    }

    setPoints((current) =>
      current.map((point) => (point.id === draft.pointId ? { ...point, x: draft.x, y: draft.y } : point)),
    )
    setDraft(null)
  }, [draft])

  function handleRadarClick(event: MouseEvent<HTMLImageElement>) {
    if (!canEdit) return

    const rect = event.currentTarget.getBoundingClientRect()
    const newPoint: WorkingPoint = {
      id: crypto.randomUUID(),
      x: clampFraction((event.clientX - rect.left) / rect.width),
      y: clampFraction((event.clientY - rect.top) / rect.height),
      description: null,
      nadeEntryId: null,
    }

    setPoints((current) => [...current, newPoint])
    setSelectedPointId(newPoint.id)
  }

  function handleUpdatePoint(pointId: string, changes: { description: string | null; nadeEntryId: string | null }) {
    setPoints((current) => current.map((point) => (point.id === pointId ? { ...point, ...changes } : point)))
  }

  function handleRemovePoint(pointId: string) {
    setPoints((current) => current.filter((point) => point.id !== pointId))
    setSelectedPointId((current) => (current === pointId ? null : current))
  }

  function handleSave() {
    updateTactic.mutate({
      name,
      economy,
      note: note || null,
      points: points.map((point) => ({
        x: point.x,
        y: point.y,
        description: point.description,
        nadeEntryId: point.nadeEntryId,
      })),
    })
  }

  function handleDelete() {
    deleteTactic.mutate(tacticId, { onSuccess: onClose })
  }

  if (isLoading) return <p className="text-neutral-400">Ładowanie…</p>
  if (isError || !tactic) return <p className="text-red-400">Nie udało się pobrać taktyki.</p>

  const selectedIndex = points.findIndex((point) => point.id === selectedPointId)
  const selectedPoint = selectedIndex >= 0 ? points[selectedIndex] : null

  return (
    <div className="flex w-full max-w-3xl flex-col gap-4">
      <div className="flex items-center justify-between">
        <button type="button" onClick={onClose} className="text-sm text-neutral-400 hover:text-white">
          ← Wróć do listy
        </button>
        <span className="text-xs text-neutral-500">
          {tactic.mapName} · {mapSideLabels[tactic.side]}
        </span>
      </div>

      {canEdit ? (
        <div className="flex flex-col gap-3 rounded-md border border-neutral-800 p-4">
          <div className="flex flex-wrap gap-3">
            <input
              value={name}
              onChange={(event) => setName(event.target.value)}
              placeholder="Nazwa taktyki"
              className="flex-1 rounded-md border border-neutral-800 bg-neutral-900 px-3 py-2 text-sm outline-none focus:border-neutral-500"
            />
            <select
              value={economy}
              onChange={(event) => setEconomy(event.target.value as EconomyType)}
              className="rounded-md border border-neutral-800 bg-neutral-900 px-3 py-2 text-sm outline-none focus:border-neutral-500"
            >
              {economyTypes.map((type) => (
                <option key={type} value={type}>
                  {economyLabels[type]}
                </option>
              ))}
            </select>
          </div>

          <textarea
            value={note}
            onChange={(event) => setNote(event.target.value)}
            placeholder="Notatka ogólna (opcjonalnie)"
            className="rounded-md border border-neutral-800 bg-neutral-900 px-3 py-2 text-sm outline-none focus:border-neutral-500"
          />

          {updateTactic.isError && <p className="text-sm text-red-400">Nie udało się zapisać taktyki.</p>}

          <div className="flex gap-2">
            <button
              type="button"
              onClick={handleSave}
              disabled={updateTactic.isPending || name.trim() === ''}
              className="self-start rounded-md bg-red-600 px-4 py-2 text-sm font-medium text-white transition hover:bg-red-500 disabled:opacity-50"
            >
              {updateTactic.isPending ? 'Zapisywanie…' : 'Zapisz'}
            </button>
            <button
              type="button"
              onClick={handleDelete}
              disabled={deleteTactic.isPending}
              className="rounded-md border border-neutral-700 px-4 py-2 text-sm text-neutral-300 transition hover:border-red-500 hover:text-red-400 disabled:opacity-50"
            >
              Usuń taktykę
            </button>
          </div>
        </div>
      ) : (
        <div className="rounded-md border border-neutral-800 p-4">
          <div className="flex items-center justify-between">
            <p className="text-lg font-medium">{tactic.name}</p>
            <span className="rounded bg-neutral-800 px-2 py-0.5 text-xs text-neutral-300">
              {economyLabels[tactic.economy]}
            </span>
          </div>
          {tactic.note && <p className="mt-1 text-sm text-neutral-400">{tactic.note}</p>}
        </div>
      )}

      <p className="text-xs text-neutral-500">
        {canEdit
          ? 'Kliknij puste miejsce na radarze, aby dodać punkt. Przeciągnij pinezkę, aby ją przesunąć, kliknij, aby edytować opis.'
          : 'Kliknij pinezkę, aby zobaczyć opis tego punktu.'}
      </p>

      <div
        ref={containerRef}
        onPointerMove={handlePointerMove}
        onPointerUp={handlePointerUp}
        onPointerCancel={() => setDraft(null)}
        className="relative w-full select-none overflow-hidden rounded-md border border-neutral-800 bg-neutral-950"
      >
        <img
          src={`/maps/${tactic.mapName.toLowerCase()}.webp`}
          alt={`Radar mapy ${tactic.mapName}`}
          draggable={false}
          onClick={handleRadarClick}
          className={`block h-auto w-full ${canEdit ? 'cursor-copy' : ''}`}
        />

        {points.map((point, index) => {
          const isDragging = draft?.pointId === point.id

          return (
            <TacticPin
              key={point.id}
              pointId={point.id}
              displayNumber={index + 1}
              x={isDragging ? draft.x : point.x}
              y={isDragging ? draft.y : point.y}
              hasDescription={Boolean(point.description)}
              isSelected={selectedPointId === point.id}
              isDragging={isDragging}
              canEdit={canEdit}
              onDragStart={handleDragStart}
            />
          )
        })}
      </div>

      {selectedPoint && (
        <TacticPointEditor
          key={selectedPoint.id}
          displayNumber={selectedIndex + 1}
          mapName={tactic.mapName}
          description={selectedPoint.description ?? ''}
          nadeEntryId={selectedPoint.nadeEntryId}
          canEdit={canEdit}
          onChange={(changes) => handleUpdatePoint(selectedPoint.id, changes)}
          onRemove={() => handleRemovePoint(selectedPoint.id)}
          onClose={() => setSelectedPointId(null)}
        />
      )}

      {points.some((point) => point.description) && (
        <ul className="flex flex-col gap-2">
          {points.map(
            (point, index) =>
              point.description && (
                <li key={point.id} className="rounded-md border border-neutral-800 px-3 py-2 text-sm">
                  <span className="font-medium text-neutral-200">Punkt {index + 1}</span>
                  <p className="mt-0.5 text-neutral-400">{point.description}</p>
                </li>
              ),
          )}
        </ul>
      )}
    </div>
  )
}
