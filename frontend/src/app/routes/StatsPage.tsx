import { PageHeader } from '../../components/ui/PageHeader'
import { MyStatsHistory } from '../../features/stats/components/MyStatsHistory'
import { PlayerLeaderboard } from '../../features/stats/components/PlayerLeaderboard'
import { TeamTrendChart } from '../../features/stats/components/TeamTrendChart'

export function StatsPage() {
  return (
    <>
      <PageHeader title="Rozwój" />
      <TeamTrendChart />
      <PlayerLeaderboard />
      <MyStatsHistory />
    </>
  )
}
