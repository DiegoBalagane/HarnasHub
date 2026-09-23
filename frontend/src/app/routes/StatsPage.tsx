import { MyStatsHistory } from '../../features/stats/components/MyStatsHistory'
import { PlayerLeaderboard } from '../../features/stats/components/PlayerLeaderboard'
import { TeamTrendChart } from '../../features/stats/components/TeamTrendChart'

export function StatsPage() {
  return (
    <>
      <h1 className="text-2xl font-semibold">Rozwój</h1>
      <TeamTrendChart />
      <PlayerLeaderboard />
      <MyStatsHistory />
    </>
  )
}
