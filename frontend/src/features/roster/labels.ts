import type { AccessLevel, PinColor, RosterSlot, TeamRole } from '../../services/rosterApi'

/** Polish labels for the access-level shown next to every member; independent of the Coach tag (see `isCoachLabel`). */
export const accessLevelLabels: Record<AccessLevel, string> = {
  Guest: 'Gość',
  Player: 'Zawodnik',
  Manager: 'Zarządca',
}

/** Polish label for the independent "is this member the team's Coach" toggle. */
export const isCoachLabel = 'Trener'

/** Tooltip text explaining each access level, shown on hover in the roster's permission select. */
export const accessLevelDescriptions: Record<AccessLevel, string> = {
  Guest: 'Konto czeka na przydzielenie roli — brak dostępu do danych drużyny.',
  Player: 'Widzi skład, kalendarz i mapy; nie zarządza uprawnieniami ani składem innych.',
  Manager: 'Pełny dostęp: zarządza uprawnieniami, składem, kalendarzem i rolą Trenera.',
}

/** Tooltip text for the Coach toggle, shown regardless of the member's access level. */
export const isCoachDescription =
  'Układa skład, role w grze i taktykę — niezależnie od poziomu uprawnień (Zawodnik lub Zarządca może być Trenerem).'

/** Polish labels for the in-game role a player fills in the team's setup. */
export const teamRoleLabels: Record<TeamRole, string> = {
  IGL: 'IGL',
  EntryFragger: 'Entry fragger',
  Support: 'Support',
  AWPer: 'AWPer',
  Lurker: 'Lurker',
  Rifler: 'Rifler',
  Bambik: 'Bambik',
}

/** Polish labels for where a player sits in the roster. */
export const rosterSlotLabels: Record<RosterSlot, string> = {
  Main: 'Główny skład',
  Bench: 'Ławka',
  StandIn: 'Stand-in',
}

/** Selectable roster slots, in the order they appear in the roster dropdown. */
export const rosterSlots: RosterSlot[] = ['Main', 'Bench', 'StandIn']

/** Polish labels for the map-radar pin colour a Main-roster player can pick. */
export const pinColorLabels: Record<PinColor, string> = {
  Orange: 'Pomarańczowy',
  Yellow: 'Żółty',
  Purple: 'Fioletowy',
  Green: 'Zielony',
  Blue: 'Niebieski',
}

/** Selectable pin colours, in the order they appear as swatches. */
export const pinColors: PinColor[] = ['Orange', 'Yellow', 'Purple', 'Green', 'Blue']

/** Tailwind background class for each pin colour, used both for the settings swatches and the radar pin itself. */
export const pinColorSwatch: Record<PinColor, string> = {
  Orange: 'bg-orange-500',
  Yellow: 'bg-yellow-400',
  Purple: 'bg-purple-500',
  Green: 'bg-green-500',
  Blue: 'bg-blue-500',
}

/** Selectable access levels, in the order they appear in the roster dropdown. */
export const accessLevels: AccessLevel[] = ['Guest', 'Player', 'Manager']

/** Selectable in-game roles, in the order they appear in the roster dropdown. */
export const teamRoles: TeamRole[] = ['IGL', 'EntryFragger', 'Support', 'AWPer', 'Lurker', 'Rifler', 'Bambik']
