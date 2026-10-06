import { REPLAY_SETTINGS } from '../../../constants'
import type { DemoGrenadeType } from '../../../services/opponentDemosApi'
import type { RoundReplay } from '../../../services/replayApi'
import { grenadeStateAt, positionAt, type PlayerSeries } from './replayMath'

/** Side colours of player dots (same as the analysis-board snapshot). */
export const sideColors = { T: '#f59e0b', CT: '#60a5fa' } as const

const effectStyle: Record<DemoGrenadeType, { color: string; radius: number }> = {
  Smoke: { color: '212, 212, 212', radius: 0.028 },
  Molotov: { color: '249, 115, 22', radius: 0.022 },
  Incendiary: { color: '249, 115, 22', radius: 0.022 },
  Flash: { color: '250, 204, 21', radius: 0.018 },
  HighExplosive: { color: '239, 68, 68', radius: 0.02 },
  Decoy: { color: '163, 163, 163', radius: 0.01 },
}

/** Everything the canvas needs that doesn't change while playing. */
export interface ReplayScene {
  replay: RoundReplay
  series: PlayerSeries[]
  deaths: Map<number, number>
}

/** Draws one moment `t` (fractional seconds) of the replay onto a canvas of CSS size width x height (already DPR-scaled). */
export function drawReplay(
  ctx: CanvasRenderingContext2D,
  width: number,
  height: number,
  scene: ScenePlusTime,
): void {
  const { replay, series, deaths, t } = scene
  const size = Math.min(width, height)
  ctx.clearRect(0, 0, width, height)

  for (const grenade of replay.grenades) {
    const state = grenadeStateAt(grenade, t)
    if (state.phase === 'hidden') continue
    const style = effectStyle[grenade.type]
    if (state.phase === 'flying') {
      ctx.fillStyle = `rgb(${style.color})`
      ctx.beginPath()
      ctx.arc(state.x * width, state.y * height, 2.5, 0, Math.PI * 2)
      ctx.fill()
      continue
    }
    ctx.fillStyle = `rgba(${style.color}, ${0.25 + 0.35 * state.remaining})`
    ctx.beginPath()
    ctx.arc(state.x * width, state.y * height, style.radius * size, 0, Math.PI * 2)
    ctx.fill()
  }

  if (replay.bomb && replay.bomb.x !== null && replay.bomb.y !== null && t >= replay.bomb.plantSecond) {
    const defused = replay.bomb.defuseSecond !== null && t >= replay.bomb.defuseSecond
    ctx.fillStyle = defused ? '#22c55e' : '#dc2626'
    ctx.fillRect(replay.bomb.x * width - 5, replay.bomb.y * height - 5, 10, 10)
  }

  ctx.lineWidth = 2
  ctx.strokeStyle = '#a3a3a3'
  for (const kill of replay.kills) {
    if (kill.second > t || kill.x === null || kill.y === null) continue
    const x = kill.x * width
    const y = kill.y * height
    ctx.beginPath()
    ctx.moveTo(x - 4, y - 4)
    ctx.lineTo(x + 4, y + 4)
    ctx.moveTo(x - 4, y + 4)
    ctx.lineTo(x + 4, y - 4)
    ctx.stroke()
  }

  ctx.font = `${REPLAY_SETTINGS.labelFontPx}px sans-serif`
  ctx.textAlign = 'center'
  replay.players.forEach((player, index) => {
    const death = deaths.get(index)
    if (death !== undefined && death <= t) return
    const point = positionAt(series[index], t)
    if (!point) return
    const x = point.x * width
    const y = point.y * height
    ctx.fillStyle = sideColors[player.side]
    ctx.beginPath()
    ctx.arc(x, y, REPLAY_SETTINGS.dotRadiusPx, 0, Math.PI * 2)
    ctx.fill()
    ctx.lineWidth = player.team === 'Ours' ? 2 : 1
    ctx.strokeStyle = player.team === 'Ours' ? '#ffffff' : 'rgba(0, 0, 0, 0.6)'
    ctx.stroke()
    ctx.fillStyle = '#f5f5f5'
    ctx.fillText(player.name, x, y - REPLAY_SETTINGS.dotRadiusPx - 3)
  })
}

/** A scene at one moment in time. */
export type ScenePlusTime = ReplayScene & { t: number }
