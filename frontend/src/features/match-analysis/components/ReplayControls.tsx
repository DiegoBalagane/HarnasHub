import { memo } from 'react'
import { REPLAY_SETTINGS } from '../../../constants'
import { formatRoundTime } from '../labels'

interface ReplayControlsProps {
  second: number
  durationSeconds: number
  playing: boolean
  speed: number
  onTogglePlay: () => void
  onSeek: (second: number) => void
  onSpeedChange: (speed: number) => void
}

/** Play/pause, per-second scrubber and speed buttons of the round replay. */
export const ReplayControls = memo(function ReplayControls({
  second,
  durationSeconds,
  playing,
  speed,
  onTogglePlay,
  onSeek,
  onSpeedChange,
}: ReplayControlsProps) {
  return (
    <div className="flex flex-wrap items-center gap-3 text-sm">
      <button
        type="button"
        onClick={onTogglePlay}
        title="Spacja"
        className="w-24 rounded-md bg-primary-500 px-3 py-1.5 font-medium text-primary-950 transition hover:bg-primary-400"
      >
        {playing ? '❚❚ Pauza' : '▶ Odtwórz'}
      </button>
      <input
        type="range"
        min={0}
        max={durationSeconds}
        step={1}
        value={second}
        onChange={(event) => onSeek(Number(event.target.value))}
        aria-label="Sekunda rundy"
        className="min-w-40 flex-1 accent-primary-500"
      />
      <span className="w-24 text-center font-mono text-neutral-300">
        {formatRoundTime(second)} / {formatRoundTime(durationSeconds)}
      </span>
      <div className="flex overflow-hidden rounded-md border border-neutral-800">
        {REPLAY_SETTINGS.speeds.map((option) => (
          <button
            key={option}
            type="button"
            onClick={() => onSpeedChange(option)}
            className={`px-2 py-1 ${speed === option ? 'bg-neutral-100 text-neutral-900' : 'text-neutral-400 hover:text-white'}`}
          >
            {option}x
          </button>
        ))}
      </div>
    </div>
  )
})
