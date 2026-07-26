import { useQueryClient } from '@tanstack/react-query'
import { useCallback, useEffect, useMemo, useState, type ReactNode } from 'react'

import { setAccessToken, setUnauthenticatedHandler } from '@/lib/api-client'
import type { AuthenticatedUser } from '@/types/api'
import { authApi } from './auth-api'
import { AuthContext, type AuthContextValue } from './auth-context'

const ADMINISTRATOR_ROLE = 'Administrator'

/**
 * Owns the session.
 *
 * On mount it attempts a silent refresh: the access token only lives in memory, so a page
 * reload has nothing to restore from except the httpOnly refresh cookie. A failure there is
 * the normal "not signed in" case, not an error worth surfacing.
 */
export function AuthProvider({ children }: { children: ReactNode }) {
  const queryClient = useQueryClient()
  const [user, setUser] = useState<AuthenticatedUser | null>(null)
  const [isInitializing, setIsInitializing] = useState(true)

  const clearSession = useCallback(() => {
    setAccessToken(null)
    setUser(null)
    queryClient.clear()
  }, [queryClient])

  useEffect(() => {
    setUnauthenticatedHandler(clearSession)
    return () => setUnauthenticatedHandler(null)
  }, [clearSession])

  useEffect(() => {
    let cancelled = false

    void authApi
      .refresh()
      .then((result) => {
        if (cancelled) {
          return
        }
        setAccessToken(result.accessToken)
        setUser(result.user)
      })
      .catch(() => {
        if (!cancelled) {
          setAccessToken(null)
        }
      })
      .finally(() => {
        if (!cancelled) {
          setIsInitializing(false)
        }
      })

    return () => {
      cancelled = true
    }
  }, [])

  const login = useCallback(
    async (email: string, password: string) => {
      const result = await authApi.login({ email, password })
      setAccessToken(result.accessToken)
      setUser(result.user)
      // A previous user's cached data must never surface under a new session.
      queryClient.clear()
    },
    [queryClient],
  )

  const register = useCallback(
    async (email: string, password: string, displayName: string) => {
      const result = await authApi.register({ email, password, displayName })
      setAccessToken(result.accessToken)
      setUser(result.user)
      queryClient.clear()
    },
    [queryClient],
  )

  const logout = useCallback(async () => {
    try {
      await authApi.logout()
    } finally {
      // Local state is cleared even if the server call fails, because a user who clicked
      // "sign out" must end up signed out of this browser regardless.
      clearSession()
    }
  }, [clearSession])

  const value = useMemo<AuthContextValue>(
    () => ({
      user,
      isInitializing,
      isAdministrator: user?.roles.includes(ADMINISTRATOR_ROLE) ?? false,
      login,
      register,
      logout,
      setUser,
    }),
    [user, isInitializing, login, register, logout],
  )

  return <AuthContext value={value}>{children}</AuthContext>
}
