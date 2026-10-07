import { render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { describe, expect, it } from 'vitest'
import type { ActiveLineup, LineupPlayer } from '../../../services/opponentReportApi'
import { ActiveLineupSection } from './ActiveLineupSection'

const player = (nickname: string, recentTeamGames: number): LineupPlayer => ({
  playerId: nickname,
  nickname,
  elo: null,
  skillLevel: null,
  recentTeamGames,
  teamGames: recentTeamGames + 2,
  lastTeamGameAtUtc: null,
})

const lineup: ActiveLineup = {
  basis:
    'Skład z ostatnich 10 meczów drużynowych (ostatnie 10 lub z 60 dni) — gracze obecni w co najmniej 2 z nich',
  windowGames: 10,
  active: [player('alpha', 10), player('bravo', 9)],
  inactive: [player('oldie', 0), player('sub', 1)],
}

describe('ActiveLineupSection', () => {
  it('names the active lineup and why it was chosen', () => {
    render(<ActiveLineupSection lineup={lineup} />)

    expect(screen.getByText(/alpha \(10\/10\), bravo \(9\/10\)/)).toBeInTheDocument()
    expect(screen.getByText(/ostatnich 10 meczów drużynowych/)).toBeInTheDocument()
  })

  it('keeps ex-members and subs collapsed until expanded', async () => {
    render(<ActiveLineupSection lineup={lineup} />)

    expect(screen.getByText('oldie')).not.toBeVisible()
    await userEvent.click(screen.getByText(/Byli \/ rezerwowi \(2\)/))
    expect(screen.getByText('oldie')).toBeVisible()
    expect(screen.getByText('sub')).toBeVisible()
  })

  it('hides the collapsed list when everyone linked is active', () => {
    render(<ActiveLineupSection lineup={{ ...lineup, inactive: [] }} />)

    expect(screen.queryByText(/Byli \/ rezerwowi/)).not.toBeInTheDocument()
  })

  it('shows the current ESEA season lineup with appearances and collapses the other team members', async () => {
    const season: ActiveLineup = {
      basis:
        'Skład z sezonu ESEA S59 — gracze, którzy zagrali dla drużyny w meczach ligowych tego sezonu (mecze: 4)',
      windowGames: 4,
      active: [player('alpha', 4), player('bravo', 3)],
      inactive: [player('random', 0)],
      source: 'EseaSeason',
      season: 'S59',
      seasonCompetition: 'S59 EU Open10 D - Regular Season',
      officialMatches: 12,
    }

    render(<ActiveLineupSection lineup={season} />)

    expect(screen.getByText('Skład z sezonu ESEA S59 (4 mecze ligowe):')).toBeInTheDocument()
    expect(screen.getByText(/alpha \(4\/4\), bravo \(3\/4\)/)).toBeInTheDocument()
    expect(screen.getByText(/S59 EU Open10 D - Regular Season/)).toBeInTheDocument()
    expect(screen.getByText('random')).not.toBeVisible()
    await userEvent.click(
      screen.getByText(/Pozostali członkowie drużyny FACEIT \(nie grali w tym sezonie\) \(1\)/),
    )
    expect(screen.getByText('random')).toBeVisible()
  })
})
