import { useState } from 'react'
import { Link } from 'react-router-dom'
import type { MatchCategory } from '../../../services/resultsApi'
import { mapNames } from '../../nades/labels'
import { matchCategories, matchCategoryLabels } from '../../results/labels'
import { lastOptions } from '../advancedStats'
import { useAdvancedStats } from '../hooks/useAdvancedStats'
import { AdvancedStatsTable } from './AdvancedStatsTable'
import { MapRatingHeatmap } from './MapRatingHeatmap'
import { PlayerFormChart } from './PlayerFormChart'

const selectClass =
  'rounded-md border border-neutral-800 bg-neutral-900 px-3 py-2 text-sm outline-none focus:border-neutral-500'

/** "Zaawansowane" tab of the Rozwoj page: filters, per-player metrics table, per-map rating heatmap and form chart,
 * all computed from the stored match timelines (demos). */
export function AdvancedStatsTab() {
  const [category, setCategory] = useState<MatchCategory | 'all'>('all')
  const [map, setMap] = useState('all')
  const [last, setLast] = useState('all')
  const { data, isLoading, isError } = useAdvancedStats(
    category === 'all' ? undefined : category,
    map === 'all' ? undefined : map,
    last === 'all' ? undefined : Number(last),
  )

  return (
    <section className="flex w-full flex-col gap-6">
      <div className="flex flex-wrap gap-2">
        <select
          aria-label="Kategoria"
          value={category}
          onChange={(e) => setCategory(e.target.value as MatchCategory | 'all')}
          className={selectClass}
        >
          <option value="all">Wszystkie kategorie</option>
          {matchCategories.map((value) => (
            <option key={value} value={value}>
              {matchCategoryLabels[value]}
            </option>
          ))}
        </select>
        <select aria-label="Mapa" value={map} onChange={(e) => setMap(e.target.value)} className={selectClass}>
          <option value="all">Wszystkie mapy</option>
          {mapNames.map((name) => (
            <option key={name} value={name}>
              {name}
            </option>
          ))}
        </select>
        <select aria-label="Liczba meczów" value={last} onChange={(e) => setLast(e.target.value)} className={selectClass}>
          {lastOptions.map((option) => (
            <option key={option.label} value={option.value ?? 'all'}>
              {option.label}
            </option>
          ))}
        </select>
      </div>

      {isLoading && <p className="text-neutral-400">Ładowanie statystyk z demek…</p>}
      {isError && <p className="text-danger-400">Nie udało się pobrać zaawansowanych statystyk.</p>}

      {data && data.players.length === 0 && (
        <p className="text-neutral-400">
          Brak meczów z demką w tym filtrze — dołącz demkę do wyniku w{' '}
          <Link to="/results" className="text-primary-400 hover:underline">
            Wynikach
          </Link>
          .
        </p>
      )}

      {data && data.players.length > 0 && (
        <>
          <p className="text-sm text-neutral-400">
            Na podstawie {data.matchesAnalyzed} meczów z demką
            {data.matchesSkipped > 0 && ` (pominięto ${data.matchesSkipped} z nieczytelną osią czasu)`}.
          </p>
          <div className="flex flex-col gap-2">
            <h2 className="font-medium">Statystyki zaawansowane</h2>
            <AdvancedStatsTable players={data.players} />
          </div>
          <div className="flex flex-col gap-2">
            <h2 className="font-medium">Rating na mapach</h2>
            <MapRatingHeatmap players={data.players} cells={data.mapCells} />
          </div>
          <div className="flex flex-col gap-2">
            <h2 className="font-medium">Forma w kolejnych meczach</h2>
            <PlayerFormChart players={data.players} form={data.form} />
          </div>
        </>
      )}
    </section>
  )
}
