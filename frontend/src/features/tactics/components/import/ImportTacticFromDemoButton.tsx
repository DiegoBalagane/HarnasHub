import { useCallback, useState } from 'react'
import { Modal } from '../../../../components/Modal'
import { DemoImportWizard } from './DemoImportWizard'

interface ImportTacticFromDemoButtonProps {
  /** Called with the imported tactic's id, e.g. to open it in the editor. */
  onImported: (tacticId: string) => void
}

/** "Importuj z demki" button opening the import wizard in a wide modal. */
export function ImportTacticFromDemoButton({ onImported }: ImportTacticFromDemoButtonProps) {
  const [isOpen, setIsOpen] = useState(false)
  const close = useCallback(() => setIsOpen(false), [])

  return (
    <>
      <button
        type="button"
        onClick={() => setIsOpen(true)}
        className="self-start rounded-md border border-neutral-700 px-4 py-2 text-sm font-medium text-neutral-100 transition hover:border-neutral-500"
      >
        Importuj z demki
      </button>

      {isOpen && (
        <Modal title="Import taktyki z demki" onClose={close} wide>
          <DemoImportWizard
            onImported={(tacticId) => {
              close()
              onImported(tacticId)
            }}
          />
        </Modal>
      )}
    </>
  )
}
