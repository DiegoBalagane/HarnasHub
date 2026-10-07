import { useState } from 'react'
import { useModalGuard } from '../../../components/ModalGuardContext'
import { Button } from '../../../components/ui/Button'
import type { TeamInfoEntry } from '../../../services/teamInfoApi'
import { defaultCategories, sortCategories } from '../entryValue'
import { useCreateTeamInfo, useUpdateTeamInfo } from '../hooks/useTeamInfo'

const inputClass =
  'rounded-md border border-neutral-800 bg-neutral-900 px-3 py-2 text-sm text-neutral-100 outline-none focus:border-neutral-500'
const NEW_CATEGORY = '__new__'

interface TeamInfoFormProps {
  /** The entry being edited; omitted when adding a new one. */
  entry?: TeamInfoEntry
  /** Categories already in use, offered next to the defaults. */
  existingCategories: string[]
  /** Called after a successful save — e.g. to close the hosting modal. */
  onDone: () => void
}

/** Coach/Manager form to add or edit an info entry: category (existing or new), title, value and the secret flag. */
export function TeamInfoForm({ entry, existingCategories, onDone }: TeamInfoFormProps) {
  const options = sortCategories([...new Set([...defaultCategories, ...existingCategories])])
  const [category, setCategory] = useState(entry?.category ?? options[0])
  const [newCategory, setNewCategory] = useState('')
  const [title, setTitle] = useState(entry?.title ?? '')
  const [value, setValue] = useState(entry?.value ?? '')
  const [isSecret, setIsSecret] = useState(entry?.isSecret ?? false)
  const create = useCreateTeamInfo()
  const update = useUpdateTeamInfo()
  const mutation = entry ? update : create
  const resolvedCategory = (category === NEW_CATEGORY ? newCategory : category).trim()

  const isDirty =
    title !== (entry?.title ?? '') ||
    value !== (entry?.value ?? '') ||
    isSecret !== (entry?.isSecret ?? false) ||
    resolvedCategory !== (entry?.category ?? options[0])
  useModalGuard({ isBusy: mutation.isPending, isDirty })

  function handleSubmit(event: React.FormEvent) {
    event.preventDefault()
    const payload = { category: resolvedCategory, title: title.trim(), value: value.trim(), isSecret }
    if (entry) update.mutate({ id: entry.id, payload }, { onSuccess: onDone })
    else create.mutate(payload, { onSuccess: onDone })
  }

  return (
    <form onSubmit={handleSubmit} className="flex flex-col gap-3">
      <label className="flex flex-col gap-1 text-xs text-neutral-400">
        Kategoria
        <select value={category} onChange={(event) => setCategory(event.target.value)} className={inputClass}>
          {options.map((option) => (
            <option key={option} value={option}>
              {option}
            </option>
          ))}
          <option value={NEW_CATEGORY}>+ Nowa kategoria…</option>
        </select>
      </label>

      {category === NEW_CATEGORY && (
        <input
          required
          maxLength={50}
          aria-label="Nazwa nowej kategorii"
          placeholder="Nazwa nowej kategorii"
          value={newCategory}
          onChange={(event) => setNewCategory(event.target.value)}
          className={inputClass}
        />
      )}

      <label className="flex flex-col gap-1 text-xs text-neutral-400">
        Tytuł
        <input
          required
          maxLength={100}
          placeholder="np. Serwer treningowy"
          value={title}
          onChange={(event) => setTitle(event.target.value)}
          className={inputClass}
        />
      </label>

      <label className="flex flex-col gap-1 text-xs text-neutral-400">
        Wartość
        <textarea
          required
          rows={4}
          maxLength={2000}
          placeholder="Adres, link, komenda connect lub fragment konfiguracji"
          value={value}
          onChange={(event) => setValue(event.target.value)}
          className={`${inputClass} font-mono`}
        />
      </label>

      <label className="flex items-center gap-2 text-sm text-neutral-300">
        <input type="checkbox" checked={isSecret} onChange={(event) => setIsSecret(event.target.checked)} />
        Ukryj wartość (np. hasło) — odsłaniana przyciskiem „Pokaż”
      </label>

      {mutation.isError && <p className="text-sm text-danger-400">Nie udało się zapisać wpisu.</p>}

      <Button type="submit" className="self-start" disabled={mutation.isPending || !resolvedCategory}>
        {mutation.isPending ? 'Zapisywanie…' : 'Zapisz'}
      </Button>
    </form>
  )
}
