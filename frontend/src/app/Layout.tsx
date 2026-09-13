import type { PropsWithChildren } from 'react'
import { Link, useNavigate } from 'react-router-dom'
import { useAuthStore } from '../features/auth/stores/useAuthStore'

/** Shared page chrome: top bar with branding and session controls. */
export function Layout({ children }: PropsWithChildren) {
  const { isAuthenticated, displayName, clearSession } = useAuthStore()
  const navigate = useNavigate()

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

          {isAuthenticated && (
            <nav className="flex gap-4 text-sm text-neutral-400">
              <Link to="/dashboard" className="hover:text-white">
                Dashboard
              </Link>
              <Link to="/calendar" className="hover:text-white">
                Kalendarz
              </Link>
              <Link to="/tasks" className="hover:text-white">
                Zadania
              </Link>
              <Link to="/results" className="hover:text-white">
                Wyniki
              </Link>
              <Link to="/nades" className="hover:text-white">
                Granaty
              </Link>
              <Link to="/materials" className="hover:text-white">
                Materiały
              </Link>
              <Link to="/roster" className="hover:text-white">
                Skład
              </Link>
            </nav>
          )}
        </div>

        {isAuthenticated && (
          <div className="flex items-center gap-4 text-sm">
            <span className="text-neutral-400">{displayName}</span>
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
