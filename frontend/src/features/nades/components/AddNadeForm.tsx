import { useState } from 'react'
import type { GrenadeType, MapName } from '../../../services/nadesApi'
import { useAddNade } from '../hooks/useNades'
import { grenadeTypeLabels, mapNames } from '../labels'

const grenadeTypes: GrenadeType[] = ['Smoke', 'Flash', 'Molotov', 'Frag']

/** Form for any team member to add a nade lineup entry. */
export function AddNadeForm() {
  const [mapName, setMapName] = useState<MapName>(mapNames[0])
  const [type, setType] = useState<GrenadeType>('Smoke')
  const [title, setTitle] = useState('')
  const [description, setDescription] = useState('')
  const [youtubeUrl, setYoutubeUrl] = useState('')
  const addNade = useAddNade()

  function handleSubmit(event: React.FormEvent) {
    event.preventDefault()
    addNade.mutate(
      { mapName, type, title, description: description || undefined, youtubeUrl: youtubeUrl || undefined },
      {
        onSuccess: () => {
          setTitle('')
          setDescription('')
          setYoutubeUrl('')
        },
      },
    )
  }

  return (
    <form
      onSubmit={handleSubmit}
      className="flex w-full max-w-2xl flex-col gap-3 rounded-md border border-neutral-800 p-4"
    >
      <h2 className="font-medium">Dodaj granat</h2>

      <div className="flex gap-3">
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

        <select
          value={type}
          onChange={(event) => setType(event.target.value as GrenadeType)}
          className="rounded-md border border-neutral-800 bg-neutral-900 px-3 py-2 text-sm outline-none focus:border-neutral-500"
        >
          {grenadeTypes.map((grenadeType) => (
            <option key={grenadeType} value={grenadeType}>
              {grenadeTypeLabels[grenadeType]}
            </option>
          ))}
        </select>

        <input
          required
          placeholder="Nazwa pozycji (np. Mid smoke z T spawn)"
          value={title}
          onChange={(event) => setTitle(event.target.value)}
          className="flex-1 rounded-md border border-neutral-800 bg-neutral-900 px-3 py-2 text-sm outline-none focus:border-neutral-500"
        />
      </div>

      <input
        placeholder="Link do YouTube (opcjonalnie)"
        value={youtubeUrl}
        onChange={(event) => setYoutubeUrl(event.target.value)}
        className="rounded-md border border-neutral-800 bg-neutral-900 px-3 py-2 text-sm outline-none focus:border-neutral-500"
      />

      <textarea
        placeholder="Opis pozycji (opcjonalnie)"
        value={description}
        onChange={(event) => setDescription(event.target.value)}
        className="rounded-md border border-neutral-800 bg-neutral-900 px-3 py-2 text-sm outline-none focus:border-neutral-500"
      />

      {addNade.isError && <p className="text-sm text-red-400">Nie udało się dodać granatu.</p>}

      <button
        type="submit"
        disabled={addNade.isPending}
        className="self-start rounded-md bg-red-600 px-4 py-2 text-sm font-medium text-white transition hover:bg-red-500 disabled:opacity-50"
      >
        {addNade.isPending ? 'Dodawanie…' : 'Dodaj granat'}
      </button>
    </form>
  )
}
