import { useState } from 'react'
import { Navigate } from 'react-router-dom'
import { PageHeader } from '../../components/ui/PageHeader'
import { Tabs, type TabItem } from '../../components/ui/Tabs'
import { IntegrationsTab } from '../../features/admin/components/IntegrationsTab'
import { PinColorAssignmentPanel } from '../../features/admin/components/PinColorAssignmentPanel'
import { UsersTab } from '../../features/admin/components/UsersTab'
import { useAuthStore } from '../../features/auth/stores/useAuthStore'

type AdminTab = 'users' | 'pins' | 'integrations'

const tabs: readonly TabItem<AdminTab>[] = [
  { id: 'users', label: 'Użytkownicy' },
  { id: 'pins', label: 'Kolory pinezek' },
  { id: 'integrations', label: 'Integracje' },
]

/** Manager-only admin panel (accounts, pin colours, integration status); everyone else is redirected to the dashboard. */
export function AdminPage() {
  const role = useAuthStore((state) => state.role)
  const [tab, setTab] = useState<AdminTab>('users')

  if (role !== 'Manager') {
    return <Navigate to="/dashboard" replace />
  }

  return (
    <>
      <PageHeader title="Panel admina" />
      <Tabs tabs={tabs} value={tab} onChange={setTab} />
      {tab === 'users' && <UsersTab />}
      {tab === 'pins' && <PinColorAssignmentPanel />}
      {tab === 'integrations' && <IntegrationsTab />}
    </>
  )
}
