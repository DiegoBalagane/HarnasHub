import { useState } from 'react'
import { PageHeader } from '../../components/ui/PageHeader'
import { Tabs, type TabItem } from '../../components/ui/Tabs'
import { AdvancedStatsTab } from '../../features/stats/components/AdvancedStatsTab'
import { MyStatsHistory } from '../../features/stats/components/MyStatsHistory'
import { PlayerLeaderboard } from '../../features/stats/components/PlayerLeaderboard'
import { TeamTrendChart } from '../../features/stats/components/TeamTrendChart'

type StatsTab = 'overview' | 'advanced'

const tabs: readonly TabItem<StatsTab>[] = [
  { id: 'overview', label: 'Przegląd' },
  { id: 'advanced', label: 'Zaawansowane' },
]

/** "Rozwój" page: team trend and leaderboard (Przegląd) plus demo-derived metrics (Zaawansowane). */
export function StatsPage() {
  const [tab, setTab] = useState<StatsTab>('overview')

  return (
    <>
      <PageHeader title="Rozwój" />
      <Tabs tabs={tabs} value={tab} onChange={setTab} className="mb-4" />
      {tab === 'overview' ? (
        <>
          <TeamTrendChart />
          <PlayerLeaderboard />
          <MyStatsHistory />
        </>
      ) : (
        <AdvancedStatsTab />
      )}
    </>
  )
}
