import { useSyncExternalStore } from 'react'

const desktopQuery = '(min-width: 768px)'

function subscribe(onChange: () => void): () => void {
  if (typeof window.matchMedia !== 'function') return () => undefined
  const list = window.matchMedia(desktopQuery)
  list.addEventListener('change', onChange)
  return () => list.removeEventListener('change', onChange)
}

function getSnapshot(): boolean {
  return typeof window.matchMedia === 'function' ? window.matchMedia(desktopQuery).matches : true
}

/** True on tablet/desktop widths (>= 768px); assumed true where matchMedia is unavailable. */
export function useIsDesktop(): boolean {
  return useSyncExternalStore(subscribe, getSnapshot, () => true)
}
