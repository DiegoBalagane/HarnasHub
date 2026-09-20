import type { GrenadeType, MapName } from '../../services/nadesApi'

export const grenadeTypeLabels: Record<GrenadeType, string> = {
  Smoke: 'Dymna',
  Flash: 'Flasha',
  Molotov: 'Molotov',
  Frag: 'Granat odłamkowy',
}

/** Single-letter marker shown on a nade's radar pin — first letter of the Polish label, chosen to avoid collisions. */
export const grenadeTypeMarks: Record<GrenadeType, string> = {
  Smoke: 'D',
  Flash: 'F',
  Molotov: 'M',
  Frag: 'G',
}

/** Tailwind background class per grenade type, used for both the radar pin and the type-filter chip. */
export const grenadeTypeColors: Record<GrenadeType, string> = {
  Smoke: 'bg-neutral-400',
  Flash: 'bg-yellow-400',
  Molotov: 'bg-orange-600',
  Frag: 'bg-red-700',
}

/** Active 2026 map pool, kept in sync with the backend MapName enum. */
export const mapNames: MapName[] = ['Ancient', 'Anubis', 'Cache', 'Dust2', 'Inferno', 'Mirage', 'Nuke']
