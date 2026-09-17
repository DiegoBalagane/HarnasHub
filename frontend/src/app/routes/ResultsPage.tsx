import { AddResultForm } from '../../features/results/components/AddResultForm'
import { ResultList } from '../../features/results/components/ResultList'
import { useIsCoachOrManager } from '../../features/auth/hooks/useIsCoachOrManager'

export function ResultsPage() {
  const canManage = useIsCoachOrManager()

  return (
    <>
      <h1 className="text-2xl font-semibold">Wyniki</h1>
      {canManage && <AddResultForm />}
      <ResultList />
    </>
  )
}
