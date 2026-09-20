import { useState } from 'react'
import { useIsCoachOrManager } from '../../auth/hooks/useIsCoachOrManager'
import { useRoster } from '../../roster/hooks/useRoster'
import { useAddPlayerStat, useMatchStats } from '../hooks/useStats'
import { DemoImportPanel } from './DemoImportPanel'

/** Expandable panel showing per-player stats for a match, with a Coach/Manager form to add a line. */
export function MatchStatsPanel({ matchResultId }: { matchResultId: string }) {
  const { data: stats, isLoading } = useMatchStats(matchResultId, true)
  const { data: roster } = useRoster()
  const canAddStats = useIsCoachOrManager()
  const addStat = useAddPlayerStat(matchResultId)

  const [userId, setUserId] = useState('')
  const [kills, setKills] = useState('')
  const [deaths, setDeaths] = useState('')
  const [assists, setAssists] = useState('')
  const [adr, setAdr] = useState('')
  const [hs, setHs] = useState('')
  const [rating, setRating] = useState('')

  function handleSubmit(event: React.FormEvent) {
    event.preventDefault()
    addStat.mutate(
      {
        userId,
        kills: Number(kills),
        deaths: Number(deaths),
        assists: Number(assists) || 0,
        adr: Number(adr) || 0,
        headshotPercentage: Number(hs) || 0,
        rating: Number(rating) || 0,
      },
      {
        onSuccess: () => {
          setUserId('')
          setKills('')
          setDeaths('')
          setAssists('')
          setAdr('')
          setHs('')
          setRating('')
        },
      },
    )
  }

  const alreadyTracked = new Set(stats?.map((s) => s.userId))
  const availablePlayers = roster?.filter((member) => !alreadyTracked.has(member.id))

  return (
    <div className="flex flex-col gap-3 border-t border-neutral-800 pt-3">
      {isLoading ? (
        <p className="text-xs text-neutral-500">Ładowanie statystyk…</p>
      ) : stats?.length === 0 ? (
        <p className="text-xs text-neutral-500">Brak wpisanych statystyk.</p>
      ) : (
        <table className="w-full text-left text-xs">
          <thead className="text-neutral-500">
            <tr>
              <th className="pb-1 font-normal">Gracz</th>
              <th className="pb-1 font-normal">K</th>
              <th className="pb-1 font-normal">D</th>
              <th className="pb-1 font-normal">A</th>
              <th className="pb-1 font-normal">ADR</th>
              <th className="pb-1 font-normal">HS%</th>
              <th className="pb-1 font-normal">Rating</th>
            </tr>
          </thead>
          <tbody>
            {stats?.map((stat) => (
              <tr key={stat.id} className="text-neutral-300">
                <td className="py-0.5">{stat.displayName}</td>
                <td>{stat.kills}</td>
                <td>{stat.deaths}</td>
                <td>{stat.assists}</td>
                <td>{stat.adr.toFixed(0)}</td>
                <td>{stat.headshotPercentage.toFixed(0)}</td>
                <td>{stat.rating.toFixed(2)}</td>
              </tr>
            ))}
          </tbody>
        </table>
      )}

      {canAddStats && availablePlayers && availablePlayers.length > 0 && (
        <form onSubmit={handleSubmit} className="flex flex-wrap items-end gap-2">
          <select
            required
            value={userId}
            onChange={(event) => setUserId(event.target.value)}
            className="rounded-md border border-neutral-800 bg-neutral-900 px-2 py-1 text-xs outline-none focus:border-neutral-500"
          >
            <option value="" disabled>
              Gracz
            </option>
            {availablePlayers.map((member) => (
              <option key={member.id} value={member.id}>
                {member.displayName}
              </option>
            ))}
          </select>
          <input
            required
            type="number"
            min={0}
            placeholder="K"
            value={kills}
            onChange={(event) => setKills(event.target.value)}
            className="w-14 rounded-md border border-neutral-800 bg-neutral-900 px-2 py-1 text-xs outline-none focus:border-neutral-500"
          />
          <input
            required
            type="number"
            min={0}
            placeholder="D"
            value={deaths}
            onChange={(event) => setDeaths(event.target.value)}
            className="w-14 rounded-md border border-neutral-800 bg-neutral-900 px-2 py-1 text-xs outline-none focus:border-neutral-500"
          />
          <input
            type="number"
            min={0}
            placeholder="A"
            value={assists}
            onChange={(event) => setAssists(event.target.value)}
            className="w-14 rounded-md border border-neutral-800 bg-neutral-900 px-2 py-1 text-xs outline-none focus:border-neutral-500"
          />
          <input
            type="number"
            min={0}
            step="any"
            placeholder="ADR"
            value={adr}
            onChange={(event) => setAdr(event.target.value)}
            className="w-16 rounded-md border border-neutral-800 bg-neutral-900 px-2 py-1 text-xs outline-none focus:border-neutral-500"
          />
          <input
            type="number"
            min={0}
            max={100}
            step="any"
            placeholder="HS%"
            value={hs}
            onChange={(event) => setHs(event.target.value)}
            className="w-16 rounded-md border border-neutral-800 bg-neutral-900 px-2 py-1 text-xs outline-none focus:border-neutral-500"
          />
          <input
            type="number"
            step="0.01"
            min={0}
            placeholder="Rating"
            value={rating}
            onChange={(event) => setRating(event.target.value)}
            className="w-16 rounded-md border border-neutral-800 bg-neutral-900 px-2 py-1 text-xs outline-none focus:border-neutral-500"
          />
          <button
            type="submit"
            disabled={addStat.isPending}
            className="rounded-md bg-red-600 px-3 py-1 text-xs font-medium text-white hover:bg-red-500 disabled:opacity-50"
          >
            Dodaj
          </button>
        </form>
      )}

      {canAddStats && availablePlayers && availablePlayers.length > 0 && (
        <DemoImportPanel matchResultId={matchResultId} availablePlayers={availablePlayers} />
      )}
    </div>
  )
}
