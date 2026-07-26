import type { ReactNode } from 'react'
import { Navigate, useLocation } from 'react-router'

import { Spinner } from '@/components/ui/Spinner'
import { useAuth } from '@/features/auth/useAuth'

function FullPageSpinner() {
  return (
    <div className="flex min-h-svh items-center justify-center">
      <Spinner className="text-brand size-8" />
    </div>
  )
}

/**
 * Blocks anonymous access.
 *
 * Rendering the spinner while the silent refresh is in flight matters: without it, every
 * reload would briefly redirect an authenticated user to the login page before bouncing them
 * back, losing their scroll position and flashing the wrong screen.
 */
export function RequireAuth({ children }: { children: ReactNode }) {
  const { user, isInitializing } = useAuth()
  const location = useLocation()

  if (isInitializing) {
    return <FullPageSpinner />
  }

  if (!user) {
    return <Navigate to="/login" replace state={{ from: location.pathname }} />
  }

  return <>{children}</>
}

/** Keeps a signed-in user off the login and register screens. */
export function RequireAnonymous({ children }: { children: ReactNode }) {
  const { user, isInitializing } = useAuth()

  if (isInitializing) {
    return <FullPageSpinner />
  }

  if (user) {
    return <Navigate to="/" replace />
  }

  return <>{children}</>
}
