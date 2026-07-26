import { cn } from '@/lib/cn'

export type ButtonVariant = 'primary' | 'secondary' | 'ghost' | 'danger'
export type ButtonSize = 'sm' | 'md' | 'lg'

const variantClasses: Record<ButtonVariant, string> = {
  primary:
    'bg-brand text-brand-content hover:bg-brand-hover disabled:hover:bg-brand',
  secondary:
    'bg-surface text-content border border-border hover:bg-surface-muted disabled:hover:bg-surface',
  ghost:
    'bg-transparent text-content-muted hover:bg-surface-muted hover:text-content',
  danger: 'bg-loss text-white hover:opacity-90',
}

const sizeClasses: Record<ButtonSize, string> = {
  sm: 'h-8 px-3 text-xs gap-1.5',
  md: 'h-10 px-4 text-sm gap-2',
  lg: 'h-11 px-5 text-sm gap-2',
}

/**
 * Kept out of `Button.tsx` so both the button and the router link that looks like one can
 * share it without that module exporting a non-component (which breaks Fast Refresh).
 */
export function buttonClasses(
  variant: ButtonVariant = 'primary',
  size: ButtonSize = 'md',
  className?: string,
): string {
  return cn(
    'inline-flex items-center justify-center rounded-lg font-medium transition-colors',
    'disabled:cursor-not-allowed disabled:opacity-60',
    variantClasses[variant],
    sizeClasses[size],
    className,
  )
}
