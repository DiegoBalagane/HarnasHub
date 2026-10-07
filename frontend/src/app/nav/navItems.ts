import type { NavIconName } from './NavIcon'

/** A single sidebar link; `to` doubles as the route prefix used for active-state matching. */
export interface NavItem {
  to: string
  label: string
  icon: NavIconName
}

/** A labelled block of sidebar links (label is omitted for the top-level links). */
export interface NavSection {
  label?: string
  items: NavItem[]
  /** When true the whole section is shown to Managers only. */
  managerOnly?: boolean
}

/** All sidebar sections in display order. */
export const navSections: NavSection[] = [
  {
    items: [
      { to: '/dashboard', label: 'Dashboard', icon: 'dashboard' },
      { to: '/calendar', label: 'Kalendarz', icon: 'calendar' },
    ],
  },
  {
    label: 'Mecze',
    items: [
      { to: '/results', label: 'Wyniki', icon: 'results' },
      { to: '/opponents', label: 'Przeciwnicy', icon: 'opponents' },
      { to: '/stats', label: 'Rozwój', icon: 'stats' },
    ],
  },
  {
    items: [{ to: '/playbook', label: 'Playbook', icon: 'playbook' }],
  },
  {
    label: 'Drużyna',
    items: [
      { to: '/roster', label: 'Skład', icon: 'roster' },
      { to: '/tasks', label: 'Zadania', icon: 'tasks' },
      { to: '/attendance', label: 'Frekwencja', icon: 'attendance' },
      { to: '/info', label: 'Info', icon: 'info' },
    ],
  },
  {
    label: 'Administracja',
    managerOnly: true,
    items: [{ to: '/admin', label: 'Panel admina', icon: 'admin' }],
  },
]

/** True when the current path is the item's route or one nested under it (e.g. /results/:id for /results). */
export function isNavItemActive(item: NavItem, pathname: string): boolean {
  return pathname === item.to || pathname.startsWith(`${item.to}/`)
}
