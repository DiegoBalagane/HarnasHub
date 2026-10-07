/** How the Info page renders a value: plain text, an external link, a Steam connect action or a multi-line snippet. */
export type EntryValueKind =
  | { type: 'text'; text: string }
  | { type: 'link'; href: string }
  | { type: 'connect'; href: string }
  | { type: 'code'; text: string }

const urlPattern = /^https?:\/\/\S+$/i
const steamConnectPattern = /^steam:\/\/connect\/\S+$/i
const connectCommandPattern = /^connect\s+([^\s;]+)(?:\s*;\s*password\s+(\S+))?\s*;?$/i

/** Classifies an entry value so the card can offer the right action (link, "Połącz", monospace block). */
export function classifyValue(value: string): EntryValueKind {
  const trimmed = value.trim()

  if (trimmed.includes('\n')) return { type: 'code', text: trimmed }
  if (steamConnectPattern.test(trimmed)) return { type: 'connect', href: trimmed }
  if (urlPattern.test(trimmed)) return { type: 'link', href: trimmed }

  const command = connectCommandPattern.exec(trimmed)
  if (command) {
    const [, address, password] = command
    const href = `steam://connect/${address}${password ? `/${encodeURIComponent(password)}` : ''}`
    return { type: 'connect', href }
  }

  return { type: 'text', text: trimmed }
}

/** Default categories offered by the form (and shown first, in this order); custom ones follow alphabetically. */
export const defaultCategories = ['Discord', 'Serwery', 'Konfiguracje', 'Inne']

/** Sorts category names: defaults first in their fixed order, then the rest alphabetically. */
export function sortCategories(categories: string[]): string[] {
  const rank = (category: string) => {
    const index = defaultCategories.indexOf(category)
    return index === -1 ? defaultCategories.length : index
  }
  return [...categories].sort((a, b) => rank(a) - rank(b) || a.localeCompare(b, 'pl'))
}
