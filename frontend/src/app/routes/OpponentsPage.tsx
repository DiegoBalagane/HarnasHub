import { PageHeader } from '../../components/ui/PageHeader'
import { Button } from '../../components/ui/Button'
import { useState } from 'react'
import { Modal } from '../../components/Modal'
import { useIsCoachOrManager } from '../../features/auth/hooks/useIsCoachOrManager'
import { AddOpponentForm } from '../../features/opponents/components/AddOpponentForm'
import { OpponentList } from '../../features/opponents/components/OpponentList'

export function OpponentsPage() {
  const canManage = useIsCoachOrManager()
  const [isAdding, setIsAdding] = useState(false)

  return (
    <>
      <PageHeader
        title="Przeciwnicy"
        actions={canManage && <Button onClick={() => setIsAdding(true)}>+ Dodaj przeciwnika</Button>}
      />

      <OpponentList />

      {isAdding && (
        <Modal title="Dodaj przeciwnika" onClose={() => setIsAdding(false)}>
          <AddOpponentForm onDone={() => setIsAdding(false)} />
        </Modal>
      )}
    </>
  )
}
