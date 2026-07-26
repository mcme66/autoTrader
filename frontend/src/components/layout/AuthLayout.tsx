import { LineChart } from 'lucide-react'
import type { ReactNode } from 'react'

/** Centred single-column shell for the sign-in and registration screens. */
export function AuthLayout({
  title,
  description,
  children,
  footer,
}: {
  title: string
  description: string
  children: ReactNode
  footer?: ReactNode
}) {
  return (
    <div className="flex min-h-svh items-center justify-center px-4 py-10">
      <div className="w-full max-w-sm">
        <div className="mb-8 flex flex-col items-center text-center">
          <LineChart className="text-brand mb-3 size-8" aria-hidden />
          <h1 className="text-content text-lg font-semibold">{title}</h1>
          <p className="text-content-muted mt-1 text-sm">{description}</p>
        </div>

        <div className="bg-surface border-border rounded-card border p-6 shadow-sm">
          {children}
        </div>

        {footer ? (
          <div className="text-content-muted mt-5 text-center text-sm">
            {footer}
          </div>
        ) : null}
      </div>
    </div>
  )
}
