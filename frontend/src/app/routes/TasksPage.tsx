import { AssignTaskForm } from '../../features/tasks/components/AssignTaskForm'
import { TaskList } from '../../features/tasks/components/TaskList'
import { useIsCoachOrManager } from '../../features/auth/hooks/useIsCoachOrManager'

export function TasksPage() {
  const canManage = useIsCoachOrManager()

  return (
    <>
      <h1 className="text-2xl font-semibold">Zadania</h1>
      {canManage && <AssignTaskForm />}
      <TaskList />
    </>
  )
}
