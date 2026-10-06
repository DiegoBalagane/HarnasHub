import { act, render } from '@testing-library/react'
import { MemoryRouter, useNavigate } from 'react-router-dom'
import { beforeEach, describe, expect, it, vi } from 'vitest'
import { PwaUpdatePrompt } from './PwaUpdatePrompt'

const updateServiceWorker = vi.fn()
let needRefresh = false

vi.mock('virtual:pwa-register/react', () => ({
  useRegisterSW: () => ({ needRefresh: [needRefresh, vi.fn()], updateServiceWorker }),
}))

let navigate: (to: string) => void = () => {}
function NavigateCapture() {
  navigate = useNavigate()
  return null
}

function renderPrompt() {
  return render(
    <MemoryRouter initialEntries={['/dashboard']}>
      <NavigateCapture />
      <PwaUpdatePrompt />
    </MemoryRouter>,
  )
}

describe('PwaUpdatePrompt', () => {
  beforeEach(() => {
    updateServiceWorker.mockReset()
    needRefresh = false
    vi.useRealTimers()
  })

  it('applies an update found right after opening without asking', () => {
    needRefresh = true
    renderPrompt()

    expect(updateServiceWorker).toHaveBeenCalledWith(true)
  })

  it('waits with a later update until the next navigation', () => {
    vi.useFakeTimers()
    const view = renderPrompt()
    vi.advanceTimersByTime(60_000)

    needRefresh = true
    view.rerender(
      <MemoryRouter initialEntries={['/dashboard']}>
        <NavigateCapture />
        <PwaUpdatePrompt />
      </MemoryRouter>,
    )
    expect(updateServiceWorker).not.toHaveBeenCalled()

    act(() => navigate('/calendar'))
    expect(updateServiceWorker).toHaveBeenCalledWith(true)
  })

  it('applies a later update when the tab goes to the background', () => {
    vi.useFakeTimers()
    const view = renderPrompt()
    vi.advanceTimersByTime(60_000)
    needRefresh = true
    view.rerender(
      <MemoryRouter initialEntries={['/dashboard']}>
        <NavigateCapture />
        <PwaUpdatePrompt />
      </MemoryRouter>,
    )

    Object.defineProperty(document, 'visibilityState', { value: 'hidden', configurable: true })
    act(() => {
      document.dispatchEvent(new Event('visibilitychange'))
    })

    expect(updateServiceWorker).toHaveBeenCalledWith(true)
    Object.defineProperty(document, 'visibilityState', { value: 'visible', configurable: true })
  })
})
