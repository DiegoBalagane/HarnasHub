import { render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { describe, expect, it, vi } from 'vitest'
import { Modal } from './Modal'
import { useModalGuard } from './ModalGuardContext'

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

function GuardReporter(props: { isBusy: boolean; isDirty: boolean; onDiscard?: () => void }) {
  useModalGuard(props)
  return <p>pola</p>
}

interface GuardedProps {
  isBusy?: boolean
  isDirty?: boolean
  onClose: () => void
  onDiscard?: () => void
}

function Guarded({ isBusy = false, isDirty = false, onClose, onDiscard }: GuardedProps) {
  return (
    <Modal title="Formularz" onClose={onClose}>
      <GuardReporter isBusy={isBusy} isDirty={isDirty} onDiscard={onDiscard} />
    </Modal>
  )
}

describe('Modal dismissal guard', () => {
  it('asks for confirmation on backdrop click and Escape while dirty, and keeps the form on cancel', async () => {
    const onClose = vi.fn()
    render(<Guarded isDirty onClose={onClose} />)

    await userEvent.click(screen.getByRole('presentation'))
    expect(onClose).not.toHaveBeenCalled()
    expect(screen.getByRole('alertdialog', { name: 'Porzucić wpisane dane?' })).toBeInTheDocument()

    await userEvent.click(screen.getByRole('button', { name: 'Wróć do formularza' }))
    expect(screen.queryByRole('alertdialog')).not.toBeInTheDocument()

    await userEvent.keyboard('{Escape}')
    expect(onClose).not.toHaveBeenCalled()
    expect(screen.getByRole('alertdialog')).toBeInTheDocument()
  })

  it('closes and discards after confirmation of a dirty form', async () => {
    const onClose = vi.fn()
    const onDiscard = vi.fn()
    render(<Guarded isDirty onClose={onClose} onDiscard={onDiscard} />)

    await userEvent.click(screen.getByTitle('Zamknij'))
    await userEvent.click(screen.getByRole('button', { name: 'Porzuć' }))
    expect(onClose).toHaveBeenCalledTimes(1)
    expect(onDiscard).toHaveBeenCalledTimes(1)
  })

  it('does not discard a running job when closing a busy form', async () => {
    const onClose = vi.fn()
    const onDiscard = vi.fn()
    render(<Guarded isBusy isDirty onClose={onClose} onDiscard={onDiscard} />)

    await userEvent.click(screen.getByRole('presentation'))
    await userEvent.click(screen.getByRole('button', { name: 'Zamknij okno' }))
    expect(onClose).toHaveBeenCalledTimes(1)
    expect(onDiscard).not.toHaveBeenCalled()
  })

  it('closes immediately when the form is clean', async () => {
    const onClose = vi.fn()
    render(<Guarded onClose={onClose} />)
    await userEvent.click(screen.getByRole('presentation'))
    expect(onClose).toHaveBeenCalledTimes(1)
  })
})
