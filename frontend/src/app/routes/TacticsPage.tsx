import { useState } from 'react'
import { useIsCoachOrManager } from '../../features/auth/hooks/useIsCoachOrManager'
import { AddTacticForm } from '../../features/tactics/components/AddTacticForm'
import { TacticEditor } from '../../features/tactics/components/TacticEditor'
import { TacticList } from '../../features/tactics/components/TacticList'

export function TacticsPage() {
  const [selectedTacticId, setSelectedTacticId] = useState<string | null>(null)
  const canEdit = useIsCoachOrManager()

  return (
    <>
      <h1 className="text-2xl font-semibold">Taktyki</h1>

      {selectedTacticId ? (
        <TacticEditor
          key={selectedTacticId}
          tacticId={selectedTacticId}
          onClose={() => setSelectedTacticId(null)}
        />
      ) : (
        <>
          {canEdit && <AddTacticForm onCreated={setSelectedTacticId} />}
          <TacticList onSelect={setSelectedTacticId} />
        </>
      )}
    </>
  )
}
