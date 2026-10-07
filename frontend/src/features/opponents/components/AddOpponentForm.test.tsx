import { screen, waitFor } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { Route, Routes, useLocation } from 'react-router-dom'
import { beforeEach, describe, expect, it, vi } from 'vitest'
import { Modal } from '../../../components/Modal'
import { mockFetch, renderWithProviders } from '../../../test/renderWithProviders'
import { useJobStore } from '../../jobs/stores/useJobStore'
import { AddOpponentForm } from './AddOpponentForm'

function Where() {
  const location = useLocation()
  return <p data-testid="where">{location.pathname + location.search}</p>
}

function renderForm() {
  return renderWithProviders(
    <Routes>
      <Route path="/" element={<AddOpponentForm />} />
      <Route path="*" element={<Where />} />
    </Routes>,
  )
}

const opponents = [
  { name: 'Team X', noteCount: 0, wins: 0, losses: 0, draws: 0, lastPlayedAtUtc: null, nextEventAtUtc: null },
]

/** Answers the list with `opponents`, a started job (202) to PUT/POST and a running job to GET /api/jobs/{id}. */
function mockApi() {
  const fetchMock = vi.fn(async (input: string | URL | Request, init?: RequestInit) => {
    const url = String(input)
    if (url.includes('/api/jobs/')) {
      return new Response(JSON.stringify({ id: 'job-1', kind: 'x', status: 'Running', progress: 10 }), { status: 200 })
    }
    if (init?.method === 'PUT' || init?.method === 'POST') {
      return new Response(JSON.stringify({ jobId: 'job-1' }), { status: 202 })
    }
    return new Response(JSON.stringify(opponents), { status: 200 })
  })
  vi.stubGlobal('fetch', fetchMock)
  return fetchMock
}

describe('AddOpponentForm', () => {
  beforeEach(() => {
    useJobStore.setState({ jobsBySlot: {}, handledJobIds: {} })
  })

  it('keeps "Dodaj" disabled until a name and a FACEIT source or demo are given', async () => {
    mockFetch([])
    renderForm()
    const add = screen.getByRole('button', { name: 'Dodaj' })
    expect(add).toBeDisabled()

    await userEvent.type(screen.getByPlaceholderText('Nazwa przeciwnika'), 'Team Y')
    expect(add).toBeDisabled()

    await userEvent.type(screen.getByLabelText(/FACEIT/), 'nick1, nick2')
    expect(add).toBeEnabled()
  })

  it('warns when the name already exists and offers the report', async () => {
    mockFetch(opponents)
    renderForm()

    await userEvent.type(screen.getByPlaceholderText('Nazwa przeciwnika'), 'team x')

    expect(await screen.findByText(/już istnieje/)).toBeInTheDocument()
    expect(screen.getByRole('link', { name: 'Przejdź do raportu' })).toHaveAttribute(
      'href',
      '/opponents/report?name=Team%20X',
    )
  })

  it('starts the FACEIT link job and goes to the report', async () => {
    const fetchMock = mockApi()
    renderForm()

    await userEvent.type(screen.getByPlaceholderText('Nazwa przeciwnika'), 'Team Y')
    await userEvent.type(screen.getByLabelText(/FACEIT/), 'nick1, nick2')
    await userEvent.click(screen.getByRole('button', { name: 'Dodaj' }))

    await waitFor(() => expect(screen.getByTestId('where')).toHaveTextContent('/opponents/report?name=Team%20Y'))
    const put = fetchMock.mock.calls.find(([, init]) => init?.method === 'PUT')
    expect(String(put?.[0])).toContain('/api/opponents/report/link')
    expect(JSON.parse(String(put?.[1]?.body))).toEqual({ opponentName: 'Team Y', source: 'nick1, nick2' })
  })

  it('shows the selected demo files and lets one be removed', async () => {
    mockFetch([])
    renderForm()

    await userEvent.upload(screen.getByLabelText('Pliki demek'), [
      new File(['a'], '1-abc.dem'),
      new File(['b'], '2-def.dem'),
    ])
    expect(screen.getByText('1-abc.dem')).toBeInTheDocument()
    expect(screen.getByText('2-def.dem')).toBeInTheDocument()

    await userEvent.click(screen.getByRole('button', { name: 'Usuń 1-abc.dem' }))
    expect(screen.queryByText('1-abc.dem')).not.toBeInTheDocument()
  })

  it('asks before closing the modal once something was typed', async () => {
    mockFetch([])
    renderWithProviders(
      <Modal title="Dodaj przeciwnika" onClose={vi.fn()}>
        <AddOpponentForm />
      </Modal>,
    )

    await userEvent.type(screen.getByPlaceholderText('Nazwa przeciwnika'), 'Team Y')
    await userEvent.keyboard('{Escape}')

    expect(await screen.findByText('Porzuć')).toBeInTheDocument()
  })
})
