import { Navigate, useSearchParams } from 'react-router-dom'
import { API_ENDPOINTS, API_SETTINGS } from '../../constants'
import { useAuthStore } from '../../features/auth/stores/useAuthStore'

const errorMessages: Record<string, string> = {
  discord_denied: 'Logowanie przez Discorda zostało anulowane.',
  discord_failed: 'Nie udało się zalogować przez Discorda. Spróbuj ponownie.',
}

export function LoginPage() {
  const [searchParams] = useSearchParams()
  const error = searchParams.get('error')
  const isAuthenticated = useAuthStore((state) => state.isAuthenticated)

  if (isAuthenticated && !error) {
    return <Navigate to="/dashboard" replace />
  }

  return (
    <>
      <h1 className="text-2xl font-semibold">Zaloguj się</h1>
      <p className="max-w-sm text-center text-sm text-neutral-400">
        Drużyna loguje się kontem Discord — bez zakładania nowego hasła. Musisz być w naszym serwerze Discord.
      </p>

      {error && <p className="text-sm text-red-400">{errorMessages[error] ?? 'Wystąpił błąd logowania.'}</p>}

      <a
        href={`${API_SETTINGS.baseUrl}${API_ENDPOINTS.auth.discordLogin}`}
        className="flex items-center gap-2 rounded-md bg-[#5865F2] px-5 py-2.5 font-medium text-white transition hover:bg-[#4752c4]"
      >
        <svg viewBox="0 0 24 24" width="20" height="20" fill="currentColor" aria-hidden="true">
          <path d="M20.317 4.37a19.79 19.79 0 0 0-4.885-1.515.074.074 0 0 0-.079.037c-.21.375-.444.864-.608 1.25a18.27 18.27 0 0 0-5.487 0 12.64 12.64 0 0 0-.617-1.25.077.077 0 0 0-.079-.037A19.736 19.736 0 0 0 3.677 4.37a.07.07 0 0 0-.032.027C.533 9.046-.32 13.58.099 18.057a.082.082 0 0 0 .031.056 19.9 19.9 0 0 0 5.993 3.03.078.078 0 0 0 .084-.028 14.09 14.09 0 0 0 1.226-1.994.076.076 0 0 0-.041-.106 13.107 13.107 0 0 1-1.872-.892.077.077 0 0 1-.008-.128 10.2 10.2 0 0 0 .372-.292.074.074 0 0 1 .077-.01c3.928 1.793 8.18 1.793 12.062 0a.074.074 0 0 1 .078.01c.12.098.246.198.373.292a.077.077 0 0 1-.006.127 12.3 12.3 0 0 1-1.873.892.076.076 0 0 0-.041.107c.36.698.772 1.362 1.225 1.993a.076.076 0 0 0 .084.028 19.84 19.84 0 0 0 6.002-3.03.077.077 0 0 0 .032-.055c.5-5.177-.838-9.674-3.549-13.66a.061.061 0 0 0-.031-.028ZM8.02 15.33c-1.183 0-2.157-1.085-2.157-2.419 0-1.333.955-2.418 2.157-2.418 1.21 0 2.176 1.094 2.157 2.418 0 1.334-.955 2.419-2.157 2.419Zm7.975 0c-1.183 0-2.157-1.085-2.157-2.419 0-1.333.955-2.418 2.157-2.418 1.21 0 2.176 1.094 2.157 2.418 0 1.334-.947 2.419-2.157 2.419Z" />
        </svg>
        Zaloguj się przez Discord
      </a>
    </>
  )
}
