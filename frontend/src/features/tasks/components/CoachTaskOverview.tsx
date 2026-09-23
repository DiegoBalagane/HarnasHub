import { useState } from 'react'
import { useMaterials } from '../../materials/hooks/useMaterials'
import type { TeamTaskWithAssignee } from '../../../services/tasksApi'
import { taskStatusColors, taskStatusLabels } from '../labels'
import { useAllTasks, useApproveTask, useDeleteTask, useRejectTask, useUpdateTask } from '../hooks/useTasks'

const dateFormatter = new Intl.DateTimeFormat('pl-PL', { dateStyle: 'medium' })

function groupByPlayer(tasks: TeamTaskWithAssignee[]) {
  const groups = new Map<string, { name: string; tasks: TeamTaskWithAssignee[] }>()

  for (const task of tasks) {
    const group = groups.get(task.assignedToUserId)
    if (group) {
      group.tasks.push(task)
    } else {
      groups.set(task.assignedToUserId, { name: task.assignedToDisplayName, tasks: [task] })
    }
  }

  return [...groups.values()]
}

/** Coach/Manager-only overview of every player's tasks: edit, mark done directly, approve/send-back a pending
 * review, or delete a mistaken assignment. */
export function CoachTaskOverview() {
  const { data: tasks, isLoading, isError } = useAllTasks()
  const approveTask = useApproveTask()
  const rejectTask = useRejectTask()
  const deleteTask = useDeleteTask()
  const [editingTaskId, setEditingTaskId] = useState<string | null>(null)

  if (isLoading) {
    return <p className="text-neutral-400">Ładowanie zadań zespołu…</p>
  }

  if (isError) {
    return <p className="text-red-400">Nie udało się pobrać zadań zespołu.</p>
  }

  if (tasks?.length === 0) {
    return <p className="text-neutral-400">Brak przydzielonych zadań.</p>
  }

  const groups = groupByPlayer(tasks ?? [])

  return (
    <div className="flex w-full max-w-3xl flex-col gap-4">
      <h2 className="font-medium">Zadania zawodników</h2>
      {groups.map((group) => (
        <div key={group.name} className="rounded-md border border-neutral-800 p-4">
          <p className="mb-2 font-medium">{group.name}</p>
          <ul className="flex flex-col gap-2">
            {group.tasks.map((task) =>
              editingTaskId === task.id ? (
                <TaskEditRow key={task.id} task={task} onDone={() => setEditingTaskId(null)} />
              ) : (
                <li
                  key={task.id}
                  className="flex items-start justify-between gap-3 rounded-md border border-neutral-800 p-3"
                >
                  <div>
                    <p className="text-sm font-medium">{task.title}</p>
                    <p className={`text-xs ${taskStatusColors[task.status]}`}>{taskStatusLabels[task.status]}</p>
                    {task.description && <p className="text-sm text-neutral-400">{task.description}</p>}
                    {task.dueAtUtc && (
                      <p className="text-xs text-neutral-500">
                        Termin: {dateFormatter.format(new Date(task.dueAtUtc))}
                      </p>
                    )}
                  </div>

                  <div className="flex shrink-0 flex-wrap justify-end gap-2">
                    {task.status !== 'Done' && (
                      <button
                        onClick={() => approveTask.mutate(task.id)}
                        disabled={approveTask.isPending}
                        className="rounded-md border border-green-700 px-2 py-1 text-xs text-green-400 hover:border-green-500 disabled:opacity-50"
                      >
                        Zaliczone
                      </button>
                    )}
                    {task.status === 'PendingReview' && (
                      <button
                        onClick={() => rejectTask.mutate(task.id)}
                        disabled={rejectTask.isPending}
                        className="rounded-md border border-red-700 px-2 py-1 text-xs text-red-400 hover:border-red-500 disabled:opacity-50"
                      >
                        Do poprawy
                      </button>
                    )}
                    <button
                      onClick={() => setEditingTaskId(task.id)}
                      className="rounded-md border border-neutral-700 px-2 py-1 text-xs hover:border-neutral-500"
                    >
                      Edytuj
                    </button>
                    <button
                      onClick={() => {
                        if (window.confirm('Usunąć to zadanie?')) {
                          deleteTask.mutate(task.id)
                        }
                      }}
                      disabled={deleteTask.isPending}
                      className="rounded-md border border-neutral-700 px-2 py-1 text-xs hover:border-neutral-500 disabled:opacity-50"
                    >
                      Usuń
                    </button>
                  </div>
                </li>
              ),
            )}
          </ul>
        </div>
      ))}
    </div>
  )
}

interface TaskEditRowProps {
  task: TeamTaskWithAssignee
  onDone: () => void
}

/** Inline form replacing one task row while its title/description/due date/material is being edited. */
function TaskEditRow({ task, onDone }: TaskEditRowProps) {
  const [title, setTitle] = useState(task.title)
  const [description, setDescription] = useState(task.description ?? '')
  const [dueAtUtc, setDueAtUtc] = useState(task.dueAtUtc ? task.dueAtUtc.slice(0, 10) : '')
  const [trainingMaterialId, setTrainingMaterialId] = useState(task.trainingMaterialId ?? '')
  const { data: materials } = useMaterials()
  const updateTask = useUpdateTask()

  function handleSubmit(event: React.FormEvent) {
    event.preventDefault()
    updateTask.mutate(
      {
        taskId: task.id,
        payload: {
          title,
          description: description || null,
          dueAtUtc: dueAtUtc ? new Date(dueAtUtc).toISOString() : null,
          trainingMaterialId: trainingMaterialId || null,
        },
      },
      { onSuccess: onDone },
    )
  }

  return (
    <li className="flex flex-col gap-2 rounded-md border border-neutral-700 bg-neutral-950 p-3">
      <form onSubmit={handleSubmit} className="flex flex-col gap-2">
        <input
          required
          value={title}
          onChange={(event) => setTitle(event.target.value)}
          className="rounded-md border border-neutral-800 bg-neutral-900 px-3 py-2 text-sm outline-none focus:border-neutral-500"
        />
        <textarea
          placeholder="Opis (opcjonalnie)"
          value={description}
          onChange={(event) => setDescription(event.target.value)}
          className="rounded-md border border-neutral-800 bg-neutral-900 px-3 py-2 text-sm outline-none focus:border-neutral-500"
        />
        <div className="flex gap-2">
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
          <input
            type="date"
            value={dueAtUtc}
            onChange={(event) => setDueAtUtc(event.target.value)}
            className="rounded-md border border-neutral-800 bg-neutral-900 px-3 py-2 text-sm outline-none focus:border-neutral-500"
          />
        </div>

        {updateTask.isError && <p className="text-sm text-red-400">Nie udało się zapisać zmian.</p>}

        <div className="flex gap-2">
          <button
            type="submit"
            disabled={updateTask.isPending}
            className="rounded-md bg-red-600 px-3 py-1.5 text-xs font-medium text-white transition hover:bg-red-500 disabled:opacity-50"
          >
            {updateTask.isPending ? 'Zapisywanie…' : 'Zapisz'}
          </button>
          <button
            type="button"
            onClick={onDone}
            className="rounded-md border border-neutral-700 px-3 py-1.5 text-xs text-neutral-300 transition hover:border-neutral-500"
          >
            Anuluj
          </button>
        </div>
      </form>
    </li>
  )
}
