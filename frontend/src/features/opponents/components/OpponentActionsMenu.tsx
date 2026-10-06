import { useState } from 'react'
import { useHideOpponent, useUnhideOpponent } from '../hooks/useOpponents'
import { DeleteOpponentDialog } from './DeleteOpponentDialog'
import { RenameOpponentDialog } from './RenameOpponentDialog'

interface OpponentActionsMenuProps {
  name: string
  isHidden?: boolean
  className?: string
  /** After a rename/merge, with the new name. */
  onRenamed?: (newName: string) => void
  /** After the opponent was deleted or hidden, i.e. it has left the visible list. */
  onGone?: () => void
}

/** "⋯" menu (Coach/Manager): rename/merge, hide/restore and delete an opponent, each with its confirmation. */
export function OpponentActionsMenu({
  name,
  isHidden = false,
  className = '',
  onRenamed,
  onGone,
}: OpponentActionsMenuProps) {
  const [open, setOpen] = useState(false)
  const [dialog, setDialog] = useState<'rename' | 'delete' | null>(null)
  const hide = useHideOpponent()
  const unhide = useUnhideOpponent()

  function choose(action: () => void) {
    setOpen(false)
    action()
  }

  return (
    <div className={`relative ${className}`}>
      <button
        type="button"
        aria-label={`Akcje przeciwnika ${name}`}
        aria-haspopup="menu"
        aria-expanded={open}
        onClick={() => setOpen((value) => !value)}
        className="rounded-md px-2 py-1 text-lg leading-none text-neutral-400 transition hover:bg-neutral-800 hover:text-white"
      >
        ⋯
      </button>

      {open && (
        <>
          <div role="presentation" className="fixed inset-0 z-10" onClick={() => setOpen(false)} />
          <ul
            role="menu"
            className="absolute right-0 z-20 mt-1 w-44 rounded-md border border-neutral-800 bg-neutral-950 py-1 text-sm shadow-xl"
          >
            <MenuItem onClick={() => choose(() => setDialog('rename'))}>Zmień nazwę / scal</MenuItem>
            {isHidden ? (
              <MenuItem onClick={() => choose(() => unhide.mutate(name))}>Przywróć</MenuItem>
            ) : (
              <MenuItem onClick={() => choose(() => hide.mutate(name, { onSuccess: () => onGone?.() }))}>
                Ukryj
              </MenuItem>
            )}
            <MenuItem danger onClick={() => choose(() => setDialog('delete'))}>
              Usuń
            </MenuItem>
          </ul>
        </>
      )}

      {dialog === 'rename' && (
        <RenameOpponentDialog
          name={name}
          onClose={() => setDialog(null)}
          onRenamed={(newName) => {
            setDialog(null)
            onRenamed?.(newName)
          }}
        />
      )}
      {dialog === 'delete' && (
        <DeleteOpponentDialog
          name={name}
          onCancel={() => setDialog(null)}
          onDeleted={() => {
            setDialog(null)
            onGone?.()
          }}
        />
      )}
    </div>
  )
}

/** One entry of the actions menu. */
function MenuItem({
  children,
  onClick,
  danger,
}: {
  children: React.ReactNode
  onClick: () => void
  danger?: boolean
}) {
  return (
    <li role="none">
      <button
        type="button"
        role="menuitem"
        onClick={onClick}
        className={`block w-full px-3 py-1.5 text-left transition hover:bg-neutral-800 ${danger ? 'text-danger-400' : 'text-neutral-200'}`}
      >
        {children}
      </button>
    </li>
  )
}
