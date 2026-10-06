import { render, screen } from '@testing-library/react'
import { describe, expect, it } from 'vitest'
import type { FormGame } from '../../../services/opponentReportApi'
import { FaceitMatchLinks } from './FaceitMatchLinks'

const game = (faceitMatchId: string, won = true): FormGame => ({
  faceitMatchId,
  playedAtUtc: '2026-10-05T18:20:53Z',
  mapName: 'Nuke',
  roundsFor: won ? 13 : 7,
  roundsAgainst: won ? 3 : 13,
  won,
  competitionName: 'Europe 5v5 Queue',
})

describe('FaceitMatchLinks', () => {
  it('links unanalysed matches to their FACEIT room and marks analysed ones', () => {
    render(<FaceitMatchLinks games={[game('1-aaa'), game('1-bbb', false)]} analysedMatchIds={new Set(['1-aaa'])} />)

    expect(screen.getByText('Przeanalizowana')).toBeInTheDocument()
    const link = screen.getByRole('link', { name: /Pokój meczu/ })
    expect(link).toHaveAttribute('href', 'https://www.faceit.com/en/cs2/room/1-bbb')
    expect(link).toHaveAttribute('target', '_blank')
    expect(screen.getByText(/Do przeanalizowania: 1 z 2/)).toBeInTheDocument()
  })

  it('renders nothing without games', () => {
    const { container } = render(<FaceitMatchLinks games={[]} analysedMatchIds={new Set()} />)

    expect(container).toBeEmptyDOMElement()
  })
})
