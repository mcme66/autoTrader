import { AlertTriangle } from 'lucide-react'
import type { ReactNode } from 'react'

import { errorMessage } from '@/lib/api-client'
import { Button } from './Button'
import { EmptyState } from './EmptyState'

/**
 * The loading and error halves of a query, in one place.
 *
 * Every page needs the same three-way branch, and writing it inline each time is how one
 * screen ends up silently rendering nothing on failure.
 */
export function QueryState({
  isLoading,
  error,
  onRetry,
  skeleton,
  children,
}: {
  isLoading: boolean
  error: unknown
  onRetry?: () => void
  skeleton: ReactNode
  children: ReactNode
}) {
  if (isLoading) {
    return <>{skeleton}</>
  }

  if (error) {
    return (
      <EmptyState
        icon={AlertTriangle}
        title="Could not load this data"
        description={errorMessage(error)}
        action={
          onRetry ? (
            <Button variant="secondary" onClick={onRetry}>
              Try again
            </Button>
          ) : undefined
        }
      />
    )
  }

  return <>{children}</>
}
