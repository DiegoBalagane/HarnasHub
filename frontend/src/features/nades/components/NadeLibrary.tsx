import { useState } from 'react'
import { useAuthStore } from '../../auth/stores/useAuthStore'
import type { GrenadeType } from '../../../services/nadesApi'
import { useDeleteNade, useNades } from '../hooks/useNades'
import { commonMaps, grenadeTypeLabels } from '../labels'
import { getYoutubeEmbedUrl } from '../youtube'

const grenadeTypes: GrenadeType[] = ['Smoke', 'Flash', 'Molotov', 'Frag']
const coachRoles = new Set(['Coach', 'Manager'])

/** Filterable per-map nade library with inline YouTube previews. */
export function NadeLibrary() {
  const [mapName, setMapName] = useState('')
  const [type, setType] = useState<GrenadeType | ''>('')
  const {
    data: nades,
    isLoading,
    isError,
  } = useNades({ mapName: mapName || undefined, type: type || undefined })
  const { userId, role } = useAuthStore()
  const deleteNade = useDeleteNade()
  const canModerate = role !== null && coachRoles.has(role)

  return (
    <div className="flex w-full max-w-2xl flex-col gap-4">
      <div className="flex gap-3">
        <select
          value={mapName}
          onChange={(event) => setMapName(event.target.value)}
          className="rounded-md border border-neutral-800 bg-neutral-900 px-3 py-2 text-sm outline-none focus:border-neutral-500"
        >
          <option value="">Wszystkie mapy</option>
          {commonMaps.map((map) => (
            <option key={map} value={map}>
              {map}
            </option>
          ))}
        </select>

        <select
          value={type}
          onChange={(event) => setType(event.target.value as GrenadeType | '')}
          className="rounded-md border border-neutral-800 bg-neutral-900 px-3 py-2 text-sm outline-none focus:border-neutral-500"
        >
          <option value="">Wszystkie typy</option>
          {grenadeTypes.map((grenadeType) => (
            <option key={grenadeType} value={grenadeType}>
              {grenadeTypeLabels[grenadeType]}
            </option>
          ))}
        </select>
      </div>

      {isLoading && <p className="text-neutral-400">Ładowanie…</p>}
      {isError && <p className="text-red-400">Nie udało się pobrać granatów.</p>}
      {nades?.length === 0 && <p className="text-neutral-400">Brak pozycji spełniających filtry.</p>}

      <ul className="flex flex-col gap-3">
        {nades?.map((nade) => {
          const embedUrl = nade.youtubeUrl ? getYoutubeEmbedUrl(nade.youtubeUrl) : null
          const canDelete = canModerate || nade.createdByUserId === userId

          return (
            <li key={nade.id} className="rounded-md border border-neutral-800 p-4">
              <div className="flex items-start justify-between">
                <div>
                  <p className="font-medium">{nade.title}</p>
                  <p className="text-sm text-neutral-400">
                    {nade.mapName} · {grenadeTypeLabels[nade.type]}
                  </p>
                </div>
                {canDelete && (
                  <button
                    onClick={() => deleteNade.mutate(nade.id)}
                    disabled={deleteNade.isPending}
                    className="text-xs text-neutral-500 hover:text-red-400"
                  >
                    Usuń
                  </button>
                )}
              </div>

              {nade.description && <p className="mt-2 text-sm text-neutral-400">{nade.description}</p>}

              {embedUrl && (
                <iframe
                  className="mt-3 aspect-video w-full rounded-md"
                  src={embedUrl}
                  title={nade.title}
                  allowFullScreen
                />
              )}
            </li>
          )
        })}
      </ul>
    </div>
  )
}
