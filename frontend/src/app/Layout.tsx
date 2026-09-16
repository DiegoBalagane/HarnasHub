import type { PropsWithChildren } from 'react'
import { Link, useNavigate } from 'react-router-dom'
import { useIsGuest } from '../features/auth/hooks/useIsGuest'
import { useAuthStore } from '../features/auth/stores/useAuthStore'
import { NicknameEditor } from '../features/roster/components/NicknameEditor'
import { useSyncOwnNickname } from '../features/roster/hooks/useSyncOwnNickname'
import { MainNav } from './MainNav'

/** Shared page chrome: top bar with branding and session controls. */
export function Layout({ children }: PropsWithChildren) {
  const { isAuthenticated, avatarUrl, clearSession } = useAuthStore()
  const isGuest = useIsGuest()
  const navigate = useNavigate()

  useSyncOwnNickname()

  function handleLogout() {
    clearSession()
    navigate('/login')
  }

  return (
    <div className="flex min-h-screen flex-col">
      <header className="flex items-center justify-between border-b border-neutral-800 px-6 py-4">
        <div className="flex items-center gap-6">
          <Link to="/" className="text-lg font-bold tracking-tight">
            Harnas<span className="text-red-500">Hub</span>
          </Link>

          {isAuthenticated && !isGuest && <MainNav />}

          {isAuthenticated && isGuest && (
            <span className="text-xs text-neutral-500">
              Konto oczekuje na przydzielenie roli przez managera
            </span>
          )}
        </div>

        {isAuthenticated && (
          <div className="flex items-center gap-3 text-sm">
            {avatarUrl && <img src={avatarUrl} alt="" className="h-7 w-7 rounded-full" />}
            {!isGuest && <NicknameEditor />}
            <button onClick={handleLogout} className="text-neutral-400 hover:text-white">
              Wyloguj
            </button>
          </div>
        )}
      </header>

      <main className="flex flex-1 flex-col items-center gap-6 px-6 py-12">{children}</main>
    </div>
  )
}
