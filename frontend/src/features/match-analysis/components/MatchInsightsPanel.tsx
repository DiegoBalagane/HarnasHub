import { memo } from 'react'
import { insightToneStyles } from '../labels'
import { isMissingTimeline, useMatchInsights } from '../hooks/useMatchAnalysis'

interface MatchInsightsPanelProps {
  matchResultId: string
}

/** Automatic insights from the match timeline ("0/4 force buyów", lost openings per zone…), problems first. */
export const MatchInsightsPanel = memo(function MatchInsightsPanel({ matchResultId }: MatchInsightsPanelProps) {
  const { data: insights, isLoading, error } = useMatchInsights(matchResultId)

  if (isLoading) {
    return <p className="text-sm text-neutral-400">Ładowanie wniosków…</p>
  }

  if (isMissingTimeline(error)) {
    return null
  }

  if (error) {
    return <p className="text-sm text-danger-400">{error.message}</p>
  }

  if (!insights || insights.length === 0) {
    return <p className="text-sm text-neutral-400">Za mało danych w demce, by wyciągnąć wnioski.</p>
  }

  return (
    <section className="flex flex-col gap-2">
      <h2 className="text-lg font-semibold">Wnioski z demki</h2>
      <ul className="grid gap-2 md:grid-cols-2">
        {insights.map((insight) => (
          <li key={insight.code} className={`rounded-md border p-3 ${insightToneStyles[insight.tone]}`}>
            <p className="font-medium">{insight.title}</p>
            <p className="mt-1 text-sm text-neutral-400">{insight.detail}</p>
          </li>
        ))}
      </ul>
    </section>
  )
})
