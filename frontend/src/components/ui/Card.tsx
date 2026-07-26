import type { HTMLAttributes, ReactNode } from 'react'

import { cn } from '@/lib/cn'

export function Card({
  className,
  ...props
}: HTMLAttributes<HTMLDivElement>) {
  return (
    <div
      className={cn(
        'bg-surface border-border rounded-card border shadow-sm',
        className,
      )}
      {...props}
    />
  )
}

export function CardHeader({
  title,
  description,
  action,
  className,
}: {
  title: ReactNode
  description?: ReactNode
  action?: ReactNode
  className?: string
}) {
  return (
    <div
      className={cn(
        'border-border flex items-start justify-between gap-4 border-b px-5 py-4',
        className,
      )}
    >
      <div className="min-w-0">
        <h2 className="text-content truncate text-sm font-semibold">{title}</h2>
        {description ? (
          <p className="text-content-muted mt-0.5 text-xs">{description}</p>
        ) : null}
      </div>
      {action ? <div className="shrink-0">{action}</div> : null}
    </div>
  )
}

export function CardBody({
  className,
  ...props
}: HTMLAttributes<HTMLDivElement>) {
  return <div className={cn('p-5', className)} {...props} />
}
