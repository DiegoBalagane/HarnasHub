import { PageHeader } from '../../components/ui/PageHeader'
import { Button } from '../../components/ui/Button'
import { useState } from 'react'
import { Modal } from '../../components/Modal'
import { useIsCoachOrManager } from '../../features/auth/hooks/useIsCoachOrManager'
import { OpponentList } from '../../features/opponents/components/OpponentList'
import { OpponentNoteForm } from '../../features/opponents/components/OpponentNoteForm'

export function OpponentsPage() {
  const canManage = useIsCoachOrManager()
  const [isAdding, setIsAdding] = useState(false)

  return (
    <>
      <PageHeader
        title="Przeciwnicy"
        actions={canManage && <Button onClick={() => setIsAdding(true)}>+ Dodaj notatkę</Button>}
      />

      <OpponentList />

      {isAdding && (
        <Modal title="Dodaj notatkę o przeciwniku" onClose={() => setIsAdding(false)}>
          <OpponentNoteForm onDone={() => setIsAdding(false)} />
        </Modal>
      )}
    </>
  )
}
