import { useCallback, useState, type ReactNode } from 'react'
import { Modal } from './Modal'
import { Button } from './ui/Button'

interface AddFormModalProps {
  /** Text on the opening button, e.g. "+ Dodaj wynik". */
  buttonLabel: string
  /** Modal heading. */
  title: string
  /** Receives a `close` callback the form should call after a successful save. */
  children: (close: () => void) => ReactNode
  wide?: boolean
}

/** A "+ Add…" button that opens the given form in a modal, keeping lists at the top of the page. */
export function AddFormModal({ buttonLabel, title, children, wide }: AddFormModalProps) {
  const [isOpen, setIsOpen] = useState(false)
  const close = useCallback(() => setIsOpen(false), [])

  return (
    <>
      <Button onClick={() => setIsOpen(true)}>{buttonLabel}</Button>

      {isOpen && (
        <Modal title={title} onClose={close} wide={wide}>
          {children(close)}
        </Modal>
      )}
    </>
  )
}
