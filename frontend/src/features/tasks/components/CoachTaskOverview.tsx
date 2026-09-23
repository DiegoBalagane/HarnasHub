import type { TeamTaskWithAssignee } from '../../../services/tasksApi'
import { taskStatusColors, taskStatusLabels } from '../labels'
import { useAllTasks, useApproveTask, useDeleteTask, useRejectTask } from '../hooks/useTasks'

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

/** Coach/Manager-only overview of every player's tasks: approve/send-back pending reviews, delete mistakes. */
export function CoachTaskOverview() {
  const { data: tasks, isLoading, isError } = useAllTasks()
  const approveTask = useApproveTask()
  const rejectTask = useRejectTask()
  const deleteTask = useDeleteTask()

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
            {group.tasks.map((task) => (
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

                <div className="flex shrink-0 gap-2">
                  {task.status === 'PendingReview' && (
                    <>
                      <button
                        onClick={() => approveTask.mutate(task.id)}
                        disabled={approveTask.isPending}
                        className="rounded-md border border-green-700 px-2 py-1 text-xs text-green-400 hover:border-green-500 disabled:opacity-50"
                      >
                        Zaliczone
                      </button>
                      <button
                        onClick={() => rejectTask.mutate(task.id)}
                        disabled={rejectTask.isPending}
                        className="rounded-md border border-red-700 px-2 py-1 text-xs text-red-400 hover:border-red-500 disabled:opacity-50"
                      >
                        Do poprawy
                      </button>
                    </>
                  )}
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
            ))}
          </ul>
        </div>
      ))}
    </div>
  )
}
