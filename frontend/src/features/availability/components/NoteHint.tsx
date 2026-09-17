import { useEffect, useRef, useState } from 'react'
import { createPortal } from 'react-dom'

interface NoteHintProps {
  note: string | null
}

/** Small amber dot on a positioned ancestor that reveals a note in a popover on click/tap — unlike a hover-only title, this also works on touch. The popover renders through a portal at a fixed screen position so it's never clipped by the calendar's horizontally-scrolling table. */
export function NoteHint({ note }: NoteHintProps) {
  const [position, setPosition] = useState<{ top: number; left: number } | null>(null)
  const triggerRef = useRef<HTMLSpanElement>(null)

  useEffect(() => {
    if (position === null) return

    function close() {
      setPosition(null)
    }

    // Any scroll (including the calendar's own horizontal one) or resize invalidates the
    // captured coordinates, so the popover closes instead of drifting away from its dot.
    window.addEventListener('scroll', close, true)
    window.addEventListener('resize', close)
    document.addEventListener('click', close)

    return () => {
      window.removeEventListener('scroll', close, true)
      window.removeEventListener('resize', close)
      document.removeEventListener('click', close)
    }
  }, [position])

  if (!note) {
    return null
  }

  function toggle(event: { stopPropagation: () => void; preventDefault: () => void }) {
    event.stopPropagation()
    event.preventDefault()

    if (position !== null) {
      setPosition(null)
      return
    }

    const rect = triggerRef.current?.getBoundingClientRect()

    if (rect) {
      setPosition({ top: rect.bottom + 4, left: rect.left + rect.width / 2 })
    }
  }

  return (
    <span
      ref={triggerRef}
      role="button"
      tabIndex={0}
      title="Pokaż notatkę"
      onClick={toggle}
      onKeyDown={(event) => {
        if (event.key === 'Enter' || event.key === ' ') {
          toggle(event)
        }
      }}
      className="absolute -right-0.5 -top-0.5 h-2 w-2 cursor-pointer rounded-full border border-black/40 bg-amber-400"
    >
      {position !== null &&
        createPortal(
          <span
            style={{ top: position.top, left: position.left }}
            className="fixed z-50 w-max max-w-[220px] -translate-x-1/2 whitespace-normal rounded-md border border-neutral-700 bg-neutral-900 px-2 py-1 text-left text-[11px] font-normal normal-case text-neutral-200 shadow-lg"
          >
            {note}
          </span>,
          document.body,
        )}
    </span>
  )
}
