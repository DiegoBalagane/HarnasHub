import { useRef, useState } from 'react'
import type { TeamMember } from '../../../services/rosterApi'
import type { DeathPosition } from '../../../services/statsApi'
import { useAddPlayerStat, useImportStatsFromDemo } from '../hooks/useStats'
import { DeathMapView } from './DeathMapView'

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
  // Read-only, computed straight from the demo — shown for context, not hand-edited like the fields above.
  entryKills: number
  entryDeaths: number
  kastPercentage: number
  multiKill2K: number
  multiKill3K: number
  multiKill4K: number
  multiKill5K: number
  utilityDamage: number
  flashAssists: number
  deathPositions: DeathPosition[]
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
  const [mapName, setMapName] = useState<string | null>(null)

  const availableIds = new Set(availablePlayers.map((member) => member.id))

  function handleFileSelected(event: React.ChangeEvent<HTMLInputElement>) {
    const file = event.target.files?.[0]
    if (!file) return

    importDemo.mutate(file, {
      onSuccess: (result) => {
        setRoundsPlayed(result.roundsPlayed)
        setMapName(result.mapName)
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
            entryKills: player.entryKills,
            entryDeaths: player.entryDeaths,
            kastPercentage: player.kastPercentage,
            multiKill2K: player.multiKill2K,
            multiKill3K: player.multiKill3K,
            multiKill4K: player.multiKill4K,
            multiKill5K: player.multiKill5K,
            utilityDamage: player.utilityDamage,
            flashAssists: player.flashAssists,
            deathPositions: player.deathPositions,
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
        entryKills: row.entryKills,
        entryDeaths: row.entryDeaths,
        kastPercentage: row.kastPercentage,
        multiKill2K: row.multiKill2K,
        multiKill3K: row.multiKill3K,
        multiKill4K: row.multiKill4K,
        multiKill5K: row.multiKill5K,
        utilityDamage: row.utilityDamage,
        flashAssists: row.flashAssists,
      },
      {
        onSuccess: () => {
          setRows((current) => current?.filter((candidate) => candidate.key !== row.key) ?? null)
        },
      },
    )
  }

  return (
    <div className="flex flex-col gap-3 border-t border-neutral-800 pt-3">
      <div className="flex items-center gap-2">
        <input ref={fileInputRef} type="file" accept=".dem" onChange={handleFileSelected} className="text-xs" />
        {importDemo.isPending && <span className="text-xs text-neutral-500">Wczytywanie demki…</span>}
      </div>

      {importDemo.isError && (
        <p className="text-xs text-red-400">Nie udało się odczytać demki — sprawdź, czy to poprawny plik .dem.</p>
      )}

      {rows && (
        <>
          <p className="text-xs text-neutral-500">
            Rund w demce: {roundsPlayed}
            {mapName ? ` · Mapa: ${mapName}` : ' · Mapa spoza aktualnej puli — brak podglądu na radarze'}
          </p>

          {mapName && (
            <DeathMapView
              mapName={mapName}
              players={rows.map((row) => ({
                key: row.key,
                name: row.demoPlayerName,
                deathPositions: row.deathPositions,
              }))}
            />
          )}

          {rows.length === 0 ? (
            <p className="text-xs text-neutral-500">Wszystkie wykryte staty zostały zapisane.</p>
          ) : (
            <div className="overflow-x-auto">
              <table className="w-full min-w-[1000px] text-left text-xs">
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
                    <th className="pb-1 pl-2 font-normal text-neutral-600">Entry K/D</th>
                    <th className="pb-1 font-normal text-neutral-600">KAST%</th>
                    <th className="pb-1 font-normal text-neutral-600" title="Rundy z 2/3/4/5 killami">
                      Multi
                    </th>
                    <th className="pb-1 font-normal text-neutral-600">UtilDmg</th>
                    <th className="pb-1 font-normal text-neutral-600">FlashA</th>
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
                      <td className="pl-2 text-neutral-500">
                        {row.entryKills}/{row.entryDeaths}
                      </td>
                      <td className="text-neutral-500">{row.kastPercentage.toFixed(0)}%</td>
                      <td className="text-neutral-500">
                        {row.multiKill2K}/{row.multiKill3K}/{row.multiKill4K}/{row.multiKill5K}
                      </td>
                      <td className="text-neutral-500">{row.utilityDamage}</td>
                      <td className="text-neutral-500">{row.flashAssists}</td>
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
            </div>
          )}
        </>
      )}
    </div>
  )
}
