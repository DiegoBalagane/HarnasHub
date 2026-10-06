import { memo } from 'react'
import { Link } from 'react-router-dom'
import { useAuthStore } from '../../features/auth/stores/useAuthStore'
import { NavIcon } from './NavIcon'

interface SidebarUserProps {
  collapsed: boolean
  onLogout: () => void
}

const actionClass =
  'flex items-center justify-center rounded-md p-2 text-neutral-400 transition hover:bg-neutral-800/60 hover:text-white'

/** Bottom block of the sidebar: avatar, nickname, settings link and logout. */
export const SidebarUser = memo(function SidebarUser({ collapsed, onLogout }: SidebarUserProps) {
  const { displayName, inGameNickname, avatarUrl } = useAuthStore()
  const name = inGameNickname ? `${inGameNickname} (${displayName})` : displayName

  return (
    <div className={`flex gap-2 border-t border-surface-border p-3 ${collapsed ? 'flex-col items-center' : 'items-center'}`}>
      {avatarUrl ? (
        <img src={avatarUrl} alt="" title={collapsed ? (name ?? undefined) : undefined} className="h-8 w-8 shrink-0 rounded-full" />
      ) : (
        <span className="h-8 w-8 shrink-0 rounded-full bg-neutral-800" />
      )}
      {!collapsed && <span className="min-w-0 flex-1 truncate text-sm text-neutral-300">{name}</span>}
      <div className={`flex ${collapsed ? 'flex-col' : ''}`}>
        <Link to="/settings" title="Ustawienia" aria-label="Ustawienia" className={actionClass}>
          <NavIcon name="settings" className="h-4 w-4" />
        </Link>
        <button type="button" onClick={onLogout} title="Wyloguj" aria-label="Wyloguj" className={actionClass}>
          <NavIcon name="logout" className="h-4 w-4" />
        </button>
      </div>
    </div>
  )
})
