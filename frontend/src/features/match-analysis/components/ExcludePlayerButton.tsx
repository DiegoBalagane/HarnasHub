import { memo } from 'react'
import { useIsCoachOrManager } from '../../auth/hooks/useIsCoachOrManager'
import { useAnalysisPlayerExclusion } from '../hooks/useMatchAnalysis'

interface ExcludePlayerButtonProps {
  matchResultId: string
  steamId64: string
  name: string
}

/** Small "×" that hides a player from this match's analysis (Coach/Manager only; renders nothing for others). */
export const ExcludePlayerButton = memo(function ExcludePlayerButton({ matchResultId, steamId64, name }: ExcludePlayerButtonProps) {
  const canManage = useIsCoachOrManager()
  const { exclude } = useAnalysisPlayerExclusion(matchResultId)

  if (!canManage) {
    return null
  }

  return (
    <button
      type="button"
      aria-label={`Wyklucz ${name} z analizy`}
      title="Wyklucz z analizy tego meczu (np. trener) — można przywrócić"
      onClick={() => exclude.mutate(steamId64)}
      disabled={exclude.isPending}
      className="ml-2 text-neutral-600 hover:text-danger-400 disabled:opacity-50"
    >
      ×
    </button>
  )
})
