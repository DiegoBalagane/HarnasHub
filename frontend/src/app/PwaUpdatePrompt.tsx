import { useRegisterSW } from 'virtual:pwa-register/react'

const checkIntervalMs = 60_000

/** Checks for a new deployed build (on load, then every minute) and shows a one-click "Odśwież" banner instead of requiring a manual hard refresh. */
export function PwaUpdatePrompt() {
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
      }, checkIntervalMs)
    },
  })

  if (!needRefresh) {
    return null
  }

  return (
    <div className="fixed inset-x-0 bottom-0 z-50 flex items-center justify-between gap-3 border-t border-neutral-700 bg-neutral-900 px-4 py-3 text-sm text-neutral-200 shadow-lg">
      <span>Dostępna nowa wersja aplikacji.</span>
      <button
        type="button"
        onClick={() => updateServiceWorker(true)}
        className="shrink-0 rounded-md bg-red-600 px-3 py-1.5 text-xs font-medium text-white transition hover:bg-red-500"
      >
        Odśwież
      </button>
    </div>
  )
}
