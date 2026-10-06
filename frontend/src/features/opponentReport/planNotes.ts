import { GAME_PLAN_NOTES_MAX_LENGTH } from '../../constants'
import type { OpponentReport, VetoFormat } from '../../services/opponentReportApi'
import { vetoActionLabels, vetoActorLabels } from '../veto/labels'

/** First line of the generated section — lets a second "Utwórz plan meczu" replace it instead of appending a duplicate. */
export const PLAN_NOTES_MARKER = '--- Raport rywala (FACEIT) ---'

const dateFormatter = new Intl.DateTimeFormat('pl-PL', { dateStyle: 'medium', timeStyle: 'short' })

/** The report's TL;DR and the recommended veto of the chosen format as plain-text plan notes. */
export function reportToNotes(report: OpponentReport, format: VetoFormat): string {
  const dataDate = report.dataSyncedAtUtc ?? report.generatedAtUtc
  const plan = report.vetoPlans.find((candidate) => candidate.format === format)
  const lines = [
    PLAN_NOTES_MARKER,
    `vs ${report.opponentName} · dane z ${dateFormatter.format(new Date(dataDate))}`,
    '',
    'TL;DR:',
    ...report.insights.map((insight) => `- ${insight.text}`),
  ]

  if (plan) {
    lines.push('', `Rekomendowane veto (${format.toUpperCase()}):`)
    lines.push(
      ...plan.steps.map(
        (step) =>
          `${step.order}. ${vetoActorLabels[step.actor]} ${vetoActionLabels[step.action]} ${step.mapName} — ${step.reason}`,
      ),
    )
  }

  return lines.join('\n')
}

/** Merges the generated section into existing notes: replaces a previous generated section, otherwise appends; capped to the server limit. */
export function buildPlanNotes(existing: string | null, report: OpponentReport, format: VetoFormat): string {
  const generated = reportToNotes(report, format)
  const current = existing ?? ''
  const markerIndex = current.indexOf(PLAN_NOTES_MARKER)
  const kept = (markerIndex >= 0 ? current.slice(0, markerIndex) : current).trimEnd()
  const merged = kept ? `${kept}\n\n${generated}` : generated
  return merged.slice(0, GAME_PLAN_NOTES_MAX_LENGTH)
}
