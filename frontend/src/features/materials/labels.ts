import type { MaterialCategory } from '../../services/materialsApi'

export const materialCategoryLabels: Record<MaterialCategory, string> = {
  Grenades: 'Granaty',
  Tactics: 'Taktyki',
  Aim: 'Celowanie',
  Positioning: 'Pozycjonowanie',
  VodReview: 'Analiza VOD',
  Communication: 'Komunikacja',
  Other: 'Inne',
}

/** Dropdown order for the category select, kept in sync with the backend MaterialCategory enum. */
export const materialCategories: MaterialCategory[] = [
  'Grenades',
  'Tactics',
  'Aim',
  'Positioning',
  'VodReview',
  'Communication',
  'Other',
]
