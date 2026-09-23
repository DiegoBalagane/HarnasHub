import { useState } from 'react'
import { useMaterials } from '../../materials/hooks/useMaterials'
import { useRoster } from '../../roster/hooks/useRoster'
import { useAssignTask } from '../hooks/useTasks'

/** Coach/Manager-only form for assigning a task to one or several players at once, optionally attaching a material to review. */
export function AssignTaskForm() {
  const { data: roster } = useRoster()
  const { data: materials } = useMaterials()
  const [title, setTitle] = useState('')
  const [description, setDescription] = useState('')
  const [assignedToUserIds, setAssignedToUserIds] = useState<string[]>([])
  const [trainingMaterialId, setTrainingMaterialId] = useState('')
  const [dueAtUtc, setDueAtUtc] = useState('')
  const [isSubmitting, setIsSubmitting] = useState(false)
  const [submitError, setSubmitError] = useState(false)
  const assignTask = useAssignTask()

  function toggleAssignee(userId: string) {
    setAssignedToUserIds((current) =>
      current.includes(userId) ? current.filter((id) => id !== userId) : [...current, userId],
    )
  }

  async function handleSubmit(event: React.FormEvent) {
    event.preventDefault()
    setSubmitError(false)
    setIsSubmitting(true)

    try {
      await Promise.all(
        assignedToUserIds.map((assignedToUserId) =>
          assignTask.mutateAsync({
            title,
            description: description || undefined,
            assignedToUserId,
            trainingMaterialId: trainingMaterialId || undefined,
            dueAtUtc: dueAtUtc ? new Date(dueAtUtc).toISOString() : undefined,
          }),
        ),
      )
      setTitle('')
      setDescription('')
      setAssignedToUserIds([])
      setTrainingMaterialId('')
      setDueAtUtc('')
    } catch {
      setSubmitError(true)
    } finally {
      setIsSubmitting(false)
    }
  }

  return (
    <form
      onSubmit={handleSubmit}
      className="flex w-full max-w-xl flex-col gap-3 rounded-md border border-neutral-800 p-4"
    >
      <h2 className="font-medium">Przydziel zadanie</h2>

      <input
        required
        placeholder="Tytuł zadania"
        value={title}
        onChange={(event) => setTitle(event.target.value)}
        className="rounded-md border border-neutral-800 bg-neutral-900 px-3 py-2 text-sm outline-none focus:border-neutral-500"
      />

      <div className="flex flex-col gap-1">
        <p className="text-sm text-neutral-400">Zawodnicy (można zaznaczyć kilku)</p>
        <div className="flex max-h-40 flex-col gap-1 overflow-y-auto rounded-md border border-neutral-800 bg-neutral-900 p-2">
          {roster?.map((member) => (
            <label key={member.id} className="flex items-center gap-2 rounded px-1 py-1 text-sm hover:bg-neutral-800">
              <input
                type="checkbox"
                checked={assignedToUserIds.includes(member.id)}
                onChange={() => toggleAssignee(member.id)}
              />
              {member.inGameNickname ?? member.displayName}
            </label>
          ))}
        </div>
      </div>

      <textarea
        placeholder="Opis (opcjonalnie)"
        value={description}
        onChange={(event) => setDescription(event.target.value)}
        className="rounded-md border border-neutral-800 bg-neutral-900 px-3 py-2 text-sm outline-none focus:border-neutral-500"
      />

      <div className="flex gap-3">
        <select
          value={trainingMaterialId}
          onChange={(event) => setTrainingMaterialId(event.target.value)}
          className="flex-1 rounded-md border border-neutral-800 bg-neutral-900 px-3 py-2 text-sm outline-none focus:border-neutral-500"
        >
          <option value="">Bez materiału do obejrzenia</option>
          {materials?.map((material) => (
            <option key={material.id} value={material.id}>
              {material.title}
            </option>
          ))}
        </select>
        <label className="flex items-center gap-2 text-sm text-neutral-400">
          Termin (opcjonalnie)
          <input
            type="date"
            value={dueAtUtc}
            onChange={(event) => setDueAtUtc(event.target.value)}
            className="rounded-md border border-neutral-800 bg-neutral-900 px-3 py-2 text-sm outline-none focus:border-neutral-500"
          />
        </label>
      </div>

      {submitError && <p className="text-sm text-red-400">Nie udało się przydzielić zadania.</p>}

      <button
        type="submit"
        disabled={isSubmitting || assignedToUserIds.length === 0}
        className="self-start rounded-md bg-red-600 px-4 py-2 text-sm font-medium text-white transition hover:bg-red-500 disabled:opacity-50"
      >
        {isSubmitting
          ? 'Przydzielanie…'
          : assignedToUserIds.length > 1
            ? `Przydziel zadanie (${assignedToUserIds.length} graczy)`
            : 'Przydziel zadanie'}
      </button>
    </form>
  )
}
