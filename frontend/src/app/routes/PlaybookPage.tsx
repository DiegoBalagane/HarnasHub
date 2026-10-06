import { useSearchParams } from 'react-router-dom'
import { AnalysisBoardsSection } from '../../features/analysis-boards/components/AnalysisBoardsSection'
import { MapPoolOverview } from '../../features/map-pool/components/MapPoolOverview'
import { MapStatsTab } from '../../features/match-analysis/components/MapStatsTab'
import { MapRadarView } from '../../features/map-strategy/components/MapRadarView'
import { MaterialsTab } from '../../features/materials/components/MaterialsTab'
import { mapNames } from '../../features/nades/labels'
import { NadesTab } from '../../features/nades/components/NadesTab'
import { TacticsTab } from '../../features/tactics/components/TacticsTab'
import { PageHeader } from '../../components/ui/PageHeader'
import { Tabs } from '../../components/ui/Tabs'
import type { MapName } from '../../services/nadesApi'

/** Playbook tab ids, used as the `tab` query parameter (also targeted by the legacy route redirects). */
const playbookTabs = [
  { id: 'positions', label: 'Pozycje' },
  { id: 'nades', label: 'Granaty' },
  { id: 'tactics', label: 'Taktyki' },
  { id: 'boards', label: 'Tablice' },
  { id: 'materials', label: 'Materiały' },
  { id: 'pool', label: 'Pula map' },
  { id: 'stats', label: 'Statystyki' },
] as const

type PlaybookTabId = (typeof playbookTabs)[number]['id']

const chipClass = (isActive: boolean) =>
  `rounded-md border px-3 py-1.5 text-sm transition ${
    isActive ? 'border-primary-500 bg-primary-500/15 text-primary-300' : 'border-neutral-800 text-neutral-400 hover:text-white'
  }`

/** Everything strategic in one place: pick a map (or all), then switch between positions, nades, tactics, boards, materials and the map pool. Map and tab live in the URL. */
export function PlaybookPage() {
  const [searchParams, setSearchParams] = useSearchParams()
  const mapParam = searchParams.get('map')
  const mapName = mapNames.find((map) => map === mapParam) as MapName | undefined
  const tabParam = searchParams.get('tab')
  const tab: PlaybookTabId = playbookTabs.find((candidate) => candidate.id === tabParam)?.id ?? 'positions'

  function updateParams(changes: { map?: MapName | null; tab?: PlaybookTabId }) {
    const next = new URLSearchParams(searchParams)
    if (changes.map !== undefined) {
      if (changes.map === null) next.delete('map')
      else next.set('map', changes.map)
    }
    if (changes.tab) next.set('tab', changes.tab)
    setSearchParams(next, { replace: true })
  }

  return (
    <>
      <PageHeader title="Playbook" />

      <div className="flex flex-wrap items-center gap-2" role="group" aria-label="Wybór mapy">
        <button type="button" onClick={() => updateParams({ map: null })} className={chipClass(mapName === undefined)}>
          Wszystkie mapy
        </button>
        {mapNames.map((map) => (
          <button key={map} type="button" onClick={() => updateParams({ map })} className={chipClass(mapName === map)}>
            {map}
          </button>
        ))}
      </div>

      <Tabs tabs={playbookTabs} value={tab} onChange={(id) => updateParams({ tab: id })} />

      {tab === 'positions' && <MapRadarView mapName={mapName} />}
      {tab === 'nades' && <NadesTab mapName={mapName} />}
      {tab === 'tactics' && <TacticsTab mapName={mapName} />}
      {tab === 'boards' && <AnalysisBoardsSection mapName={mapName} />}
      {tab === 'materials' && <MaterialsTab />}
      {tab === 'pool' && <MapPoolOverview mapName={mapName} />}
      {tab === 'stats' && <MapStatsTab mapName={mapName} />}
    </>
  )
}
