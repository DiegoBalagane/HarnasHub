import { screen, waitFor, within } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { describe, expect, it } from 'vitest'
import type { AdvancedStats } from '../../../services/advancedStatsApi'
import { mockFetch, renderWithProviders } from '../../../test/renderWithProviders'
import { makePlayer } from '../advancedStats.fixtures'
import { AdvancedStatsTab } from './AdvancedStatsTab'

const data: AdvancedStats = {
  matchesAnalyzed: 2,
  matchesSkipped: 0,
  players: [
    makePlayer({ userId: 'u1', name: 'Alice', avgRating: 1.3 }),
    makePlayer({ userId: 'u2', name: 'Bob', avgRating: 0.9 }),
  ],
  mapCells: [{ userId: 'u1', map: 'Mirage', matches: 2, avgRating: 1.3, avgAdr: 85 }],
  form: [
    { userId: 'u1', matchResultId: 'm1', playedAtUtc: '2026-01-01T00:00:00Z', opponent: 'Foo', map: 'Mirage', rating: 1.2, adr: 80 },
    { userId: 'u1', matchResultId: 'm2', playedAtUtc: '2026-01-08T00:00:00Z', opponent: 'Bar', map: 'Mirage', rating: 1.4, adr: 90 },
  ],
}

describe('AdvancedStatsTab', () => {
  it('renders the metrics table with tooltips, heatmap and form chart', async () => {
    mockFetch(data)
    renderWithProviders(<AdvancedStatsTab />)

    expect((await screen.findAllByText('Alice')).length).toBeGreaterThan(0)
    expect(screen.getByRole('columnheader', { name: /Pomszczenia/ })).toHaveAttribute('title', expect.stringContaining('pomścił'))
    expect(screen.getByRole('columnheader', { name: /Zgony pomszczone/ })).toBeInTheDocument()
    expect(screen.getAllByText('4/10 (40%)').length).toBeGreaterThan(0)
    expect(screen.getByText('Rating na mapach')).toBeInTheDocument()
    expect(screen.getByRole('img', { name: 'Rating w kolejnych meczach' })).toBeInTheDocument()
  })

  it('sorts by a clicked column', async () => {
    mockFetch(data)
    renderWithProviders(<AdvancedStatsTab />)
    await screen.findAllByText('Alice')

    const names = () => within(screen.getAllByRole('table')[0]).getAllByRole('row').slice(1).map((row) => row.querySelector('td')?.textContent)
    expect(names()).toEqual(['Alice', 'Bob'])
    await userEvent.click(screen.getByRole('columnheader', { name: /Rating/ }))
    expect(names()).toEqual(['Bob', 'Alice'])
  })

  it('refetches with the chosen filters', async () => {
    const fetchMock = mockFetch(data)
    renderWithProviders(<AdvancedStatsTab />)
    await screen.findAllByText('Alice')

    await userEvent.selectOptions(screen.getByLabelText('Mapa'), 'Mirage')
    await userEvent.selectOptions(screen.getByLabelText('Liczba meczów'), '10')

    await waitFor(() => {
      const urls = fetchMock.mock.calls.map((call) => String(call[0]))
      expect(urls.some((url) => url.includes('map=Mirage') && url.includes('last=10'))).toBe(true)
    })
  })

  it('shows an empty state without demos', async () => {
    mockFetch({ matchesAnalyzed: 0, matchesSkipped: 0, players: [], mapCells: [], form: [] })
    renderWithProviders(<AdvancedStatsTab />)

    expect(await screen.findByText(/Brak meczów z demką/)).toBeInTheDocument()
  })
})
