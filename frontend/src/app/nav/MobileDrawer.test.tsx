import { screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { describe, expect, it, vi } from 'vitest'
import { renderWithProviders } from '../../test/renderWithProviders'
import { MobileDrawer } from './MobileDrawer'

describe('MobileDrawer', () => {
  it('renders nothing when closed and does not lock scroll', () => {
    renderWithProviders(<MobileDrawer open={false} onClose={vi.fn()} onLogout={vi.fn()} />)
    expect(screen.queryByRole('dialog')).not.toBeInTheDocument()
    expect(document.body.style.overflow).toBe('')
  })

  it('locks body scroll while open and restores it on close', () => {
    const { unmount } = renderWithProviders(<MobileDrawer open onClose={vi.fn()} onLogout={vi.fn()} />)
    expect(screen.getByRole('dialog', { name: 'Menu' })).toBeInTheDocument()
    expect(document.body.style.overflow).toBe('hidden')
    unmount()
    expect(document.body.style.overflow).toBe('')
  })

  it('closes on Escape', async () => {
    const onClose = vi.fn()
    renderWithProviders(<MobileDrawer open onClose={onClose} onLogout={vi.fn()} />)
    onClose.mockClear() // the route-change effect also fires once on mount
    await userEvent.keyboard('{Escape}')
    expect(onClose).toHaveBeenCalledTimes(1)
  })

  it('closes when the close button is pressed', async () => {
    const onClose = vi.fn()
    renderWithProviders(<MobileDrawer open onClose={onClose} onLogout={vi.fn()} />)
    onClose.mockClear()
    await userEvent.click(screen.getByRole('button', { name: 'Zamknij menu' }))
    expect(onClose).toHaveBeenCalled()
  })
})
