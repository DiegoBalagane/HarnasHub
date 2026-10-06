import type { ButtonHTMLAttributes } from 'react'

/** Visual variants: primary = main action, secondary = neutral, danger = destructive, ghost = low-emphasis. */
export type ButtonVariant = 'primary' | 'secondary' | 'danger' | 'ghost'
/** Button sizes. */
export type ButtonSize = 'sm' | 'md'

const variantClasses: Record<ButtonVariant, string> = {
  primary: 'bg-primary-500 text-primary-950 hover:bg-primary-400',
  secondary: 'border border-neutral-700 text-neutral-200 hover:border-neutral-500 hover:text-white',
  danger: 'bg-danger-600 text-white hover:bg-danger-500',
  ghost: 'text-neutral-400 hover:bg-neutral-800/60 hover:text-white',
}

const sizeClasses: Record<ButtonSize, string> = {
  sm: 'px-3 py-1.5 text-xs',
  md: 'px-4 py-2 text-sm',
}

interface ButtonProps extends ButtonHTMLAttributes<HTMLButtonElement> {
  variant?: ButtonVariant
  size?: ButtonSize
}

/** Shared button with consistent colours, sizes and disabled state. */
export function Button({ variant = 'primary', size = 'md', type = 'button', className = '', ...rest }: ButtonProps) {
  return (
    <button
      type={type}
      className={`rounded-md font-medium transition disabled:opacity-50 ${variantClasses[variant]} ${sizeClasses[size]} ${className}`}
      {...rest}
    />
  )
}
