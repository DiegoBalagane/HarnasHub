import { memo, useState } from 'react'
import type { MapVetoSuggestion } from '../../../services/vetoApi'
import { useVetoSuggestion } from '../hooks/useVeto'
import { vetoActionClasses, vetoRecommendationLabels } from '../labels'

/** Suggested veto against an opponent: maps from best pick to first ban, each expandable to the reasons behind its score. */
export function VetoSuggestionPanel({ opponentName }: { opponentName: string }) {
  const { data: suggestion, isLoading, isError } = useVetoSuggestion(opponentName)

  return (
    <section className="flex flex-col gap-2">
      <h2 className="text-lg font-medium">Asystent veto</h2>
      <p className="text-xs text-neutral-500">
        Na podstawie puli map, Waszych wyników (ogólnie i z tym przeciwnikiem), taktyk w bibliotece i
        zapisanych veto przeciwnika. To podpowiedź, nie wyrocznia — kliknij mapę, żeby zobaczyć dlaczego.
      </p>

      {isLoading && <p className="text-sm text-neutral-400">Liczenie sugestii…</p>}
      {isError && <p className="text-sm text-danger-400">Nie udało się pobrać sugestii veto.</p>}

      {suggestion && (
        <>
          <ol className="flex flex-col gap-1">
            {suggestion.maps.map((map) => (
              <SuggestionRow key={map.mapName} map={map} />
            ))}
          </ol>

          <p className="text-xs text-neutral-500">
            {suggestion.recordedOpponentVetoes === 0
              ? 'Brak zapisanych veto przeciwnika — zapisz veto przy wydarzeniu, a asystent zacznie uwzględniać ich nawyki.'
              : `Nawyki przeciwnika z ${suggestion.recordedOpponentVetoes} zapisanych veto: ` +
                suggestion.opponentTendencies
                  .map(
                    (t) =>
                      `${t.mapName} (${[t.picks && `pick ${t.picks}×`, t.bans && `ban ${t.bans}×`].filter(Boolean).join(', ')})`,
                  )
                  .join(', ')}
          </p>
        </>
      )}
    </section>
  )
}

/** One suggested map; clicking it reveals the reasons. */
const SuggestionRow = memo(function SuggestionRow({ map }: { map: MapVetoSuggestion }) {
  const [isOpen, setIsOpen] = useState(false)

  return (
    <li className="rounded-md border border-neutral-800">
      <button
        type="button"
        onClick={() => setIsOpen((current) => !current)}
        aria-expanded={isOpen}
        className="flex w-full items-center justify-between gap-2 px-3 py-2 text-left text-sm"
      >
        <span className="flex items-center gap-2">
          <span
            className={`w-14 rounded-full border px-2 py-0.5 text-center text-xs ${vetoActionClasses[map.recommendation]}`}
          >
            {vetoRecommendationLabels[map.recommendation]}
          </span>
          {map.mapName}
        </span>
        <span className="tabular-nums text-neutral-500">
          {map.score > 0 ? `+${map.score}` : map.score} {isOpen ? '▲' : '▼'}
        </span>
      </button>
      {isOpen && (
        <ul className="list-disc px-8 pb-2 text-xs text-neutral-400">
          {map.reasons.map((reason) => (
            <li key={reason}>{reason}</li>
          ))}
        </ul>
      )}
    </li>
  )
})
