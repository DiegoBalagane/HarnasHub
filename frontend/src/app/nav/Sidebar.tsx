import { memo } from 'react'
import { Logo } from './Logo'
import { NavIcon } from './NavIcon'
import { SidebarNav } from './SidebarNav'
import { SidebarUser } from './SidebarUser'

interface SidebarProps {
  collapsed: boolean
  onLogout: () => void
  /** Desktop only: shows the collapse/expand toggle. */
  onToggleCollapsed?: () => void
  /** Drawer only: shows a close button in the header. */
  onClose?: () => void
}

/** Sidebar content (logo, navigation, user block) shared by the fixed desktop rail and the mobile drawer. */
export const Sidebar = memo(function Sidebar({ collapsed, onLogout, onToggleCollapsed, onClose }: SidebarProps) {
  return (
    <div className="flex h-full flex-col">
      <div
        className={`flex h-14 shrink-0 items-center border-b border-surface-border px-4 ${collapsed ? 'justify-center' : 'justify-between'}`}
      >
        <Logo className="text-xl" compact={collapsed} />
        {onClose && (
          <button type="button" onClick={onClose} aria-label="Zamknij menu" className="rounded-md p-1.5 text-neutral-400 hover:text-white">
            <NavIcon name="close" />
          </button>
        )}
      </div>

      <div className="min-h-0 flex-1 overflow-y-auto">
        <SidebarNav collapsed={collapsed} />
      </div>

      {onToggleCollapsed && (
        <button
          type="button"
          onClick={onToggleCollapsed}
          title={collapsed ? 'Rozwiń menu' : 'Zwiń menu'}
          aria-label={collapsed ? 'Rozwiń menu' : 'Zwiń menu'}
          className={`mx-3 mb-2 flex items-center gap-3 rounded-md px-3 py-2 text-sm text-neutral-500 transition hover:bg-neutral-800/60 hover:text-white ${collapsed ? 'justify-center' : ''}`}
        >
          <NavIcon name={collapsed ? 'chevronRight' : 'chevronLeft'} />
          {!collapsed && <span>Zwiń</span>}
        </button>
      )}

      <SidebarUser collapsed={collapsed} onLogout={onLogout} />
    </div>
  )
})
