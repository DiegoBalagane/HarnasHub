import { screen, waitFor } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { describe, expect, it, vi } from 'vitest'
import { mockFetch, renderWithProviders } from '../../../test/renderWithProviders'
import { DeleteOpponentDialog } from './DeleteOpponentDialog'

const previewWithHistory = {
  notes: 2,
  demoAnalyses: 0,
  hasFaceitLink: true,
  hasReportSnapshot: false,
  matchResults: 3,
  events: 1,
  isHidden: false,
}

describe('DeleteOpponentDialog', () => {
  it('shows the counts and keeps match history by default', async () => {
    mockFetch(previewWithHistory)
    renderWithProviders(<DeleteOpponentDialog name="Team Test" onCancel={vi.fn()} onDeleted={vi.fn()} />)

    expect(await screen.findByText('3 wyniki, 1 wydarzenie')).toBeInTheDocument()
    expect(screen.getByText(/2 notatki, powiązanie FACEIT/)).toBeInTheDocument()
    expect(screen.getByRole('checkbox', { name: 'Usuń też wyniki i wydarzenia' })).not.toBeChecked()
    expect(screen.getByRole('button', { name: 'Usuń' })).toBeInTheDocument()
    expect(screen.getByText(/zacznie od zera/)).toBeInTheDocument()
  })

  it('sends includeHistory=true once the checkbox is ticked', async () => {
    const fetchMock = mockFetch(previewWithHistory)
    const onDeleted = vi.fn()
    renderWithProviders(<DeleteOpponentDialog name="Team Test" onCancel={vi.fn()} onDeleted={onDeleted} />)

    await userEvent.click(await screen.findByRole('checkbox', { name: 'Usuń też wyniki i wydarzenia' }))
    fetchMock.mockImplementation(async () => new Response(JSON.stringify({ hidden: false }), { status: 200 }))
    await userEvent.click(screen.getByRole('button', { name: 'Usuń' }))

    await waitFor(() => expect(onDeleted).toHaveBeenCalledWith(false))
    const deleteCall = fetchMock.mock.calls.find(([, init]) => init?.method === 'DELETE')
    expect(String(deleteCall?.[0])).toContain('/api/opponents?name=Team%20Test&includeHistory=true')
  })

  it('offers no history checkbox when only scouting data exists', async () => {
    mockFetch({ ...previewWithHistory, matchResults: 0, events: 0 })
    renderWithProviders(<DeleteOpponentDialog name="Team Test" onCancel={vi.fn()} onDeleted={vi.fn()} />)

    expect(await screen.findByRole('button', { name: 'Usuń' })).toBeInTheDocument()
    expect(screen.queryByRole('checkbox')).not.toBeInTheDocument()
  })
})
