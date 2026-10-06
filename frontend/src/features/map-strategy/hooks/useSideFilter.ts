import { useCallback, useState } from 'react'
import { MAP_STRATEGY_SETTINGS } from '../../../constants'
import type { SideFilter } from '../labels'

function readStored(): SideFilter {
  try {
    const value = localStorage.getItem(MAP_STRATEGY_SETTINGS.sideFilterStorageKey)
    return value === 'T' || value === 'CT' ? value : 'both'
  } catch {
    return 'both'
  }
}

/** Per-viewer T/CT/both visibility filter, remembered in localStorage (storage failures are ignored). */
export function useSideFilter() {
  const [filter, setFilterState] = useState<SideFilter>(readStored)

  const setFilter = useCallback((next: SideFilter) => {
    setFilterState(next)
    try {
      localStorage.setItem(MAP_STRATEGY_SETTINGS.sideFilterStorageKey, next)
    } catch {
      // Storage unavailable (private mode etc.) — the filter just won't be remembered.
    }
  }, [])

  return [filter, setFilter] as const
}
