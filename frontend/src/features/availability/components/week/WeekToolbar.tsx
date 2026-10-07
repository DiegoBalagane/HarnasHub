import { dayStatusLabels } from '../../labels'

export type AvailabilityTab = 'upcoming' | 'history'

const legendDots = [
  { label: dayStatusLabels.Available, className: 'bg-success-500' },
  { label: dayStatusLabels.PartiallyAvailable, className: 'bg-warning-500' },
  { label: 'Urlop', className: 'bg-purple-500' },
  { label: dayStatusLabels.Off, className: 'bg-neutral-600' },
  { label: dayStatusLabels.NotSet, className: 'border border-dashed border-neutral-500' },
]
const navClass =
  'rounded-md border border-neutral-700 px-2 py-1 text-xs text-neutral-300 transition hover:border-neutral-500 disabled:cursor-not-allowed disabled:opacity-30'

interface WeekToolbarProps {
  tab: AvailabilityTab
  onTabChange: (tab: AvailabilityTab) => void
  rangeLabel: string
  canGoBack: boolean
  canGoForward: boolean
  onPrev: () => void
  onNext: () => void
  onToday: () => void
}

/** Single compact line: Nadchodzące/Historia toggle, week navigation and a dot legend. */
export function WeekToolbar({
  tab,
  onTabChange,
  rangeLabel,
  canGoBack,
  canGoForward,
  onPrev,
  onNext,
  onToday,
}: WeekToolbarProps) {
  const tabClass = (active: boolean) =>
    `rounded px-2 py-0.5 transition ${active ? 'bg-neutral-800 text-neutral-100' : 'text-neutral-500 hover:text-neutral-300'}`

  return (
    <div className="flex flex-wrap items-center gap-x-3 gap-y-2">
      <div className="flex gap-1 rounded-md border border-neutral-800 p-0.5 text-xs">
        <button type="button" className={tabClass(tab === 'upcoming')} onClick={() => onTabChange('upcoming')}>
          Nadchodzące
        </button>
        <button type="button" className={tabClass(tab === 'history')} onClick={() => onTabChange('history')}>
          Historia
        </button>
      </div>

      <div className="flex items-center gap-1">
        <button type="button" aria-label="Poprzedni tydzień" disabled={!canGoBack} className={navClass} onClick={onPrev}>
          ◀
        </button>
        <span className="min-w-24 text-center text-xs font-medium text-neutral-200">{rangeLabel}</span>
        <button type="button" aria-label="Następny tydzień" disabled={!canGoForward} className={navClass} onClick={onNext}>
          ▶
        </button>
        {tab === 'upcoming' && (
          <button type="button" className={navClass} onClick={onToday}>
            Dziś
          </button>
        )}
      </div>

      <ul aria-label="Legenda" className="ml-auto flex flex-wrap items-center gap-x-3 gap-y-1 text-[11px] text-neutral-400">
        {legendDots.map((item) => (
          <li key={item.label} className="flex items-center gap-1">
            <span className={`inline-block h-2.5 w-2.5 rounded-full ${item.className}`} />
            {item.label}
          </li>
        ))}
      </ul>
    </div>
  )
}
