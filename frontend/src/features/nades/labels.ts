import type { GrenadeType } from '../../services/nadesApi'

export const grenadeTypeLabels: Record<GrenadeType, string> = {
  Smoke: 'Dymna',
  Flash: 'Flasha',
  Molotov: 'Molotov',
  Frag: 'Granat odłamkowy',
}

export const commonMaps = ['Mirage', 'Inferno', 'Nuke', 'Ancient', 'Anubis', 'Dust2', 'Train', 'Vertigo']
