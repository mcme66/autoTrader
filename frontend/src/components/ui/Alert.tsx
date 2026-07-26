import { AlertTriangle, CheckCircle2, Info, XCircle } from 'lucide-react'
import type { ReactNode } from 'react'

import { cn } from '@/lib/cn'

type Tone = 'info' | 'success' | 'warning' | 'error'

const config = {
  info: { icon: Info, classes: 'bg-brand-soft text-brand' },
  success: { icon: CheckCircle2, classes: 'bg-gain-soft text-gain' },
  warning: { icon: AlertTriangle, classes: 'bg-warn-soft text-warn' },
  error: { icon: XCircle, classes: 'bg-loss-soft text-loss' },
} as const satisfies Record<Tone, { icon: unknown; classes: string }>

export function Alert({
  tone = 'info',
  title,
  children,
  className,
}: {
  tone?: Tone
  title?: string
  children?: ReactNode
  className?: string
}) {
  const { icon: Icon, classes } = config[tone]

  return (
    <div
      role={tone === 'error' ? 'alert' : 'status'}
      className={cn('flex gap-3 rounded-lg p-3 text-sm', classes, className)}
    >
      <Icon className="mt-0.5 size-4 shrink-0" aria-hidden />
      <div className="min-w-0">
        {title ? <p className="font-medium">{title}</p> : null}
        {children ? <div className="opacity-90">{children}</div> : null}
      </div>
    </div>
  )
}
