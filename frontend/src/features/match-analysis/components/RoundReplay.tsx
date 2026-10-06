import { memo, useEffect } from 'react'
import { Link } from 'react-router-dom'
import { REPLAY_SETTINGS } from '../../../constants'
import type { ReplaySource, RoundReplay as RoundReplayData } from '../../../services/replayApi'
import { useIsCoachOrManager } from '../../auth/hooks/useIsCoachOrManager'
import { useCreateBoardFromRound, useRoundReplay } from '../hooks/useRoundReplay'
import { useReplayPlayback } from '../hooks/useReplayPlayback'
import { endReasonLabels } from '../labels'
import { ReplayCanvas } from './ReplayCanvas'
import { ReplayControls } from './ReplayControls'
import { ReplayKillFeed } from './ReplayKillFeed'

interface RoundReplayProps {
  source: ReplaySource
  /** Match result id or opponent demo id, depending on `source`. */
  sourceId: string
  roundNumber: number
  /** When given, previous/next round buttons are shown. */
  onRoundChange?: (roundNumber: number) => void
}

const statusMessages = {
  UncalibratedMap: 'Mapa niekalibrowana — pozycje niedostępne. Poniżej tylko przebieg zabójstw.',
  NotRecorded:
    'Ta demka została przeanalizowana bez pozycji graczy — dołącz ją ponownie, aby odtworzyć rundę na radarze.',
} as const

function isTypingTarget(target: EventTarget | null): boolean {
  return (
    target instanceof HTMLElement &&
    ['INPUT', 'SELECT', 'TEXTAREA'].includes(target.tagName) &&
    target.getAttribute('type') !== 'range'
  )
}

/** Loads one round and renders its player; round navigation lives outside the player so playback state resets per round. */
export function RoundReplay({ source, sourceId, roundNumber, onRoundChange }: RoundReplayProps) {
  const { data, isLoading, error } = useRoundReplay(source, sourceId, roundNumber)

  return (
    <div className="flex flex-col gap-3">
      {onRoundChange && (
        <div className="flex items-center gap-2 text-sm">
          <button
            type="button"
            disabled={roundNumber <= 1}
            onClick={() => onRoundChange(roundNumber - 1)}
            className="rounded border border-neutral-800 px-2 py-1 text-neutral-300 hover:border-neutral-500 disabled:opacity-40"
          >
            ← Poprzednia
          </button>
          <span className="font-medium">Runda {roundNumber}</span>
          <button
            type="button"
            disabled={data !== undefined && roundNumber >= data.roundsCount}
            onClick={() => onRoundChange(roundNumber + 1)}
            className="rounded border border-neutral-800 px-2 py-1 text-neutral-300 hover:border-neutral-500 disabled:opacity-40"
          >
            Następna →
          </button>
        </div>
      )}
      {isLoading && <p className="text-sm text-neutral-400">Ładowanie rundy…</p>}
      {error && <p className="text-sm text-danger-400">{error.message}</p>}
      {data && (
        <ReplayPlayer
          key={`${source}-${sourceId}-${roundNumber}`}
          replay={data}
          source={source}
          sourceId={sourceId}
        />
      )}
    </div>
  )
}

interface ReplayPlayerProps {
  replay: RoundReplayData
  source: ReplaySource
  sourceId: string
}

/** The 2D player itself: radar canvas, controls with space/←/→ shortcuts, kill feed and "board from this round". */
const ReplayPlayer = memo(function ReplayPlayer({ replay, source, sourceId }: ReplayPlayerProps) {
  const playback = useReplayPlayback(replay.durationSeconds)
  const { second, togglePlay, seek, timeRef } = playback
  const canManage = useIsCoachOrManager()
  const createBoard = useCreateBoardFromRound(source, sourceId, replay.roundNumber)
  const hasPositions = replay.positionsStatus === 'Available' && replay.mapName !== null

  useEffect(() => {
    function handleKeyDown(event: KeyboardEvent) {
      if (isTypingTarget(event.target)) return
      const step = event.shiftKey ? REPLAY_SETTINGS.fastSeekStepSeconds : REPLAY_SETTINGS.seekStepSeconds
      if (event.code === 'Space') {
        event.preventDefault()
        togglePlay()
      } else if (event.key === 'ArrowLeft') {
        event.preventDefault()
        seek(Math.floor(timeRef.current) - step)
      } else if (event.key === 'ArrowRight') {
        event.preventDefault()
        seek(Math.floor(timeRef.current) + step)
      }
    }

    document.addEventListener('keydown', handleKeyDown)
    return () => document.removeEventListener('keydown', handleKeyDown)
  }, [togglePlay, seek, timeRef])

  return (
    <div className="flex flex-col gap-3">
      <p className="text-xs text-neutral-500">
        {replay.mapName ?? 'nieznana mapa'} · wygrana {replay.winnerSide ?? '?'} (
        {endReasonLabels[replay.endReason]}){replay.ourSide ? ` · my: ${replay.ourSide}` : ''} · skróty:
        spacja, ←/→ (Shift = 5 s)
      </p>
      {replay.positionsStatus !== 'Available' && (
        <p className="rounded-md border border-warning-900/60 p-2 text-sm text-warning-300">
          {statusMessages[replay.positionsStatus]}
        </p>
      )}
      <ReplayControls
        second={second}
        durationSeconds={replay.durationSeconds}
        playing={playback.playing}
        speed={playback.speed}
        onTogglePlay={togglePlay}
        onSeek={seek}
        onSpeedChange={playback.setSpeed}
      />
      <div className="grid gap-3 md:grid-cols-[minmax(0,2fr)_minmax(0,1fr)]">
        {hasPositions ? (
          <ReplayCanvas replay={replay} mapName={replay.mapName as string} subscribe={playback.subscribe} />
        ) : (
          <div className="hidden md:block" />
        )}
        <ReplayKillFeed replay={replay} second={second} onSeek={seek} />
      </div>
      {canManage && hasPositions && (
        <div className="flex flex-wrap items-center gap-3 text-sm">
          <button
            type="button"
            disabled={createBoard.isPending}
            onClick={() => createBoard.mutate(Math.floor(timeRef.current))}
            className="rounded-md border border-neutral-700 px-3 py-1.5 text-neutral-200 transition hover:border-neutral-400 disabled:opacity-50"
          >
            Utwórz tablicę analizy z tej rundy
          </button>
          {createBoard.isSuccess && (
            <Link
              to={`/playbook?map=${createBoard.data.mapName}&tab=boards`}
              className="text-success-400 hover:underline"
            >
              Zapisano „{createBoard.data.title}” — otwórz tablice
            </Link>
          )}
          {createBoard.isError && <span className="text-danger-400">{createBoard.error.message}</span>}
        </div>
      )}
    </div>
  )
})
