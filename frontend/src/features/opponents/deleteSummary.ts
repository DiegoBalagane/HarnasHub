import type { OpponentDeletePreview } from '../../services/opponentsApi'

/** Polish noun form: 1 -> one, 2-4 (except 12-14) -> few, otherwise many. */
export function pluralPl(count: number, one: string, few: string, many: string): string {
  if (count === 1) return one
  const lastDigit = count % 10
  const lastTwo = count % 100
  return lastDigit >= 2 && lastDigit <= 4 && (lastTwo < 12 || lastTwo > 14) ? few : many
}

/** "3 wyniki, 1 wydarzenie" — the history a delete would not touch by default; empty string when there is none. */
export function historyLabel(preview: Pick<OpponentDeletePreview, 'matchResults' | 'events'>): string {
  const parts: string[] = []
  if (preview.matchResults > 0) {
    parts.push(`${preview.matchResults} ${pluralPl(preview.matchResults, 'wynik', 'wyniki', 'wyników')}`)
  }
  if (preview.events > 0) {
    parts.push(`${preview.events} ${pluralPl(preview.events, 'wydarzenie', 'wydarzenia', 'wydarzeń')}`)
  }
  return parts.join(', ')
}

/** "2 notatki, powiązanie FACEIT, raport" — scouting data that is always removed; empty string when there is none. */
export function scoutingLabel(preview: OpponentDeletePreview): string {
  const parts: string[] = []
  if (preview.notes > 0)
    parts.push(`${preview.notes} ${pluralPl(preview.notes, 'notatka', 'notatki', 'notatek')}`)
  if (preview.hasFaceitLink) parts.push('powiązanie FACEIT')
  if (preview.hasReportSnapshot) parts.push('raport')
  if (preview.demoAnalyses > 0) {
    parts.push(
      `${preview.demoAnalyses} ${pluralPl(preview.demoAnalyses, 'analiza demki', 'analizy demek', 'analiz demek')}`,
    )
  }
  return parts.join(', ')
}

/** True when results or events reference the opponent, i.e. a plain delete can only hide it. */
export function hasHistory(preview: Pick<OpponentDeletePreview, 'matchResults' | 'events'>): boolean {
  return preview.matchResults > 0 || preview.events > 0
}
