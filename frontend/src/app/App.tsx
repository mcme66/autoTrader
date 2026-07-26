import { QueryClientProvider } from '@tanstack/react-query'
import { RouterProvider } from 'react-router'

import { AuthProvider } from '@/features/auth/AuthProvider'
import { ThemeProvider } from '@/features/theme/ThemeProvider'
import { queryClient } from '@/lib/query-client'
import { router } from './router'

/**
 * Provider order is load-bearing: AuthProvider clears the query cache on sign-out, so it has
 * to sit inside QueryClientProvider.
 */
export default function App() {
  return (
    <ThemeProvider>
      <QueryClientProvider client={queryClient}>
        <AuthProvider>
          <RouterProvider router={router} />
        </AuthProvider>
      </QueryClientProvider>
    </ThemeProvider>
  )
}
