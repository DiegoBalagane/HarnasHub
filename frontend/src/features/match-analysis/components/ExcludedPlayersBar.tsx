import { memo, useState } from 'react'
import { useIsCoachOrManager } from '../../auth/hooks/useIsCoachOrManager'
import { useAnalysisPlayerExclusion, useMatchTimeline } from '../hooks/useMatchAnalysis'

interface ExcludedPlayersBarProps {
  matchResultId: string
}

/** "Pokaż wykluczonych (n)" toggle listing the players hidden from this match's analysis, each with a restore button (Coach/Manager). */
export const ExcludedPlayersBar = memo(function ExcludedPlayersBar({ matchResultId }: ExcludedPlayersBarProps) {
  const canManage = useIsCoachOrManager()
  const { data: timeline } = useMatchTimeline(matchResultId)
  const { include } = useAnalysisPlayerExclusion(matchResultId)
  const [open, setOpen] = useState(false)
  const excluded = timeline?.excludedPlayers ?? []

  if (!canManage || excluded.length === 0) {
    return null
  }

  return (
    <div className="flex flex-col gap-2 text-sm">
      <button
        type="button"
        onClick={() => setOpen((current) => !current)}
        className="self-start text-xs text-neutral-400 hover:text-white"
      >
        {open ? 'Ukryj wykluczonych' : `Pokaż wykluczonych (${excluded.length})`}
      </button>
      {open && (
        <ul className="flex flex-col gap-1">
          {excluded.map((player) => (
            <li key={player.steamId64} className="flex items-center gap-3 rounded-md border border-neutral-800 px-3 py-1.5">
              <span className="text-neutral-300">{player.name}</span>
              <button
                type="button"
                onClick={() => include.mutate(player.steamId64)}
                disabled={include.isPending}
                className="text-xs text-primary-400 hover:underline disabled:opacity-50"
              >
                Przywróć
              </button>
            </li>
          ))}
        </ul>
      )}
    </div>
  )
})
