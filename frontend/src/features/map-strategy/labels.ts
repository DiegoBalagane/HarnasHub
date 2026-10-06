import type { MapSide } from '../../services/mapStrategyApi'

/** Polish labels for the two map sides, used on the CT/T toggle. */
export const mapSideLabels: Record<MapSide, string> = {
  CT: 'Obrońcy (CT)',
  T: 'Atakujący (T)',
}

/** Both sides, in the order they appear on the toggle. */
export const mapSides: MapSide[] = ['CT', 'T']

/** Filter over which sides' pins are shown on the shared radar. */
export type SideFilter = 'both' | MapSide

/** Options of the visibility filter, in toggle order. */
export const sideFilterOptions: { value: SideFilter; label: string }[] = [
  { value: 'both', label: 'Obie' },
  { value: 'T', label: 'T' },
  { value: 'CT', label: 'CT' },
]

// Written out in full so Tailwind's scanner keeps these classes in the build.
/** Ring, badge and text classes per side, from the semantic side tokens (T = orange, CT = sky). */
export const sideStyles: Record<MapSide, { ring: string; badge: string; text: string }> = {
  T: { ring: 'ring-side-t', badge: 'bg-side-t', text: 'text-side-t' },
  CT: { ring: 'ring-side-ct', badge: 'bg-side-ct', text: 'text-side-ct' },
}
