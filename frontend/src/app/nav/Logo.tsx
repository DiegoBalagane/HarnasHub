import { Link } from 'react-router-dom'

interface LogoProps {
  className?: string
  /** Icon-only rail: the full wordmark doesn't fit in 64px, so only a red "H" monogram is shown. */
  compact?: boolean
}

/** HarnasHub wordmark linking to the home route; the red "Hub" is the only brand-red element of the UI. */
export function Logo({ className = 'text-lg', compact = false }: LogoProps) {
  return (
    <Link to="/" title="HarnasHub" className={`font-bold tracking-tight ${className}`}>
      {compact ? <span className="text-brand">H</span> : <>Harnas<span className="text-brand">Hub</span></>}
    </Link>
  )
}
