import { useCallback, useState, type CSSProperties, type PropsWithChildren } from 'react'
import { useNavigate } from 'react-router-dom'
import { SIDEBAR_SETTINGS } from '../constants'
import { useIsGuest } from '../features/auth/hooks/useIsGuest'
import { useAuthStore } from '../features/auth/stores/useAuthStore'
import { useSyncOwnNickname } from '../features/roster/hooks/useSyncOwnNickname'
import { Logo } from './nav/Logo'
import { MobileDrawer } from './nav/MobileDrawer'
import { MobileTopBar } from './nav/MobileTopBar'
import { Sidebar } from './nav/Sidebar'
import { useSidebarCollapsed } from './nav/useSidebarCollapsed'

/** Shared page chrome: sidebar navigation for team members; a plain top bar for anonymous visitors and Guests. */
export function Layout({ children }: PropsWithChildren) {
  const { isAuthenticated, displayName, inGameNickname, clearSession } = useAuthStore()
  const isGuest = useIsGuest()
  const navigate = useNavigate()
  const [collapsed, toggleCollapsed] = useSidebarCollapsed()
  const [drawerOpen, setDrawerOpen] = useState(false)
  const closeDrawer = useCallback(() => setDrawerOpen(false), [])

  useSyncOwnNickname()

  function handleLogout() {
    clearSession()
    navigate('/login')
  }

  if (!isAuthenticated || isGuest) {
    return (
      <div className="flex min-h-screen flex-col">
        <header className="flex items-center justify-between border-b border-surface-border px-6 py-4">
          <div className="flex items-center gap-6">
            <Logo />
            {isAuthenticated && (
              <span className="text-xs text-neutral-500">Konto oczekuje na przydzielenie roli przez managera</span>
            )}
          </div>

          {isAuthenticated && (
            <div className="flex items-center gap-3 text-sm">
              <span className="text-neutral-300">
                {inGameNickname ? `${inGameNickname} (${displayName})` : displayName}
              </span>
              <button onClick={handleLogout} className="text-neutral-400 hover:text-white">
                Wyloguj
              </button>
            </div>
          )}
        </header>

        <main className="mx-auto flex w-full max-w-7xl flex-1 flex-col gap-6 px-4 py-8 sm:px-6 sm:py-12">{children}</main>
      </div>
    )
  }

  const sidebarWidth = collapsed ? SIDEBAR_SETTINGS.collapsedWidthPx : SIDEBAR_SETTINGS.expandedWidthPx
  const widthVar = { '--sidebar-w': `${sidebarWidth}px` } as CSSProperties

  return (
    <div className="min-h-screen" style={widthVar}>
      <aside className="fixed inset-y-0 left-0 z-30 hidden w-(--sidebar-w) border-r border-surface-border bg-surface-sidebar transition-[width] lg:block">
        <Sidebar collapsed={collapsed} onLogout={handleLogout} onToggleCollapsed={toggleCollapsed} />
      </aside>

      <MobileTopBar onOpen={() => setDrawerOpen(true)} />
      <MobileDrawer open={drawerOpen} onClose={closeDrawer} onLogout={handleLogout} />

      <div className="flex min-h-screen flex-col transition-[padding] lg:pl-(--sidebar-w)">
        <main className="mx-auto flex w-full max-w-7xl flex-1 flex-col gap-6 px-4 py-6 sm:px-6 lg:px-8 lg:py-10">{children}</main>
      </div>
    </div>
  )
}
