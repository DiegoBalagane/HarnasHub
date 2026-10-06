import { Link } from 'react-router-dom'
import { PageHeader } from '../../components/ui/PageHeader'
import { useIsCoachOrManager } from '../../features/auth/hooks/useIsCoachOrManager'
import { useAuthStore } from '../../features/auth/stores/useAuthStore'
import { RosterBoard } from '../../features/roster/components/RosterBoard'
import { RosterList } from '../../features/roster/components/RosterList'

export function RosterPage() {
  const canManageRoster = useIsCoachOrManager()
  const isManager = useAuthStore((state) => state.role === 'Manager')

  return (
    <>
      <PageHeader
        title="Skład drużyny"
        actions={
          isManager && (
            <Link to="/admin" className="text-xs text-neutral-500 transition hover:text-neutral-300">
              Zarządzaj kontami → Panel admina
            </Link>
          )
        }
      />
      {canManageRoster && <RosterBoard />}
      <RosterList />
    </>
  )
}
