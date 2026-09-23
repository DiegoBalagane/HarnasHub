import { useEffect } from 'react'

interface ModalProps {
  title: string
  onClose: () => void
  children: React.ReactNode
  /** Wider content (e.g. a drawing canvas) needs more than the default form-sized max width. */
  wide?: boolean
}

/** Generic popup overlay for a form/content block — click outside or Escape to close. */
export function Modal({ title, onClose, children, wide }: ModalProps) {
  useEffect(() => {
    function handleKeyDown(event: KeyboardEvent) {
      if (event.key === 'Escape') onClose()
    }

    document.addEventListener('keydown', handleKeyDown)
    return () => document.removeEventListener('keydown', handleKeyDown)
  }, [onClose])

  return (
    <div
      role="presentation"
      className="fixed inset-0 z-50 flex items-center justify-center overflow-y-auto bg-black/70 p-4"
      onClick={onClose}
    >
      <div
        role="dialog"
        aria-modal="true"
        aria-labelledby="modal-title"
        onClick={(event) => event.stopPropagation()}
        className={`w-full ${wide ? 'max-w-4xl' : 'max-w-xl'} rounded-lg border border-neutral-800 bg-neutral-950 p-5 shadow-xl`}
      >
        <div className="mb-4 flex items-center justify-between">
          <h2 id="modal-title" className="text-base font-semibold text-white">
            {title}
          </h2>
          <button
            type="button"
            onClick={onClose}
            title="Zamknij"
            className="text-neutral-500 transition hover:text-neutral-200"
          >
            ✕
          </button>
        </div>

        {children}
      </div>
    </div>
  )
}
