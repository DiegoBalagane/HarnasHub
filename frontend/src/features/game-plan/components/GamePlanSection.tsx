import { useState } from 'react'
import { Link } from 'react-router-dom'
import { Modal } from '../../../components/Modal'
import { useIsCoachOrManager } from '../../auth/hooks/useIsCoachOrManager'
import { mapSideLabels } from '../../map-strategy/labels'
import { economyLabels } from '../../tactics/labels'
import { useGamePlan } from '../hooks/useGamePlan'
import { GamePlanEditor } from './GamePlanEditor'

const dateFormatter = new Intl.DateTimeFormat('pl-PL', { dateStyle: 'medium', timeStyle: 'short' })

/** An event's game plan for the whole team: notes plus links straight into the attached tactics and boards. */
export function GamePlanSection({ eventId, title }: { eventId: string; title: string }) {
  const canManage = useIsCoachOrManager()
  const [isEditing, setIsEditing] = useState(false)
  const { data: plan, isLoading, isError } = useGamePlan(eventId)
  const isEmpty = plan && !plan.notes && plan.tactics.length === 0 && plan.boards.length === 0

  return (
    <div className="flex flex-col gap-2 rounded-md border border-neutral-800 p-3">
      <div className="flex flex-wrap items-center justify-between gap-2">
        <h3 className="text-sm font-medium">Plan</h3>
        {canManage && plan && (
          <button
            type="button"
            onClick={() => setIsEditing(true)}
            className="text-xs text-neutral-400 hover:text-neutral-200"
          >
            {isEmpty ? 'Przygotuj plan' : 'Edytuj plan'}
          </button>
        )}
      </div>

      {isLoading && <p className="text-xs text-neutral-500">Ładowanie planu…</p>}
      {isError && <p className="text-xs text-danger-400">Nie udało się pobrać planu.</p>}
      {isEmpty && <p className="text-xs text-neutral-500">Plan nie został jeszcze przygotowany.</p>}

      {plan && !isEmpty && (
        <>
          {plan.notes && <p className="whitespace-pre-wrap text-sm text-neutral-300">{plan.notes}</p>}

          {plan.tactics.length > 0 && (
            <ul className="flex flex-col gap-1 text-sm">
              {plan.tactics.map((tactic) => (
                <li key={tactic.id}>
                  <Link to={`/playbook?tab=tactics&tactic=${tactic.id}`} className="hover:underline">
                    🗺️ {tactic.name}
                  </Link>
                  <span className="text-xs text-neutral-500">
                    {' '}
                    · {tactic.mapName} · {mapSideLabels[tactic.side]} · {economyLabels[tactic.economy]}
                  </span>
                </li>
              ))}
            </ul>
          )}

          {plan.boards.length > 0 && (
            <ul className="flex flex-col gap-1 text-sm">
              {plan.boards.map((board) => (
                <li key={board.id}>
                  <Link to={`/playbook?tab=boards&board=${board.id}`} className="hover:underline">
                    ✏️ {board.title}
                  </Link>
                  <span className="text-xs text-neutral-500"> · {board.mapName}</span>
                </li>
              ))}
            </ul>
          )}

          {plan.updatedAtUtc && (
            <p className="text-xs text-neutral-600">
              Zaktualizowano {dateFormatter.format(new Date(plan.updatedAtUtc))}
            </p>
          )}
        </>
      )}

      {isEditing && plan && (
        <Modal title={`Plan — ${title}`} onClose={() => setIsEditing(false)} wide>
          <GamePlanEditor plan={plan} onDone={() => setIsEditing(false)} />
        </Modal>
      )}
    </div>
  )
}
