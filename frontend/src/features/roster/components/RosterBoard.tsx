import { DndContext, useDraggable, useDroppable, type DragEndEvent } from '@dnd-kit/core'
import type { RosterSlot, TeamMember } from '../../../services/rosterApi'
import { useRoster, useUpdateRosterSlot } from '../hooks/useRoster'

type ColumnId = 'Main' | 'Bench' | 'Unassigned'

const columns: { id: ColumnId; title: string }[] = [
  { id: 'Main', title: 'Główny skład (max 5)' },
  { id: 'Bench', title: 'Ławka' },
  { id: 'Unassigned', title: 'Pozostali' },
]

function columnFor(member: TeamMember): ColumnId {
  if (member.rosterSlot === 'Main') return 'Main'
  if (member.rosterSlot === 'Bench') return 'Bench'
  return 'Unassigned'
}

/** Column drop target that holds one card per roster slot. */
function Column({ id, title, members }: { id: ColumnId; title: string; members: TeamMember[] }) {
  const { setNodeRef, isOver } = useDroppable({ id })

  return (
    <div
      ref={setNodeRef}
      className={`flex min-h-[120px] flex-1 flex-col gap-2 rounded-md border p-3 transition ${
        isOver ? 'border-red-500 bg-neutral-900' : 'border-neutral-800'
      }`}
    >
      <h3 className="text-xs font-medium text-neutral-400">{title}</h3>
      {members.map((member) => (
        <MemberCard key={member.id} member={member} />
      ))}
      {members.length === 0 && <p className="text-xs text-neutral-600">Przeciągnij tu zawodnika</p>}
    </div>
  )
}

/** Draggable card for one roster member. */
function MemberCard({ member }: { member: TeamMember }) {
  const { attributes, listeners, setNodeRef, transform, isDragging } = useDraggable({ id: member.id })
  const name = member.inGameNickname ?? member.displayName

  return (
    <div
      ref={setNodeRef}
      {...listeners}
      {...attributes}
      style={
        transform ? { transform: `translate(${transform.x}px, ${transform.y}px)`, zIndex: 10 } : undefined
      }
      className={`cursor-grab touch-none rounded-md border border-neutral-700 bg-neutral-950 px-3 py-2 text-sm active:cursor-grabbing ${
        isDragging ? 'opacity-50' : ''
      }`}
    >
      {name}
      {member.rosterSlot === 'StandIn' && <span className="ml-2 text-xs text-neutral-500">(stand-in)</span>}
    </div>
  )
}

/** Drag-and-drop board for Coach/Manager to assign Main/Bench by dragging cards between columns; dropping into "Pozostali" clears the slot back to unassigned (a stand-in tag is set separately from the roster list). */
export function RosterBoard() {
  const { data: roster } = useRoster()
  const updateRosterSlot = useUpdateRosterSlot()

  const members = (roster ?? []).filter((member) => member.role !== 'Guest')

  function handleDragEnd(event: DragEndEvent) {
    const { active, over } = event

    if (!over) return

    const member = members.find((candidate) => candidate.id === active.id)

    if (!member || columnFor(member) === over.id) return

    const nextSlot: RosterSlot | null = over.id === 'Unassigned' ? null : (over.id as RosterSlot)
    updateRosterSlot.mutate({ userId: member.id, rosterSlot: nextSlot })
  }

  return (
    <div className="flex w-full max-w-3xl flex-col gap-2">
      {updateRosterSlot.isError && <p className="text-sm text-red-400">{updateRosterSlot.error.message}</p>}
      <DndContext onDragEnd={handleDragEnd}>
        <div className="flex flex-col gap-3 sm:flex-row">
          {columns.map((column) => (
            <Column
              key={column.id}
              id={column.id}
              title={column.title}
              members={members.filter((member) => columnFor(member) === column.id)}
            />
          ))}
        </div>
      </DndContext>
    </div>
  )
}
