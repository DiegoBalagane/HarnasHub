import { useState } from 'react'
import { useRoster } from '../../roster/hooks/useRoster'
import { useAssignTask } from '../hooks/useTasks'

/** Coach/Manager-only form for assigning a task to a player. */
export function AssignTaskForm() {
  const { data: roster } = useRoster()
  const [title, setTitle] = useState('')
  const [description, setDescription] = useState('')
  const [assignedToUserId, setAssignedToUserId] = useState('')
  const assignTask = useAssignTask()

  function handleSubmit(event: React.FormEvent) {
    event.preventDefault()
    assignTask.mutate(
      { title, description: description || undefined, assignedToUserId },
      {
        onSuccess: () => {
          setTitle('')
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
      <h2 className="font-medium">Przydziel zadanie</h2>

      <div className="flex gap-3">
        <input
          required
          placeholder="Tytuł zadania"
          value={title}
          onChange={(event) => setTitle(event.target.value)}
          className="flex-1 rounded-md border border-neutral-800 bg-neutral-900 px-3 py-2 text-sm outline-none focus:border-neutral-500"
        />
        <select
          required
          value={assignedToUserId}
          onChange={(event) => setAssignedToUserId(event.target.value)}
          className="rounded-md border border-neutral-800 bg-neutral-900 px-3 py-2 text-sm outline-none focus:border-neutral-500"
        >
          <option value="" disabled>
            Wybierz zawodnika
          </option>
          {roster?.map((member) => (
            <option key={member.id} value={member.id}>
              {member.displayName}
            </option>
          ))}
        </select>
      </div>

      <textarea
        placeholder="Opis (opcjonalnie)"
        value={description}
        onChange={(event) => setDescription(event.target.value)}
        className="rounded-md border border-neutral-800 bg-neutral-900 px-3 py-2 text-sm outline-none focus:border-neutral-500"
      />

      {assignTask.isError && <p className="text-sm text-red-400">Nie udało się przydzielić zadania.</p>}

      <button
        type="submit"
        disabled={assignTask.isPending}
        className="self-start rounded-md bg-red-600 px-4 py-2 text-sm font-medium text-white transition hover:bg-red-500 disabled:opacity-50"
      >
        {assignTask.isPending ? 'Przydzielanie…' : 'Przydziel zadanie'}
      </button>
    </form>
  )
}
