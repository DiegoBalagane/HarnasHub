import { useCallback, useEffect, useState } from 'react'
import { ConfirmDialog } from './ConfirmDialog'
import { ModalGuardContext, type ModalGuardState } from './ModalGuardContext'

interface ModalProps {
  title: string
  onClose: () => void
  children: React.ReactNode
  /** Wider content (e.g. a drawing canvas) needs more than the default form-sized max width. */
  wide?: boolean
  /** Near full-screen width for content that benefits from every pixel, such as the 2D round replay. */
  extraWide?: boolean
}

/** Generic popup overlay for a form/content block — click outside, Escape or ✕ close it, unless a form inside reports
 * (via `useModalGuard`) that it is busy or dirty: then closing first asks for confirmation instead of losing the work. */
export function Modal({ title, onClose, children, wide, extraWide }: ModalProps) {
  const [guards, setGuards] = useState<Record<string, ModalGuardState>>({})
  const [isConfirming, setIsConfirming] = useState(false)
  const reporters = Object.values(guards)
  const isBusy = reporters.some((state) => state.isBusy)
  const needsConfirm = isBusy || reporters.some((state) => state.isDirty)
  const report = useCallback((key: string, state: ModalGuardState | null) => {
    setGuards((current) => {
      const { [key]: _removed, ...rest } = current
      return state ? { ...rest, [key]: state } : rest
    })
  }, [])

  const requestClose = useCallback(() => {
    if (needsConfirm) setIsConfirming(true)
    else onClose()
  }, [needsConfirm, onClose])

  useEffect(() => {
    function handleKeyDown(event: KeyboardEvent) {
      if (event.key === 'Escape' && !isConfirming) requestClose()
    }

    document.addEventListener('keydown', handleKeyDown)
    return () => document.removeEventListener('keydown', handleKeyDown)
  }, [requestClose, isConfirming])

  function confirmClose() {
    if (!isBusy) reporters.forEach((state) => state.onDiscard?.())
    setIsConfirming(false)
    onClose()
  }

  return (
    <ModalGuardContext.Provider value={report}>
      <div
        role="presentation"
        className="fixed inset-0 z-50 flex items-center justify-center overflow-y-auto bg-black/70 p-4"
        onClick={requestClose}
      >
        <div
          role="dialog"
          aria-modal="true"
          aria-labelledby="modal-title"
          onClick={(event) => event.stopPropagation()}
          className={`w-full ${extraWide ? 'max-w-7xl' : wide ? 'max-w-4xl' : 'max-w-xl'} rounded-lg border border-neutral-800 bg-neutral-950 p-5 shadow-xl`}
        >
          <div className="mb-4 flex items-center justify-between">
            <h2 id="modal-title" className="text-base font-semibold text-white">
              {title}
            </h2>
            <button
              type="button"
              onClick={requestClose}
              title="Zamknij"
              className="text-neutral-500 transition hover:text-neutral-200"
            >
              ✕
            </button>
          </div>

          {children}
        </div>
      </div>

      {isConfirming && (
        <ConfirmDialog
          title="Porzucić wpisane dane?"
          message={
            isBusy
              ? 'Trwa wgrywanie lub analiza. Zamknięcie okna nie przerwie zadania — jego wynik wróci do formularza po ponownym otwarciu.'
              : 'Formularz zawiera niezapisane dane. Zamknięcie okna je utraci.'
          }
          confirmLabel={isBusy ? 'Zamknij okno' : 'Porzuć'}
          cancelLabel="Wróć do formularza"
          isDanger={!isBusy}
          onConfirm={confirmClose}
          onCancel={() => setIsConfirming(false)}
        />
      )}
    </ModalGuardContext.Provider>
  )
}
