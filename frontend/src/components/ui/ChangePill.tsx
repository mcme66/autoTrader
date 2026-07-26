import { Minus, TrendingDown, TrendingUp } from 'lucide-react'

import { cn } from '@/lib/cn'
import { changeTone, formatPercent } from '@/lib/format'

/** Percentage change with a direction icon, coloured by sign. */
export function ChangePill({
  value,
  showIcon = true,
  className,
}: {
  value: number | null | undefined
  showIcon?: boolean
  className?: string
}) {
  const tone = changeTone(value)
  const Icon =
    tone === 'gain' ? TrendingUp : tone === 'loss' ? TrendingDown : Minus

  return (
    <span
      className={cn(
        'tabular inline-flex items-center gap-1 text-sm font-medium',
        tone === 'gain' && 'text-gain',
        tone === 'loss' && 'text-loss',
        tone === 'neutral' && 'text-content-muted',
        className,
      )}
    >
      {showIcon ? <Icon className="size-3.5" aria-hidden /> : null}
      {formatPercent(value)}
    </span>
  )
}
