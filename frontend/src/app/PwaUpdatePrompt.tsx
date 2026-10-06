import { useEffect, useRef } from 'react'
import { useLocation } from 'react-router-dom'
import { useRegisterSW } from 'virtual:pwa-register/react'
import { PWA_UPDATE_SETTINGS } from '../constants'

/** Applies a newly deployed build without making people click "Odśwież": right away when it shows up just after the
 * app opened (nothing typed yet), otherwise on the next in-app navigation or when the tab/app goes to the background,
 * so a half-filled form is never reloaded away. The banner stays visible until one of those moments. */
export function PwaUpdatePrompt() {
  const openedAt = useRef(Date.now())
  const location = useLocation()
  const lastPath = useRef(location.pathname)
  const {
    needRefresh: [needRefresh],
    updateServiceWorker,
  } = useRegisterSW({
    onRegisteredSW(_url, registration) {
      if (!registration) return

      setInterval(() => {
        registration.update().catch(() => {
          // A failed background check just means we'll try again on the next tick.
        })
      }, PWA_UPDATE_SETTINGS.checkIntervalMs)
    },
  })

  // Fresh start: the update was found while the app was still loading, so reloading costs the user nothing.
  useEffect(() => {
    if (needRefresh && Date.now() - openedAt.current < PWA_UPDATE_SETTINGS.applyImmediatelyWithinMs) {
      void updateServiceWorker(true)
    }
  }, [needRefresh, updateServiceWorker])

  // Leaving a page: whatever was on it is being left anyway, so the reload lands on the new route with the new build.
  useEffect(() => {
    if (location.pathname === lastPath.current) return
    lastPath.current = location.pathname
    if (needRefresh) {
      void updateServiceWorker(true)
    }
  }, [location.pathname, needRefresh, updateServiceWorker])

  // Backgrounded tab/app: nobody is looking, so the next time they come back it's already the new version.
  useEffect(() => {
    if (!needRefresh) return

    function handleVisibilityChange() {
      if (document.visibilityState === 'hidden') {
        void updateServiceWorker(true)
      }
    }

    document.addEventListener('visibilitychange', handleVisibilityChange)
    return () => document.removeEventListener('visibilitychange', handleVisibilityChange)
  }, [needRefresh, updateServiceWorker])

  if (!needRefresh) {
    return null
  }

  return (
    <div className="fixed inset-x-0 bottom-0 z-50 flex items-center justify-between gap-3 border-t border-neutral-700 bg-neutral-900 px-4 py-3 text-sm text-neutral-200 shadow-lg">
      <span>Dostępna nowa wersja — wczyta się sama przy przejściu na inną stronę.</span>
      <button
        type="button"
        onClick={() => updateServiceWorker(true)}
        className="shrink-0 rounded-md bg-primary-500 px-3 py-1.5 text-xs font-medium text-primary-950 transition hover:bg-primary-400"
      >
        Odśwież teraz
      </button>
    </div>
  )
}
