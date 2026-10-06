/** Route of an opponent's profile — the name travels in the query string so team names with "/" or "?" stay intact. */
export function opponentProfilePath(name: string): string {
  return `/opponents/profile?name=${encodeURIComponent(name.trim())}`
}

/** Route of an opponent's FACEIT "them vs us" report, same query-string convention as the profile. */
export function opponentReportPath(name: string): string {
  return `/opponents/report?name=${encodeURIComponent(name.trim())}`
}
