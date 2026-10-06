import { PageHeader } from '../../components/ui/PageHeader'
import { AddFormModal } from '../../components/AddFormModal'
import { AddResultForm } from '../../features/results/components/AddResultForm'
import { ResultList } from '../../features/results/components/ResultList'
import { useIsCoachOrManager } from '../../features/auth/hooks/useIsCoachOrManager'

export function ResultsPage() {
  const canManage = useIsCoachOrManager()

  return (
    <>
      <PageHeader
        title="Wyniki"
        actions={
          canManage && (
          <AddFormModal buttonLabel="+ Dodaj wynik" title="Dodaj wynik">
            {(close) => <AddResultForm onDone={close} />}
          </AddFormModal>
          )
        }
      />
      <ResultList />
    </>
  )
}
