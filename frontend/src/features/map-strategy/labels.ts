import type { MapSide } from '../../services/mapStrategyApi'

/** Polish labels for the two map sides, used on the CT/T toggle. */
export const mapSideLabels: Record<MapSide, string> = {
  CT: 'Obrońcy (CT)',
  T: 'Atakujący (T)',
}

/** Both sides, in the order they appear on the toggle. */
export const mapSides: MapSide[] = ['CT', 'T']
