import { useEffect } from 'react'

interface ConfirmDialogProps {
  title: string
  message: React.ReactNode
  confirmLabel: string
  cancelLabel?: string
  isConfirming?: boolean
  isDanger?: boolean
  onConfirm: () => void
  onCancel: () => void
}

/** Modal overlay that blocks the page until the user explicitly confirms or cancels a destructive action. */
export function ConfirmDialog({
  title,
  message,
  confirmLabel,
  cancelLabel = 'Anuluj',
  isConfirming = false,
  isDanger = true,
  onConfirm,
  onCancel,
}: ConfirmDialogProps) {
  useEffect(() => {
    function handleKeyDown(event: KeyboardEvent) {
      if (event.key === 'Escape') onCancel()
    }

    document.addEventListener('keydown', handleKeyDown)
    return () => document.removeEventListener('keydown', handleKeyDown)
  }, [onCancel])

  return (
    <div
      role="presentation"
      className="fixed inset-0 z-50 flex items-center justify-center bg-black/70 p-4"
      onClick={onCancel}
    >
      <div
        role="alertdialog"
        aria-modal="true"
        aria-labelledby="confirm-dialog-title"
        onClick={(event) => event.stopPropagation()}
        className="w-full max-w-sm rounded-lg border border-neutral-800 bg-neutral-950 p-5 shadow-xl"
      >
        <h2 id="confirm-dialog-title" className="text-base font-semibold text-white">
          {title}
        </h2>
        <div className="mt-2 text-sm text-neutral-300">{message}</div>

        <div className="mt-5 flex justify-end gap-2">
          <button
            type="button"
            onClick={onCancel}
            disabled={isConfirming}
            className="rounded-md border border-neutral-700 px-3 py-1.5 text-sm text-neutral-300 transition hover:border-neutral-500 disabled:opacity-50"
          >
            {cancelLabel}
          </button>
          <button
            type="button"
            onClick={onConfirm}
            disabled={isConfirming}
            className={`rounded-md px-3 py-1.5 text-sm font-medium text-white transition disabled:opacity-50 ${
              isDanger ? 'bg-red-600 hover:bg-red-500' : 'bg-neutral-700 hover:bg-neutral-600'
            }`}
          >
            {isConfirming ? 'Usuwanie…' : confirmLabel}
          </button>
        </div>
      </div>
    </div>
  )
}
