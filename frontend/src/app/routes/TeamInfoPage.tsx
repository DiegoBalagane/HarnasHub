import { useState } from 'react'
import { Button } from '../../components/ui/Button'
import { PageHeader } from '../../components/ui/PageHeader'
import { useIsCoachOrManager } from '../../features/auth/hooks/useIsCoachOrManager'
import { TeamInfoBoard } from '../../features/team-info/components/TeamInfoBoard'
import type { TeamInfoEntry } from '../../services/teamInfoApi'

/** Team info page: constant technical facts (Discord, servers, configs) visible to every member; Coach/Manager can edit them. */
export function TeamInfoPage() {
  const canManage = useIsCoachOrManager()
  const [isAdding, setIsAdding] = useState(false)
  const [editing, setEditing] = useState<TeamInfoEntry | null>(null)

  function closeForm() {
    setIsAdding(false)
    setEditing(null)
  }

  return (
    <>
      <PageHeader title="Info" actions={canManage && <Button onClick={() => setIsAdding(true)}>+ Dodaj wpis</Button>} />
      <TeamInfoBoard canManage={canManage} editing={editing} isAdding={isAdding} onCloseForm={closeForm} onEdit={setEditing} />
    </>
  )
}
