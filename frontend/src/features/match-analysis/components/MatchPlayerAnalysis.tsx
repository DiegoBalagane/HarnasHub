import { memo, useMemo } from 'react'
import { useMatchDeepAnalysis } from '../hooks/useMatchAnalysis'

interface MatchPlayerAnalysisProps {
  matchResultId: string
}

interface PlayerRow {
  name: string
  tradeKills: number
  tradedDeaths: number
  deaths: number
  clutchWon: number
  clutchAttempts: number
  openingWon: number
  openingLost: number
  enemiesFlashed: number
  avgBlind: number
  teamFlashes: number
}

function emptyRow(name: string): PlayerRow {
  return {
    name,
    tradeKills: 0,
    tradedDeaths: 0,
    deaths: 0,
    clutchWon: 0,
    clutchAttempts: 0,
    openingWon: 0,
    openingLost: 0,
    enemiesFlashed: 0,
    avgBlind: 0,
    teamFlashes: 0,
  }
}

/** Per-player table from the match analysis: trades, clutches, opening duels and flashes of our players. */
export const MatchPlayerAnalysis = memo(function MatchPlayerAnalysis({ matchResultId }: MatchPlayerAnalysisProps) {
  const { data: analysis } = useMatchDeepAnalysis(matchResultId)

  const rows = useMemo(() => {
    if (!analysis) return []
    const map = new Map<string, PlayerRow>()
    const row = (steamId64: string, name: string) => {
      const existing = map.get(steamId64)
      if (existing) return existing
      const created = emptyRow(name)
      map.set(steamId64, created)
      return created
    }

    analysis.trades.players.forEach((p) =>
      Object.assign(row(p.steamId64, p.name), { tradeKills: p.tradeKills, tradedDeaths: p.tradedDeaths, deaths: p.deaths }),
    )
    analysis.clutches.players.forEach((p) =>
      Object.assign(row(p.steamId64, p.name), { clutchWon: p.won, clutchAttempts: p.attempts }),
    )
    analysis.openings.players.forEach((p) =>
      Object.assign(row(p.steamId64, p.name), { openingWon: p.won, openingLost: p.lost }),
    )
    analysis.flashes.players.forEach((p) =>
      Object.assign(row(p.steamId64, p.name), {
        enemiesFlashed: p.enemiesFlashed,
        avgBlind: p.avgEnemyBlindSeconds,
        teamFlashes: p.teamFlashes,
      }),
    )
    return [...map.values()].sort((a, b) => a.name.localeCompare(b.name))
  }, [analysis])

  if (!analysis || rows.length === 0) {
    return null
  }

  const hasFlashes = analysis.flashes.hasData

  return (
    <section className="flex flex-col gap-2">
      <h2 className="text-lg font-semibold">Analiza graczy z demki</h2>
      <div className="overflow-x-auto rounded-md border border-neutral-800">
        <table className="w-full text-left text-sm">
          <thead className="text-xs text-neutral-500">
            <tr>
              <th className="px-3 py-2">Gracz</th>
              <th className="px-3 py-2">Trade kille</th>
              <th className="px-3 py-2">Odpłacone zgony</th>
              <th className="px-3 py-2">Clutche</th>
              <th className="px-3 py-2">Otwarcia</th>
              <th className="px-3 py-2">Wrogowie oślepieni</th>
              <th className="px-3 py-2">Śr. oślepienie</th>
              <th className="px-3 py-2">Team flashe</th>
            </tr>
          </thead>
          <tbody>
            {rows.map((player) => (
              <tr key={player.name} className="border-t border-neutral-800">
                <td className="px-3 py-1.5 font-medium">{player.name}</td>
                <td className="px-3 py-1.5">{player.tradeKills}</td>
                <td className="px-3 py-1.5">
                  {player.tradedDeaths}/{player.deaths}
                </td>
                <td className="px-3 py-1.5">
                  {player.clutchWon}/{player.clutchAttempts}
                </td>
                <td className="px-3 py-1.5">
                  {player.openingWon}–{player.openingLost}
                </td>
                <td className="px-3 py-1.5">{hasFlashes ? player.enemiesFlashed : '—'}</td>
                <td className="px-3 py-1.5">
                  {hasFlashes && player.enemiesFlashed > 0 ? `${player.avgBlind.toFixed(1)} s` : '—'}
                </td>
                <td className={`px-3 py-1.5 ${player.teamFlashes > 2 ? 'text-danger-400' : ''}`}>
                  {hasFlashes ? player.teamFlashes : '—'}
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>
    </section>
  )
})
