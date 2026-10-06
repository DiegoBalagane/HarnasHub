import { memo } from 'react'
import type { OpponentInsight } from '../../../services/opponentReportApi'
import { severityClasses } from '../labels'

/** The report's TL;DR: 3–5 short points, each with the numbers behind it on hover. */
export const InsightList = memo(function InsightList({ insights }: { insights: OpponentInsight[] }) {
  if (insights.length === 0) {
    return null
  }

  return (
    <section className="flex flex-col gap-2">
      <h2 className="text-lg font-medium">Najważniejsze</h2>
      <ul className="flex flex-col gap-1.5">
        {insights.map((insight) => (
          <li
            key={`${insight.kind}-${insight.text}`}
            title={insight.evidence}
            className={`rounded-md border px-3 py-2 text-sm ${severityClasses[insight.severity]}`}
          >
            {insight.text}
            <span className="ml-2 text-xs opacity-60">({insight.evidence})</span>
          </li>
        ))}
      </ul>
    </section>
  )
})
