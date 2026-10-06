import { useState } from 'react'
import type { MapName } from '../../../services/nadesApi'
import type { VetoAction, VetoActor, VetoStep, VetoStepInput } from '../../../services/vetoApi'
import { mapNames } from '../../nades/labels'
import { useSetEventVeto } from '../hooks/useVeto'
import { vetoActionLabels, vetoActions, vetoActorLabels, vetoActors } from '../labels'

const selectClass =
  'rounded-md border border-neutral-800 bg-neutral-900 px-2 py-1.5 text-sm outline-none focus:border-neutral-500'

interface EventVetoEditorProps {
  eventId: string
  initialSteps: VetoStep[]
  /** Called after a successful save — e.g. to close the hosting modal. */
  onDone: () => void
}

/** Coach/Manager editor for the veto of one event: an ordered list of "who / ban-pick-decider / map" rows, saved at once. */
export function EventVetoEditor({ eventId, initialSteps, onDone }: EventVetoEditorProps) {
  const [steps, setSteps] = useState<VetoStepInput[]>(() =>
    initialSteps.map(({ actor, action, mapName }) => ({ actor, action, mapName })),
  )
  const setEventVeto = useSetEventVeto()
  const unusedMaps = mapNames.filter((map) => !steps.some((step) => step.mapName === map))
  const hasDeciderBeforeEnd = steps.slice(0, -1).some((step) => step.action === 'Decider')

  function updateStep(index: number, patch: Partial<VetoStepInput>) {
    setSteps((current) => current.map((step, i) => (i === index ? { ...step, ...patch } : step)))
  }

  function addStep() {
    const previous = steps.at(-1)
    // Teams alternate, and the single map left once the rest are gone is the decider.
    const actor: VetoActor = previous?.actor === 'Us' ? 'Opponent' : 'Us'
    const action: VetoAction = unusedMaps.length === 1 ? 'Decider' : 'Ban'
    setSteps((current) => [...current, { actor, action, mapName: unusedMaps[0] }])
  }

  return (
    <div className="flex flex-col gap-3">
      {steps.length === 0 && <p className="text-sm text-neutral-500">Brak kroków — dodaj pierwszy ban.</p>}

      <ol className="flex flex-col gap-2">
        {steps.map((step, index) => (
          <li key={index} className="flex flex-wrap items-center gap-2">
            <span className="w-5 text-right text-sm tabular-nums text-neutral-500">{index + 1}.</span>
            <select
              aria-label="Kto"
              value={step.actor}
              onChange={(event) => updateStep(index, { actor: event.target.value as VetoActor })}
              className={selectClass}
            >
              {vetoActors.map((actor) => (
                <option key={actor} value={actor}>
                  {vetoActorLabels[actor]}
                </option>
              ))}
            </select>
            <select
              aria-label="Akcja"
              value={step.action}
              onChange={(event) => updateStep(index, { action: event.target.value as VetoAction })}
              className={selectClass}
            >
              {vetoActions.map((action) => (
                <option key={action} value={action}>
                  {vetoActionLabels[action]}
                </option>
              ))}
            </select>
            <select
              aria-label="Mapa"
              value={step.mapName}
              onChange={(event) => updateStep(index, { mapName: event.target.value as MapName })}
              className={selectClass}
            >
              {[step.mapName, ...unusedMaps].sort().map((map) => (
                <option key={map} value={map}>
                  {map}
                </option>
              ))}
            </select>
            <button
              type="button"
              title="Usuń krok"
              onClick={() => setSteps((current) => current.filter((_, i) => i !== index))}
              className="rounded-md px-2 py-1 text-sm text-neutral-500 transition hover:bg-neutral-800 hover:text-danger-400"
            >
              ✕
            </button>
          </li>
        ))}
      </ol>

      {unusedMaps.length > 0 && (
        <button
          type="button"
          onClick={addStep}
          className="self-start rounded-md border border-neutral-700 px-3 py-1.5 text-sm text-neutral-300 transition hover:border-neutral-500"
        >
          + Dodaj krok
        </button>
      )}

      {hasDeciderBeforeEnd && (
        <p className="text-sm text-warning-400">Decider może być tylko ostatnim krokiem.</p>
      )}
      {setEventVeto.isError && <p className="text-sm text-danger-400">Nie udało się zapisać veto.</p>}

      <button
        type="button"
        disabled={setEventVeto.isPending || hasDeciderBeforeEnd}
        onClick={() => setEventVeto.mutate({ eventId, steps }, { onSuccess: onDone })}
        className="self-start rounded-md bg-primary-500 px-4 py-2 text-sm font-medium text-primary-950 transition hover:bg-primary-400 disabled:opacity-50"
      >
        {setEventVeto.isPending ? 'Zapisywanie…' : 'Zapisz veto'}
      </button>
    </div>
  )
}
