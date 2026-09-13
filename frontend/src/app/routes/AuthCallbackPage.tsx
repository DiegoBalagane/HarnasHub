import { useEffect, useRef } from 'react'
import { useNavigate } from 'react-router-dom'
import { useAuthStore } from '../../features/auth/stores/useAuthStore'

/** Landing point after the Discord OAuth redirect: reads the JWT from the URL fragment and starts the session. */
export function AuthCallbackPage() {
  const loginWithToken = useAuthStore((state) => state.loginWithToken)
  const navigate = useNavigate()
  const handled = useRef(false)

  useEffect(() => {
    if (handled.current) return
    handled.current = true

    const token = new URLSearchParams(window.location.hash.slice(1)).get('token')
    const ok = token ? loginWithToken(token) : false

    navigate(ok ? '/dashboard' : '/login?error=discord_failed', { replace: true })
  }, [loginWithToken, navigate])

  return <p className="text-neutral-400">Logowanie…</p>
}
