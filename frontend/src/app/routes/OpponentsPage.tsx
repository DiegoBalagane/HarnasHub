import { AddOpponentNoteForm } from '../../features/opponents/components/AddOpponentNoteForm'
import { OpponentNotesList } from '../../features/opponents/components/OpponentNotesList'
import { useAuthStore } from '../../features/auth/stores/useAuthStore'

const coachRoles = new Set(['Coach', 'Manager'])

export function OpponentsPage() {
  const role = useAuthStore((state) => state.role)
  const canManage = role !== null && coachRoles.has(role)

  return (
    <>
      <h1 className="text-2xl font-semibold">Przeciwnicy</h1>
      {canManage && <AddOpponentNoteForm />}
      <OpponentNotesList />
    </>
  )
}
