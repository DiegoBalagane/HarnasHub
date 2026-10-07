import React from 'react'
import type { DayEntry } from '../../../../services/availabilityApi'
import { pillView, type PillKind } from '../../weekModel'

const kindClass: Record<PillKind, string> = {
  available: 'border-success-700 bg-success-950 text-success-300',
  partial: 'border-warning-700 bg-warning-950 text-warning-300',
  vacation: 'border-purple-700 bg-purple-950 text-purple-300',
  off: 'border-neutral-800 bg-neutral-900 text-neutral-500',
  none: 'border-dashed border-neutral-700 text-neutral-600',
}

interface AvailabilityPillProps {
  entry: DayEntry | undefined
  /** Accessible prefix, usually "<player> <date>". */
  name: string
  /** Own row: slightly stronger look. */
  mine: boolean
  /** Editable cells render as buttons that report their screen rect to the caller. */
  editable: boolean
  active: boolean
  onOpen?: (rect: DOMRect) => void
}

/** Slim status pill for one player on one day: colour by status, hours for partial, dashed outline for no answer. */
export const AvailabilityPill = React.memo(function AvailabilityPill({
  entry,
  name,
  mine,
  editable,
  active,
  onOpen,
}: AvailabilityPillProps) {
  const view = pillView(entry)
  const className = `relative flex h-full max-h-9 min-h-5 w-full items-center justify-center truncate rounded-full border px-2 text-[11px] leading-none ${
    kindClass[view.kind]
  } ${mine ? 'font-semibold' : ''} ${active ? 'ring-2 ring-neutral-300' : ''}`
  const title = entry?.note ? `${view.label} — ${entry.note}` : view.label
  const noteDot = entry?.note ? (
    <span aria-hidden className="absolute right-1.5 top-0.5 h-1.5 w-1.5 rounded-full bg-primary-400" />
  ) : null

  if (!editable) {
    return (
      <div role="img" aria-label={`${name}: ${view.label}`} title={title} className={className}>
        {view.text}
        {noteDot}
      </div>
    )
  }

  return (
    <button
      type="button"
      aria-label={`${name}: ${view.label}`}
      title={`${title} — kliknij, aby zmienić`}
      onClick={(event) => onOpen?.(event.currentTarget.getBoundingClientRect())}
      className={`${className} cursor-pointer transition hover:brightness-125`}
    >
      {view.text}
      {noteDot}
    </button>
  )
})
