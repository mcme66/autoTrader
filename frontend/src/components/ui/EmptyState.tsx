import type { LucideIcon } from 'lucide-react'
import { Inbox } from 'lucide-react'
import type { ReactNode } from 'react'

import { cn } from '@/lib/cn'

/**
 * The single placeholder used everywhere data can legitimately be absent.
 *
 * "No data yet" and "something failed" are different states and are styled differently, so a
 * user can tell an untrained ML pipeline apart from a broken request.
 */
export function EmptyState({
  icon: Icon = Inbox,
  title,
  description,
  action,
  className,
}: {
  icon?: LucideIcon
  title: string
  description?: ReactNode
  action?: ReactNode
  className?: string
}) {
  return (
    <div
      className={cn(
        'flex flex-col items-center justify-center px-6 py-14 text-center',
        className,
      )}
    >
      <div className="bg-surface-muted text-content-subtle mb-4 rounded-full p-3">
        <Icon className="size-6" aria-hidden />
      </div>
      <h3 className="text-content text-sm font-semibold">{title}</h3>
      {description ? (
        <p className="text-content-muted mt-1.5 max-w-md text-sm">
          {description}
        </p>
      ) : null}
      {action ? <div className="mt-5">{action}</div> : null}
    </div>
  )
}
