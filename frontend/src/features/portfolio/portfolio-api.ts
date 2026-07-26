import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'

import { apiClient } from '@/lib/api-client'
import { queryKeys } from '@/lib/query-client'
import type { Holding, Portfolio, PortfolioSummary } from '@/types/api'

export interface CreatePortfolioPayload {
  name: string
  description?: string | null
  baseCurrency?: string | null
  isDefault: boolean
}

export interface UpdatePortfolioPayload {
  name: string
  description?: string | null
  isDefault: boolean
}

export interface CreateHoldingPayload {
  symbol: string
  quantity: number
  averageCost: number
  openedOn?: string | null
  notes?: string | null
}

export interface UpdateHoldingPayload {
  quantity: number
  averageCost: number
  openedOn?: string | null
  notes?: string | null
}

export function usePortfolios() {
  return useQuery({
    queryKey: queryKeys.portfolios,
    queryFn: async () => {
      const { data } = await apiClient.get<Portfolio[]>('/portfolios')
      return data
    },
  })
}

export function usePortfolioSummary(portfolioId: string | undefined) {
  return useQuery({
    queryKey: queryKeys.portfolioSummary(portfolioId ?? ''),
    queryFn: async () => {
      const { data } = await apiClient.get<PortfolioSummary>(
        `/portfolios/${portfolioId}/summary`,
      )
      return data
    },
    enabled: Boolean(portfolioId),
  })
}

/**
 * The dashboard's portfolio panel. The endpoint answers 204 for a user with no portfolios,
 * which axios surfaces as an empty body — mapped to null so the caller renders the
 * "create your first portfolio" state instead of an error.
 */
export function useDefaultPortfolioSummary() {
  return useQuery({
    queryKey: queryKeys.defaultPortfolioSummary,
    queryFn: async () => {
      const response = await apiClient.get<PortfolioSummary | ''>(
        '/portfolios/default/summary',
      )
      return response.status === 204 || response.data === ''
        ? null
        : response.data
    },
  })
}

/** Everything portfolio-shaped, so a mutation cannot leave a stale panel behind. */
function invalidatePortfolios(queryClient: ReturnType<typeof useQueryClient>) {
  void queryClient.invalidateQueries({ queryKey: ['portfolios'] })
}

export function useCreatePortfolio() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: async (payload: CreatePortfolioPayload) => {
      const { data } = await apiClient.post<Portfolio>('/portfolios', payload)
      return data
    },
    onSuccess: () => invalidatePortfolios(queryClient),
  })
}

export function useUpdatePortfolio(portfolioId: string) {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: async (payload: UpdatePortfolioPayload) => {
      const { data } = await apiClient.put<Portfolio>(
        `/portfolios/${portfolioId}`,
        payload,
      )
      return data
    },
    onSuccess: () => invalidatePortfolios(queryClient),
  })
}

export function useDeletePortfolio() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: async (portfolioId: string) => {
      await apiClient.delete(`/portfolios/${portfolioId}`)
    },
    onSuccess: () => invalidatePortfolios(queryClient),
  })
}

export function useAddHolding(portfolioId: string) {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: async (payload: CreateHoldingPayload) => {
      const { data } = await apiClient.post<Holding>(
        `/portfolios/${portfolioId}/holdings`,
        payload,
      )
      return data
    },
    onSuccess: () => invalidatePortfolios(queryClient),
  })
}

export function useUpdateHolding(portfolioId: string) {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: async ({
      holdingId,
      payload,
    }: {
      holdingId: string
      payload: UpdateHoldingPayload
    }) => {
      const { data } = await apiClient.put<Holding>(
        `/portfolios/${portfolioId}/holdings/${holdingId}`,
        payload,
      )
      return data
    },
    onSuccess: () => invalidatePortfolios(queryClient),
  })
}

export function useRemoveHolding(portfolioId: string) {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: async (holdingId: string) => {
      await apiClient.delete(`/portfolios/${portfolioId}/holdings/${holdingId}`)
    },
    onSuccess: () => invalidatePortfolios(queryClient),
  })
}
