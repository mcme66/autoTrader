import { subDays, subMonths, subYears } from 'date-fns'
import { ArrowLeft, Brain, ExternalLink, LineChart } from 'lucide-react'
import { useMemo, useState } from 'react'
import { Link, useParams } from 'react-router'

import { Badge } from '@/components/ui/Badge'
import { Card, CardBody, CardHeader } from '@/components/ui/Card'
import { ChangePill } from '@/components/ui/ChangePill'
import { EmptyState } from '@/components/ui/EmptyState'
import { LinkButton } from '@/components/ui/LinkButton'
import { PageHeader } from '@/components/ui/PageHeader'
import { QueryState } from '@/components/ui/QueryState'
import { Skeleton, SkeletonTable } from '@/components/ui/Skeleton'
import { StatCard } from '@/components/ui/StatCard'
import { Table, Tbody, Td, Th, Thead, Tr } from '@/components/ui/Table'
import { cn } from '@/lib/cn'
import {
  formatCompact,
  formatCurrency,
  formatDate,
  formatPercent,
  toIsoDate,
} from '@/lib/format'
import { SignalBadge } from '@/features/recommendations/SignalBadge'
import { PriceChart } from './PriceChart'
import { useStock, useStockPredictions, useStockPrices } from './stocks-api'

const ranges = {
  '1M': (today: Date) => subMonths(today, 1),
  '3M': (today: Date) => subMonths(today, 3),
  '6M': (today: Date) => subMonths(today, 6),
  '1Y': (today: Date) => subYears(today, 1),
  '5Y': (today: Date) => subYears(today, 5),
} as const

type RangeKey = keyof typeof ranges

