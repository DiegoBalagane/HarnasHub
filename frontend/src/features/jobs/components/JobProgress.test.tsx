import { render, screen } from '@testing-library/react'
import { describe, expect, it } from 'vitest'
import { JobProgress } from './JobProgress'

describe('JobProgress', () => {
  it('shows percentage and stage of a running job', () => {
    render(<JobProgress job={{ status: 'Running', progress: 42, stage: 'Analiza demki' }} />)

    expect(screen.getByRole('progressbar', { name: 'Analiza demki' })).toHaveAttribute('aria-valuenow', '42')
    expect(screen.getByText('42%')).toBeInTheDocument()
  })

  it('shows an indeterminate bar with the starting label while uploading', () => {
    render(<JobProgress isStarting startingLabel="Wgrywanie demki…" />)

    expect(screen.getByRole('progressbar', { name: 'Wgrywanie demki…' })).not.toHaveAttribute('aria-valuenow')
  })

  it('shows the polish error instead of the bar', () => {
    render(<JobProgress job={{ status: 'Failed', progress: 30, stage: null }} error="Nie udało się odczytać demki." />)

    expect(screen.getByRole('alert')).toHaveTextContent('Nie udało się odczytać demki.')
    expect(screen.queryByRole('progressbar')).not.toBeInTheDocument()
  })

  it('renders nothing once the job succeeded', () => {
    const { container } = render(<JobProgress job={{ status: 'Succeeded', progress: 100, stage: null }} />)

    expect(container).toBeEmptyDOMElement()
  })
})
