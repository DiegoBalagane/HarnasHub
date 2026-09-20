import { useRef, useState } from 'react'
import type { TeamMember } from '../../../services/rosterApi'
import { useAddPlayerStat, useImportStatsFromDemo } from '../hooks/useStats'

interface DemoImportPanelProps {
  matchResultId: string
  /** Roster members not already tracked for this match — the only valid targets for a saved row. */
  availablePlayers: TeamMember[]
}

interface DraftRow {
  key: string
  demoPlayerName: string
  userId: string
  kills: number
  deaths: number
  assists: number
  adr: number
  headshotPercentage: number
  rating: number
}

const numberInputClass =
  'w-16 rounded-md border border-neutral-800 bg-neutral-900 px-1.5 py-1 text-xs outline-none focus:border-neutral-500'

/** Coach/Manager tool: upload a CS2 demo, review its computed per-player stats, and save each row individually — nothing is written until "Zapisz" is clicked on that row. */
export function DemoImportPanel({ matchResultId, availablePlayers }: DemoImportPanelProps) {
  const fileInputRef = useRef<HTMLInputElement>(null)
  const importDemo = useImportStatsFromDemo()
  const addStat = useAddPlayerStat(matchResultId)
  const [rows, setRows] = useState<DraftRow[] | null>(null)
  const [roundsPlayed, setRoundsPlayed] = useState<number | null>(null)

  const availableIds = new Set(availablePlayers.map((member) => member.id))

  function handleFileSelected(event: React.ChangeEvent<HTMLInputElement>) {
    const file = event.target.files?.[0]
    if (!file) return

    importDemo.mutate(file, {
      onSuccess: (result) => {
        setRoundsPlayed(result.roundsPlayed)
        setRows(
          result.players.map((player) => ({
            key: player.steamId64,
            demoPlayerName: player.demoPlayerName,
            // A matched player already tracked for this match has nothing left to import.
            userId: player.matchedUserId && availableIds.has(player.matchedUserId) ? player.matchedUserId : '',
            kills: player.kills,
            deaths: player.deaths,
            assists: player.assists,
            adr: player.adr,
            headshotPercentage: player.headshotPercentage,
            rating: player.rating,
          })),
        )
      },
      onSettled: () => {
        if (fileInputRef.current) fileInputRef.current.value = ''
      },
    })
  }

  function updateRow(key: string, changes: Partial<DraftRow>) {
    setRows((current) => current?.map((row) => (row.key === key ? { ...row, ...changes } : row)) ?? null)
  }

  function saveRow(row: DraftRow) {
    if (!row.userId) return

    addStat.mutate(
      {
        userId: row.userId,
        kills: row.kills,
        deaths: row.deaths,
        assists: row.assists,
        adr: row.adr,
        headshotPercentage: row.headshotPercentage,
        rating: row.rating,
      },
      {
        onSuccess: () => {
          setRows((current) => current?.filter((candidate) => candidate.key !== row.key) ?? null)
        },
      },
    )
  }

  return (
    <div className="flex flex-col gap-2 border-t border-neutral-800 pt-3">
      <div className="flex items-center gap-2">
        <input ref={fileInputRef} type="file" accept=".dem" onChange={handleFileSelected} className="text-xs" />
        {importDemo.isPending && <span className="text-xs text-neutral-500">Wczytywanie demki…</span>}
      </div>

      {importDemo.isError && (
        <p className="text-xs text-red-400">Nie udało się odczytać demki — sprawdź, czy to poprawny plik .dem.</p>
      )}

      {rows && (
        <>
          {roundsPlayed !== null && <p className="text-xs text-neutral-500">Rund w demce: {roundsPlayed}</p>}

          {rows.length === 0 ? (
            <p className="text-xs text-neutral-500">Wszystkie wykryte staty zostały zapisane.</p>
          ) : (
            <table className="w-full text-left text-xs">
              <thead className="text-neutral-500">
                <tr>
                  <th className="pb-1 pr-2 font-normal">Gracz w demce</th>
                  <th className="pb-1 pr-2 font-normal">Zawodnik</th>
                  <th className="pb-1 font-normal">K</th>
                  <th className="pb-1 font-normal">D</th>
                  <th className="pb-1 font-normal">A</th>
                  <th className="pb-1 font-normal">ADR</th>
                  <th className="pb-1 font-normal">HS%</th>
                  <th className="pb-1 font-normal">Rating</th>
                  <th className="pb-1" />
                </tr>
              </thead>
              <tbody>
                {rows.map((row) => (
                  <tr key={row.key} className="text-neutral-300">
                    <td className="py-1 pr-2">{row.demoPlayerName}</td>
                    <td className="pr-2">
                      <select
                        value={row.userId}
                        onChange={(event) => updateRow(row.key, { userId: event.target.value })}
                        className="rounded-md border border-neutral-800 bg-neutral-900 px-1.5 py-1 text-xs outline-none focus:border-neutral-500"
                      >
                        <option value="">Niedopasowany</option>
                        {availablePlayers.map((member) => (
                          <option key={member.id} value={member.id}>
                            {member.inGameNickname ?? member.displayName}
                          </option>
                        ))}
                      </select>
                    </td>
                    <td>
                      <input
                        type="number"
                        min={0}
                        value={row.kills}
                        onChange={(event) => updateRow(row.key, { kills: Number(event.target.value) })}
                        className={numberInputClass}
                      />
                    </td>
                    <td>
                      <input
                        type="number"
                        min={0}
                        value={row.deaths}
                        onChange={(event) => updateRow(row.key, { deaths: Number(event.target.value) })}
                        className={numberInputClass}
                      />
                    </td>
                    <td>
                      <input
                        type="number"
                        min={0}
                        value={row.assists}
                        onChange={(event) => updateRow(row.key, { assists: Number(event.target.value) })}
                        className={numberInputClass}
                      />
                    </td>
                    <td>
                      <input
                        type="number"
                        min={0}
                        step="any"
                        value={row.adr}
                        onChange={(event) => updateRow(row.key, { adr: Number(event.target.value) })}
                        className={numberInputClass}
                      />
                    </td>
                    <td>
                      <input
                        type="number"
                        min={0}
                        max={100}
                        step="any"
                        value={row.headshotPercentage}
                        onChange={(event) => updateRow(row.key, { headshotPercentage: Number(event.target.value) })}
                        className={numberInputClass}
                      />
                    </td>
                    <td>
                      <input
                        type="number"
                        min={0}
                        step="0.01"
                        value={row.rating}
                        onChange={(event) => updateRow(row.key, { rating: Number(event.target.value) })}
                        className={numberInputClass}
                      />
                    </td>
                    <td>
                      <button
                        type="button"
                        disabled={!row.userId || addStat.isPending}
                        onClick={() => saveRow(row)}
                        className="rounded-md bg-red-600 px-2 py-1 text-xs font-medium text-white hover:bg-red-500 disabled:opacity-50"
                      >
                        Zapisz
                      </button>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          )}
        </>
      )}
    </div>
  )
}
