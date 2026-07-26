import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'

import { apiClient } from '@/lib/api-client'
import { queryKeys } from '@/lib/query-client'
import type {
  PagedResult,
  PriceHistory,
  Recommendation,
  Sector,
  Stock,
  StockDetail,
} from '@/types/api'

export type StockSortOrder = 'Symbol' | 'CompanyName' | 'Sector'

export interface StockSearchParams {
  query?: string
  sector?: string
  trackedOnly?: boolean
  sortBy?: StockSortOrder
  descending?: boolean
  page?: number
  pageSize?: number
}

export function useSectors() {
  return useQuery({
    queryKey: queryKeys.sectors,
    queryFn: async () => {
      const { data } = await apiClient.get<Sector[]>('/sectors')
      return data
    },
    // The GICS taxonomy is seeded by migration and effectively immutable at runtime.
    staleTime: Infinity,
  })
}

export function useStockSearch(params: StockSearchParams) {
  return useQuery({
    queryKey: queryKeys.stocks(params),
    queryFn: async () => {
      const { data } = await apiClient.get<PagedResult<Stock>>('/stocks', {
        params,
      })
      return data
    },
    // Keeps the previous page on screen while the next one loads, so the table does not
    // collapse to a skeleton on every pagination click.
    placeholderData: (previous) => previous,
  })
}

export function useStock(symbol: string) {
  return useQuery({
    queryKey: queryKeys.stock(symbol),
    queryFn: async () => {
      const { data } = await apiClient.get<StockDetail>(`/stocks/${symbol}`)
      return data
    },
    enabled: symbol.length > 0,
  })
}

export function useStockPrices(symbol: string, from?: string, to?: string) {
  return useQuery({
    queryKey: queryKeys.stockPrices(symbol, from, to),
    queryFn: async () => {
      const { data } = await apiClient.get<PriceHistory>(
        `/stocks/${symbol}/prices`,
        { params: { from, to } },
      )
      return data
    },
    enabled: symbol.length > 0,
  })
}

export function useStockPredictions(symbol: string) {
  return useQuery({
    queryKey: queryKeys.stockPredictions(symbol),
    queryFn: async () => {
      const { data } = await apiClient.get<Recommendation[]>(
        `/stocks/${symbol}/predictions`,
      )
      return data
    },
    enabled: symbol.length > 0,
  })
}

export function useSetTracking(symbol: string) {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: async (isTracked: boolean) => {
      const { data } = await apiClient.patch<Stock>(
        `/stocks/${symbol}/tracking`,
        { isTracked },
      )
      return data
    },
    onSuccess: () => {
      void queryClient.invalidateQueries({ queryKey: ['stocks'] })
    },
  })
}
