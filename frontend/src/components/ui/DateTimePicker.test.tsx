import { render, screen, within } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { useState } from 'react'
import { describe, expect, it, vi } from 'vitest'
import { DatePicker, DateTimePicker, TimeRangePicker } from './DateTimePicker'

function Harness({ initial = '2026-10-07T20:30', onChange = vi.fn() }: { initial?: string; onChange?: (value: string) => void }) {
  const [value, setValue] = useState(initial)
  return (
    <DateTimePicker
      label="Początek"
      value={value}
      onChange={(next) => {
        setValue(next)
        onChange(next)
      }}
    />
  )
}

describe('DateTimePicker', () => {
  it('shows the formatted value in the trigger and opens a dialog with Zatwierdź/Anuluj', async () => {
    render(<Harness />)
    const trigger = screen.getByRole('button', { name: /Początek/ })
    expect(trigger).toHaveTextContent('śr, 7 paź 2026 · 20:30')

    await userEvent.click(trigger)
    const dialog = screen.getByRole('dialog', { name: 'Początek' })
    expect(within(dialog).getByRole('button', { name: 'Zatwierdź' })).toBeInTheDocument()
    expect(within(dialog).getByRole('button', { name: 'Anuluj' })).toBeInTheDocument()
    expect(within(dialog).getByText('październik 2026')).toBeInTheDocument()
  })

  it('navigates months and commits the picked date and time only on Zatwierdź', async () => {
    const onChange = vi.fn()
    render(<Harness onChange={onChange} />)
    await userEvent.click(screen.getByRole('button', { name: /Początek/ }))

    await userEvent.click(screen.getByRole('button', { name: 'Następny miesiąc' }))
    expect(screen.getByText('listopad 2026')).toBeInTheDocument()
    await userEvent.click(screen.getByRole('button', { name: /15 listopada 2026/ }))
    await userEvent.click(within(screen.getByRole('listbox', { name: 'Godziny' })).getByRole('option', { name: '18' }))
    await userEvent.click(within(screen.getByRole('listbox', { name: 'Minuty' })).getByRole('option', { name: '45' }))

    expect(onChange).not.toHaveBeenCalled()
    await userEvent.click(screen.getByRole('button', { name: 'Zatwierdź' }))

    expect(onChange).toHaveBeenCalledWith('2026-11-15T18:45')
    expect(screen.queryByRole('dialog')).not.toBeInTheDocument()
    expect(screen.getByRole('button', { name: /Początek/ })).toHaveTextContent('niedz, 15 lis 2026 · 18:45')
  })

  it('keeps the old value on Anuluj and on Escape', async () => {
    const onChange = vi.fn()
    render(<Harness onChange={onChange} />)

    await userEvent.click(screen.getByRole('button', { name: /Początek/ }))
    await userEvent.click(screen.getByRole('button', { name: /10 października 2026/ }))
    await userEvent.click(screen.getByRole('button', { name: 'Anuluj' }))
    expect(screen.getByRole('button', { name: /Początek/ })).toHaveTextContent('śr, 7 paź 2026 · 20:30')

    await userEvent.click(screen.getByRole('button', { name: /Początek/ }))
    await userEvent.click(screen.getByRole('button', { name: /11 października 2026/ }))
    await userEvent.keyboard('{Escape}')
    expect(screen.queryByRole('dialog')).not.toBeInTheDocument()
    expect(onChange).not.toHaveBeenCalled()
  })

  it('moves the selection with arrow keys', async () => {
    const onChange = vi.fn()
    render(<Harness onChange={onChange} />)
    await userEvent.click(screen.getByRole('button', { name: /Początek/ }))

    screen.getByRole('button', { name: /środa, 7 października 2026/ }).focus()
    await userEvent.keyboard('{ArrowRight}{ArrowDown}')
    expect(screen.getByRole('button', { name: /15 października 2026/ })).toHaveFocus()
    await userEvent.keyboard('{Enter}')
    await userEvent.click(screen.getByRole('button', { name: 'Zatwierdź' }))
    expect(onChange).toHaveBeenCalledWith('2026-10-15T20:30')
  })

  it('accepts a typed time', async () => {
    const onChange = vi.fn()
    render(<Harness onChange={onChange} />)
    await userEvent.click(screen.getByRole('button', { name: /Początek/ }))

    const input = screen.getByLabelText('Godzina (HH:mm)')
    await userEvent.clear(input)
    await userEvent.type(input, '2145{Enter}')
    await userEvent.click(screen.getByRole('button', { name: 'Zatwierdź' }))
    expect(onChange).toHaveBeenCalledWith('2026-10-07T21:45')
  })

  it('clears an optional value', async () => {
    const onChange = vi.fn()
    render(<DateTimePicker label="Koniec" clearable value="2026-10-07T20:30" onChange={onChange} />)
    await userEvent.click(screen.getByRole('button', { name: /Koniec/ }))
    await userEvent.click(screen.getByRole('button', { name: 'Wyczyść' }))
    expect(onChange).toHaveBeenCalledWith('')
  })
})

describe('DatePicker and TimeRangePicker', () => {
  it('commits a yyyy-MM-dd value', async () => {
    const onChange = vi.fn()
    render(<DatePicker label="Data" value="2026-10-07" onChange={onChange} />)
    await userEvent.click(screen.getByRole('button', { name: /Data/ }))
    await userEvent.click(screen.getByRole('button', { name: /20 października 2026/ }))
    await userEvent.click(screen.getByRole('button', { name: 'Zatwierdź' }))
    expect(onChange).toHaveBeenCalledWith('2026-10-20')
  })

  it('reports the whole range when one end changes', async () => {
    const onChange = vi.fn()
    render(<TimeRangePicker from="18:00" to="22:00" onChange={onChange} />)
    await userEvent.click(screen.getByRole('button', { name: /Do godziny/ }))
    await userEvent.click(within(screen.getByRole('listbox', { name: 'Godziny' })).getByRole('option', { name: '23' }))
    await userEvent.click(screen.getByRole('button', { name: 'Zatwierdź' }))
    expect(onChange).toHaveBeenCalledWith({ from: '18:00', to: '23:00' })
  })
})
