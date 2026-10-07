import { screen } from '@testing-library/react'
import { afterEach, describe, expect, it } from 'vitest'
import { useAuthStore } from '../../features/auth/stores/useAuthStore'
import { mockFetch, renderWithProviders } from '../../test/renderWithProviders'
import { TeamInfoPage } from './TeamInfoPage'

const entries = [
  { id: '1', category: 'Discord', title: 'Discord serwera', value: 'https://discord.gg/abc', isSecret: false, sortOrder: 0, updatedAtUtc: '2026-01-01T00:00:00Z' },
  { id: '2', category: 'Serwery', title: 'Serwer treningowy', value: 'connect 1.2.3.4:27015', isSecret: true, sortOrder: 0, updatedAtUtc: '2026-01-01T00:00:00Z' },
]

describe('TeamInfoPage', () => {
  afterEach(() => useAuthStore.setState({ role: null, isCoach: false }))

  it('groups entries by category and hides management controls from players', async () => {
    useAuthStore.setState({ role: 'Player', isCoach: false })
    mockFetch(entries)

    renderWithProviders(<TeamInfoPage />)

    expect(await screen.findByRole('heading', { name: 'Discord' })).toBeInTheDocument()
    expect(screen.getByRole('heading', { name: 'Serwery' })).toBeInTheDocument()
    expect(screen.queryByRole('button', { name: '+ Dodaj wpis' })).not.toBeInTheDocument()
    expect(screen.queryByRole('button', { name: /Edytuj/ })).not.toBeInTheDocument()
  })

  it('shows add, edit, delete and reorder controls to a Manager', async () => {
    useAuthStore.setState({ role: 'Manager', isCoach: false })
    mockFetch(entries)

    renderWithProviders(<TeamInfoPage />)

    expect(await screen.findByRole('button', { name: 'Edytuj: Discord serwera' })).toBeInTheDocument()
    expect(screen.getByRole('button', { name: '+ Dodaj wpis' })).toBeInTheDocument()
    expect(screen.getByRole('button', { name: 'Usuń: Serwer treningowy' })).toBeInTheDocument()
    expect(screen.getByRole('button', { name: 'Przenieś wyżej: Serwer treningowy' })).toBeDisabled()
  })

  it('shows the empty state with example placeholders', async () => {
    useAuthStore.setState({ role: 'Player', isCoach: false })
    mockFetch([])

    renderWithProviders(<TeamInfoPage />)

    expect(await screen.findByText(/Brak wpisów/)).toBeInTheDocument()
    expect(screen.getByText('Serwer treningowy')).toBeInTheDocument()
  })
})
