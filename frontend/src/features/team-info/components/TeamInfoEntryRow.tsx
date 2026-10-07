import { memo, useState } from 'react'
import { Button } from '../../../components/ui/Button'
import type { TeamInfoEntry } from '../../../services/teamInfoApi'
import { classifyValue } from '../entryValue'

const FEEDBACK_MS = 1500
const SECRET_MASK = '••••••'

interface TeamInfoEntryRowProps {
  entry: TeamInfoEntry
  /** Coach/Manager: shows edit, delete and reorder controls. */
  canManage: boolean
  isFirst: boolean
  isLast: boolean
  onMoveUp: (entry: TeamInfoEntry) => void
  onMoveDown: (entry: TeamInfoEntry) => void
  onEdit: (entry: TeamInfoEntry) => void
  onDelete: (entry: TeamInfoEntry) => void
}

/** One info entry: title, value (link / connect button / monospace block / masked secret) with copy and management controls. */
export const TeamInfoEntryRow = memo(function TeamInfoEntryRow({
  entry,
  canManage,
  isFirst,
  isLast,
  onMoveUp,
  onMoveDown,
  onEdit,
  onDelete,
}: TeamInfoEntryRowProps) {
  const [isRevealed, setIsRevealed] = useState(false)
  const [copyState, setCopyState] = useState<'idle' | 'copied' | 'failed'>('idle')
  const isMasked = entry.isSecret && !isRevealed
  const kind = classifyValue(entry.value)

  async function handleCopy() {
    try {
      await navigator.clipboard.writeText(entry.value)
      setCopyState('copied')
    } catch {
      setCopyState('failed')
    }
    setTimeout(() => setCopyState('idle'), FEEDBACK_MS)
  }

  return (
    <div className="flex flex-col gap-2 rounded-md border border-neutral-800 bg-neutral-950/40 p-3">
      <div className="flex flex-wrap items-center justify-between gap-2">
        <h3 className="text-sm font-medium">{entry.title}</h3>
        {canManage && (
          <div className="flex items-center gap-1">
            <Button variant="ghost" size="sm" disabled={isFirst} onClick={() => onMoveUp(entry)} aria-label={`Przenieś wyżej: ${entry.title}`}>
              ↑
            </Button>
            <Button variant="ghost" size="sm" disabled={isLast} onClick={() => onMoveDown(entry)} aria-label={`Przenieś niżej: ${entry.title}`}>
              ↓
            </Button>
            <Button variant="ghost" size="sm" onClick={() => onEdit(entry)} aria-label={`Edytuj: ${entry.title}`}>
              Edytuj
            </Button>
            <Button variant="ghost" size="sm" onClick={() => onDelete(entry)} aria-label={`Usuń: ${entry.title}`}>
              Usuń
            </Button>
          </div>
        )}
      </div>

      {isMasked ? (
        <p className="font-mono text-sm tracking-widest text-neutral-400" aria-label="Wartość ukryta">
          {SECRET_MASK}
        </p>
      ) : kind.type === 'code' ? (
        <pre className="max-h-64 overflow-auto whitespace-pre-wrap rounded-md bg-neutral-900 p-3 font-mono text-xs text-neutral-200">
          {kind.text}
        </pre>
      ) : kind.type === 'link' ? (
        <a href={kind.href} target="_blank" rel="noopener noreferrer" className="break-all text-sm text-primary-400 hover:underline">
          {entry.value}
        </a>
      ) : (
        <p className="break-all font-mono text-sm text-neutral-200">{entry.value}</p>
      )}

      <div className="flex flex-wrap items-center gap-2">
        {kind.type === 'connect' && !isMasked && (
          <a
            href={kind.href}
            className="rounded-md bg-primary-500 px-3 py-1.5 text-xs font-medium text-primary-950 transition hover:bg-primary-400"
          >
            Połącz
          </a>
        )}
        <Button variant="secondary" size="sm" onClick={handleCopy}>
          {copyState === 'copied' ? 'Skopiowano' : copyState === 'failed' ? 'Nie udało się' : 'Kopiuj'}
        </Button>
        {entry.isSecret && (
          <Button variant="ghost" size="sm" onClick={() => setIsRevealed((current) => !current)}>
            {isRevealed ? 'Ukryj' : 'Pokaż'}
          </Button>
        )}
      </div>
    </div>
  )
})
