import { useEffect } from 'react'
import { createPortal } from 'react-dom'

const popoverWidth = 320
const margin = 8

interface CellPopoverProps {
  /** Screen rect of the clicked cell. */
  anchor: DOMRect
  onClose: () => void
  children: React.ReactNode
}

/** Small popover anchored to a grid cell (below it, or above when the cell sits in the lower half); closes on Escape or backdrop click. */
export function CellPopover({ anchor, onClose, children }: CellPopoverProps) {
  useEffect(() => {
    function handleKeyDown(event: KeyboardEvent) {
      if (event.key === 'Escape') onClose()
    }

    document.addEventListener('keydown', handleKeyDown)
    window.addEventListener('resize', onClose)
    return () => {
      document.removeEventListener('keydown', handleKeyDown)
      window.removeEventListener('resize', onClose)
    }
  }, [onClose])

  const left = Math.max(
    margin,
    Math.min(anchor.left + anchor.width / 2 - popoverWidth / 2, window.innerWidth - popoverWidth - margin),
  )
  const placeAbove = anchor.bottom > window.innerHeight / 2
  const vertical = placeAbove
    ? { bottom: window.innerHeight - anchor.top + 6, maxHeight: anchor.top - 14 }
    : { top: anchor.bottom + 6, maxHeight: window.innerHeight - anchor.bottom - 14 }

  return createPortal(
    <>
      <div className="fixed inset-0 z-40" onClick={onClose} aria-hidden />
      <div
        role="dialog"
        aria-label="Edycja dostępności"
        style={{ left, width: popoverWidth, ...vertical }}
        className="fixed z-50 overflow-y-auto rounded-lg shadow-2xl"
      >
        {children}
      </div>
    </>,
    document.body,
  )
}
