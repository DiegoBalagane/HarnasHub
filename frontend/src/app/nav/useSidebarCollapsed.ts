import { useCallback, useState } from 'react'
import { SIDEBAR_SETTINGS } from '../../constants'

function readStored(): boolean {
  try {
    return localStorage.getItem(SIDEBAR_SETTINGS.storageKey) === '1'
  } catch {
    return false
  }
}

/** Desktop sidebar collapsed flag, persisted in localStorage (storage failures are ignored). */
export function useSidebarCollapsed(): [boolean, () => void] {
  const [collapsed, setCollapsed] = useState<boolean>(readStored)

  const toggle = useCallback(() => {
    setCollapsed((current) => {
      const next = !current
      try {
        localStorage.setItem(SIDEBAR_SETTINGS.storageKey, next ? '1' : '0')
      } catch {
        // Persisting is a convenience only.
      }
      return next
    })
  }, [])

  return [collapsed, toggle]
}
