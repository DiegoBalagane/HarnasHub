import type { EconomyType } from '../../services/tacticsApi'
import type { TacticEffectiveness } from '../../services/tacticMatchingApi'

export const economyLabels: Record<EconomyType, string> = {
  Eco: 'Eco',
  ForceBuy: 'Force buy',
  FullBuy: 'Full buy',
  AntiEco: 'Anti-eco',
}

/** All economy types, in the order they appear in filters/selects. */
export const economyTypes: EconomyType[] = ['Eco', 'ForceBuy', 'FullBuy', 'AntiEco']

/** "Skuteczność: 5/8 rund (63%)" — rounds won out of rounds automatically matched to a tactic. */
export function formatEffectiveness(effectiveness: TacticEffectiveness | undefined): string {
  if (!effectiveness || effectiveness.roundsPlayed === 0) return 'Skuteczność: brak dopasowanych rund'
  const percent = Math.round((100 * effectiveness.roundsWon) / effectiveness.roundsPlayed)
  return `Skuteczność: ${effectiveness.roundsWon}/${effectiveness.roundsPlayed} rund (${percent}%)`
}

/** Shown wherever effectiveness is requested for a map whose radar fit isn't verified. */
export const UNCALIBRATED_MAP_MESSAGE = 'Mapa niekalibrowana — pozycje niedostępne, skuteczność taktyk nie jest liczona.'
