import { PageHeader } from '../../components/ui/PageHeader'
import { DashboardSummary } from '../../features/dashboard/components/DashboardSummary'

export function DashboardPage() {
  return (
    <>
      <PageHeader title="Dashboard" />
      <DashboardSummary />
    </>
  )
}
