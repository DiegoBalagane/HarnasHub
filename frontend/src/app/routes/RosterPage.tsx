import { useIsCoachOrManager } from '../../features/auth/hooks/useIsCoachOrManager'
import { PinColorAssignmentPanel } from '../../features/roster/components/PinColorAssignmentPanel'
import { RosterBoard } from '../../features/roster/components/RosterBoard'
import { RosterList } from '../../features/roster/components/RosterList'
import { SteamIdAssignmentPanel } from '../../features/roster/components/SteamIdAssignmentPanel'

export function RosterPage() {
  const canManageRoster = useIsCoachOrManager()

  return (
    <>
      <h1 className="text-2xl font-semibold">Skład drużyny</h1>
      {canManageRoster && <RosterBoard />}
      <PinColorAssignmentPanel />
      <SteamIdAssignmentPanel />
      <RosterList />
    </>
  )
}
