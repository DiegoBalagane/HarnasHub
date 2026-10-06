import { render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { describe, expect, it, vi } from 'vitest'
import { Tabs } from './Tabs'

const tabs = [
  { id: 'a', label: 'Pierwsza' },
  { id: 'b', label: 'Druga' },
] as const

describe('Tabs', () => {
  it('marks only the active tab as selected', () => {
    render(<Tabs tabs={tabs} value="b" onChange={vi.fn()} />)
    expect(screen.getByRole('tab', { name: 'Druga' })).toHaveAttribute('aria-selected', 'true')
    expect(screen.getByRole('tab', { name: 'Pierwsza' })).toHaveAttribute('aria-selected', 'false')
  })

  it('reports the clicked tab id', async () => {
    const onChange = vi.fn()
    render(<Tabs tabs={tabs} value="a" onChange={onChange} />)
    await userEvent.click(screen.getByRole('tab', { name: 'Druga' }))
    expect(onChange).toHaveBeenCalledWith('b')
  })
})
