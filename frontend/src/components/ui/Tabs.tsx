import { memo } from 'react'

/** One tab definition. */
export interface TabItem<T extends string> {
  id: T
  label: string
}

interface TabsProps<T extends string> {
  tabs: readonly TabItem<T>[]
  value: T
  onChange: (id: T) => void
  className?: string
}

/** Underlined tab bar; the active tab is marked with the primary colour. */
function TabsInner<T extends string>({ tabs, value, onChange, className = '' }: TabsProps<T>) {
  return (
    <div className={`flex flex-wrap gap-1 border-b border-surface-border ${className}`} role="tablist">
      {tabs.map((tab) => {
        const active = tab.id === value
        return (
          <button
            key={tab.id}
            type="button"
            role="tab"
            aria-selected={active}
            onClick={() => onChange(tab.id)}
            className={`-mb-px border-b-2 px-3 py-2 text-sm transition ${
              active ? 'border-primary-500 text-white' : 'border-transparent text-neutral-400 hover:text-white'
            }`}
          >
            {tab.label}
          </button>
        )
      })}
    </div>
  )
}

export const Tabs = memo(TabsInner) as typeof TabsInner
