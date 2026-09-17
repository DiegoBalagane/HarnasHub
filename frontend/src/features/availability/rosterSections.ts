export type RosterSection = 'Main' | 'Bench' | 'Other' | 'Coach'

const sectionRank: Record<RosterSection, number> = { Main: 0, Bench: 1, Other: 2, Coach: 3 }

/** The Coach always sits in its own section at the very bottom, regardless of roster slot. Shared by the weekly calendar and the dashboard so both group members the same way. */
export function sectionOf(member: { rosterSlot: string | null; isCoach: boolean }): RosterSection {
  if (member.isCoach) return 'Coach'
  if (member.rosterSlot === 'Main') return 'Main'
  if (member.rosterSlot === 'Bench') return 'Bench'
  return 'Other'
}

/** Compares two members by roster section (Main → Bench → Coach), for use as a `sort` comparator's first tiebreak. */
export function compareSections(
  left: { rosterSlot: string | null; isCoach: boolean },
  right: { rosterSlot: string | null; isCoach: boolean },
): number {
  return sectionRank[sectionOf(left)] - sectionRank[sectionOf(right)]
}
