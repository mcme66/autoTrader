import type { ReactNode } from 'react'

import { cn } from '@/lib/cn'

export type BadgeTone = 'neutral' | 'brand' | 'gain' | 'loss' | 'warn'

type Tone = BadgeTone

const toneClasses: Record<Tone, string> = {
  neutral: 'bg-surface-muted text-content-muted',
  brand: 'bg-brand-soft text-brand',
  gain: 'bg-gain-soft text-gain',
  loss: 'bg-loss-soft text-loss',
  warn: 'bg-warn-soft text-warn',
}

export function Badge({
  tone = 'neutral',
  children,
  className,
}: {
  tone?: Tone
  children: ReactNode
  className?: string
}) {
  return (
    <span
      className={cn(
        'inline-flex items-center rounded-full px-2 py-0.5 text-xs font-medium whitespace-nowrap',
        toneClasses[tone],
        className,
      )}
    >
      {children}
    </span>
  )
}
