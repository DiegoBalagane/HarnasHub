import { useEffect, useId, useRef, useState } from 'react'
import { createPortal } from 'react-dom'
import { PickerCalendar } from './PickerCalendar'
import { PickerTimeSelector } from './PickerTimeSelector'
import {
  addMinutesToLocalValue,
  addMinutesToTime,
  formatDateTimeValue,
  formatDateValue,
  joinLocalValue,
  parseTimeText,
  snapTime,
  splitLocalValue,
  toLocalValue,
  todayIso,
} from './dateTime'
import { addDaysIso } from '../../features/availability/weekDates'

type PickerMode = 'datetime' | 'date' | 'time'

const panelWidth = 320
const panelHeight = 470
const sheetBreakpoint = 640
const defaultMinuteStep = 5

interface PickerFieldProps {
  mode: PickerMode
  /** `yyyy-MM-ddTHH:mm` (datetime), `yyyy-MM-dd` (date) or `HH:mm` (time); '' when empty. */
  value: string
  /** Called only when the user presses "Zatwierdź" (or "Wyczyść"). */
  onChange: (value: string) => void
  /** Accessible name of the trigger and of the popover. */
  label: string
  placeholder?: string
  /** Adds a "Wyczyść" button that commits ''. */
  clearable?: boolean
  /** Granularity of the minute list, in minutes. */
  minuteStep?: number
  disabled?: boolean
  className?: string
}

/** Where the popover opens: under (or above) the trigger on desktop, a full-width bottom sheet on small screens. */
function computePlacement(trigger: HTMLElement | null): React.CSSProperties | undefined {
  if (!trigger || window.innerWidth < sheetBreakpoint) return undefined
  const rect = trigger.getBoundingClientRect()
  const left = Math.max(8, Math.min(rect.left, window.innerWidth - panelWidth - 8))
  const fitsBelow = rect.bottom + panelHeight + 8 <= window.innerHeight
  return fitsBelow ? { left, top: rect.bottom + 4 } : { left, bottom: Math.max(8, window.innerHeight - rect.top + 4) }
}

interface PopoverProps extends Pick<PickerFieldProps, 'mode' | 'value' | 'label' | 'clearable'> {
  step: number
  style: React.CSSProperties | undefined
  onCommit: (value: string) => void
  onCancel: () => void
}

/** The open picker: draft date/time, shortcuts and explicit Zatwierdź/Anuluj — nothing leaves it until confirmed. */
function PickerPopover({ mode, value, label, clearable, step, style, onCommit, onCancel }: PopoverProps) {
  const today = todayIso()
  const initial = mode === 'datetime' ? splitLocalValue(value) : mode === 'date' ? { date: value, time: '' } : { date: '', time: value }
  const [date, setDate] = useState(initial.date)
  const [time, setTime] = useState(initial.time)
  const [focused, setFocused] = useState(initial.date || today)
  const showDate = mode !== 'time'
  const showTime = mode !== 'date'

  // Escape cancels only the picker, not the modal underneath (capture phase, before the Modal's listener).
  useEffect(() => {
    function handleKeyDown(event: KeyboardEvent) {
      if (event.key !== 'Escape') return
      event.stopImmediatePropagation()
      event.preventDefault()
      onCancel()
    }
    document.addEventListener('keydown', handleKeyDown, true)
    return () => document.removeEventListener('keydown', handleKeyDown, true)
  }, [onCancel])

  function chooseDate(iso: string) {
    setDate(iso)
    setFocused(iso)
    if (showTime && !time) setTime(snapTime(toLocalValue(new Date()).slice(11), step))
  }


  function shortcutPlusHour() {
    if (mode === 'datetime') {
      const next = addMinutesToLocalValue(toLocalValue(new Date()), 60)
      const parts = splitLocalValue(next)
      setDate(parts.date)
      setFocused(parts.date)
      setTime(snapTime(parts.time, step))
    } else {
      setTime(addMinutesToTime(time || toLocalValue(new Date()).slice(11), 60))
    }
  }

  function confirm() {
    const effectiveTime = parseTimeText(time) ?? (showTime ? snapTime(toLocalValue(new Date()).slice(11), step) : '')
    if (mode === 'date') onCommit(date)
    else if (mode === 'time') onCommit(effectiveTime)
    else onCommit(joinLocalValue(date || today, effectiveTime))
  }

  const canConfirm = (!showDate || mode === 'datetime' || date !== '') && (mode !== 'time' || parseTimeText(time) !== null)
  const shortcutClass = 'rounded-full border border-neutral-700 px-3 py-1 text-xs text-neutral-300 hover:border-neutral-500'

  return createPortal(
    <div
      role="presentation"
      className="fixed inset-0 z-[70] flex items-end bg-black/50 sm:items-start sm:bg-transparent"
      onClick={onCancel}
    >
      <div
        role="dialog"
        aria-label={label}
        onClick={(event) => event.stopPropagation()}
        style={style}
        className="flex max-h-[92vh] w-full flex-col gap-3 overflow-y-auto rounded-t-xl border border-neutral-700 bg-neutral-950 p-4 shadow-2xl sm:fixed sm:w-80 sm:rounded-lg"
      >
        <div className="flex flex-wrap gap-2">
          {showDate && (
            <>
              <button type="button" className={shortcutClass} onClick={() => chooseDate(today)}>
                Dziś
              </button>
              <button type="button" className={shortcutClass} onClick={() => chooseDate(addDaysIso(today, 1))}>
                Jutro
              </button>
            </>
          )}
          {showTime && (
            <button type="button" className={shortcutClass} onClick={shortcutPlusHour}>
              +1 h
            </button>
          )}
        </div>

        {showDate && (
          <PickerCalendar selected={date} focused={focused} today={today} onFocusedChange={setFocused} onSelect={chooseDate} />
        )}
        {showTime && <PickerTimeSelector value={parseTimeText(time) ?? ''} step={step} onChange={setTime} />}

        <div className="flex items-center justify-between gap-2 border-t border-neutral-800 pt-3">
          {clearable ? (
            <button type="button" onClick={() => onCommit('')} className="text-xs text-neutral-400 hover:text-neutral-200">
              Wyczyść
            </button>
          ) : (
            <span />
          )}
          <div className="flex gap-2">
            <button
              type="button"
              onClick={onCancel}
              className="rounded-md border border-neutral-700 px-3 py-1.5 text-sm text-neutral-300 hover:border-neutral-500"
            >
              Anuluj
            </button>
            <button
              type="button"
              disabled={!canConfirm}
              onClick={confirm}
              className="rounded-md bg-primary-500 px-3 py-1.5 text-sm font-medium text-primary-950 hover:bg-primary-400 disabled:opacity-50"
            >
              Zatwierdź
            </button>
          </div>
        </div>
      </div>
    </div>,
    document.body,
  )
}

