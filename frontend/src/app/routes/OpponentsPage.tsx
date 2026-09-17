import { AddOpponentNoteForm } from '../../features/opponents/components/AddOpponentNoteForm'
import { OpponentNotesList } from '../../features/opponents/components/OpponentNotesList'
import { useIsCoachOrManager } from '../../features/auth/hooks/useIsCoachOrManager'

export function OpponentsPage() {
  const canManage = useIsCoachOrManager()

  return (
    <>
      <h1 className="text-2xl font-semibold">Przeciwnicy</h1>
      {canManage && <AddOpponentNoteForm />}
      <OpponentNotesList />
    </>
  )
}
