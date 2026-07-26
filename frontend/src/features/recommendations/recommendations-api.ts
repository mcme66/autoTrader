import { useQuery } from '@tanstack/react-query'

import { apiClient } from '@/lib/api-client'
import { queryKeys } from '@/lib/query-client'
import type { Recommendations } from '@/types/api'

export function useRecommendations(model: string | null, page: number) {
  return useQuery({
    queryKey: queryKeys.recommendations(model, page),
    queryFn: async () => {
      const { data } = await apiClient.get<Recommendations>(
        '/recommendations',
        { params: { model: model ?? undefined, page, pageSize: 25 } },
      )
      return data
    },
    placeholderData: (previous) => previous,
  })
}
