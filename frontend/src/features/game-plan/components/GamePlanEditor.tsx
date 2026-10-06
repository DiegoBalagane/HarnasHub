import { useMemo, useState } from 'react'
import type { EventGamePlan } from '../../../services/gamePlanApi'
import type { MapName } from '../../../services/nadesApi'
import { useAnalysisBoards } from '../../analysis-boards/hooks/useAnalysisBoards'
import { mapSideLabels } from '../../map-strategy/labels'
import { mapNames } from '../../nades/labels'
import { useTactics } from '../../tactics/hooks/useTactics'
import { useSetGamePlan } from '../hooks/useGamePlan'

const inputClass =
  'rounded-md border border-neutral-800 bg-neutral-900 px-3 py-2 text-sm outline-none focus:border-neutral-500'

interface GamePlanEditorProps {
  plan: EventGamePlan
  /** Called after a successful save — e.g. to close the hosting modal. */
  onDone: () => void
}

/** Toggles an id in an ordered selection: removes it if present, otherwise appends it (click order = display order). */
function toggle(ids: string[], id: string): string[] {
  return ids.includes(id) ? ids.filter((current) => current !== id) : [...ids, id]
}

/** Coach/Manager editor for an event's plan: free-text notes plus tactics and analysis boards picked from the libraries. */
export function GamePlanEditor({ plan, onDone }: GamePlanEditorProps) {
  const [notes, setNotes] = useState(plan.notes ?? '')
  const [tacticIds, setTacticIds] = useState(plan.tactics.map((t) => t.id))
  const [boardIds, setBoardIds] = useState(plan.boards.map((b) => b.id))
  const [mapFilter, setMapFilter] = useState<MapName | ''>(plan.tactics[0]?.mapName ?? '')
  const { data: tactics } = useTactics({})
  const { data: boards } = useAnalysisBoards()
  const setGamePlan = useSetGamePlan()

  const visibleTactics = useMemo(
    () => tactics?.filter((t) => !mapFilter || t.mapName === mapFilter || tacticIds.includes(t.id)),
    [tactics, mapFilter, tacticIds],
  )
  const visibleBoards = useMemo(
    () => boards?.filter((b) => !mapFilter || b.mapName === mapFilter || boardIds.includes(b.id)),
    [boards, mapFilter, boardIds],
  )

  function handleSave() {
    setGamePlan.mutate(
      { eventId: plan.eventId, payload: { notes: notes.trim() ? notes : null, tacticIds, boardIds } },
      { onSuccess: onDone },
    )
  }

  return (
    <div className="flex flex-col gap-4">
      <textarea
        rows={6}
        maxLength={4000}
        placeholder="Plan: pistolety, ekonomia, na co uważać, kto co robi…"
        value={notes}
        onChange={(event) => setNotes(event.target.value)}
        className={inputClass}
      />

      <select
        aria-label="Filtr mapy"
        value={mapFilter}
        onChange={(event) => setMapFilter(event.target.value as MapName | '')}
        className={`self-start ${inputClass}`}
      >
        <option value="">Wszystkie mapy</option>
        {mapNames.map((map) => (
          <option key={map} value={map}>
            {map}
          </option>
        ))}
      </select>

      <fieldset className="flex flex-col gap-1">
        <legend className="mb-1 text-sm font-medium">Taktyki ({tacticIds.length})</legend>
        {visibleTactics?.length === 0 && (
          <p className="text-xs text-neutral-500">Brak taktyk dla tej mapy.</p>
        )}
        <div className="flex max-h-48 flex-col gap-1 overflow-y-auto">
          {visibleTactics?.map((tactic) => (
            <label key={tactic.id} className="flex items-center gap-2 text-sm">
              <input
                type="checkbox"
                checked={tacticIds.includes(tactic.id)}
                onChange={() => setTacticIds((current) => toggle(current, tactic.id))}
              />
              {tactic.name}
              <span className="text-xs text-neutral-500">
                {tactic.mapName} · {mapSideLabels[tactic.side]}
              </span>
            </label>
          ))}
        </div>
      </fieldset>

      <fieldset className="flex flex-col gap-1">
        <legend className="mb-1 text-sm font-medium">Tablice analiz ({boardIds.length})</legend>
        {visibleBoards?.length === 0 && <p className="text-xs text-neutral-500">Brak tablic dla tej mapy.</p>}
        <div className="flex max-h-48 flex-col gap-1 overflow-y-auto">
          {visibleBoards?.map((board) => (
            <label key={board.id} className="flex items-center gap-2 text-sm">
              <input
                type="checkbox"
                checked={boardIds.includes(board.id)}
                onChange={() => setBoardIds((current) => toggle(current, board.id))}
              />
              {board.title}
              <span className="text-xs text-neutral-500">{board.mapName}</span>
            </label>
          ))}
        </div>
      </fieldset>

      {setGamePlan.isError && <p className="text-sm text-danger-400">Nie udało się zapisać planu.</p>}

      <button
        type="button"
        disabled={setGamePlan.isPending}
        onClick={handleSave}
        className="self-start rounded-md bg-primary-500 px-4 py-2 text-sm font-medium text-primary-950 transition hover:bg-primary-400 disabled:opacity-50"
      >
        {setGamePlan.isPending ? 'Zapisywanie…' : 'Zapisz plan'}
      </button>
    </div>
  )
}
