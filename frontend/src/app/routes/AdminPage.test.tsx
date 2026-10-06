import { screen } from '@testing-library/react'
import { Route, Routes } from 'react-router-dom'
import { afterEach, describe, expect, it } from 'vitest'
import { useAuthStore } from '../../features/auth/stores/useAuthStore'
import { mockFetch, renderWithProviders } from '../../test/renderWithProviders'
import { AdminPage } from './AdminPage'

function renderAdmin() {
  return renderWithProviders(
    <Routes>
      <Route path="/admin" element={<AdminPage />} />
      <Route path="/dashboard" element={<p>Dashboard</p>} />
    </Routes>,
    { route: '/admin' },
  )
}

describe('AdminPage', () => {
  afterEach(() => useAuthStore.setState({ role: null }))

  it('redirects a non-Manager to the dashboard', () => {
    mockFetch([])
    useAuthStore.setState({ role: 'Player' })
    renderAdmin()
    expect(screen.getByText('Dashboard')).toBeInTheDocument()
    expect(screen.queryByRole('heading', { name: 'Panel admina' })).not.toBeInTheDocument()
  })

  it('renders the tabs for a Manager', () => {
    mockFetch([])
    useAuthStore.setState({ role: 'Manager' })
    renderAdmin()
    expect(screen.getByRole('heading', { name: 'Panel admina' })).toBeInTheDocument()
    expect(screen.getByRole('tab', { name: 'Użytkownicy' })).toBeInTheDocument()
    expect(screen.getByRole('tab', { name: 'Kolory pinezek' })).toBeInTheDocument()
    expect(screen.getByRole('tab', { name: 'Integracje' })).toBeInTheDocument()
  })
})
