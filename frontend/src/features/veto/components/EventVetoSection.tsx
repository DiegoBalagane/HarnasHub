import { useState } from 'react'
import { Link } from 'react-router-dom'
import { Modal } from '../../../components/Modal'
import { useIsCoachOrManager } from '../../auth/hooks/useIsCoachOrManager'
import { opponentProfilePath } from '../../opponents/paths'
import { useEventVeto } from '../hooks/useVeto'
import { vetoActionClasses, vetoActionLabels, vetoActorLabels } from '../labels'
import { EventVetoEditor } from './EventVetoEditor'

interface EventVetoSectionProps {
  eventId: string
  opponent: string
}

/** The veto of a match-like event shown as a row of chips, with a Coach/Manager button to record or edit it. */
export function EventVetoSection({ eventId, opponent }: EventVetoSectionProps) {
  const canManage = useIsCoachOrManager()
  const [isEditing, setIsEditing] = useState(false)
  const { data: steps, isLoading, isError } = useEventVeto(eventId)

  return (
    <div className="flex flex-col gap-2 rounded-md border border-neutral-800 p-3">
      <div className="flex flex-wrap items-center justify-between gap-2">
        <h3 className="text-sm font-medium">Veto</h3>
        <div className="flex items-center gap-3 text-xs">
          <Link to={opponentProfilePath(opponent)} className="text-primary-400 hover:underline">
            Sugestia asystenta →
          </Link>
          {canManage && steps && (
            <button
              type="button"
              onClick={() => setIsEditing(true)}
              className="text-neutral-400 hover:text-neutral-200"
            >
              {steps.length === 0 ? 'Zapisz veto' : 'Edytuj veto'}
            </button>
          )}
        </div>
      </div>

      {isLoading && <p className="text-xs text-neutral-500">Ładowanie veto…</p>}
      {isError && <p className="text-xs text-danger-400">Nie udało się pobrać veto.</p>}
      {steps?.length === 0 && <p className="text-xs text-neutral-500">Veto nie zostało jeszcze zapisane.</p>}

      {steps && steps.length > 0 && (
        <ol className="flex flex-wrap gap-1.5">
          {steps.map((step) => (
            <li
              key={step.order}
              className={`rounded-full border px-2 py-0.5 text-xs ${vetoActionClasses[step.action]}`}
            >
              {step.order}. {vetoActorLabels[step.actor]} {vetoActionLabels[step.action]} {step.mapName}
            </li>
          ))}
        </ol>
      )}

      {isEditing && steps && (
        <Modal title={`Veto vs ${opponent}`} onClose={() => setIsEditing(false)}>
          <EventVetoEditor eventId={eventId} initialSteps={steps} onDone={() => setIsEditing(false)} />
        </Modal>
      )}
    </div>
  )
}
