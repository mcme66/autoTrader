import { LogOut, Menu } from 'lucide-react'
import { useState } from 'react'
import { Outlet } from 'react-router'

import { Button } from '@/components/ui/Button'
import { useAuth } from '@/features/auth/useAuth'
import { Sidebar } from './Sidebar'
import { ThemeToggle } from './ThemeToggle'

export function AppLayout() {
  const { user, logout } = useAuth()
  const [navOpen, setNavOpen] = useState(false)

  return (
    <div className="min-h-svh">
      <Sidebar open={navOpen} onClose={() => setNavOpen(false)} />

      <div className="lg:pl-64">
        <header className="bg-surface/80 border-border sticky top-0 z-20 flex h-14 items-center justify-between gap-3 border-b px-4 backdrop-blur">
          <Button
            variant="ghost"
            size="sm"
            className="lg:hidden"
            onClick={() => setNavOpen(true)}
            aria-label="Open navigation"
          >
            <Menu className="size-4" />
          </Button>

          <div className="ml-auto flex items-center gap-3">
            <ThemeToggle />
            <div className="hidden text-right sm:block">
              <p className="text-content text-xs font-medium">
                {user?.displayName}
              </p>
              <p className="text-content-subtle text-xs">{user?.email}</p>
            </div>
            <Button
              variant="ghost"
              size="sm"
              onClick={() => void logout()}
              icon={<LogOut className="size-4" />}
            >
              <span className="hidden sm:inline">Sign out</span>
            </Button>
          </div>
        </header>

        <main className="mx-auto max-w-7xl px-4 py-6 sm:px-6 lg:px-8">
          <Outlet />
        </main>
      </div>
    </div>
  )
}