function displayOf(mode: PickerMode, value: string): string {
  if (mode === 'datetime') return formatDateTimeValue(value)
  if (mode === 'date') return formatDateValue(value)
  return parseTimeText(value) ? value : ''
}

function PickerField({ mode, value, onChange, label, placeholder, clearable, minuteStep, disabled, className }: PickerFieldProps) {
  const [isOpen, setIsOpen] = useState(false)
  const [placement, setPlacement] = useState<React.CSSProperties | undefined>(undefined)
  const triggerRef = useRef<HTMLButtonElement>(null)
  const popupId = useId()
  const display = displayOf(mode, value)

  function open() {
    setPlacement(computePlacement(triggerRef.current))
    setIsOpen(true)
  }

  function close() {
    setIsOpen(false)
    triggerRef.current?.focus()
  }

  return (
    <>
      <button
        ref={triggerRef}
        type="button"
        disabled={disabled}
        aria-label={display ? `${label}: ${display}` : label}
        aria-haspopup="dialog"
        aria-expanded={isOpen}
        aria-controls={isOpen ? popupId : undefined}
        onClick={open}
        className={`flex items-center justify-between gap-2 rounded-md border border-neutral-800 bg-neutral-900 px-3 py-2 text-left text-sm outline-none transition hover:border-neutral-600 focus:border-neutral-500 disabled:opacity-50 ${
          display ? 'text-neutral-100' : 'text-neutral-500'
        } ${className ?? ''}`}
      >
        <span className="truncate">{display || placeholder || 'Wybierz'}</span>
        <span aria-hidden="true" className="text-neutral-500">
          {mode === 'time' ? '◷' : '▾'}
        </span>
      </button>

      {isOpen && (
        <PickerPopover
          mode={mode}
          value={value}
          label={label}
          clearable={clearable && value !== ''}
          step={minuteStep ?? defaultMinuteStep}
          style={placement}
          onCommit={(next) => {
            onChange(next)
            close()
          }}
          onCancel={close}
        />
      )}
    </>
  )
}

type PickerProps = Omit<PickerFieldProps, 'mode'>

/** Date + time picker; value is a local `yyyy-MM-ddTHH:mm` string (convert with the helpers in `dateTime.ts`). */
export function DateTimePicker(props: PickerProps) {
  return <PickerField mode="datetime" {...props} />
}

/** Date-only picker; value is `yyyy-MM-dd`. */
export function DatePicker(props: PickerProps) {
  return <PickerField mode="date" {...props} />
}

/** Time-only picker; value is `HH:mm`. */
export function TimePicker(props: PickerProps) {
  return <PickerField mode="time" {...props} />
}

interface TimeRangePickerProps {
  from: string
  to: string
  /** Called with the new range whenever either end is confirmed. */
  onChange: (range: { from: string; to: string }) => void
  className?: string
}

/** Two time pickers ("od" – "do") for an hour range; each end is committed on its own Zatwierdź. */
export function TimeRangePicker({ from, to, onChange, className }: TimeRangePickerProps) {
  return (
    <div className={`flex items-center gap-2 ${className ?? ''}`}>
      <TimePicker label="Od godziny" value={from} className="flex-1" onChange={(next) => onChange({ from: next, to })} />
      <span className="text-xs text-neutral-500">–</span>
      <TimePicker label="Do godziny" value={to} className="flex-1" onChange={(next) => onChange({ from, to: next })} />
    </div>
  )
}
