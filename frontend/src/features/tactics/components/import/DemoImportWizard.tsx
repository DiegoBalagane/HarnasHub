import { useCallback, useMemo, useState } from 'react'
import type { MapSide } from '../../../../services/mapStrategyApi'
import type { DemoNades } from '../../../../services/tacticsApi'
import { DemoImportSaveForm } from './DemoImportSaveForm'
import { DemoNadeRadarPreview } from './DemoNadeRadarPreview'
import { DemoNadeSelectionList } from './DemoNadeSelectionList'
import { DemoRoundPicker } from './DemoRoundPicker'
import { DemoUploadStep } from './DemoUploadStep'
import { grenadesForSide, toImportInputs } from './demoImport'

interface DemoImportWizardProps {
  /** Called with the new tactic's id once it has been saved. */
  onImported: (tacticId: string) => void
}

/** Import-tactic-from-demo wizard: upload → pick round/side and grenades on the radar → name and save. */
export function DemoImportWizard({ onImported }: DemoImportWizardProps) {
  const [demo, setDemo] = useState<DemoNades | null>(null)
  const [roundNumber, setRoundNumber] = useState(1)
  const [side, setSide] = useState<MapSide>('T')
  const [selectedIds, setSelectedIds] = useState<ReadonlySet<number>>(new Set())
  const [isSaving, setIsSaving] = useState(false)

  const round = demo?.rounds.find((candidate) => candidate.number === roundNumber)
  const grenades = useMemo(() => grenadesForSide(round, side), [round, side])

  const selectRoundAndSide = useCallback(
    (nextDemo: DemoNades, nextRound: number, nextSide: MapSide) => {
      setRoundNumber(nextRound)
      setSide(nextSide)
      const nextGrenades = grenadesForSide(nextDemo.rounds.find((r) => r.number === nextRound), nextSide)
      setSelectedIds(new Set(nextGrenades.map((grenade) => grenade.id)))
    },
    [],
  )

  const toggle = useCallback((ids: number[], selected: boolean) => {
    setSelectedIds((current) => {
      const next = new Set(current)
      for (const id of ids) {
        if (selected) next.add(id)
        else next.delete(id)
      }
      return next
    })
  }, [])

  if (!demo) {
    return (
      <DemoUploadStep
        onExtracted={(extracted) => {
          setDemo(extracted)
          selectRoundAndSide(extracted, extracted.rounds[0]?.number ?? 1, 'T')
        }}
      />
    )
  }

  if (demo.rounds.length === 0) {
    return <p className="text-sm text-neutral-400">W tej demce nie znaleziono żadnej rundy meczowej.</p>
  }

  if (isSaving) {
    return (
      <DemoImportSaveForm
        mapName={demo.mapName}
        side={side}
        roundNumber={roundNumber}
        grenades={toImportInputs(grenades, selectedIds)}
        onBack={() => setIsSaving(false)}
        onSaved={onImported}
      />
    )
  }

  const selectedCount = grenades.filter((grenade) => selectedIds.has(grenade.id)).length

  return (
    <div className="flex flex-col gap-3">
      <div className="grid gap-3 md:grid-cols-[14rem_1fr_16rem]">
        <DemoRoundPicker
          rounds={demo.rounds}
          selectedRound={roundNumber}
          side={side}
          onRoundChange={(number) => selectRoundAndSide(demo, number, side)}
          onSideChange={(nextSide) => selectRoundAndSide(demo, roundNumber, nextSide)}
        />
        <DemoNadeRadarPreview mapName={demo.mapName} grenades={grenades} selectedIds={selectedIds} />
        <DemoNadeSelectionList grenades={grenades} selectedIds={selectedIds} onToggle={toggle} />
      </div>

      <button
        type="button"
        disabled={selectedCount === 0}
        onClick={() => setIsSaving(true)}
        className="self-end rounded-md bg-primary-500 px-4 py-2 text-sm font-medium text-primary-950 transition hover:bg-primary-400 disabled:opacity-50"
      >
        Dalej ({selectedCount})
      </button>
    </div>
  )
}
