import { Logo } from './Logo'
import { NavIcon } from './NavIcon'

/** Slim top bar shown below the lg breakpoint: hamburger that opens the drawer, plus the logo. */
export function MobileTopBar({ onOpen }: { onOpen: () => void }) {
  return (
    <header className="sticky top-0 z-30 flex h-14 items-center gap-3 border-b border-surface-border bg-surface-sidebar px-4 lg:hidden">
      <button
        type="button"
        onClick={onOpen}
        aria-label="Otwórz menu"
        className="rounded-md p-1.5 text-neutral-300 hover:bg-neutral-800/60 hover:text-white"
      >
        <NavIcon name="menu" />
      </button>
      <Logo />
    </header>
  )
}
