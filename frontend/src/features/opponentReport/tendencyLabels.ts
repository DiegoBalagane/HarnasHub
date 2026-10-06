import type { GrenadeType } from '../../services/nadesApi'
import type { DemoGrenadeType, GrenadeCluster, PlayerTendency } from '../../services/opponentDemosApi'
import type { Stroke } from '../analysis-boards/canvas/types'

/** Polish labels of execute timing buckets. */
export const timingLabels: Record<string, string> = {
  Fast: 'szybko (<0:35)',
  Mid: 'średnio',
  Late: 'późno (>1:15)',
}

/** Polish labels of buy types (BuyType names). */
export const buyLabels: Record<string, string> = {
  Pistol: 'pistolet',
  Eco: 'eco',
  SemiEco: 'semi-eco',
  Force: 'force',
  Full: 'pełny zakup',
}

/** Polish labels of the demo grenade kinds. */
export const grenadeLabels: Record<DemoGrenadeType, string> = {
  Smoke: 'Smoke',
  Flash: 'Flash',
  HighExplosive: 'HE',
  Molotov: 'Molotov',
  Incendiary: 'Incendiary',
  Decoy: 'Decoy',
}

/** Marker colour per grenade kind, shared by the mini radar and the saved analysis board. */
export const grenadeColors: Record<DemoGrenadeType, string> = {
  Smoke: '#d4d4d4',
  Flash: '#facc15',
  HighExplosive: '#ef4444',
  Molotov: '#f97316',
  Incendiary: '#f97316',
  Decoy: '#a3a3a3',
}

/** Polish labels of player roles. */
export const roleLabels: Record<NonNullable<PlayerTendency['role']>, string> = {
  AWP: 'AWPer',
  Entry: 'entry',
  Clutch: 'clutcher',
}

/** Library grenade type of a demo grenade kind; null for decoys, which the library has no type for. */
export function libraryGrenadeType(type: DemoGrenadeType): GrenadeType | null {
  switch (type) {
    case 'Smoke':
      return 'Smoke'
    case 'Flash':
      return 'Flash'
    case 'HighExplosive':
      return 'Frag'
    case 'Molotov':
    case 'Incendiary':
      return 'Molotov'
    default:
      return null
  }
}

/** Seconds as in-game round time, e.g. 70 → "1:10". */
export function formatRoundTime(seconds: number): string {
  const total = Math.round(seconds)
  return `${Math.floor(total / 60)}:${String(total % 60).padStart(2, '0')}`
}

/** Grenade clusters as analysis-board strokes: a circle (radius ≈ 2% of the radar, the clustering radius) per cluster,
 * coloured by type — the board format only holds freehand strokes, so markers are drawn as closed polylines. */
export function clusterStrokes(clusters: GrenadeCluster[], radius = 0.02, segments = 20): Stroke[] {
  return clusters.map((cluster) => ({
    color: grenadeColors[cluster.type],
    width: 4,
    points: Array.from({ length: segments + 1 }, (_, i) => {
      const angle = (i / segments) * 2 * Math.PI
      return { x: cluster.x + radius * Math.cos(angle), y: cluster.y + radius * Math.sin(angle) }
    }),
  }))
}
