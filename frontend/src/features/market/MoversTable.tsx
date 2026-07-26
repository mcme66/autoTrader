import { Link } from 'react-router'

import { ChangePill } from '@/components/ui/ChangePill'
import { EmptyState } from '@/components/ui/EmptyState'
import { Table, Tbody, Td, Th, Thead, Tr } from '@/components/ui/Table'
import { formatCompact, formatCurrency } from '@/lib/format'
import type { Stock } from '@/types/api'

/** Compact symbol / price / change / volume table shared by the movers panels. */
export function MoversTable({
  stocks,
  emptyMessage = 'No price data for this trading day yet.',
}: {
  stocks: Stock[]
  emptyMessage?: string
}) {
  if (stocks.length === 0) {
    return <EmptyState title="Nothing to show" description={emptyMessage} />
  }

  return (
    <Table>
      <Thead>
        <Tr>
          <Th>Symbol</Th>
          <Th numeric>Close</Th>
          <Th numeric>Change</Th>
          <Th numeric>Volume</Th>
        </Tr>
      </Thead>
      <Tbody>
        {stocks.map((stock) => (
          <Tr key={stock.symbol}>
            <Td>
              <Link
                to={`/stocks/${stock.symbol}`}
                className="text-content hover:text-brand font-medium"
              >
                {stock.symbol}
              </Link>
              <span className="text-content-subtle ml-2 hidden text-xs sm:inline">
                {stock.companyName}
              </span>
            </Td>
            <Td numeric>{formatCurrency(stock.latestClose)}</Td>
            <Td numeric>
              <ChangePill value={stock.changePercent} showIcon={false} />
            </Td>
            <Td numeric className="text-content-muted">
              {formatCompact(stock.latestVolume)}
            </Td>
          </Tr>
        ))}
      </Tbody>
    </Table>
  )
}
