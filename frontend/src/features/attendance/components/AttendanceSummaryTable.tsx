import { useAttendanceSummary } from '../hooks/useAttendance'

/** Team-wide overview: every roster player's total lateness/absence counts — visible to everyone. */
export function AttendanceSummaryTable() {
  const { data: entries, isLoading, isError } = useAttendanceSummary()

  if (isLoading) {
    return <p className="text-neutral-400">Ładowanie…</p>
  }

  if (isError) {
    return <p className="text-red-400">Nie udało się pobrać podsumowania.</p>
  }

  if (entries?.length === 0) {
    return <p className="text-neutral-400">Brak zawodników do pokazania.</p>
  }

  return (
    <div className="overflow-x-auto">
      <table className="w-full text-left text-sm">
        <thead className="text-neutral-500">
          <tr>
            <th className="pb-2 pr-3 font-normal">Zawodnik</th>
            <th className="pb-2 pr-3 font-normal">Spóźnienia</th>
            <th className="pb-2 pr-3 font-normal">Nieobecności</th>
          </tr>
        </thead>
        <tbody>
          {entries?.map((entry) => (
            <tr key={entry.userId} className="border-t border-neutral-900 text-neutral-300">
              <td className="py-1.5 pr-3 font-medium">{entry.playerName}</td>
              <td className="pr-3">{entry.lateCount}</td>
              <td className="pr-3">{entry.absentCount}</td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  )
}
