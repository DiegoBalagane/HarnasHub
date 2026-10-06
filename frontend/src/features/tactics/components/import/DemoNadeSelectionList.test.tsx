import { render, screen, within } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { describe, expect, it, vi } from 'vitest'
import type { DemoNade } from '../../../../services/tacticsApi'
import { DemoNadeSelectionList } from './DemoNadeSelectionList'

function nade(id: number, throwerName: string, steam: string): DemoNade {
  return {
    id,
    type: 'Smoke',
    throwerName,
    throwerSteamId: steam,
    side: 'T',
    throwX: 0,
    throwY: 0,
    landX: 1,
    landY: 1,
    secondsIntoRound: 5,
  }
}

const grenades = [nade(1, 'Adam', 'a'), nade(2, 'Adam', 'a'), nade(3, 'Bob', 'b')]

describe('DemoNadeSelectionList', () => {
  it('shows an empty-state message without grenades', () => {
    render(<DemoNadeSelectionList grenades={[]} selectedIds={new Set()} onToggle={vi.fn()} />)
    expect(screen.getByText(/nie rzuciła/)).toBeInTheDocument()
  })

  it('selecting a player toggles all of their grenades at once', async () => {
    const onToggle = vi.fn()
    render(<DemoNadeSelectionList grenades={grenades} selectedIds={new Set()} onToggle={onToggle} />)
    await userEvent.click(screen.getByRole('checkbox', { name: /Adam/ }))
    expect(onToggle).toHaveBeenCalledWith([1, 2], true)
  })

  it('unchecking a fully selected player deselects all of their grenades', async () => {
    const onToggle = vi.fn()
    render(<DemoNadeSelectionList grenades={grenades} selectedIds={new Set([1, 2])} onToggle={onToggle} />)
    await userEvent.click(screen.getByRole('checkbox', { name: /Adam/ }))
    expect(onToggle).toHaveBeenCalledWith([1, 2], false)
  })

  it('toggles a single grenade and shows the partial state of its player', async () => {
    const onToggle = vi.fn()
    render(<DemoNadeSelectionList grenades={grenades} selectedIds={new Set([1])} onToggle={onToggle} />)

    const adam = screen.getByRole('checkbox', { name: /Adam/ })
    expect(adam).not.toBeChecked()
    expect((adam as HTMLInputElement).indeterminate).toBe(true)
    expect(screen.getByText('1/2')).toBeInTheDocument()

    const adamGroup = adam.closest('li') as HTMLElement
    const [first, second] = within(adamGroup).getAllByRole('checkbox').slice(1)
    expect(first).toBeChecked()
    await userEvent.click(second)
    expect(onToggle).toHaveBeenCalledWith([2], true)
  })
})
