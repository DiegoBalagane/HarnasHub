import { useMaterials } from '../hooks/useMaterials'

/** Lists training materials as clickable links, grouped visually by category. */
export function MaterialList() {
  const { data: materials, isLoading, isError } = useMaterials()

  if (isLoading) {
    return <p className="text-neutral-400">Ładowanie materiałów…</p>
  }

  if (isError) {
    return <p className="text-red-400">Nie udało się pobrać materiałów.</p>
  }

  if (materials?.length === 0) {
    return <p className="text-neutral-400">Brak materiałów treningowych.</p>
  }

  return (
    <ul className="flex w-full max-w-xl flex-col gap-3">
      {materials?.map((material) => (
        <li key={material.id} className="rounded-md border border-neutral-800 p-4">
          <a
            href={material.url}
            target="_blank"
            rel="noreferrer"
            className="font-medium text-red-400 hover:underline"
          >
            {material.title}
          </a>
          {material.category && <p className="text-sm text-neutral-500">{material.category}</p>}
          {material.description && <p className="mt-1 text-sm text-neutral-400">{material.description}</p>}
        </li>
      ))}
    </ul>
  )
}
