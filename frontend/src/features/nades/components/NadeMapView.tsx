import { useCallback, useMemo, useRef, useState, type MouseEvent, type PointerEvent } from 'react'
import type { GrenadeType, MapName, NadeEntry } from '../../../services/nadesApi'
import { YoutubeEmbed } from '../../../components/YoutubeEmbed'
import { useAuthStore } from '../../auth/stores/useAuthStore'
import { useIsCoachOrManager } from '../../auth/hooks/useIsCoachOrManager'
import { useNades, useUpdateNadePosition } from '../hooks/useNades'
import { grenadeTypeColors, grenadeTypeLabels, mapNames } from '../labels'
import { NadePin } from './NadePin'

const grenadeTypes: GrenadeType[] = ['Smoke', 'Flash', 'Molotov', 'Frag']

interface DragDraft {
  nadeId: string
  x: number
  y: number
  /** Distinguishes a real drag from a plain click, so a tap opens the details instead of nudging the pin. */
  moved: boolean
}

function clampFraction(value: number): number {
  return Math.min(1, Math.max(0, value))
}

/** Per-map radar of grenade landing spots: pick a map, see every lineup's pin, click one to watch it, drag to reposition. */
export function NadeMapView() {
  const [mapName, setMapName] = useState<MapName>(mapNames[0])
  const [visibleTypes, setVisibleTypes] = useState<Set<GrenadeType>>(new Set(grenadeTypes))
  const [selectedNadeId, setSelectedNadeId] = useState<string | null>(null)
  const [armedNadeId, setArmedNadeId] = useState<string | null>(null)
  const [draft, setDraft] = useState<DragDraft | null>(null)
  const containerRef = useRef<HTMLDivElement>(null)

  const userId = useAuthStore((state) => state.userId)
  const isCoachOrManager = useIsCoachOrManager()
  const { data: nades, isLoading, isError } = useNades({ mapName })
  const updatePosition = useUpdateNadePosition()

  const canEditNade = useCallback(
    (nade: NadeEntry): boolean => isCoachOrManager || nade.createdByUserId === userId,
    [isCoachOrManager, userId],
  )

  const allForMap = useMemo(
    () => (nades ?? []).filter((nade) => visibleTypes.has(nade.type)),
    [nades, visibleTypes],
  )
  const positioned = useMemo(
    () => allForMap.filter((nade) => nade.landingX !== null && nade.landingY !== null),
    [allForMap],
  )
  const unpositioned = useMemo(
    () => allForMap.filter((nade) => nade.landingX === null || nade.landingY === null),
    [allForMap],
  )
  const selectedNade = positioned.find((nade) => nade.id === selectedNadeId) ?? null

  function toggleType(type: GrenadeType) {
    setVisibleTypes((current) => {
      const next = new Set(current)
      if (next.has(type)) {
        next.delete(type)
      } else {
        next.add(type)
      }
      return next
    })
  }

  const handleDragStart = useCallback(
    (event: PointerEvent<HTMLDivElement>, nadeId: string) => {
      const nade = positioned.find((candidate) => candidate.id === nadeId)
      if (!nade || nade.landingX === null || nade.landingY === null) return

      event.preventDefault()
      event.currentTarget.setPointerCapture(event.pointerId)
      setDraft({ nadeId, x: nade.landingX, y: nade.landingY, moved: false })
    },
    [positioned],
  )

  const handlePointerMove = useCallback(
    (event: PointerEvent<HTMLDivElement>) => {
      const rect = containerRef.current?.getBoundingClientRect()
      if (!draft || !rect || rect.width === 0 || rect.height === 0) return

      setDraft({
        nadeId: draft.nadeId,
        x: clampFraction((event.clientX - rect.left) / rect.width),
        y: clampFraction((event.clientY - rect.top) / rect.height),
        moved: true,
      })
    },
    [draft],
  )

  const handlePointerUp = useCallback(() => {
    if (!draft) return

    const nade = positioned.find((candidate) => candidate.id === draft.nadeId)

    if (!nade || !canEditNade(nade)) {
      setDraft(null)
      return
    }

    if (!draft.moved) {
      setSelectedNadeId((current) => (current === nade.id ? null : nade.id))
      setDraft(null)
      return
    }

    updatePosition.mutate(
      { nadeId: nade.id, x: draft.x, y: draft.y },
      { onSettled: () => setDraft(null) },
    )
  }, [draft, positioned, canEditNade, updatePosition])

  function handleRadarClick(event: MouseEvent<HTMLImageElement>) {
    if (!armedNadeId) return

    const rect = event.currentTarget.getBoundingClientRect()
    const x = clampFraction((event.clientX - rect.left) / rect.width)
    const y = clampFraction((event.clientY - rect.top) / rect.height)

    updatePosition.mutate({ nadeId: armedNadeId, x, y })
    setArmedNadeId(null)
  }

  function clearPosition(nade: NadeEntry) {
    updatePosition.mutate({ nadeId: nade.id, x: null, y: null })
    setSelectedNadeId(null)
  }

  return (
    <section className="flex w-full max-w-3xl flex-col gap-4">
      <div className="flex flex-wrap items-center gap-3">
        <select
          value={mapName}
          onChange={(event) => {
            setMapName(event.target.value as MapName)
            setSelectedNadeId(null)
            setArmedNadeId(null)
          }}
          className="rounded-md border border-neutral-800 bg-neutral-900 px-3 py-2 text-sm outline-none focus:border-neutral-500"
        >
          {mapNames.map((map) => (
            <option key={map} value={map}>
              {map}
            </option>
          ))}
        </select>

        <div className="flex flex-wrap gap-1">
          {grenadeTypes.map((type) => (
            <button
              key={type}
              type="button"
              onClick={() => toggleType(type)}
              className={`flex items-center gap-1 rounded-md border px-2 py-1 text-xs transition ${
                visibleTypes.has(type)
                  ? 'border-neutral-600 text-neutral-100'
                  : 'border-neutral-800 text-neutral-600'
              }`}
            >
              <span className={`h-2 w-2 rounded-full ${grenadeTypeColors[type]}`} />
              {grenadeTypeLabels[type]}
            </button>
          ))}
        </div>
      </div>

      {isLoading && <p className="text-neutral-400">Ładowanie…</p>}
      {isError && <p className="text-red-400">Nie udało się pobrać granatów.</p>}

      {nades && (
        <>
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
              onClick={handleRadarClick}
              className={`block h-auto w-full ${armedNadeId ? 'cursor-copy' : ''}`}
            />

            {positioned.map((nade) => {
              const isDragging = draft?.nadeId === nade.id

              return (
                <NadePin
                  key={nade.id}
                  nade={nade}
                  x={isDragging ? draft.x : (nade.landingX ?? 0)}
                  y={isDragging ? draft.y : (nade.landingY ?? 0)}
                  canEdit={canEditNade(nade)}
                  isSelected={selectedNadeId === nade.id}
                  isDragging={isDragging}
                  onDragStart={handleDragStart}
                />
              )
            })}
          </div>

          <p className="text-xs text-neutral-500">
            {armedNadeId
              ? 'Kliknij na mapie, aby ustawić pozycję wybranego granatu.'
              : positioned.length === 0
                ? 'Żaden granat na tej mapie nie ma jeszcze pozycji na radarze.'
                : 'Kliknij pinezkę, aby zobaczyć wideo i opis. Przeciągnij, aby zmienić pozycję (własne wpisy lub coach/manager).'}
          </p>

          {selectedNade && (
            <div className="rounded-md border border-neutral-800 p-4">
              <div className="flex items-start justify-between gap-3">
                <div>
                  <p className="font-medium">{selectedNade.title}</p>
                  <p className="text-sm text-neutral-400">{grenadeTypeLabels[selectedNade.type]}</p>
                </div>
                <button
                  type="button"
                  onClick={() => setSelectedNadeId(null)}
                  className="shrink-0 text-xs text-neutral-500 hover:text-white"
                >
                  Zamknij
                </button>
              </div>

              {selectedNade.description && <p className="mt-2 text-sm text-neutral-400">{selectedNade.description}</p>}

              {selectedNade.youtubeUrl && (
                <YoutubeEmbed url={selectedNade.youtubeUrl} title={selectedNade.title} className="mt-3" />
              )}

              {canEditNade(selectedNade) && (
                <button
                  type="button"
                  onClick={() => clearPosition(selectedNade)}
                  disabled={updatePosition.isPending}
                  className="mt-3 text-xs text-neutral-500 hover:text-red-400"
                >
                  Usuń pozycję z mapy
                </button>
              )}
            </div>
          )}

          {unpositioned.length > 0 && (
            <div className="flex flex-col gap-2">
              <p className="text-sm font-medium text-neutral-300">Bez pozycji na mapie</p>
              <ul className="flex flex-col gap-2">
                {unpositioned.map((nade) => (
                  <li
                    key={nade.id}
                    className="flex items-center justify-between gap-3 rounded-md border border-neutral-800 px-3 py-2 text-sm"
                  >
                    <span>
                      <span className={`mr-2 inline-block h-2 w-2 rounded-full ${grenadeTypeColors[nade.type]}`} />
                      {nade.title}
                      <span className="ml-2 text-xs text-neutral-500">{grenadeTypeLabels[nade.type]}</span>
                    </span>
                    {canEditNade(nade) && (
                      <button
                        type="button"
                        onClick={() => setArmedNadeId(nade.id)}
                        disabled={armedNadeId === nade.id}
                        className="shrink-0 rounded-md border border-neutral-700 px-2 py-1 text-xs text-neutral-300 transition hover:border-neutral-500 disabled:opacity-50"
                      >
                        {armedNadeId === nade.id ? 'Kliknij na mapie…' : 'Ustaw pozycję'}
                      </button>
                    )}
                  </li>
                ))}
              </ul>
            </div>
          )}
        </>
      )}
    </section>
  )
}
