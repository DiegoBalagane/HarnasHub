import { PageHeader } from '../../components/ui/PageHeader'
import { AddFormModal } from '../../components/AddFormModal'
import { AssignTaskForm } from '../../features/tasks/components/AssignTaskForm'
import { CoachTaskOverview } from '../../features/tasks/components/CoachTaskOverview'
import { TaskList } from '../../features/tasks/components/TaskList'
import { useIsCoachOrManager } from '../../features/auth/hooks/useIsCoachOrManager'

export function TasksPage() {
  const canManage = useIsCoachOrManager()

  return (
    <>
      <PageHeader
        title="Zadania"
        actions={
          canManage && (
          <AddFormModal buttonLabel="+ Przydziel zadanie" title="Przydziel zadanie">
            {(close) => <AssignTaskForm onDone={close} />}
          </AddFormModal>
          )
        }
      />
      <TaskList />
      {canManage && <CoachTaskOverview />}
    </>
  )
}
