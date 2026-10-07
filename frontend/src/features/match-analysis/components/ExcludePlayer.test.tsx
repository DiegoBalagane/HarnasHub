import { screen, waitFor } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { describe, expect, it, vi } from 'vitest'
import { mockFetch, renderWithProviders } from '../../../test/renderWithProviders'
import { ExcludedPlayersBar } from './ExcludedPlayersBar'
import { ExcludePlayerButton } from './ExcludePlayerButton'

let canManage = true
vi.mock('../../auth/hooks/useIsCoachOrManager', () => ({ useIsCoachOrManager: () => canManage }))

const timeline = {
  mapName: 'Mirage',
  hasZones: true,
  ourTeamResolved: true,
  parserVersion: 1,
  rounds: [],
  excludedPlayers: [{ steamId64: '76561198210690651', name: 'Trener' }],
}

describe('ExcludePlayerButton', () => {
  it('posts the exclusion for the player', async () => {
    canManage = true
    const fetchMock = mockFetch(null, 204)

    renderWithProviders(<ExcludePlayerButton matchResultId="m1" steamId64="76561198210690651" name="Trener" />)
    await userEvent.click(screen.getByRole('button', { name: /wyklucz trener/i }))

    await waitFor(() => expect(fetchMock).toHaveBeenCalled())
    const [url, init] = fetchMock.mock.calls[0]
    expect(String(url)).toContain('/api/results/m1/analysis/exclude')
    expect(init?.body).toContain('76561198210690651')
  })

  it('renders nothing for non Coach/Manager users', () => {
    canManage = false
    mockFetch(null, 204)

    renderWithProviders(<ExcludePlayerButton matchResultId="m1" steamId64="1" name="X" />)

    expect(screen.queryByRole('button')).toBeNull()
  })
})

describe('ExcludedPlayersBar', () => {
  it('lists excluded players and restores one', async () => {
    canManage = true
    const fetchMock = mockFetch(timeline)

    renderWithProviders(<ExcludedPlayersBar matchResultId="m1" />)
    await userEvent.click(await screen.findByRole('button', { name: /pokaż wykluczonych \(1\)/i }))
    expect(screen.getByText('Trener')).toBeInTheDocument()

    await userEvent.click(screen.getByRole('button', { name: 'Przywróć' }))

    await waitFor(() =>
      expect(fetchMock.mock.calls.some(([url]) => String(url).includes('/analysis/include'))).toBe(true),
    )
  })
})
