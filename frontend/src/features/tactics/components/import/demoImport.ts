import type { GrenadeType } from '../../../../services/nadesApi'
import type { DemoNade, DemoNadeRound, ImportedNadeInput } from '../../../../services/tacticsApi'
import type { MapSide } from '../../../../services/mapStrategyApi'

/** All grenades of one player within a round/side, for the grouped selection list. */
export interface ThrowerGroup {
  /** Stable identity — SteamID64 when known, else the nick. */
  key: string
  throwerName: string
  grenades: DemoNade[]
}

/** SVG stroke colour per grenade type (hex, since Tailwind background classes can't colour SVG lines). */
export const grenadeStrokeColors: Record<GrenadeType, string> = {
  Smoke: '#a3a3a3',
  Flash: '#facc15',
  Molotov: '#ea580c',
  Frag: '#b91c1c',
}

/** Seconds as "m:ss", e.g. 14.7 → "0:14" — same format the backend uses in point descriptions. */
export function formatRoundTime(seconds: number): string {
  const whole = Math.max(0, Math.floor(seconds))
  return `${Math.floor(whole / 60)}:${String(whole % 60).padStart(2, '0')}`
}

/** Grenades thrown by the given side in a round, in throw order. */
export function grenadesForSide(round: DemoNadeRound | undefined, side: MapSide): DemoNade[] {
  return (round?.grenades ?? []).filter((grenade) => grenade.side === side)
}

/** Groups grenades by thrower (keeping throw order inside each group), players sorted by name. */
export function groupByThrower(grenades: DemoNade[]): ThrowerGroup[] {
  const groups = new Map<string, DemoNade[]>()
  for (const grenade of grenades) {
    const key = grenade.throwerSteamId ?? grenade.throwerName
    groups.set(key, [...(groups.get(key) ?? []), grenade])
  }

  return [...groups.entries()]
    .map(([key, list]) => ({ key, throwerName: list[0].throwerName, grenades: list }))
    .sort((a, b) => a.throwerName.localeCompare(b.throwerName))
}

/** Converts the selected grenades into the import payload shape. */
export function toImportInputs(grenades: DemoNade[], selectedIds: ReadonlySet<number>): ImportedNadeInput[] {
  return grenades
    .filter((grenade) => selectedIds.has(grenade.id))
    .map(({ type, throwerName, throwX, throwY, landX, landY, secondsIntoRound }) => ({
      type,
      throwerName,
      throwX,
      throwY,
      landX,
      landY,
      secondsIntoRound,
    }))
}
