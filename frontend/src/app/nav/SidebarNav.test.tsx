import { screen } from '@testing-library/react'
import { describe, expect, it } from 'vitest'
import { renderWithProviders } from '../../test/renderWithProviders'
import { isNavItemActive } from './navItems'
import { SidebarNav } from './SidebarNav'

function activeLinks(): string[] {
  return screen
    .getAllByRole('link')
    .filter((link) => link.getAttribute('aria-current') === 'page')
    .map((link) => link.getAttribute('href') ?? '')
}

describe('isNavItemActive', () => {
  const item = { to: '/results', label: 'Wyniki', icon: 'results' } as const

  it('matches the route itself and nested routes only', () => {
    expect(isNavItemActive(item, '/results')).toBe(true)
    expect(isNavItemActive(item, '/results/abc')).toBe(true)
    expect(isNavItemActive(item, '/results-archive')).toBe(false)
  })
})

describe('SidebarNav', () => {
  it('marks the matching link active for nested routes', () => {
    renderWithProviders(<SidebarNav collapsed={false} />, { route: '/results/42' })
    expect(activeLinks()).toEqual(['/results'])
  })

  it('keeps Przeciwnicy active on /opponents/report with a query string', () => {
    renderWithProviders(<SidebarNav collapsed={false} />, { route: '/opponents/report?name=Navi' })
    expect(activeLinks()).toEqual(['/opponents'])
  })

  it('keeps Playbook active with a map query', () => {
    renderWithProviders(<SidebarNav collapsed={false} />, { route: '/playbook?map=mirage' })
    expect(activeLinks()).toEqual(['/playbook'])
  })

  it('marks nothing active on an unrelated route', () => {
    renderWithProviders(<SidebarNav collapsed={false} />, { route: '/settings' })
    expect(activeLinks()).toEqual([])
  })

  it('shows labels when expanded and icon-only links with aria-labels when collapsed', () => {
    const { unmount } = renderWithProviders(<SidebarNav collapsed={false} />)
    expect(screen.getByText('Kalendarz')).toBeInTheDocument()
    unmount()

    renderWithProviders(<SidebarNav collapsed />)
    expect(screen.queryByText('Kalendarz')).not.toBeInTheDocument()
    expect(screen.getByRole('link', { name: 'Kalendarz' })).toHaveAttribute('title', 'Kalendarz')
  })
})
