import { render, screen, waitFor } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { describe, expect, it, vi } from 'vitest'
import type { TeamInfoEntry } from '../../../services/teamInfoApi'
import { TeamInfoEntryRow } from './TeamInfoEntryRow'

function makeEntry(overrides: Partial<TeamInfoEntry>): TeamInfoEntry {
  return {
    id: '1',
    category: 'Serwery',
    title: 'Wpis',
    value: 'zwykły tekst',
    isSecret: false,
    sortOrder: 0,
    updatedAtUtc: '2026-01-01T00:00:00Z',
    ...overrides,
  }
}

function renderRow(entry: TeamInfoEntry, canManage = false) {
  const handlers = { onMoveUp: vi.fn(), onMoveDown: vi.fn(), onEdit: vi.fn(), onDelete: vi.fn() }
  render(<TeamInfoEntryRow entry={entry} canManage={canManage} isFirst={false} isLast={false} {...handlers} />)
  return handlers
}

describe('TeamInfoEntryRow', () => {
  it('renders URLs as external links', () => {
    renderRow(makeEntry({ value: 'https://discord.gg/abc' }))

    const link = screen.getByRole('link', { name: 'https://discord.gg/abc' })
    expect(link).toHaveAttribute('href', 'https://discord.gg/abc')
    expect(link).toHaveAttribute('target', '_blank')
  })

  it('offers a Połącz button for connect commands', () => {
    renderRow(makeEntry({ value: 'connect 1.2.3.4:27015' }))

    expect(screen.getByRole('link', { name: 'Połącz' })).toHaveAttribute('href', 'steam://connect/1.2.3.4:27015')
  })

  it('shows multi-line values in a monospace block', () => {
    renderRow(makeEntry({ value: 'rate 1\ncl_interp 0' }))

    expect(screen.getByText(/cl_interp 0/).tagName).toBe('PRE')
  })

  it('masks secrets until revealed and hides the connect button meanwhile', async () => {
    renderRow(makeEntry({ value: 'connect 1.2.3.4:27015; password tajne', isSecret: true }))

    expect(screen.queryByText(/tajne/)).not.toBeInTheDocument()
    expect(screen.queryByRole('link', { name: 'Połącz' })).not.toBeInTheDocument()
    expect(screen.getByText('••••••')).toBeInTheDocument()

    await userEvent.click(screen.getByRole('button', { name: 'Pokaż' }))

    expect(screen.getByRole('link', { name: 'Połącz' })).toBeInTheDocument()
    expect(screen.getByRole('button', { name: 'Ukryj' })).toBeInTheDocument()
  })

  it('copies the value and confirms it', async () => {
    const user = userEvent.setup()
    const writeText = vi.spyOn(navigator.clipboard, 'writeText').mockResolvedValue()
    renderRow(makeEntry({ value: '1.2.3.4:27015' }))

    await user.click(screen.getByRole('button', { name: 'Kopiuj' }))

    expect(writeText).toHaveBeenCalledWith('1.2.3.4:27015')
    await waitFor(() => expect(screen.getByRole('button', { name: 'Skopiowano' })).toBeInTheDocument())
  })

  it('shows management controls only for Coach/Manager', async () => {
    const readonly = renderRow(makeEntry({ title: 'A' }), false)
    expect(screen.queryByRole('button', { name: /Edytuj/ })).not.toBeInTheDocument()
    expect(readonly.onEdit).not.toHaveBeenCalled()
  })

  it('calls edit and delete handlers when managing', async () => {
    const entry = makeEntry({ title: 'A' })
    const handlers = renderRow(entry, true)

    await userEvent.click(screen.getByRole('button', { name: 'Edytuj: A' }))
    await userEvent.click(screen.getByRole('button', { name: 'Usuń: A' }))

    expect(handlers.onEdit).toHaveBeenCalledWith(entry)
    expect(handlers.onDelete).toHaveBeenCalledWith(entry)
  })
})
