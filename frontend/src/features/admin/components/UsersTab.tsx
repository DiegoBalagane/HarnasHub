import { useMemo, useState } from 'react'
import { ConfirmDialog } from '../../../components/ConfirmDialog'
import type { TeamMember } from '../../../services/rosterApi'
import { useAuthStore } from '../../auth/stores/useAuthStore'
import { useDeleteMember, useRoster } from '../../roster/hooks/useRoster'
import { PendingAccounts } from './PendingAccounts'
import { UserRow, userGridColumns } from './UserRow'

/** "Użytkownicy" tab: pending accounts on top, then one searchable table of every account. */
export function UsersTab() {
  const { data: roster, isLoading, isError } = useRoster()
  const userId = useAuthStore((state) => state.userId)
  const deleteMember = useDeleteMember()
  const [search, setSearch] = useState('')
  const [deleting, setDeleting] = useState<TeamMember | null>(null)

  const filtered = useMemo(() => {
    const needle = search.trim().toLowerCase()
    return (roster ?? []).filter(
      (member) =>
        needle === '' ||
        member.displayName.toLowerCase().includes(needle) ||
        (member.inGameNickname ?? '').toLowerCase().includes(needle),
    )
  }, [roster, search])

  if (isLoading) {
    return <p className="text-neutral-400">Ładowanie użytkowników…</p>
  }

  if (isError) {
    return <p className="text-danger-400">Nie udało się pobrać listy użytkowników.</p>
  }

  return (
    <div className="flex flex-col gap-4">
      <PendingAccounts />

      <p className="max-w-3xl text-xs text-neutral-400">
        Statystyki — gdy odznaczone, gracz znika z rankingu i raportu rywala (jego „Moje statystyki” zostają). Kalendarz — gdy
        odznaczone, znika z kalendarza dostępności, list dnia i liczników (własną dostępność nadal może ustawiać). Dane nie są
        usuwane, ukrycie można cofnąć w każdej chwili. Nick FACEIT — dla graczy bez SteamID lub z SteamID niepowiązanym z FACEIT.
      </p>

      <input
        type="search"
        value={search}
        onChange={(event) => setSearch(event.target.value)}
        placeholder="Szukaj po nicku lub nazwie z Discorda"
        aria-label="Szukaj użytkownika"
        className="w-full max-w-sm rounded-md border border-neutral-800 bg-neutral-900 px-3 py-1.5 text-sm outline-none focus:border-neutral-500"
      />

      {deleteMember.isError && <p className="text-sm text-danger-400">{deleteMember.error.message}</p>}

      <div className="w-full overflow-x-auto rounded-md border border-neutral-800">
        <div
          className={`grid min-w-[1400px] ${userGridColumns} gap-x-3 border-b border-neutral-800 px-4 py-2 text-xs text-neutral-500`}
        >
          <span>Użytkownik</span>
          <span>Uprawnienia</span>
          <span>Trener</span>
          <span>Skład</span>
          <span title="Czy gracz występuje w statystykach (ranking, raport rywala)">Statystyki</span>
          <span title="Czy gracz występuje w kalendarzu dostępności">Kalendarz</span>
          <span>SteamID64</span>
          <span>Nick FACEIT</span>
          <span className="text-right">Akcje</span>
        </div>
        <ul className="flex min-w-[1400px] flex-col divide-y divide-neutral-800">
          {filtered.map((member) => (
            <UserRow key={member.id} member={member} isSelf={member.id === userId} onDelete={setDeleting} />
          ))}
        </ul>
        {filtered.length === 0 && <p className="px-4 py-3 text-sm text-neutral-500">Brak wyników.</p>}
      </div>

      {deleting && (
        <ConfirmDialog
          title="Usunąć konto?"
          message={
            <>
              Na pewno usunąć <span className="font-medium">{deleting.inGameNickname ?? deleting.displayName}</span>?
              Wyniki, granaty i taktyki które dodał zostają — konto i jego dane osobiste znikają bezpowrotnie.
            </>
          }
          confirmLabel="Tak, usuń"
          isConfirming={deleteMember.isPending}
          onConfirm={() => deleteMember.mutate(deleting.id, { onSuccess: () => setDeleting(null) })}
          onCancel={() => setDeleting(null)}
        />
      )}
    </div>
  )
}
