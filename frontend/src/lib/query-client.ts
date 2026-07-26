import { QueryClient } from '@tanstack/react-query'
import axios from 'axios'

/**
 * Market data changes once per trading day, so aggressive refetching would generate load
 * without ever producing different numbers. A one-minute stale window keeps navigation
 * instant while still picking up an ingest that lands mid-session.
 */
export const queryClient = new QueryClient({
  defaultOptions: {
    queries: {
      staleTime: 60_000,
      gcTime: 5 * 60_000,
      refetchOnWindowFocus: false,
      retry: (failureCount, error) => {
        // A 4xx will fail identically on retry; only transient failures are worth repeating.
        if (axios.isAxiosError(error)) {
          const status = error.response?.status ?? 0
          if (status >= 400 && status < 500) {
            return false
          }
        }

        return failureCount < 2
      },
    },
    mutations: {
      retry: false,
    },
  },
})

/** Centralised cache keys so an invalidation cannot silently miss a query. */
export const queryKeys = {
  currentUser: ['currentUser'] as const,
  sectors: ['sectors'] as const,
  marketOverview: (movers: number) => ['market', 'overview', movers] as const,
  stocks: (params: object) => ['stocks', params] as const,
  stock: (symbol: string) => ['stocks', symbol] as const,
  stockPrices: (symbol: string, from?: string, to?: string) =>
    ['stocks', symbol, 'prices', from ?? null, to ?? null] as const,
  stockPredictions: (symbol: string) =>
    ['stocks', symbol, 'predictions'] as const,
  portfolios: ['portfolios'] as const,
  portfolioSummary: (id: string) => ['portfolios', id, 'summary'] as const,
  defaultPortfolioSummary: ['portfolios', 'default', 'summary'] as const,
  recommendations: (model: string | null, page: number) =>
    ['recommendations', model, page] as const,
}
