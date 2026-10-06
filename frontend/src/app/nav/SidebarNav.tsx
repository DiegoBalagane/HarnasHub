import { memo } from 'react'
import { Link, useLocation } from 'react-router-dom'
import { NavIcon } from './NavIcon'
import { isNavItemActive, navSections } from './navItems'

interface SidebarNavProps {
  collapsed: boolean
}

/** Every navigation group expanded as a labelled section; collapses to an icon-only column with title tooltips. */
export const SidebarNav = memo(function SidebarNav({ collapsed }: SidebarNavProps) {
  const { pathname } = useLocation()

  return (
    <nav aria-label="Menu główne" className="flex flex-col gap-4 px-3 py-4">
      {navSections.map((section, index) => (
        <div key={section.label ?? index} className="flex flex-col gap-1">
          {section.label &&
            (collapsed ? (
              <div className="mx-2 my-1 border-t border-surface-border" />
            ) : (
              <p className="px-3 pb-1 text-[11px] font-semibold uppercase tracking-wider text-neutral-500">
                {section.label}
              </p>
            ))}

          {section.items.map((item) => {
            const active = isNavItemActive(item, pathname)
            return (
              <Link
                key={item.to}
                to={item.to}
                title={collapsed ? item.label : undefined}
                aria-label={collapsed ? item.label : undefined}
                aria-current={active ? 'page' : undefined}
                className={`flex items-center gap-3 rounded-md px-3 py-2 text-sm font-medium transition ${
                  collapsed ? 'justify-center' : ''
                } ${
                  active
                    ? 'bg-primary-500/15 text-primary-300'
                    : 'text-neutral-400 hover:bg-neutral-800/60 hover:text-white'
                }`}
              >
                <NavIcon name={item.icon} />
                {!collapsed && <span className="truncate">{item.label}</span>}
              </Link>
            )
          })}
        </div>
      ))}
    </nav>
  )
})
