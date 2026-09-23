import { useIsCoachOrManager } from '../../features/auth/hooks/useIsCoachOrManager'
import { AddIncidentForm } from '../../features/attendance/components/AddIncidentForm'
import { AttendanceSummaryTable } from '../../features/attendance/components/AttendanceSummaryTable'
import { IncidentList } from '../../features/attendance/components/IncidentList'

/** Team attendance overview: everyone sees every player's lateness/absence totals and history; only Coach/Manager can log or remove entries. */
export function AttendancePage() {
  const canManage = useIsCoachOrManager()

  return (
    <>
      <h1 className="text-2xl font-semibold">Frekwencja</h1>

      <AttendanceSummaryTable />

      {canManage && <AddIncidentForm />}

      <h2 className="font-medium">Historia wpisów</h2>
      <IncidentList />
    </>
  )
}
