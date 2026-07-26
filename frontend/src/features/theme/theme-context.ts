import { createContext } from 'react'

export type ThemePreference = 'light' | 'dark' | 'system'

export interface ThemeContextValue {
  preference: ThemePreference
  /** What is actually on screen once `system` has been resolved. */
  resolved: 'light' | 'dark'
  setPreference: (preference: ThemePreference) => void
}

export const ThemeContext = createContext<ThemeContextValue | null>(null)

export const THEME_STORAGE_KEY = 'fap.theme'
