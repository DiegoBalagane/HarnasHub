import { useState } from 'react'
import { useSearchParams } from 'react-router-dom'
import { AddFormModal } from '../../../components/AddFormModal'
import type { MapName } from '../../../services/nadesApi'
import { useIsCoachOrManager } from '../../auth/hooks/useIsCoachOrManager'
import { AddTacticForm } from './AddTacticForm'
import { ImportTacticFromDemoButton } from './import/ImportTacticFromDemoButton'
import { TacticEditor } from './TacticEditor'
import { TacticList } from './TacticList'

interface TacticsTabProps {
  /** Map picked on the Playbook page; undefined shows tactics of every map. */
  mapName?: MapName
}

/** Playbook "Taktyki" tab — the single place where the tactics UI is composed (list, add button, editor, deep link). */
export function TacticsTab({ mapName }: TacticsTabProps) {
  const [pickedTacticId, setSelectedTacticId] = useState<string | null>(null)
  const canEdit = useIsCoachOrManager()
  const [searchParams, setSearchParams] = useSearchParams()
  // A ?tactic= deep link (e.g. from an event's game plan) opens that tactic until the user picks or closes one.
  const selectedTacticId = pickedTacticId ?? searchParams.get('tactic')

  function closeTactic() {
    setSelectedTacticId(null)
    if (searchParams.has('tactic')) {
      const nextParams = new URLSearchParams(searchParams)
      nextParams.delete('tactic')
      setSearchParams(nextParams, { replace: true })
    }
  }

  if (selectedTacticId) {
    return <TacticEditor key={selectedTacticId} tacticId={selectedTacticId} onClose={closeTactic} />
  }

  return (
    <>
      {canEdit && (
        <div className="flex flex-wrap gap-2">
          <AddFormModal buttonLabel="+ Nowa taktyka" title="Nowa taktyka">
            {(close) => (
              <AddTacticForm
                defaultMapName={mapName}
                onCreated={(tacticId) => {
                  close()
                  setSelectedTacticId(tacticId)
                }}
              />
            )}
          </AddFormModal>
          <ImportTacticFromDemoButton onImported={setSelectedTacticId} />
        </div>
      )}
      <TacticList onSelect={setSelectedTacticId} mapName={mapName} />
    </>
  )
}
