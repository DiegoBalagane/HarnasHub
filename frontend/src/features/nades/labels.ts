import type { GrenadeType, MapName } from '../../services/nadesApi'

export const grenadeTypeLabels: Record<GrenadeType, string> = {
  Smoke: 'Dymna',
  Flash: 'Flasha',
  Molotov: 'Molotov',
  Frag: 'Granat odłamkowy',
}

/** Active 2026 map pool, kept in sync with the backend MapName enum. */
export const mapNames: MapName[] = ['Ancient', 'Anubis', 'Dust2', 'Inferno', 'Mirage', 'Nuke']
