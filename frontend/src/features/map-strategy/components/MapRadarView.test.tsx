import { fireEvent, screen, waitFor, within } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest'
import { MAP_STRATEGY_SETTINGS } from '../../../constants'
import type { MapPosition, MapTextAnnotation } from '../../../services/mapStrategyApi'
import { renderWithProviders } from '../../../test/renderWithProviders'
import { useAuthStore } from '../../auth/stores/useAuthStore'
import { MapRadarView } from './MapRadarView'

function position(id: string, userId: string, nick: string, x: number, y: number): MapPosition {
  return {
    id, userId, x, y, displayName: nick, inGameNickname: nick, teamRole: null,
    pinColor: null, pinMark: null, label: null, note: null,
  }
}

const data: Record<string, MapPosition[]> = {
  T: [position('t1', 'u1', 'Alfa', 0.2, 0.2)],
  CT: [position('c1', 'u2', 'Bravo', 0.7, 0.7)],
}
const annotations: Record<string, MapTextAnnotation[]> = {
  T: [{ id: 'n1', text: 'Smoke tu', color: '#ffffff', fontSizePx: 16, x: 0.4, y: 0.4 }],
  CT: [],
}

function stubFetch() {
  const fetchMock = vi.fn(async (input: string | URL | Request, init?: RequestInit) => {
    const url = String(input)
    if (init?.method && init.method !== 'GET') return new Response(JSON.stringify({}), { status: 200 })
    if (url.includes('/api/roster')) {
      return new Response(
        JSON.stringify([
          { id: 'u1', displayName: 'Alfa', inGameNickname: 'Alfa', rosterSlot: 'Main' },
          { id: 'u2', displayName: 'Bravo', inGameNickname: 'Bravo', rosterSlot: 'Main' },
        ]),
        { status: 200 },
      )
    }
    const side = url.includes('/CT') ? 'CT' : 'T'
    const body = url.includes('text-annotations') ? annotations[side] : data[side]
    return new Response(JSON.stringify(body), { status: 200 })
  })
  vi.stubGlobal('fetch', fetchMock)
  return fetchMock
}

function writes(fetchMock: ReturnType<typeof stubFetch>) {
  return fetchMock.mock.calls
    .filter(([, init]) => init?.method === 'POST')
    .map(([, init]) => JSON.parse(String(init?.body)) as Record<string, unknown>)
}

describe('MapRadarView (shared radar)', () => {
  beforeEach(() => {
    localStorage.clear()
    useAuthStore.setState({ role: 'Manager', isCoach: false })
    Element.prototype.setPointerCapture = vi.fn()
    Element.prototype.getBoundingClientRect = () =>
      ({ left: 0, top: 0, width: 1000, height: 500, right: 1000, bottom: 500, x: 0, y: 0, toJSON: () => ({}) }) as DOMRect
  })
  afterEach(() => useAuthStore.setState({ role: null, isCoach: false }))

  it('renders both sides on one radar with side markers and a legend', async () => {
    stubFetch()
    renderWithProviders(<MapRadarView mapName="Mirage" />)

    expect(await screen.findByTestId('side-badge-t1')).toHaveTextContent('T')
    expect(screen.getByTestId('side-badge-c1')).toHaveTextContent('CT')
    expect(screen.getByText('Smoke tu')).toBeInTheDocument()
    expect(within(screen.getByLabelText('Legenda')).getByText('Alfa')).toBeInTheDocument()
    expect(within(screen.getByLabelText('Legenda')).getByText('Bravo')).toBeInTheDocument()
  })

  it('filters pins by side and remembers the choice', async () => {
    stubFetch()
    renderWithProviders(<MapRadarView mapName="Mirage" />)
    await screen.findByTestId('side-badge-t1')

    await userEvent.click(within(screen.getByRole('radiogroup', { name: 'Pokaż' })).getByRole('radio', { name: 'CT' }))

    expect(screen.queryByTestId('side-badge-t1')).not.toBeInTheDocument()
    expect(screen.getByTestId('side-badge-c1')).toBeInTheDocument()
    expect(localStorage.getItem(MAP_STRATEGY_SETTINGS.sideFilterStorageKey)).toBe('CT')
  })

  it('restores the stored filter', async () => {
    localStorage.setItem(MAP_STRATEGY_SETTINGS.sideFilterStorageKey, 'T')
    stubFetch()
    renderWithProviders(<MapRadarView mapName="Mirage" />)

    expect(await screen.findByTestId('side-badge-t1')).toBeInTheDocument()
    expect(screen.queryByTestId('side-badge-c1')).not.toBeInTheDocument()
  })

  it('saves a new placement on the side chosen in "Ustawiasz"', async () => {
    const fetchMock = stubFetch()
    renderWithProviders(<MapRadarView mapName="Mirage" />)
    await screen.findByTestId('side-badge-t1')

    // Alfa is already on T, but not on CT, so she is selectable while placing on CT.
    await userEvent.click(within(screen.getByRole('radiogroup', { name: 'Ustawiasz' })).getByRole('radio', { name: 'CT' }))
    await userEvent.selectOptions(await screen.findByRole('combobox'), 'u1')
    await userEvent.click(screen.getByRole('button', { name: 'Dodaj pozycję' }))
    await waitFor(() => expect(writes(fetchMock).some((body) => body.userId === 'u1')).toBe(true))
    expect(writes(fetchMock).find((body) => body.userId === 'u1')).toMatchObject({ side: 'CT' })

    await userEvent.click(within(screen.getByRole('radiogroup', { name: 'Ustawiasz' })).getByRole('radio', { name: 'T' }))
    await userEvent.selectOptions(await screen.findByRole('combobox'), 'u2')
    await userEvent.click(screen.getByRole('button', { name: 'Dodaj pozycję' }))
    await waitFor(() => expect(writes(fetchMock).some((body) => body.userId === 'u2')).toBe(true))
    expect(writes(fetchMock).find((body) => body.userId === 'u2')).toMatchObject({ side: 'T' })
  })

  it('saves a drag on the pin own side regardless of the editing control', async () => {
    const fetchMock = stubFetch()
    renderWithProviders(<MapRadarView mapName="Mirage" />)
    const tPin = (await screen.findByTestId('side-badge-t1')).parentElement as HTMLElement
    const handle = within(tPin).getByTitle(/Alfa/)

    // The control says CT (default) but the dragged pin lives on T.
    fireEvent.pointerDown(handle, { pointerId: 1, clientX: 200, clientY: 100 })
    fireEvent.pointerMove(handle, { pointerId: 1, clientX: 500, clientY: 250 })
    fireEvent.pointerUp(handle, { pointerId: 1 })

    await waitFor(() => expect(writes(fetchMock).length).toBe(1))
    expect(writes(fetchMock)[0]).toMatchObject({ side: 'T', userId: 'u1', x: 0.5, y: 0.5 })
  })
})
