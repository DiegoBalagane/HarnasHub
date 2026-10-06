import { PageHeader } from '../../components/ui/PageHeader'
import { AddFormModal } from '../../components/AddFormModal'
import { useIsCoachOrManager } from '../../features/auth/hooks/useIsCoachOrManager'
import { AddIncidentForm } from '../../features/attendance/components/AddIncidentForm'
import { AttendanceSummaryTable } from '../../features/attendance/components/AttendanceSummaryTable'
import { IncidentList } from '../../features/attendance/components/IncidentList'

/** Team attendance overview: everyone sees every player's lateness/absence totals and history; only Coach/Manager can log or remove entries. */
export function AttendancePage() {
  const canManage = useIsCoachOrManager()

  return (
    <>
      <PageHeader
        title="Frekwencja"
        actions={
          canManage && (
          <AddFormModal buttonLabel="+ Dodaj wpis" title="Dodaj spóźnienie / nieobecność">
            {(close) => <AddIncidentForm onDone={close} />}
          </AddFormModal>
          )
        }
      />

      <AttendanceSummaryTable />

      <h2 className="font-medium">Historia wpisów</h2>
      <IncidentList />
    </>
  )
}
