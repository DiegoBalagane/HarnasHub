import { memo } from 'react'
import { Link } from 'react-router-dom'

const navItems = [
  { to: '/dashboard', label: 'Dashboard' },
  { to: '/calendar', label: 'Kalendarz' },
  { to: '/tasks', label: 'Zadania' },
  { to: '/results', label: 'Wyniki' },
  { to: '/stats', label: 'Rozwój' },
  { to: '/opponents', label: 'Przeciwnicy' },
  { to: '/nades', label: 'Granaty' },
  { to: '/map-strategy', label: 'Pozycje' },
  { to: '/materials', label: 'Materiały' },
  { to: '/roster', label: 'Skład' },
] as const

/** Top-bar navigation across every team area; only rendered for members who already have an access level. */
export const MainNav = memo(function MainNav() {
  return (
    <nav className="flex flex-wrap gap-4 text-sm text-neutral-400">
      {navItems.map((item) => (
        <Link key={item.to} to={item.to} className="hover:text-white">
          {item.label}
        </Link>
      ))}
    </nav>
  )
})
