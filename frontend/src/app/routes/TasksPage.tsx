import { AssignTaskForm } from '../../features/tasks/components/AssignTaskForm'
import { TaskList } from '../../features/tasks/components/TaskList'
import { useAuthStore } from '../../features/auth/stores/useAuthStore'

const coachRoles = new Set(['Coach', 'Manager'])

export function TasksPage() {
  const role = useAuthStore((state) => state.role)
  const canManage = role !== null && coachRoles.has(role)

  return (
    <>
      <h1 className="text-2xl font-semibold">Zadania</h1>
      {canManage && <AssignTaskForm />}
      <TaskList />
    </>
  )
}
