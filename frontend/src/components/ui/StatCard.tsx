import type { LucideIcon } from 'lucide-react'
import type { ReactNode } from 'react'

import { cn } from '@/lib/cn'
import { changeTextClass } from '@/lib/format'
import { Card } from './Card'

/** A single headline figure with an optional signed delta beneath it. */
export function StatCard({
  label,
  value,
  delta,
  deltaValue,
  icon: Icon,
  footnote,
  className,
}: {
  label: string
  value: ReactNode
  delta?: ReactNode
  deltaValue?: number | null
  icon?: LucideIcon
  footnote?: ReactNode
  className?: string
}) {
  return (
    <Card className={cn('p-5', className)}>
      <div className="flex items-start justify-between gap-3">
        <p className="text-content-muted text-xs font-medium">{label}</p>
        {Icon ? (
          <Icon className="text-content-subtle size-4 shrink-0" aria-hidden />
        ) : null}
      </div>
      <p className="text-content tabular mt-2 text-2xl font-semibold">
        {value}
      </p>
      {delta ? (
        <p className={cn('tabular mt-1 text-xs', changeTextClass(deltaValue))}>
          {delta}
        </p>
      ) : null}
      {footnote ? (
        <p className="text-content-subtle mt-1 text-xs">{footnote}</p>
      ) : null}
    </Card>
  )
}
