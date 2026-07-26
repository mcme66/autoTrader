import { createContext } from 'react'

import type { AuthenticatedUser } from '@/types/api'

export interface AuthContextValue {
  user: AuthenticatedUser | null
  /** True until the initial silent refresh settles, so routes do not flash the login page. */
  isInitializing: boolean
  isAdministrator: boolean
  login: (email: string, password: string) => Promise<void>
  register: (
    email: string,
    password: string,
    displayName: string,
  ) => Promise<void>
  logout: () => Promise<void>
  setUser: (user: AuthenticatedUser) => void
}

/**
 * Split from the provider component so the module exports only a constant. Fast Refresh
 * cannot preserve state for a module that exports both a component and other values.
 */
export const AuthContext = createContext<AuthContextValue | null>(null)
