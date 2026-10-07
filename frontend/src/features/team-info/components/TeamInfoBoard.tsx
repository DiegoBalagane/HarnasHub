import { useMemo, useState } from 'react'
import { ConfirmDialog } from '../../../components/ConfirmDialog'
import { Modal } from '../../../components/Modal'
import type { TeamInfoEntry } from '../../../services/teamInfoApi'
import { sortCategories } from '../entryValue'
import { useDeleteTeamInfo, useReorderTeamInfo, useTeamInfo } from '../hooks/useTeamInfo'
import { TeamInfoEmptyState } from './TeamInfoEmptyState'
import { TeamInfoEntryRow } from './TeamInfoEntryRow'
import { TeamInfoForm } from './TeamInfoForm'

interface TeamInfoBoardProps {
  /** Coach/Manager: enables editing, deleting and reordering. */
  canManage: boolean
  /** Entry currently being edited in the modal; null when the modal is closed or adding. */
  editing: TeamInfoEntry | null
  isAdding: boolean
  onCloseForm: () => void
  onEdit: (entry: TeamInfoEntry) => void
}

/** Entries grouped into one card per category, plus the add/edit modal and delete confirmation. */
export function TeamInfoBoard({ canManage, editing, isAdding, onCloseForm, onEdit }: TeamInfoBoardProps) {
  const { data, isLoading, isError } = useTeamInfo()
  const reorder = useReorderTeamInfo()
  const remove = useDeleteTeamInfo()
  const [pendingDelete, setPendingDelete] = useState<TeamInfoEntry | null>(null)

  const groups = useMemo(() => {
    const byCategory = new Map<string, TeamInfoEntry[]>()
    for (const entry of data ?? []) {
      byCategory.set(entry.category, [...(byCategory.get(entry.category) ?? []), entry])
    }
    return sortCategories([...byCategory.keys()]).map((category) => ({
      category,
      entries: byCategory.get(category) ?? [],
    }))
  }, [data])

  function move(entry: TeamInfoEntry, offset: -1 | 1) {
    const ids = groups.find((group) => group.category === entry.category)?.entries.map((item) => item.id) ?? []
    const index = ids.indexOf(entry.id)
    const target = index + offset
    if (index === -1 || target < 0 || target >= ids.length) return
    ;[ids[index], ids[target]] = [ids[target], ids[index]]
    reorder.mutate(ids)
  }

  if (isLoading) return <p className="text-sm text-neutral-400">Ładowanie…</p>
  if (isError) return <p className="text-sm text-danger-400">Nie udało się załadować informacji.</p>

  return (
    <>
      {groups.length === 0 ? (
        <TeamInfoEmptyState canManage={canManage} />
      ) : (
        <div className="grid gap-4 lg:grid-cols-2">
          {groups.map(({ category, entries }) => (
            <section key={category} className="flex flex-col gap-3 rounded-lg border border-neutral-800 bg-neutral-900/50 p-4">
              <h2 className="font-medium">{category}</h2>
              {entries.map((entry, index) => (
                <TeamInfoEntryRow
                  key={entry.id}
                  entry={entry}
                  canManage={canManage}
                  isFirst={index === 0}
                  isLast={index === entries.length - 1}
                  onMoveUp={(item) => move(item, -1)}
                  onMoveDown={(item) => move(item, 1)}
                  onEdit={onEdit}
                  onDelete={setPendingDelete}
                />
              ))}
            </section>
          ))}
        </div>
      )}

      {(isAdding || editing) && (
        <Modal title={editing ? 'Edytuj wpis' : 'Dodaj wpis'} onClose={onCloseForm}>
          <TeamInfoForm
            entry={editing ?? undefined}
            existingCategories={groups.map((group) => group.category)}
            onDone={onCloseForm}
          />
        </Modal>
      )}

      {pendingDelete && (
        <ConfirmDialog
          title="Usunąć wpis?"
          message={`Wpis „${pendingDelete.title}” zostanie trwale usunięty.`}
          confirmLabel="Usuń"
          isConfirming={remove.isPending}
          onCancel={() => setPendingDelete(null)}
          onConfirm={() => remove.mutate(pendingDelete.id, { onSuccess: () => setPendingDelete(null) })}
        />
      )}
    </>
  )
}
