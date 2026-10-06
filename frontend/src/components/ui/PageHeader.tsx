import type { ReactNode } from 'react'

interface PageHeaderProps {
  title: string
  /** Optional controls rendered on the right (buttons, modals triggers). */
  actions?: ReactNode
}

/** Consistent page header: h1 on the left, optional actions on the right. */
export function PageHeader({ title, actions }: PageHeaderProps) {
  return (
    <div className="flex flex-wrap items-center justify-between gap-3">
      <h1 className="text-2xl font-semibold">{title}</h1>
      {actions && <div className="flex flex-wrap items-center gap-2">{actions}</div>}
    </div>
  )
}
