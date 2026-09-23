import { memo, useEffect, useRef, useState } from 'react'
import { Link, useLocation } from 'react-router-dom'

interface NavItem {
  to: string
  label: string
}

interface NavGroup {
  label: string
  items: NavItem[]
}

const standaloneItem: NavItem = { to: '/dashboard', label: 'Dashboard' }

const navGroups: NavGroup[] = [
  {
    label: 'Zespół',
    items: [
      { to: '/calendar', label: 'Kalendarz' },
      { to: '/events', label: 'Wydarzenia' },
      { to: '/tasks', label: 'Zadania' },
      { to: '/roster', label: 'Skład' },
    ],
  },
  {
    label: 'Mecze',
    items: [
      { to: '/results', label: 'Wyniki' },
      { to: '/stats', label: 'Rozwój' },
      { to: '/opponents', label: 'Przeciwnicy' },
    ],
  },
  {
    label: 'Strategia',
    items: [
      { to: '/nades', label: 'Granaty' },
      { to: '/map-strategy', label: 'Pozycje' },
      { to: '/tactics', label: 'Taktyki' },
      { to: '/analysis-boards', label: 'Analizy' },
      { to: '/materials', label: 'Materiały' },
    ],
  },
]

/** Top-bar navigation across every team area, grouped into topics with expandable subcategories — a flat list of a
 * dozen links stopped fitting on a phone. Desktop shows each group as a click-to-open dropdown; mobile collapses the
 * whole thing behind a hamburger button that opens an accordion. Only rendered for members who already have an access level. */
export const MainNav = memo(function MainNav() {
  const [isMobileOpen, setIsMobileOpen] = useState(false)
  const [openGroup, setOpenGroup] = useState<string | null>(null)
  const [openMobileGroup, setOpenMobileGroup] = useState<string | null>(null)
  const containerRef = useRef<HTMLDivElement>(null)
  const location = useLocation()

  // Close whatever's open whenever the route changes, and on an outside click for the desktop dropdown.
  useEffect(() => {
    setIsMobileOpen(false)
    setOpenGroup(null)
    setOpenMobileGroup(null)
  }, [location.pathname])

  useEffect(() => {
    function handleClickOutside(event: MouseEvent) {
      if (containerRef.current && !containerRef.current.contains(event.target as Node)) {
        setOpenGroup(null)
      }
    }

    document.addEventListener('mousedown', handleClickOutside)
    return () => document.removeEventListener('mousedown', handleClickOutside)
  }, [])

  return (
    <div ref={containerRef} className="relative">
      {/* Desktop: flat row of group dropdowns */}
      <nav className="hidden items-center gap-4 text-sm text-neutral-400 sm:flex">
        <Link to={standaloneItem.to} className="hover:text-white">
          {standaloneItem.label}
        </Link>

        {navGroups.map((group) => (
          <div key={group.label} className="relative">
            <button
              type="button"
              onClick={() => setOpenGroup((current) => (current === group.label ? null : group.label))}
              className={`flex items-center gap-1 hover:text-white ${openGroup === group.label ? 'text-white' : ''}`}
            >
              {group.label}
              <span className="text-xs">{openGroup === group.label ? '▲' : '▼'}</span>
            </button>

            {openGroup === group.label && (
              <div className="absolute left-0 top-full z-40 mt-2 flex min-w-[10rem] flex-col gap-1 rounded-md border border-neutral-800 bg-neutral-950 p-2 shadow-xl">
                {group.items.map((item) => (
                  <Link key={item.to} to={item.to} className="rounded px-2 py-1.5 hover:bg-neutral-900 hover:text-white">
                    {item.label}
                  </Link>
                ))}
              </div>
            )}
          </div>
        ))}
      </nav>

      {/* Mobile: hamburger opening a grouped accordion */}
      <div className="sm:hidden">
        <button
          type="button"
          onClick={() => setIsMobileOpen((current) => !current)}
          aria-label="Menu"
          className="flex h-9 w-9 items-center justify-center rounded-md border border-neutral-800 text-neutral-300"
        >
          {isMobileOpen ? '✕' : '☰'}
        </button>

        {isMobileOpen && (
          <div className="absolute left-0 top-full z-40 mt-2 flex w-64 flex-col gap-1 rounded-md border border-neutral-800 bg-neutral-950 p-3 text-sm shadow-xl">
            <Link to={standaloneItem.to} className="rounded px-2 py-2 text-neutral-300 hover:bg-neutral-900 hover:text-white">
              {standaloneItem.label}
            </Link>

            {navGroups.map((group) => (
              <div key={group.label}>
                <button
                  type="button"
                  onClick={() => setOpenMobileGroup((current) => (current === group.label ? null : group.label))}
                  className="flex w-full items-center justify-between rounded px-2 py-2 text-left text-neutral-300 hover:bg-neutral-900 hover:text-white"
                >
                  {group.label}
                  <span className="text-xs">{openMobileGroup === group.label ? '▲' : '▼'}</span>
                </button>

                {openMobileGroup === group.label && (
                  <div className="ml-3 flex flex-col gap-1 border-l border-neutral-800 pl-3">
                    {group.items.map((item) => (
                      <Link key={item.to} to={item.to} className="rounded px-2 py-1.5 text-neutral-400 hover:bg-neutral-900 hover:text-white">
                        {item.label}
                      </Link>
                    ))}
                  </div>
                )}
              </div>
            ))}
          </div>
        )}
      </div>
    </div>
  )
})
