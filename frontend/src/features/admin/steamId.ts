/** True for an empty value (clears the ID) or a valid SteamID64: 17 digits starting with 7656119. */
export function isValidSteamId64(value: string): boolean {
  const trimmed = value.trim()
  return trimmed === '' || /^7656119\d{10}$/.test(trimmed)
}
