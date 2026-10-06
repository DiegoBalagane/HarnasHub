import { render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { describe, expect, it, vi } from 'vitest'
import { Button } from './Button'

describe('Button', () => {
  it('defaults to type=button so it never submits a surrounding form', async () => {
    const onSubmit = vi.fn((event: React.FormEvent) => event.preventDefault())
    render(
      <form onSubmit={onSubmit}>
        <Button>Zapisz</Button>
      </form>,
    )
    await userEvent.click(screen.getByRole('button', { name: 'Zapisz' }))
    expect(onSubmit).not.toHaveBeenCalled()
  })

  it('submits when type=submit is requested', async () => {
    const onSubmit = vi.fn((event: React.FormEvent) => event.preventDefault())
    render(
      <form onSubmit={onSubmit}>
        <Button type="submit">Wyślij</Button>
      </form>,
    )
    await userEvent.click(screen.getByRole('button', { name: 'Wyślij' }))
    expect(onSubmit).toHaveBeenCalledTimes(1)
  })

  it('does not fire onClick when disabled', async () => {
    const onClick = vi.fn()
    render(<Button disabled onClick={onClick}>X</Button>)
    await userEvent.click(screen.getByRole('button'))
    expect(onClick).not.toHaveBeenCalled()
  })

  it('applies variant and size classes plus a custom className', () => {
    render(<Button variant="danger" size="sm" className="extra">Usuń</Button>)
    const button = screen.getByRole('button', { name: 'Usuń' })
    expect(button).toHaveClass('bg-danger-600', 'text-xs', 'extra')
  })
})
