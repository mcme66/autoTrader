import { Compass } from 'lucide-react'
import { Link } from 'react-router'

import { EmptyState } from '@/components/ui/EmptyState'

export function NotFoundPage() {
  return (
    <EmptyState
      icon={Compass}
      title="Page not found"
      description="That URL does not match anything in this application."
      action={
        <Link
          to="/"
          className="bg-brand text-brand-content hover:bg-brand-hover inline-flex h-10 items-center rounded-lg px-4 text-sm font-medium transition-colors"
        >
          Back to dashboard
        </Link>
      }
    />
  )
}
