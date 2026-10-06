import { useState } from 'react'
import { useModalGuard } from '../../../../components/ModalGuardContext'
import type { MapSide } from '../../../../services/mapStrategyApi'
import type { MapName } from '../../../../services/nadesApi'
import type { EconomyType, ImportedNadeInput } from '../../../../services/tacticsApi'
import { useImportTacticFromDemo } from '../../hooks/useDemoTacticImport'
import { economyLabels, economyTypes } from '../../labels'

interface DemoImportSaveFormProps {
  mapName: MapName
  side: MapSide
  roundNumber: number
  grenades: ImportedNadeInput[]
  onBack: () => void
  /** Called with the new tactic's id after a successful save. */
  onSaved: (tacticId: string) => void
}

/** Wizard last step: tactic name, economy, note and the "add to nade library" switch, then save. */
export function DemoImportSaveForm({ mapName, side, roundNumber, grenades, onBack, onSaved }: DemoImportSaveFormProps) {
  const [name, setName] = useState(`${mapName} ${side} — runda ${roundNumber}`)
  const [economy, setEconomy] = useState<EconomyType>('FullBuy')
  const [note, setNote] = useState('')
  const [addToNadeLibrary, setAddToNadeLibrary] = useState(true)
  const importTactic = useImportTacticFromDemo()
  useModalGuard({ isBusy: importTactic.isPending, isDirty: false })

  function handleSubmit(event: React.FormEvent) {
    event.preventDefault()
    importTactic.mutate(
      { mapName, side, name, economy, note: note || null, grenades, addToNadeLibrary },
      { onSuccess: (tactic) => onSaved(tactic.id) },
    )
  }

  return (
    <form onSubmit={handleSubmit} className="flex flex-col gap-3">
      <p className="text-sm text-neutral-400">
        {mapName} · {side} · runda {roundNumber} · {grenades.length} granatów
      </p>

      <div className="flex flex-wrap gap-3">
        <input
          required
          maxLength={100}
          placeholder="Nazwa taktyki"
          value={name}
          onChange={(event) => setName(event.target.value)}
          className="flex-1 rounded-md border border-neutral-800 bg-neutral-900 px-3 py-2 text-sm outline-none focus:border-neutral-500"
        />
        <select
          value={economy}
          onChange={(event) => setEconomy(event.target.value as EconomyType)}
          className="rounded-md border border-neutral-800 bg-neutral-900 px-3 py-2 text-sm outline-none focus:border-neutral-500"
        >
          {economyTypes.map((type) => (
            <option key={type} value={type}>
              {economyLabels[type]}
            </option>
          ))}
        </select>
      </div>

      <textarea
        maxLength={500}
        placeholder="Notatka (opcjonalnie)"
        value={note}
        onChange={(event) => setNote(event.target.value)}
        className="rounded-md border border-neutral-800 bg-neutral-900 px-3 py-2 text-sm outline-none focus:border-neutral-500"
      />

      <label className="flex items-center gap-2 text-sm text-neutral-300">
        <input type="checkbox" checked={addToNadeLibrary} onChange={(event) => setAddToNadeLibrary(event.target.checked)} />
        Dodaj granaty do biblioteki (istniejące w tym samym miejscu zostaną podlinkowane)
      </label>

      {importTactic.isError && (
        <p className="text-sm text-danger-400">
          {importTactic.error instanceof Error ? importTactic.error.message : 'Nie udało się zapisać taktyki.'}
        </p>
      )}

      <div className="flex gap-2">
        <button
          type="button"
          onClick={onBack}
          className="rounded-md border border-neutral-700 px-4 py-2 text-sm text-neutral-200 hover:border-neutral-500"
        >
          Wstecz
        </button>
        <button
          type="submit"
          disabled={importTactic.isPending}
          className="rounded-md bg-primary-500 px-4 py-2 text-sm font-medium text-primary-950 transition hover:bg-primary-400 disabled:opacity-50"
        >
          {importTactic.isPending ? 'Zapisywanie…' : 'Zapisz taktykę'}
        </button>
      </div>
    </form>
  )
}
