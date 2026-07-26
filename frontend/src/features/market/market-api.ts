import { useQuery } from '@tanstack/react-query'

import { apiClient } from '@/lib/api-client'
import { queryKeys } from '@/lib/query-client'
import type { MarketOverview } from '@/types/api'

export function useMarketOverview(movers = 5) {
  return useQuery({
    queryKey: queryKeys.marketOverview(movers),
    queryFn: async () => {
      const { data } = await apiClient.get<MarketOverview>('/market/overview', {
        params: { movers },
      })
      return data
    },
  })
}