export function StockDetailPage() {
  const { symbol = '' } = useParams()
  const upperSymbol = symbol.toUpperCase()
  const [range, setRange] = useState<RangeKey>('1Y')

  const from = useMemo(() => {
    const today = new Date()
    return toIsoDate(ranges[range](today))
  }, [range])
  const to = useMemo(() => toIsoDate(subDays(new Date(), 0)), [])

  const stock = useStock(upperSymbol)
  const prices = useStockPrices(upperSymbol, from, to)
  const predictions = useStockPredictions(upperSymbol)

  const summary = stock.data?.summary

  return (
    <>
      <Link
        to="/stocks"
        className="text-content-muted hover:text-content mb-4 inline-flex items-center gap-1.5 text-xs"
      >
        <ArrowLeft className="size-3.5" aria-hidden />
        Back to search
      </Link>

      <QueryState
        isLoading={stock.isLoading}
        error={stock.error}
        onRetry={() => void stock.refetch()}
        skeleton={<Skeleton className="h-24" />}
      >
        {stock.data && summary ? (
          <>
            <PageHeader
              title={
                <span className="flex flex-wrap items-center gap-3">
                  {summary.symbol}
                  <span className="text-content-muted text-base font-normal">
                    {summary.companyName}
                  </span>
                  {!summary.isTracked ? (
                    <Badge tone="warn">Not tracked</Badge>
                  ) : null}
                </span>
              }
              description={
                <span className="flex flex-wrap items-center gap-2">
                  {summary.sectorName ? (
                    <Badge>{summary.sectorName}</Badge>
                  ) : null}
                  {summary.industryName ? (
                    <span>{summary.industryName}</span>
                  ) : null}
                  {summary.exchange ? <span>· {summary.exchange}</span> : null}
                </span>
              }
              actions={
                stock.data.homepageUrl ? (
                  <a
                    href={stock.data.homepageUrl}
                    target="_blank"
                    rel="noreferrer noopener"
                    className="text-brand inline-flex items-center gap-1.5 text-sm"
                  >
                    Company site
                    <ExternalLink className="size-3.5" aria-hidden />
                  </a>
                ) : undefined
              }
            />

            <div className="space-y-6">
              <div className="grid gap-4 sm:grid-cols-2 xl:grid-cols-4">
                <StatCard
                  label="Last close"
                  value={formatCurrency(summary.latestClose)}
                  deltaValue={summary.changePercent}
                  delta={formatPercent(summary.changePercent)}
                  footnote={
                    summary.latestTradeDate
                      ? formatDate(summary.latestTradeDate)
                      : 'No prices collected'
                  }
                />
                <StatCard
                  label="Period high"
                  value={formatCurrency(stock.data.statistics?.periodHigh)}
                />
                <StatCard
                  label="Period low"
                  value={formatCurrency(stock.data.statistics?.periodLow)}
                />
                <StatCard
                  label="Average volume"
                  value={formatCompact(stock.data.statistics?.averageVolume)}
                  footnote={
                    stock.data.statistics
                      ? `${stock.data.statistics.barCount} sessions stored`
                      : undefined
                  }
                />
              </div>

              <Card>
                <CardHeader
                  title="Price history"
                  action={
                    <div className="flex gap-1" role="group" aria-label="Range">
                      {(Object.keys(ranges) as RangeKey[]).map((key) => (
                        <button
                          key={key}
                          type="button"
                          onClick={() => setRange(key)}
                          aria-pressed={range === key}
                          className={cn(
                            'rounded-md px-2.5 py-1 text-xs font-medium transition-colors',
                            range === key
                              ? 'bg-brand text-brand-content'
                              : 'text-content-muted hover:bg-surface-muted',
                          )}
                        >
                          {key}
                        </button>
                      ))}
                    </div>
                  }
                />
                <CardBody>
                  <QueryState
                    isLoading={prices.isPending}
                    error={prices.error}
                    onRetry={() => void prices.refetch()}
                    skeleton={<Skeleton className="h-80" />}
                  >
                    {prices.data && prices.data.bars.length > 0 ? (
                      <PriceChart bars={prices.data.bars} />
                    ) : (
                      <EmptyState
                        icon={LineChart}
                        title="No prices in this window"
                        description="Try a longer range, or run the ingestion job to collect history."
                      />
                    )}
                  </QueryState>
                </CardBody>
              </Card>

              <div className="grid gap-6 xl:grid-cols-2">
                <Card>
                  <CardHeader
                    title="Recent sessions"
                    description="Newest first. Historical rows are never modified."
                  />
                  <QueryState
                    isLoading={prices.isPending}
                    error={prices.error}
                    skeleton={<SkeletonTable rows={6} columns={5} />}
                  >
                    {prices.data && prices.data.bars.length > 0 ? (
                      <Table>
                        <Thead>
                          <Tr>
                            <Th>Date</Th>
                            <Th numeric>Open</Th>
                            <Th numeric>High</Th>
                            <Th numeric>Low</Th>
                            <Th numeric>Close</Th>
                            <Th numeric>Volume</Th>
                          </Tr>
                        </Thead>
                        <Tbody>
                          {[...prices.data.bars]
                            .reverse()
                            .slice(0, 10)
                            .map((bar) => (
                              <Tr key={bar.tradeDate}>
                                <Td>{formatDate(bar.tradeDate)}</Td>
                                <Td numeric>{formatCurrency(bar.open)}</Td>
                                <Td numeric>{formatCurrency(bar.high)}</Td>
                                <Td numeric>{formatCurrency(bar.low)}</Td>
                                <Td numeric>{formatCurrency(bar.close)}</Td>
                                <Td numeric className="text-content-muted">
                                  {formatCompact(bar.volume)}
                                </Td>
                              </Tr>
                            ))}
                        </Tbody>
                      </Table>
                    ) : (
                      <EmptyState
                        title="No sessions stored"
                        description="This symbol has no daily bars in the selected window."
                      />
                    )}
                  </QueryState>
                </Card>

                <Card>
                  <CardHeader
                    title="Model predictions"
                    description="Written by the external ML pipeline."
                  />
                  <QueryState
                    isLoading={predictions.isPending}
                    error={predictions.error}
                    skeleton={<SkeletonTable rows={4} columns={4} />}
                  >
                    {predictions.data && predictions.data.length > 0 ? (
                      <Table>
                        <Thead>
                          <Tr>
                            <Th>Target</Th>
                            <Th>Model</Th>
                            <Th>Signal</Th>
                            <Th numeric>Predicted</Th>
                            <Th numeric>Upside</Th>
                          </Tr>
                        </Thead>
                        <Tbody>
                          {predictions.data.map((prediction) => (
                            <Tr key={prediction.predictionId}>
                              <Td>{formatDate(prediction.targetDate)}</Td>
                              <Td className="text-content-muted">
                                {prediction.modelName}
                              </Td>
                              <Td>
                                <SignalBadge signal={prediction.signal} />
                              </Td>
                              <Td numeric>
                                {formatCurrency(prediction.predictedClose)}
                              </Td>
                              <Td numeric>
                                <ChangePill
                                  value={prediction.impliedUpsidePercent}
                                  showIcon={false}
                                />
                              </Td>
                            </Tr>
                          ))}
                        </Tbody>
                      </Table>
                    ) : (
                      <EmptyState
                        icon={Brain}
                        title="No predictions for this symbol"
                        description="Predictions haven't been generated yet — the ML pipeline writes into these tables."
                      />
                    )}
                  </QueryState>
                </Card>
              </div>

              {stock.data.description ? (
                <Card>
                  <CardHeader title="About" />
                  <CardBody>
                    <p className="text-content-muted max-w-3xl text-sm leading-relaxed">
                      {stock.data.description}
                    </p>
                  </CardBody>
                </Card>
              ) : null}
            </div>
          </>
        ) : (
          <Card>
            <EmptyState
              title="Symbol not found"
              description={`${upperSymbol} is not in the catalogue.`}
              action={
                <LinkButton to="/stocks" variant="secondary">
                  Back to search
                </LinkButton>
              }
            />
          </Card>
        )}
      </QueryState>
    </>
  )
}
