import { screen, within } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { describe, expect, it } from 'vitest'
import type { UnresolvedRosterPlayer } from '../../../services/opponentReportApi'
import { renderWithProviders } from '../../../test/renderWithProviders'
import { IndividualFormSection } from './IndividualFormSection'

const unresolved: UnresolvedRosterPlayer[] = [
  { userId: '1', displayName: 'Zenek', reason: 'brak SteamID i nicku FACEIT' },
  { userId: '2', displayName: 'Antek', reason: 'SteamID bez konta FACEIT — ustaw nick FACEIT w panelu admina' },
]

describe('IndividualFormSection unresolved players', () => {
  it('lists unresolved roster players with reasons on the "My" tab even without any form data', async () => {
    renderWithProviders(<IndividualFormSection form={null} unresolved={unresolved} />)

    await userEvent.click(screen.getByRole('tab', { name: 'My' }))

    const list = screen.getByRole('list', { name: 'Gracze bez konta FACEIT' })
    expect(within(list).getByText(/brak SteamID i nicku FACEIT/)).toBeInTheDocument()
    expect(within(list).getByText(/SteamID bez konta FACEIT/)).toBeInTheDocument()
  })

  it('links Managers to the admin panel', async () => {
    renderWithProviders(<IndividualFormSection form={null} unresolved={unresolved} canSetNickname />)

    await userEvent.click(screen.getByRole('tab', { name: 'My' }))

    expect(screen.getByRole('link', { name: 'Ustaw nick FACEIT w panelu admina' })).toHaveAttribute('href', '/admin')
  })

  it('hides the admin link for non-managers and the list on the opponent tab', async () => {
    renderWithProviders(<IndividualFormSection form={null} unresolved={unresolved} />)

    expect(screen.queryByRole('list', { name: 'Gracze bez konta FACEIT' })).not.toBeInTheDocument()
    await userEvent.click(screen.getByRole('tab', { name: 'My' }))
    expect(screen.queryByRole('link')).not.toBeInTheDocument()
  })

  it('renders nothing when there is no form and nobody is unresolved', () => {
    const { container } = renderWithProviders(<IndividualFormSection form={null} unresolved={[]} />)

    expect(container).toBeEmptyDOMElement()
  })
})
