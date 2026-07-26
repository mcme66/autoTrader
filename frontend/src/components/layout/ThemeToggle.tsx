import { Monitor, Moon, Sun } from 'lucide-react'

import { cn } from '@/lib/cn'
import { useTheme } from '@/features/theme/useTheme'
import type { ThemePreference } from '@/features/theme/theme-context'

const options = [
  { value: 'light', label: 'Light', icon: Sun },
  { value: 'system', label: 'System', icon: Monitor },
  { value: 'dark', label: 'Dark', icon: Moon },
] as const satisfies ReadonlyArray<{
  value: ThemePreference
  label: string
  icon: unknown
}>

export function ThemeToggle() {
  const { preference, setPreference } = useTheme()

  return (
    <div
      role="radiogroup"
      aria-label="Colour theme"
      className="bg-surface-muted flex gap-0.5 rounded-lg p-0.5"
    >
      {options.map(({ value, label, icon: Icon }) => (
        <button
          key={value}
          type="button"
          role="radio"
          aria-checked={preference === value}
          aria-label={label}
          title={label}
          onClick={() => setPreference(value)}
          className={cn(
            'rounded-md p-1.5 transition-colors',
            preference === value
              ? 'bg-surface text-content shadow-sm'
              : 'text-content-subtle hover:text-content',
          )}
        >
          <Icon className="size-4" aria-hidden />
        </button>
      ))}
    </div>
  )
}
