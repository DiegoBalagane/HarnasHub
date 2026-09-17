import { useState } from 'react'
import type { MapSide } from '../../../services/mapStrategyApi'
import type { MapName } from '../../../services/nadesApi'
import type { EconomyType } from '../../../services/tacticsApi'
import { mapNames } from '../../nades/labels'
import { mapSideLabels, mapSides } from '../../map-strategy/labels'
import { useCreateTactic } from '../hooks/useTactics'
import { economyLabels, economyTypes } from '../labels'

interface AddTacticFormProps {
  onCreated: (tacticId: string) => void
}

/** Coach/Manager form for starting a new, empty tactic — points are added afterwards in the editor. */
export function AddTacticForm({ onCreated }: AddTacticFormProps) {
  const [mapName, setMapName] = useState<MapName>(mapNames[0])
  const [side, setSide] = useState<MapSide>(mapSides[0])
  const [economy, setEconomy] = useState<EconomyType>(economyTypes[0])
  const [name, setName] = useState('')
  const [note, setNote] = useState('')
  const createTactic = useCreateTactic()

  function handleSubmit(event: React.FormEvent) {
    event.preventDefault()
    createTactic.mutate(
      { mapName, side, name, economy, note: note || undefined },
      {
        onSuccess: (tactic) => {
          setName('')
          setNote('')
          onCreated(tactic.id)
        },
      },
    )
  }

  return (
    <form
      onSubmit={handleSubmit}
      className="flex w-full max-w-3xl flex-col gap-3 rounded-md border border-neutral-800 p-4"
    >
      <h2 className="font-medium">Nowa taktyka</h2>

      <div className="flex flex-wrap gap-3">
        <select
          value={mapName}
          onChange={(event) => setMapName(event.target.value as MapName)}
          className="rounded-md border border-neutral-800 bg-neutral-900 px-3 py-2 text-sm outline-none focus:border-neutral-500"
        >
          {mapNames.map((map) => (
            <option key={map} value={map}>
              {map}
            </option>
          ))}
        </select>

        <div className="flex overflow-hidden rounded-md border border-neutral-800">
          {mapSides.map((option) => (
            <button
              key={option}
              type="button"
              onClick={() => setSide(option)}
              className={`px-3 py-2 text-sm ${
                side === option ? 'bg-neutral-100 text-neutral-900' : 'text-neutral-400 hover:text-white'
              }`}
            >
              {mapSideLabels[option]}
            </button>
          ))}
        </div>

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

        <input
          required
          placeholder="Nazwa taktyki (np. Eco rush B)"
          value={name}
          onChange={(event) => setName(event.target.value)}
          className="flex-1 rounded-md border border-neutral-800 bg-neutral-900 px-3 py-2 text-sm outline-none focus:border-neutral-500"
        />
      </div>

      <textarea
        placeholder="Notatka ogólna (opcjonalnie)"
        value={note}
        onChange={(event) => setNote(event.target.value)}
        className="rounded-md border border-neutral-800 bg-neutral-900 px-3 py-2 text-sm outline-none focus:border-neutral-500"
      />

      {createTactic.isError && <p className="text-sm text-red-400">Nie udało się utworzyć taktyki.</p>}

      <button
        type="submit"
        disabled={createTactic.isPending}
        className="self-start rounded-md bg-red-600 px-4 py-2 text-sm font-medium text-white transition hover:bg-red-500 disabled:opacity-50"
      >
        {createTactic.isPending ? 'Tworzenie…' : 'Utwórz i edytuj'}
      </button>
    </form>
  )
}
