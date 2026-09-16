import type { TeamRole, UserRole } from '../../services/rosterApi'

/** Polish labels for the access-level role shown next to every member. */
export const roleLabels: Record<UserRole, string> = {
  Guest: 'Gość',
  Player: 'Zawodnik',
  Coach: 'Coach',
  Manager: 'Manager',
}

/** Polish labels for the in-game role a player fills in the team's setup. */
export const teamRoleLabels: Record<TeamRole, string> = {
  IGL: 'IGL',
  EntryFragger: 'Entry fragger',
  Support: 'Support',
  AWPer: 'AWPer',
  Lurker: 'Lurker',
  Rifler: 'Rifler',
}

/** Selectable access levels, in the order they appear in the roster dropdown. */
export const userRoles: UserRole[] = ['Guest', 'Player', 'Coach', 'Manager']

/** Selectable in-game roles, in the order they appear in the roster dropdown. */
export const teamRoles: TeamRole[] = ['IGL', 'EntryFragger', 'Support', 'AWPer', 'Lurker', 'Rifler']
