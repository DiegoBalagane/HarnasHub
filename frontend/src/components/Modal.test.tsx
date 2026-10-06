import { render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { describe, expect, it, vi } from 'vitest'
import { Modal } from './Modal'

describe('Modal', () => {
  it('renders the title and children in a labelled dialog', () => {
    render(
      <Modal title="Nowe zadanie" onClose={vi.fn()}>
        <p>treść</p>
      </Modal>,
    )
    expect(screen.getByRole('dialog', { name: 'Nowe zadanie' })).toBeInTheDocument()
    expect(screen.getByText('treść')).toBeInTheDocument()
  })

  it('closes on Escape', async () => {
    const onClose = vi.fn()
    render(<Modal title="T" onClose={onClose}>x</Modal>)
    await userEvent.keyboard('{Escape}')
    expect(onClose).toHaveBeenCalledTimes(1)
  })

  it('closes on backdrop click but not on clicks inside the dialog', async () => {
    const onClose = vi.fn()
    render(<Modal title="T" onClose={onClose}><button type="button">wewnątrz</button></Modal>)

    await userEvent.click(screen.getByText('wewnątrz'))
    expect(onClose).not.toHaveBeenCalled()

    await userEvent.click(screen.getByRole('presentation'))
    expect(onClose).toHaveBeenCalledTimes(1)
  })

  it('closes with the X button', async () => {
    const onClose = vi.fn()
    render(<Modal title="T" onClose={onClose}>x</Modal>)
    await userEvent.click(screen.getByTitle('Zamknij'))
    expect(onClose).toHaveBeenCalledTimes(1)
  })
})
