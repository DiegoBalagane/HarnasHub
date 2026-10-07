const examples = [
  { title: 'Discord serwera', value: 'https://discord.gg/…' },
  { title: 'Serwer treningowy', value: 'connect 123.45.67.89:27015' },
  { title: 'Adres serwera testowego', value: '123.45.67.89:27016' },
  { title: 'Konfig autoexec', value: 'rate 786432\ncl_interp_ratio 1' },
]

/** Shown when no entries exist yet: explains the page and previews what an entry can look like. */
export function TeamInfoEmptyState({ canManage }: { canManage: boolean }) {
  return (
    <div className="flex flex-col gap-4 rounded-lg border border-dashed border-neutral-700 p-6">
      <p className="text-sm text-neutral-300">
        Brak wpisów. Tu trafiają stałe informacje techniczne drużyny — adresy serwerów, linki i konfiguracje.
        {canManage ? ' Użyj „+ Dodaj wpis”, aby dodać pierwszy.' : ' Trener lub menedżer może je dodać.'}
      </p>
      <ul className="grid gap-2 sm:grid-cols-2" aria-label="Przykładowe wpisy">
        {examples.map((example) => (
          <li key={example.title} className="rounded-md border border-neutral-800 p-3 opacity-60">
            <p className="text-sm font-medium">{example.title}</p>
            <p className="whitespace-pre-wrap font-mono text-xs text-neutral-400">{example.value}</p>
          </li>
        ))}
      </ul>
    </div>
  )
}
