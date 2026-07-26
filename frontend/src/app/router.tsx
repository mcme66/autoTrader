import { createBrowserRouter } from 'react-router'

import { AppLayout } from '@/components/layout/AppLayout'
import { NotFoundPage } from '@/components/layout/NotFoundPage'
import {
  RequireAnonymous,
  RequireAuth,
} from '@/components/layout/RouteGuards'

/**
 * Pages are loaded per route.
 *
 * Charting and form libraries are only needed on a few screens, and eager-importing them put
 * every page's dependencies in the initial bundle. React Router resolves `lazy` before it
 * renders the route, so there is no flash of an empty layout.
 */
export const router = createBrowserRouter([
  {
    path: '/login',
    lazy: async () => {
      const { LoginPage } = await import('@/features/auth/LoginPage')
      return {
        element: (
          <RequireAnonymous>
            <LoginPage />
          </RequireAnonymous>
        ),
      }
    },
  },
  {
    path: '/register',
    lazy: async () => {
      const { RegisterPage } = await import('@/features/auth/RegisterPage')
      return {
        element: (
          <RequireAnonymous>
            <RegisterPage />
          </RequireAnonymous>
        ),
      }
    },
  },
  {
    path: '/',
    element: (
      // Authentication is structural: every application route sits under this one guarded
      // layout, so no page has to remember to check for itself.
      <RequireAuth>
        <AppLayout />
      </RequireAuth>
    ),
    children: [
      {
        index: true,
        lazy: async () => ({
          Component: (await import('@/features/dashboard/DashboardPage'))
            .DashboardPage,
        }),
      },
      {
        path: 'portfolio',
        lazy: async () => ({
          Component: (await import('@/features/portfolio/PortfolioPage'))
            .PortfolioPage,
        }),
      },
      {
        path: 'market',
        lazy: async () => ({
          Component: (await import('@/features/market/MarketOverviewPage'))
            .MarketOverviewPage,
        }),
      },
      {
        path: 'stocks',
        lazy: async () => ({
          Component: (await import('@/features/stocks/StockSearchPage'))
            .StockSearchPage,
        }),
      },
      {
        path: 'stocks/:symbol',
        lazy: async () => ({
          Component: (await import('@/features/stocks/StockDetailPage'))
            .StockDetailPage,
        }),
      },
      {
        path: 'recommendations',
        lazy: async () => ({
          Component: (
            await import('@/features/recommendations/RecommendationsPage')
          ).RecommendationsPage,
        }),
      },
      {
        path: 'settings',
        lazy: async () => ({
          Component: (await import('@/features/settings/SettingsPage'))
            .SettingsPage,
        }),
      },
      { path: '*', element: <NotFoundPage /> },
    ],
  },
])
