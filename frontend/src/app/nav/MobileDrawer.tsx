import { useEffect, useRef } from 'react'
import { useLocation } from 'react-router-dom'
import { Sidebar } from './Sidebar'

interface MobileDrawerProps {
  open: boolean
  onClose: () => void
  onLogout: () => void
}

/** Overlay drawer with the sidebar for small screens: closes on backdrop click, Escape and route change; locks body scroll. */
export function MobileDrawer({ open, onClose, onLogout }: MobileDrawerProps) {
  const panelRef = useRef<HTMLDivElement>(null)
  const onCloseRef = useRef(onClose)
  const { pathname } = useLocation()

  useEffect(() => {
    onCloseRef.current = onClose
  }, [onClose])

  useEffect(() => {
    onCloseRef.current()
  }, [pathname])

  useEffect(() => {
    if (!open) return

    const previousOverflow = document.body.style.overflow
    document.body.style.overflow = 'hidden'
    panelRef.current?.focus()

    function handleKeyDown(event: KeyboardEvent) {
      if (event.key === 'Escape') onCloseRef.current()
    }
    document.addEventListener('keydown', handleKeyDown)

    return () => {
      document.body.style.overflow = previousOverflow
      document.removeEventListener('keydown', handleKeyDown)
    }
  }, [open])

  if (!open) return null

  return (
    <div className="fixed inset-0 z-50 lg:hidden" role="presentation">
      <div className="absolute inset-0 bg-black/70" onClick={onClose} aria-hidden="true" />
      <div
        ref={panelRef}
        role="dialog"
        aria-modal="true"
        aria-label="Menu"
        tabIndex={-1}
        className="absolute inset-y-0 left-0 w-72 max-w-[85vw] border-r border-surface-border bg-surface-sidebar shadow-xl outline-none"
      >
        <Sidebar collapsed={false} onLogout={onLogout} onClose={onClose} />
      </div>
    </div>
  )
}
