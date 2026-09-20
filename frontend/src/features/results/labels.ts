import type { MatchCategory } from '../../services/resultsApi'
import type { LeagueType } from '../../services/leaguesApi'

export const matchCategoryLabels: Record<MatchCategory, string> = {
  Scrimmage: 'Sparing',
  League: 'Liga',
  Tournament: 'Turniej',
}

export const matchCategories: MatchCategory[] = ['Scrimmage', 'League', 'Tournament']

export const leagueTypeLabels: Record<LeagueType, string> = {
  Online: 'Online',
  Lan: 'LAN',
  Division1: 'Dywizja 1',
  Division2: 'Dywizja 2',
  Other: 'Inne',
}

export const leagueTypes: LeagueType[] = ['Online', 'Lan', 'Division1', 'Division2', 'Other']
