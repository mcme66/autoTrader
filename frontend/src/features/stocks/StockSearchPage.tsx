import { Search } from 'lucide-react'
import { useEffect, useState } from 'react'
import { Link, useSearchParams } from 'react-router'

import { Badge } from '@/components/ui/Badge'
import { Card } from '@/components/ui/Card'
import { ChangePill } from '@/components/ui/ChangePill'
import { EmptyState } from '@/components/ui/EmptyState'
import { Input, Select } from '@/components/ui/Input'
import { PageHeader } from '@/components/ui/PageHeader'
import { Pagination } from '@/components/ui/Pagination'
import { QueryState } from '@/components/ui/QueryState'
import { SkeletonTable } from '@/components/ui/Skeleton'
import { Table, Tbody, Td, Th, Thead, Tr } from '@/components/ui/Table'
import { formatCompact, formatCurrency } from '@/lib/format'
import { useDebouncedValue } from '@/lib/useDebouncedValue'
import { useSectors, useStockSearch } from './stocks-api'

export function StockSearchPage() {
  // The URL is the source of truth for the filters, so a search is shareable and survives a
  // reload; local state only exists to keep the text input responsive while debouncing.
  const [searchParams, setSearchParams] = useSearchParams()
  const [term, setTerm] = useState(() => searchParams.get('q') ?? '')
  const debouncedTerm = useDebouncedValue(term)

  const sector = searchParams.get('sector') ?? ''
  const page = Number(searchParams.get('page') ?? '1')

  useEffect(() => {
    setSearchParams(
      (current) => {
        const next = new URLSearchParams(current)

        if (debouncedTerm) {
          next.set('q', debouncedTerm)
        } else {
          next.delete('q')
        }

        next.delete('page')
        return next
      },
      { replace: true },
    )
  }, [debouncedTerm, setSearchParams])

  const sectors = useSectors()
  const results = useStockSearch({
    query: debouncedTerm || undefined,
    sector: sector || undefined,
    page,
    pageSize: 25,
  })

  const updateParam = (key: string, value: string) => {
    setSearchParams((current) => {
      const next = new URLSearchParams(current)

      if (value) {
        next.set(key, value)
      } else {
        next.delete(key)
      }

      if (key !== 'page') {
        next.delete('page')
      }

      return next
    })
  }

  return (
    <>
      <PageHeader
        title="Stock Search"
        description="Every symbol in the tracked universe, with its most recent close."
      />

      <Card>
        <div className="border-border grid gap-3 border-b p-5 sm:grid-cols-[1fr_14rem]">
          <Input
            placeholder="Search by symbol or company name"
            aria-label="Search stocks"
            leading={<Search className="size-4" />}
            value={term}
            onChange={(event) => setTerm(event.target.value)}
          />
          <Select
            aria-label="Filter by sector"
            value={sector}
            onChange={(event) => updateParam('sector', event.target.value)}
          >
            <option value="">All sectors</option>
            {sectors.data?.map((item) => (
              <option key={item.key} value={item.key}>
                {item.name}
              </option>
            ))}
          </Select>
        </div>

        <QueryState
          isLoading={results.isPending}
          error={results.error}
          onRetry={() => void results.refetch()}
          skeleton={<SkeletonTable rows={8} columns={5} />}
        >
          {results.data && results.data.items.length > 0 ? (
            <>
              <Table>
                <Thead>
                  <Tr>
                    <Th>Symbol</Th>
                    <Th>Company</Th>
                    <Th>Sector</Th>
                    <Th numeric>Close</Th>
                    <Th numeric>Change</Th>
                    <Th numeric>Volume</Th>
                  </Tr>
                </Thead>
                <Tbody>
                  {results.data.items.map((stock) => (
                    <Tr key={stock.symbol}>
                      <Td>
                        <Link
                          to={`/stocks/${stock.symbol}`}
                          className="text-content hover:text-brand font-medium"
                        >
                          {stock.symbol}
                        </Link>
                      </Td>
                      <Td className="text-content-muted max-w-64 truncate">
                        {stock.companyName}
                      </Td>
                      <Td>
                        {stock.sectorName ? (
                          <Badge>{stock.sectorName}</Badge>
                        ) : (
                          <span className="text-content-subtle">—</span>
                        )}
                      </Td>
                      <Td numeric>{formatCurrency(stock.latestClose)}</Td>
                      <Td numeric>
                        <ChangePill
                          value={stock.changePercent}
                          showIcon={false}
                        />
                      </Td>
                      <Td numeric className="text-content-muted">
                        {formatCompact(stock.latestVolume)}
                      </Td>
                    </Tr>
                  ))}
                </Tbody>
              </Table>

              <Pagination
                page={results.data.page}
                totalPages={results.data.totalPages}
                totalCount={results.data.totalCount}
                hasPreviousPage={results.data.hasPreviousPage}
                hasNextPage={results.data.hasNextPage}
                onChange={(next) => updateParam('page', String(next))}
              />
            </>
          ) : (
            <EmptyState
              icon={Search}
              title="No matches"
              description="No tracked symbol matches those filters. Try a broader search."
            />
          )}
        </QueryState>
      </Card>
    </>
  )
}
