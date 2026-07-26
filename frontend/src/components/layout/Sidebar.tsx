import {
  BarChart3,
  Briefcase,
  LayoutDashboard,
  LineChart,
  Search,
  Settings,
  Sparkles,
  X,
} from 'lucide-react'
import { NavLink } from 'react-router'

import { cn } from '@/lib/cn'
import { Button } from '@/components/ui/Button'

const navigation = [
  { to: '/', label: 'Dashboard', icon: LayoutDashboard, end: true },
  { to: '/portfolio', label: 'Portfolio', icon: Briefcase, end: false },
  { to: '/market', label: 'Market Overview', icon: BarChart3, end: false },
  { to: '/stocks', label: 'Stock Search', icon: Search, end: true },
  {
    to: '/recommendations',
    label: 'ML Recommendations',
    icon: Sparkles,
    end: false,
  },
  { to: '/settings', label: 'Settings', icon: Settings, end: false },
] as const

export function Sidebar({
  open,
  onClose,
}: {
  open: boolean
  onClose: () => void
}) {
  return (
    <>
      {/* Scrim exists only on mobile, where the sidebar overlays content. */}
      {open ? (
        <button
          type="button"
          aria-label="Close navigation"
          onClick={onClose}
          className="fixed inset-0 z-30 bg-black/40 lg:hidden"
        />
      ) : null}

      <aside
        className={cn(
          'bg-surface border-border fixed inset-y-0 left-0 z-40 flex w-64 flex-col border-r transition-transform duration-200',
          'lg:translate-x-0',
          open ? 'translate-x-0' : '-translate-x-full',
        )}
      >
        <div className="border-border flex h-14 items-center justify-between border-b px-4">
          <span className="flex items-center gap-2">
            <LineChart className="text-brand size-5" aria-hidden />
            <span className="text-content text-sm font-semibold">
              Finance Analysis
            </span>
          </span>
          <Button
            variant="ghost"
            size="sm"
            className="lg:hidden"
            onClick={onClose}
            aria-label="Close navigation"
          >
            <X className="size-4" />
          </Button>
        </div>

        <nav className="flex-1 space-y-1 overflow-y-auto p-3">
          {navigation.map(({ to, label, icon: Icon, end }) => (
            <NavLink
              key={to}
              to={to}
              end={end}
              onClick={onClose}
              className={({ isActive }) =>
                cn(
                  'flex items-center gap-3 rounded-lg px-3 py-2 text-sm font-medium transition-colors',
                  isActive
                    ? 'bg-brand-soft text-brand'
                    : 'text-content-muted hover:bg-surface-muted hover:text-content',
                )
              }
            >
              <Icon className="size-4 shrink-0" aria-hidden />
              {label}
            </NavLink>
          ))}
        </nav>

        <div className="border-border text-content-subtle border-t px-4 py-3 text-xs">
          Predictions are produced by a separate ML pipeline.
        </div>
      </aside>
    </>
  )
}
