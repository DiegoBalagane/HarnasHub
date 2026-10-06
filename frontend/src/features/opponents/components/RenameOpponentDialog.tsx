import { useState } from 'react'
import { Modal } from '../../../components/Modal'
import { Button } from '../../../components/ui/Button'
import { useOpponents, useRenameOpponent } from '../hooks/useOpponents'
import { OpponentNameInput } from './OpponentNameInput'

interface RenameOpponentDialogProps {
  name: string
  onClose: () => void
  /** Called with the new name after a successful rename/merge. */
  onRenamed: (newName: string) => void
}

const inputClass =
  'w-full rounded-md border border-neutral-800 bg-neutral-900 px-3 py-2 text-sm outline-none focus:border-neutral-500'

/** "Zmień nazwę / scal": renames an opponent, or merges it into another known one when the typed name already exists. */
export function RenameOpponentDialog({ name, onClose, onRenamed }: RenameOpponentDialogProps) {
  const [target, setTarget] = useState(name)
  const { data: opponents } = useOpponents()
  const rename = useRenameOpponent()

  const targetKey = target.trim().toLowerCase()
  const isSameKey = targetKey === name.trim().toLowerCase()
  const isMerge =
    !isSameKey && targetKey !== '' && !!opponents?.some((o) => o.name.trim().toLowerCase() === targetKey)
  const unchanged = target.trim() === name.trim()

  function submit(event: React.FormEvent) {
    event.preventDefault()
    rename.mutate({ from: name, to: target.trim() }, { onSuccess: (result) => onRenamed(result.name) })
  }

  return (
    <Modal title={`Zmień nazwę / scal — ${name}`} onClose={onClose}>
      <form onSubmit={submit} className="flex flex-col gap-3">
        <label className="flex flex-col gap-1 text-sm text-neutral-300">
          Nowa nazwa (lub istniejący przeciwnik, z którym scalić)
          <OpponentNameInput required value={target} onChange={setTarget} className={inputClass} />
        </label>

        {isMerge && (
          <p className="text-sm text-warning-400">
            Taki przeciwnik już istnieje — notatki, wyniki i wydarzenia zostaną scalone. Jeśli oba mają
            powiązanie FACEIT, zostanie to z docelowego.
          </p>
        )}
        {rename.isError && <p className="text-sm text-danger-400">{rename.error.message}</p>}

        <div className="flex justify-end gap-2">
          <Button variant="secondary" onClick={onClose} disabled={rename.isPending}>
            Anuluj
          </Button>
          <Button type="submit" disabled={rename.isPending || target.trim() === '' || unchanged}>
            {rename.isPending ? 'Zapisywanie…' : isMerge ? 'Scal' : 'Zmień nazwę'}
          </Button>
        </div>
      </form>
    </Modal>
  )
}
