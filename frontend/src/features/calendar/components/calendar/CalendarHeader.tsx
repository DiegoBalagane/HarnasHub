import { Button } from '../../../../components/ui/Button'
import type { CalendarView } from '../../calendarGrid'

const viewOptions: readonly { id: CalendarView; label: string }[] = [
  { id: 'month', label: 'Miesiąc' },
  { id: 'week', label: 'Tydzień' },
  { id: 'agenda', label: 'Agenda' },
]

interface CalendarHeaderProps {
  label: string
  view: CalendarView
  onViewChange: (view: CalendarView) => void
  onPrev: () => void
  onNext: () => void
  onToday: () => void
}

/** Calendar toolbar: Today, prev/next, the current range label and the view switcher. */
export function CalendarHeader({ label, view, onViewChange, onPrev, onNext, onToday }: CalendarHeaderProps) {
  return (
    <div className="flex flex-wrap items-center justify-between gap-2">
      <div className="flex items-center gap-2">
        <Button variant="secondary" size="sm" onClick={onToday}>
          Dziś
        </Button>
        <Button variant="ghost" size="sm" aria-label="Poprzedni" onClick={onPrev}>
          ◀
        </Button>
        <Button variant="ghost" size="sm" aria-label="Następny" onClick={onNext}>
          ▶
        </Button>
        <h2 className="text-base font-semibold capitalize text-white">{label}</h2>
      </div>
      <div role="group" aria-label="Widok kalendarza" className="flex gap-0.5 rounded-md border border-neutral-800 p-0.5 text-xs">
        {viewOptions.map((option) => (
          <button
            key={option.id}
            type="button"
            aria-pressed={view === option.id}
            onClick={() => onViewChange(option.id)}
            className={`rounded px-3 py-1 transition ${
              view === option.id ? 'bg-neutral-800 text-white' : 'text-neutral-500 hover:text-neutral-200'
            }`}
          >
            {option.label}
          </button>
        ))}
      </div>
    </div>
  )
}
