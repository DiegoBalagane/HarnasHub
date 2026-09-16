import { useState } from 'react'

interface NoteHintProps {
  note: string | null
}

/** Small amber dot on a positioned ancestor that reveals a note in a popover on click/tap — unlike a hover-only title, this also works on touch. */
export function NoteHint({ note }: NoteHintProps) {
  const [open, setOpen] = useState(false)

  if (!note) {
    return null
  }

  function toggle(event: { stopPropagation: () => void; preventDefault: () => void }) {
    event.stopPropagation()
    event.preventDefault()
    setOpen((current) => !current)
  }

  return (
    <span
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
      {open && (
        <span className="absolute left-1/2 top-full z-10 mt-1 w-max max-w-[180px] -translate-x-1/2 whitespace-normal rounded-md border border-neutral-700 bg-neutral-900 px-2 py-1 text-left text-[11px] font-normal normal-case text-neutral-200 shadow-lg">
          {note}
        </span>
      )}
    </span>
  )
}
