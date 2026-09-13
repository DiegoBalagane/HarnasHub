import { AddResultForm } from '../../features/results/components/AddResultForm'
import { ResultList } from '../../features/results/components/ResultList'
import { useAuthStore } from '../../features/auth/stores/useAuthStore'

const coachRoles = new Set(['Coach', 'Manager'])

export function ResultsPage() {
  const role = useAuthStore((state) => state.role)
  const canManage = role !== null && coachRoles.has(role)

  return (
    <>
      <h1 className="text-2xl font-semibold">Wyniki</h1>
      {canManage && <AddResultForm />}
      <ResultList />
    </>
  )
}
