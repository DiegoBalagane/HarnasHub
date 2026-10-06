import { memo } from 'react'
import type { MapTendencies } from '../../../services/opponentDemosApi'
import { MapTendencyCard } from './MapTendencyCard'

interface MapTendenciesSectionProps {
  opponentName: string
  tendencies: MapTendencies[]
  canManage: boolean
}

/** Per-map tendency cards built from the opponent's demos; renders nothing without demos (FACEIT-only report unchanged). */
export const MapTendenciesSection = memo(function MapTendenciesSection({
  opponentName,
  tendencies,
  canManage,
}: MapTendenciesSectionProps) {
  if (tendencies.length === 0) {
    return null
  }

  return (
    <section className="flex flex-col gap-3">
      <h2 className="text-lg font-semibold">Jak grają — tendencje z demek</h2>
      {tendencies.map((map) => (
        <MapTendencyCard
          key={map.mapName}
          opponentName={opponentName}
          tendencies={map}
          canManage={canManage}
        />
      ))}
    </section>
  )
})
