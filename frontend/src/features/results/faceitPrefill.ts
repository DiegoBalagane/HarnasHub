import type { AnalyzeDemoResult, FaceitFactionPrefill, FaceitMatchPrefill, MatchCategory } from '../../services/resultsApi'

/** The opponent's faction of a recognised FACEIT match: the one that isn't ours when the server found us in the room,
 * otherwise the one that didn't play as the picked demo team; null when neither tells. */
export function opponentFaction(prefill: FaceitMatchPrefill, ourDemoTeam: 'A' | 'B' | '' | null): FaceitFactionPrefill | null {
  if (prefill.factions.length !== 2) return null

  if (prefill.ourFactionIndex === 0 || prefill.ourFactionIndex === 1) {
    return prefill.factions[1 - prefill.ourFactionIndex]
  }

  if (!ourDemoTeam) return null
  const theirs = prefill.factions.filter((faction) => faction.demoTeam !== null && faction.demoTeam !== ourDemoTeam)
  return theirs.length === 1 ? theirs[0] : null
}

/** Opponent name to prefill: an already linked opponent's name beats the FACEIT faction name ("team_Nick" in pickup rooms). */
export function opponentNameOf(faction: FaceitFactionPrefill): string {
  return faction.linkedOpponentName ?? faction.name
}

/** Source for the FACEIT link of the opponent: the faction's nicknames, the format LinkOpponentFaceit accepts. */
export function linkSourceOf(faction: FaceitFactionPrefill): string {
  return faction.nicknames.join(', ')
}

/** An ISO timestamp as the local `YYYY-MM-DDTHH:mm` value a datetime-local input expects; '' when missing/invalid. */
export function toDateTimeLocal(iso: string | null): string {
  if (!iso) return ''
  const date = new Date(iso)
  if (Number.isNaN(date.getTime())) return ''
  const pad = (value: number) => String(value).padStart(2, '0')
  return `${date.getFullYear()}-${pad(date.getMonth() + 1)}-${pad(date.getDate())}T${pad(date.getHours())}:${pad(date.getMinutes())}`
}

/** Form fields the coach can edit by hand; once touched, a demo analysis must never overwrite them. */
export type TouchedField = 'opponent' | 'score' | 'map' | 'playedAt' | 'category'

/** Fields an analysis result wants to fill; absent keys mean "leave the form field alone". */
export interface AnalysisPatch {
  mapName?: string
  ourScore?: string
  opponentScore?: string
  selectedTeam?: 'A' | 'B'
  playedAt?: string
  category?: MatchCategory
}

/** What a finished demo analysis (plus its FACEIT prefill) may write into the result form: only fields the coach hasn't touched. */
export function buildAnalysisPatch(result: AnalyzeDemoResult, touched: ReadonlySet<TouchedField>): AnalysisPatch {
  const patch: AnalysisPatch = {}
  const faceit = result.faceitMatch ?? null

  const mapName = result.mapName ?? faceit?.mapName
  if (mapName && !touched.has('map')) patch.mapName = mapName

  if (result.suggestedTeam) {
    patch.selectedTeam = result.suggestedTeam
    if (!touched.has('score')) {
      const preview = result.suggestedTeam === 'A' ? result.teamA : result.teamB
      patch.ourScore = String(preview.ourScore)
      patch.opponentScore = String(preview.opponentScore)
    }
  }

  if (faceit) {
    const playedAt = toDateTimeLocal(faceit.playedAtUtc)
    if (playedAt && !touched.has('playedAt')) patch.playedAt = playedAt
    if (!touched.has('category')) patch.category = faceit.category
  }

  return patch
}
