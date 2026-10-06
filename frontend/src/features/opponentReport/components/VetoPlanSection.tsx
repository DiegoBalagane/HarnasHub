import { memo } from 'react'
import type { VetoFormat, VetoPlan } from '../../../services/opponentReportApi'
import { vetoActionClasses, vetoActionLabels, vetoActorLabels } from '../../veto/labels'

/** Props of {@link VetoPlanSection}. */
interface VetoPlanSectionProps {
  plans: VetoPlan[]
  format: VetoFormat
  onFormatChange: (format: VetoFormat) => void
}

const formats: VetoFormat[] = ['Bo1', 'Bo3']

/** Simulated veto step by step (assuming we start), with the reason behind every move. */
export const VetoPlanSection = memo(function VetoPlanSection({ plans, format, onFormatChange }: VetoPlanSectionProps) {
  const plan = plans.find((candidate) => candidate.format === format)

  return (
    <section className="flex flex-col gap-2">
      <div className="flex flex-wrap items-center justify-between gap-2">
        <h2 className="text-lg font-medium">Rekomendowane veto</h2>
        <div className="flex gap-1" role="tablist">
          {formats.map((option) => (
            <button
              key={option}
              type="button"
              role="tab"
              aria-selected={option === format}
              onClick={() => onFormatChange(option)}
              className={`rounded-md px-3 py-1 text-sm ${option === format ? 'bg-neutral-700 text-white' : 'text-neutral-400 hover:text-white'}`}
            >
              {option.toUpperCase()}
            </button>
          ))}
        </div>
      </div>
      <p className="text-xs text-neutral-500">
        FACEIT nie podaje kolejności banów — ruchy rywala są przewidywane z tego, co grają. Zakładamy, że zaczynamy.
      </p>
      {plan && (
        <ol className="flex flex-col gap-1">
          {plan.steps.map((step) => (
            <li key={step.order} className="flex gap-3 rounded-md border border-neutral-800 px-3 py-2 text-sm">
              <span className="w-5 text-neutral-500 tabular-nums">{step.order}.</span>
              <span className="w-24 text-neutral-400">{vetoActorLabels[step.actor]}</span>
              <span className={`h-fit rounded-full border px-2 py-0.5 text-xs ${vetoActionClasses[step.action]}`}>
                {vetoActionLabels[step.action]}
              </span>
              <span className="w-20 font-medium">{step.mapName}</span>
              <span className="flex-1 text-xs text-neutral-400">{step.reason}</span>
            </li>
          ))}
        </ol>
      )}
    </section>
  )
})
