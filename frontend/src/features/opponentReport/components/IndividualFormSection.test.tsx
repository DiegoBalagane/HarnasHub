import { render, screen, within } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { describe, expect, it } from 'vitest'
import type { IndividualForm } from '../../../services/opponentReportApi'
import { makeMap, makePlayer } from '../individualForm.fixtures'
import { IndividualFormSection } from './IndividualFormSection'

const form: IndividualForm = {
  theirs: {
    players: [
      makePlayer({
        nickname: 'f0xelon',
        maps: [makeMap('Mirage', 12, { share: 30 }), makeMap('Inferno', 4, { share: 10 })],
        recentForm: {
          recentGames: 10,
          earlierGames: 15,
          recentKdRatio: 1.45,
          earlierKdRatio: 1.05,
          recentWinRate: 60,
          earlierWinRate: 48,
          kdDelta: 0.4,
          winRateDelta: 12,
          direction: 'Up',
        },
      }),
    ],
    mapComfort: [],
  },
  ours: { players: [makePlayer({ playerId: 'u1', nickname: 'Kacper', elo: null, skillLevel: null })], mapComfort: [] },
}

describe('IndividualFormSection', () => {
  it('shows the opponent cards with ELO, form arrow and map chips by default', () => {
    render(<IndividualFormSection form={form} />)

    const card = screen.getByRole('article', { name: 'f0xelon' })
    expect(within(card).getByText('2450 ELO · lvl 10')).toBeInTheDocument()
    expect(within(card).getByLabelText('forma w górę')).toHaveClass('text-success-400')
    const chips = within(card).getByRole('list', { name: 'Najczęstsze mapy' })
    expect(within(chips).getByText('Mirage 30%')).toBeInTheDocument()
    expect(within(card).getAllByRole('row')).toHaveLength(3)
  })

  it('switches to our players', async () => {
    render(<IndividualFormSection form={form} />)

    await userEvent.click(screen.getByRole('tab', { name: 'My' }))

    expect(screen.getByRole('article', { name: 'Kacper' })).toBeInTheDocument()
    expect(screen.getByText('— ELO')).toBeInTheDocument()
    expect(screen.queryByRole('article', { name: 'f0xelon' })).not.toBeInTheDocument()
  })

  it('renders nothing for an old report without individual form', () => {
    const { container } = render(<IndividualFormSection form={undefined} />)
    expect(container).toBeEmptyDOMElement()
  })
})
