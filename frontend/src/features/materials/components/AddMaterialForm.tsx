import { useState } from 'react'
import { useAddMaterial } from '../hooks/useMaterials'

/** Coach/Manager-only form for adding a training material link. */
export function AddMaterialForm() {
  const [title, setTitle] = useState('')
  const [url, setUrl] = useState('')
  const [category, setCategory] = useState('')
  const [description, setDescription] = useState('')
  const addMaterial = useAddMaterial()

  function handleSubmit(event: React.FormEvent) {
    event.preventDefault()
    addMaterial.mutate(
      { title, url, category: category || undefined, description: description || undefined },
      {
        onSuccess: () => {
          setTitle('')
          setUrl('')
          setCategory('')
          setDescription('')
        },
      },
    )
  }

  return (
    <form
      onSubmit={handleSubmit}
      className="flex w-full max-w-xl flex-col gap-3 rounded-md border border-neutral-800 p-4"
    >
      <h2 className="font-medium">Dodaj materiał</h2>

      <div className="flex gap-3">
        <input
          required
          placeholder="Tytuł"
          value={title}
          onChange={(event) => setTitle(event.target.value)}
          className="flex-1 rounded-md border border-neutral-800 bg-neutral-900 px-3 py-2 text-sm outline-none focus:border-neutral-500"
        />
        <input
          placeholder="Kategoria (opcjonalnie)"
          value={category}
          onChange={(event) => setCategory(event.target.value)}
          className="w-48 rounded-md border border-neutral-800 bg-neutral-900 px-3 py-2 text-sm outline-none focus:border-neutral-500"
        />
      </div>

      <input
        required
        placeholder="Link"
        value={url}
        onChange={(event) => setUrl(event.target.value)}
        className="rounded-md border border-neutral-800 bg-neutral-900 px-3 py-2 text-sm outline-none focus:border-neutral-500"
      />

      <textarea
        placeholder="Opis (opcjonalnie)"
        value={description}
        onChange={(event) => setDescription(event.target.value)}
        className="rounded-md border border-neutral-800 bg-neutral-900 px-3 py-2 text-sm outline-none focus:border-neutral-500"
      />

      {addMaterial.isError && <p className="text-sm text-red-400">Nie udało się dodać materiału.</p>}

      <button
        type="submit"
        disabled={addMaterial.isPending}
        className="self-start rounded-md bg-red-600 px-4 py-2 text-sm font-medium text-white transition hover:bg-red-500 disabled:opacity-50"
      >
        {addMaterial.isPending ? 'Dodawanie…' : 'Dodaj materiał'}
      </button>
    </form>
  )
}
