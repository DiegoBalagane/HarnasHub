import { useMyTasks, useCompleteTask } from '../hooks/useTasks'

const dateFormatter = new Intl.DateTimeFormat('pl-PL', { dateStyle: 'medium' })

/** Lists the current user's tasks with a button to mark open ones done. */
export function TaskList() {
  const { data: tasks, isLoading, isError } = useMyTasks()
  const completeTask = useCompleteTask()

  if (isLoading) {
    return <p className="text-neutral-400">Ładowanie zadań…</p>
  }

  if (isError) {
    return <p className="text-red-400">Nie udało się pobrać zadań.</p>
  }

  if (tasks?.length === 0) {
    return <p className="text-neutral-400">Brak przypisanych zadań.</p>
  }

  return (
    <ul className="flex w-full max-w-xl flex-col gap-3">
      {tasks?.map((task) => (
        <li
          key={task.id}
          className="flex items-start justify-between rounded-md border border-neutral-800 p-4"
        >
          <div>
            <p
              className={task.status === 'Done' ? 'font-medium line-through text-neutral-500' : 'font-medium'}
            >
              {task.title}
            </p>
            {task.description && <p className="text-sm text-neutral-400">{task.description}</p>}
            {task.trainingMaterialUrl && (
              <a
                href={task.trainingMaterialUrl}
                target="_blank"
                rel="noreferrer"
                className="mt-1 inline-block text-sm text-red-400 hover:text-red-300"
              >
                📎 {task.trainingMaterialTitle}
              </a>
            )}
            {task.dueAtUtc && (
              <p className="text-xs text-neutral-500">
                Termin: {dateFormatter.format(new Date(task.dueAtUtc))}
              </p>
            )}
          </div>

          {task.status === 'Todo' && (
            <button
              onClick={() => completeTask.mutate(task.id)}
              disabled={completeTask.isPending}
              className="rounded-md border border-neutral-700 px-3 py-1 text-xs hover:border-neutral-500 disabled:opacity-50"
            >
              Zrobione
            </button>
          )}
        </li>
      ))}
    </ul>
  )
}
