import type { HTMLAttributes, ThHTMLAttributes, TdHTMLAttributes } from 'react'

import { cn } from '@/lib/cn'

export function Table({
  className,
  ...props
}: HTMLAttributes<HTMLTableElement>) {
  return (
    <div className="scrollbar-thin w-full overflow-x-auto">
      <table
        className={cn('w-full border-collapse text-sm', className)}
        {...props}
      />
    </div>
  )
}

export function Thead({
  className,
  ...props
}: HTMLAttributes<HTMLTableSectionElement>) {
  return (
    <thead
      className={cn(
        'text-content-muted border-border border-b text-xs',
        className,
      )}
      {...props}
    />
  )
}

export function Tbody({
  className,
  ...props
}: HTMLAttributes<HTMLTableSectionElement>) {
  return <tbody className={cn('divide-border divide-y', className)} {...props} />
}

export function Tr({
  className,
  ...props
}: HTMLAttributes<HTMLTableRowElement>) {
  return <tr className={cn('hover:bg-surface-muted/60', className)} {...props} />
}

export function Th({
  className,
  numeric,
  ...props
}: ThHTMLAttributes<HTMLTableCellElement> & { numeric?: boolean }) {
  return (
    <th
      scope="col"
      className={cn(
        'px-4 py-2.5 font-medium whitespace-nowrap',
        numeric ? 'text-right' : 'text-left',
        className,
      )}
      {...props}
    />
  )
}

export function Td({
  className,
  numeric,
  ...props
}: TdHTMLAttributes<HTMLTableCellElement> & { numeric?: boolean }) {
  return (
    <td
      className={cn(
        'px-4 py-2.5 whitespace-nowrap',
        numeric && 'tabular text-right',
        className,
      )}
      {...props}
    />
  )
}
