import { ChevronLeft, ChevronRight } from 'lucide-react'

import { Button } from './Button'

/** Page N of M with previous/next controls, driven entirely by the API's paging metadata. */
export function Pagination({
  page,
  totalPages,
  totalCount,
  hasPreviousPage,
  hasNextPage,
  onChange,
}: {
  page: number
  totalPages: number
  totalCount: number
  hasPreviousPage: boolean
  hasNextPage: boolean
  onChange: (page: number) => void
}) {
  if (totalPages <= 1) {
    return null
  }

  return (
    <div className="border-border flex items-center justify-between gap-4 border-t px-5 py-3">
      <p className="text-content-muted text-xs">
        Page {page} of {totalPages} · {totalCount.toLocaleString('en-US')}{' '}
        results
      </p>
      <div className="flex gap-2">
        <Button
          variant="secondary"
          size="sm"
          disabled={!hasPreviousPage}
          onClick={() => onChange(page - 1)}
          icon={<ChevronLeft className="size-4" />}
        >
          Previous
        </Button>
        <Button
          variant="secondary"
          size="sm"
          disabled={!hasNextPage}
          onClick={() => onChange(page + 1)}
        >
          Next
          <ChevronRight className="size-4" />
        </Button>
      </div>
    </div>
  )
}
