import { memo } from 'react'
import { grenadeTypeLabels } from '../../nades/labels'
import { useAddNade, useUpdateNadePosition } from '../../nades/hooks/useNades'
import type { GrenadeLibraryComparison, UnlistedGrenade } from '../../../services/matchAnalysisApi'
import type { MapName } from '../../../services/nadesApi'

interface GrenadeLibrarySectionProps {
  mapName: MapName
  comparison: GrenadeLibraryComparison
}

/** "Rzucacie X z Y trenowanych granatów": coverage of the nade library in this match, the never-thrown ones and unlisted spots to add. */
export const GrenadeLibrarySection = memo(function GrenadeLibrarySection({ mapName, comparison }: GrenadeLibrarySectionProps) {
  const addNade = useAddNade()
  const updatePosition = useUpdateNadePosition()
  const neverThrown = comparison.library.filter((nade) => nade.timesThrown === 0)

  async function addCandidate(candidate: UnlistedGrenade) {
    const created = await addNade.mutateAsync({
      mapName,
      type: candidate.type,
      title: `${grenadeTypeLabels[candidate.type]} — ${candidate.zone ?? 'z meczu'}`,
      description: `Dodane z analizy meczu (rzucone ${candidate.count}×).`,
    })
    await updatePosition.mutateAsync({ nadeId: created.id, x: candidate.landingX, y: candidate.landingY })
  }

  return (
    <section className="flex flex-col gap-3">
      <h2 className="text-lg font-semibold">Granaty a biblioteka</h2>
      <p className="text-sm text-neutral-300">
        Rzucacie <span className="font-semibold text-white">{comparison.trainedThrown}</span> z{' '}
        <span className="font-semibold text-white">{comparison.trainedTotal}</span> trenowanych granatów na tej mapie
        {' '}(łącznie rzuciliście {comparison.ourGrenadesThrown}).
      </p>
      <ul className="flex flex-wrap gap-2 text-xs">
        {comparison.byType.map((type) => (
          <li key={type.type} className="rounded-md border border-neutral-800 px-2 py-1 text-neutral-300">
            {grenadeTypeLabels[type.type]}: {type.thrown}/{type.total}
          </li>
        ))}
      </ul>

      {neverThrown.length > 0 && (
        <div>
          <h3 className="text-sm font-medium">Nigdy nie rzucone z biblioteki</h3>
          <ul className="mt-1 list-disc pl-5 text-sm text-neutral-400">
            {neverThrown.map((nade) => (
              <li key={nade.id}>
                {grenadeTypeLabels[nade.type]} — {nade.title}
              </li>
            ))}
          </ul>
        </div>
      )}

      {comparison.candidates.length > 0 && (
        <div>
          <h3 className="text-sm font-medium">Często rzucane, a spoza biblioteki</h3>
          <ul className="mt-1 flex flex-col gap-1">
            {comparison.candidates.map((candidate) => (
              <li
                key={`${candidate.type}-${candidate.landingX}-${candidate.landingY}`}
                className="flex items-center justify-between gap-2 rounded-md border border-neutral-800 px-3 py-2 text-sm"
              >
                <span>
                  {grenadeTypeLabels[candidate.type]}
                  {candidate.zone ? ` — ${candidate.zone}` : ''} · {candidate.count}×
                </span>
                <button
                  type="button"
                  disabled={addNade.isPending || updatePosition.isPending}
                  onClick={() => addCandidate(candidate)}
                  className="rounded-md border border-neutral-700 px-2 py-1 text-xs transition hover:bg-neutral-800 disabled:opacity-50"
                >
                  Dodaj do biblioteki
                </button>
              </li>
            ))}
          </ul>
          {(addNade.error || updatePosition.error) && (
            <p className="mt-1 text-xs text-danger-400">{(addNade.error ?? updatePosition.error)?.message}</p>
          )}
        </div>
      )}
    </section>
  )
})
