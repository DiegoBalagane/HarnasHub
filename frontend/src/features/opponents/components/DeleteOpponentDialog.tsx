import { useState } from 'react'
import { ConfirmDialog } from '../../../components/ConfirmDialog'
import { useDeleteOpponent, useOpponentDeletePreview } from '../hooks/useOpponents'
import { hasHistory, historyLabel, scoutingLabel } from '../deleteSummary'

interface DeleteOpponentDialogProps {
  name: string
  onCancel: () => void
  /** Called after a successful delete; `hidden` is true when history stayed and the opponent was hidden instead. */
  onDeleted: (hidden: boolean) => void
}

/** Confirmation for "Usuń przeciwnika": shows what goes, the results/events counts and the opt-in to delete them too. */
export function DeleteOpponentDialog({ name, onCancel, onDeleted }: DeleteOpponentDialogProps) {
  const [includeHistory, setIncludeHistory] = useState(false)
  const { data: preview, isLoading, isError } = useOpponentDeletePreview(name, true)
  const deleteOpponent = useDeleteOpponent()

  const history = preview ? historyLabel(preview) : ''
  const scouting = preview ? scoutingLabel(preview) : ''

  function confirm() {
    deleteOpponent.mutate({ name, includeHistory }, { onSuccess: (result) => onDeleted(result.hidden) })
  }

  return (
    <ConfirmDialog
      title={`Usunąć przeciwnika „${name}”?`}
      confirmLabel="Usuń"
      isConfirming={deleteOpponent.isPending}
      onConfirm={confirm}
      onCancel={onCancel}
      message={
        <div className="flex flex-col gap-3">
          {isLoading && <p>Sprawdzanie, co zostanie usunięte…</p>}
          {isError && <p className="text-danger-400">Nie udało się sprawdzić zawartości.</p>}
          {preview && (
            <>
              <p>
                {scouting ? `Zostanie usunięte: ${scouting}.` : 'Brak notatek i danych FACEIT do usunięcia.'}
              </p>
              {hasHistory(preview) && (
                <div className="flex flex-col gap-2 rounded-md border border-neutral-800 p-3">
                  <p>
                    Przeciwnik ma też: <strong>{history}</strong>.
                  </p>
                  <label className="flex items-start gap-2">
                    <input
                      type="checkbox"
                      checked={includeHistory}
                      onChange={(event) => setIncludeHistory(event.target.checked)}
                      className="mt-1"
                    />
                    <span>Usuń też wyniki i wydarzenia</span>
                  </label>
                  {!includeHistory && (
                    <p className="text-xs text-neutral-400">
                      Bez zaznaczenia mecze zostaną w historii wyników, ale przeciwnik zniknie z listy. Gdy dodasz go ponownie pod tą samą nazwą, zacznie od zera.
                    </p>
                  )}
                </div>
              )}
            </>
          )}
          {deleteOpponent.isError && <p className="text-danger-400">{deleteOpponent.error.message}</p>}
        </div>
      }
    />
  )
}
