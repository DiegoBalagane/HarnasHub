import { render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { describe, expect, it, vi } from 'vitest'
import type { MatchRound } from '../../../services/matchAnalysisApi'
import { RoundRow } from './RoundRow'

const round = {
  number: 7,
  winnerSide: 'T',
  ourSide: 'T',
  weWon: true,
  ourScoreAfter: 4,
  opponentScoreAfter: 3,
  endReason: 'BombExploded',
  durationSeconds: 100,
  ourEconomy: { buyType: 'Full', equipmentValue: 20000 },
  opponentEconomy: null,
  bomb: { site: 'A', plantSecondsIntoRound: 75, defused: false },
  kills: [
    {
      secondsIntoRound: 12,
      killerName: 'Ann',
      victimName: 'Bob',
      weapon: 'ak47',
      headshot: true,
      wallbang: false,
      throughSmoke: false,
      noScope: false,
      attackerBlind: false,
      isOpening: true,
      isTeamKill: false,
      byUs: true,
      victimZone: 'Mid',
      assisterName: null,
    },
  ],
} as unknown as MatchRound

describe('RoundRow', () => {
  it('shows the collapsed summary without the kill list', () => {
    render(<RoundRow round={round} expanded={false} onToggle={vi.fn()} onReplay={vi.fn()} />)
    expect(screen.getByText('R7')).toBeInTheDocument()
    expect(screen.getByText('4:3')).toBeInTheDocument()
    expect(screen.getByText(/Plant A 1:15/)).toBeInTheDocument()
    expect(screen.queryByText(/ak47/)).not.toBeInTheDocument()
  })

  it('calls onToggle with the round number when the header is clicked', async () => {
    const onToggle = vi.fn()
    render(<RoundRow round={round} expanded={false} onToggle={onToggle} onReplay={vi.fn()} />)
    await userEvent.click(screen.getByText('R7'))
    expect(onToggle).toHaveBeenCalledWith(7)
  })

  it('lists kills with tags and offers a replay when expanded', async () => {
    const onReplay = vi.fn()
    render(<RoundRow round={round} expanded onToggle={vi.fn()} onReplay={onReplay} />)
    expect(screen.getByText(/Ann → Bob/)).toBeInTheDocument()
    expect(screen.getByText(/otwarcie, HS/)).toBeInTheDocument()
    await userEvent.click(screen.getByRole('button', { name: /Odtwórz/ }))
    expect(onReplay).toHaveBeenCalledWith(7)
  })

  it('shows the matched tactic and an empty-kills message', () => {
    render(
      <RoundRow
        round={{ ...round, kills: [] }}
        expanded
        onToggle={vi.fn()}
        onReplay={vi.fn()}
        tacticMatch={{ tacticName: 'A split', scorePercent: 82 } as never}
      />,
    )
    expect(screen.getByText(/Taktyka: A split \(dopasowanie 82%\)/)).toBeInTheDocument()
    expect(screen.getByText('Brak zabójstw w tej rundzie.')).toBeInTheDocument()
  })
})
