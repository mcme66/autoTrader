import { useCallback, useEffect, useMemo, useState, type ReactNode } from 'react'

import {
  THEME_STORAGE_KEY,
  ThemeContext,
  type ThemeContextValue,
  type ThemePreference,
} from './theme-context'

function readStoredPreference(): ThemePreference {
  const stored = localStorage.getItem(THEME_STORAGE_KEY)
  return stored === 'light' || stored === 'dark' ? stored : 'system'
}

export function ThemeProvider({ children }: { children: ReactNode }) {
  const [preference, setPreferenceState] =
    useState<ThemePreference>(readStoredPreference)
  const [systemPrefersDark, setSystemPrefersDark] = useState(
    () => window.matchMedia('(prefers-color-scheme: dark)').matches,
  )

  useEffect(() => {
    const query = window.matchMedia('(prefers-color-scheme: dark)')
    const onChange = (event: MediaQueryListEvent) =>
      setSystemPrefersDark(event.matches)

    query.addEventListener('change', onChange)
    return () => query.removeEventListener('change', onChange)
  }, [])

  const resolved = useMemo<'light' | 'dark'>(() => {
    if (preference === 'system') {
      return systemPrefersDark ? 'dark' : 'light'
    }
    return preference
  }, [preference, systemPrefersDark])

  useEffect(() => {
    document.documentElement.classList.toggle('dark', resolved === 'dark')
  }, [resolved])

  const setPreference = useCallback((next: ThemePreference) => {
    setPreferenceState(next)

    // 'system' is the absence of a choice, so it is stored as the absence of a key.
    if (next === 'system') {
      localStorage.removeItem(THEME_STORAGE_KEY)
    } else {
      localStorage.setItem(THEME_STORAGE_KEY, next)
    }
  }, [])

  const value = useMemo<ThemeContextValue>(
    () => ({ preference, resolved, setPreference }),
    [preference, resolved, setPreference],
  )

  return <ThemeContext value={value}>{children}</ThemeContext>
}
