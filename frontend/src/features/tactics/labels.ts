import type { EconomyType } from '../../services/tacticsApi'

export const economyLabels: Record<EconomyType, string> = {
  Eco: 'Eco',
  ForceBuy: 'Force buy',
  FullBuy: 'Full buy',
  AntiEco: 'Anti-eco',
}

/** All economy types, in the order they appear in filters/selects. */
export const economyTypes: EconomyType[] = ['Eco', 'ForceBuy', 'FullBuy', 'AntiEco']
