import { cn } from '@/lib/cn'

export function Skeleton({ className }: { className?: string }) {
  return (
    <div
      className={cn('bg-surface-muted animate-pulse rounded-md', className)}
      aria-hidden
    />
  )
}

/**
 * Table placeholder sized to the real table, so the layout does not jump when data lands.
 */
export function SkeletonTable({
  rows = 5,
  columns = 4,
}: {
  rows?: number
  columns?: number
}) {
  return (
    <div className="space-y-2 p-5" role="status" aria-label="Loading">
      {Array.from({ length: rows }, (_, rowIndex) => (
        <div key={rowIndex} className="flex gap-3">
          {Array.from({ length: columns }, (_, columnIndex) => (
            <Skeleton
              key={columnIndex}
              className={cn('h-6 flex-1', columnIndex === 0 && 'max-w-32')}
            />
          ))}
        </div>
      ))}
    </div>
  )
}

export function SkeletonCards({ count = 4 }: { count?: number }) {
  return (
    <div
      className="grid gap-4 sm:grid-cols-2 xl:grid-cols-4"
      role="status"
      aria-label="Loading"
    >
      {Array.from({ length: count }, (_, index) => (
        <Skeleton key={index} className="h-28" />
      ))}
    </div>
  )
}
