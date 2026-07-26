import {
  ArrowDownRight,
  ArrowUpRight,
  Briefcase,
  Brain,
  LineChart,
  Wallet,
} from 'lucide-react'
import { Link } from 'react-router'

import { Card, CardHeader } from '@/components/ui/Card'
import { ChangePill } from '@/components/ui/ChangePill'
import { EmptyState } from '@/components/ui/EmptyState'
import { LinkButton } from '@/components/ui/LinkButton'
import { PageHeader } from '@/components/ui/PageHeader'
import { QueryState } from '@/components/ui/QueryState'
import { Skeleton, SkeletonCards } from '@/components/ui/Skeleton'
import { StatCard } from '@/components/ui/StatCard'
import { Table, Tbody, Td, Th, Thead, Tr } from '@/components/ui/Table'
import {
  formatCurrency,
  formatDate,
  formatPercent,
  formatSignedCurrency,
} from '@/lib/format'
import { useAuth } from '@/features/auth/useAuth'
import { useMarketOverview } from '@/features/market/market-api'
import { MoversTable } from '@/features/market/MoversTable'
import { useDefaultPortfolioSummary } from '@/features/portfolio/portfolio-api'
import { useRecommendations } from '@/features/recommendations/recommendations-api'
import { SignalBadge } from '@/features/recommendations/SignalBadge'

export function DashboardPage() {
  const { user } = useAuth()
  const market = useMarketOverview(5)
  const portfolio = useDefaultPortfolioSummary()
  const recommendations = useRecommendations(null, 1)

  const summary = portfolio.data
  const currency = summary?.portfolio.baseCurrency ?? 'USD'

  return (
    <>
      <PageHeader
        title={`Welcome back, ${user?.displayName.split(' ')[0] ?? 'there'}`}
        description="Your portfolio, the market, and the latest model output at a glance."
      />

      <div className="space-y-6">
        <QueryState
          isLoading={portfolio.isLoading}
          error={portfolio.error}
          onRetry={() => void portfolio.refetch()}
          skeleton={<SkeletonCards />}
        >
          {summary ? (
            <div className="grid gap-4 sm:grid-cols-2 xl:grid-cols-4">
              <StatCard
                label="Market value"
                value={formatCurrency(summary.totalMarketValue, currency)}
                icon={Wallet}
                footnote={
                  summary.valuedAsOf
                    ? `Valued ${formatDate(summary.valuedAsOf)}`
                    : 'Awaiting price data'
                }
              />
              <StatCard
                label="Cost basis"
                value={formatCurrency(summary.totalCostBasis, currency)}
                icon={Briefcase}
                footnote={`${summary.holdings.length} position${summary.holdings.length === 1 ? '' : 's'}`}
              />
              <StatCard
                label="Unrealised gain"
                value={formatSignedCurrency(
                  summary.totalUnrealizedGain,
                  currency,
                )}
                deltaValue={summary.totalUnrealizedGain}
                delta={formatPercent(summary.totalUnrealizedGainPercent)}
                icon={
                  (summary.totalUnrealizedGain ?? 0) >= 0
                    ? ArrowUpRight
                    : ArrowDownRight
                }
              />
              <StatCard
                label="Day change"
                value={formatSignedCurrency(summary.dayChange, currency)}
                deltaValue={summary.dayChange}
                delta={formatPercent(summary.dayChangePercent)}
                icon={LineChart}
              />
            </div>
          ) : (
            <Card>
              <EmptyState
                icon={Briefcase}
                title="No portfolio yet"
                description="Create a portfolio to start tracking positions against daily closes."
                action={
                  <LinkButton to="/portfolio">Create a portfolio</LinkButton>
                }
              />
            </Card>
          )}
        </QueryState>

        <div className="grid gap-6 xl:grid-cols-3">
          <Card className="xl:col-span-2">
            <CardHeader
              title="Your holdings"
              description={
                summary?.portfolio.name ?? 'Positions in your default portfolio'
              }
              action={
                <LinkButton to="/portfolio" variant="ghost" size="sm">
                  Manage
                </LinkButton>
              }
            />
            <QueryState
              isLoading={portfolio.isLoading}
              error={portfolio.error}
              skeleton={<Skeleton className="m-5 h-48" />}
            >
              {!summary || summary.holdings.length === 0 ? (
                <EmptyState
                  title="No positions"
                  description="Add a holding to see it valued at the most recent close."
                />
              ) : (
                <Table>
                  <Thead>
                    <Tr>
                      <Th>Symbol</Th>
                      <Th numeric>Qty</Th>
                      <Th numeric>Value</Th>
                      <Th numeric>Gain</Th>
                      <Th numeric>Weight</Th>
                    </Tr>
                  </Thead>
                  <Tbody>
                    {summary.holdings.slice(0, 8).map((holding) => (
                      <Tr key={holding.id}>
                        <Td>
                          <Link
                            to={`/stocks/${holding.symbol}`}
                            className="text-content hover:text-brand font-medium"
                          >
                            {holding.symbol}
                          </Link>
                        </Td>
                        <Td numeric>{holding.quantity}</Td>
                        <Td numeric>
                          {formatCurrency(holding.marketValue, currency)}
                        </Td>
                        <Td numeric>
                          <ChangePill
                            value={holding.unrealizedGainPercent}
                            showIcon={false}
                          />
                        </Td>
                        <Td numeric className="text-content-muted">
                          {holding.weight == null
                            ? '—'
                            : `${holding.weight.toFixed(1)}%`}
                        </Td>
                      </Tr>
                    ))}
                  </Tbody>
                </Table>
              )}
            </QueryState>
          </Card>

          <Card>
            <CardHeader
              title="Market breadth"
              description={
                market.data?.breadth.tradeDate
                  ? formatDate(market.data.breadth.tradeDate)
                  : undefined
              }
              action={
                <LinkButton to="/market" variant="ghost" size="sm">
                  Details
                </LinkButton>
              }
            />
            <QueryState
              isLoading={market.isLoading}
              error={market.error}
              skeleton={<Skeleton className="m-5 h-48" />}
            >
              {market.data && market.data.breadth.tradeDate ? (
                <dl className="divide-border divide-y">
                  <BreadthRow
                    label="Advancing"
                    value={market.data.breadth.advancers}
                    tone="gain"
                  />
                  <BreadthRow
                    label="Declining"
                    value={market.data.breadth.decliners}
                    tone="loss"
                  />
                  <BreadthRow
                    label="Unchanged"
                    value={market.data.breadth.unchanged}
                  />
                  <BreadthRow
                    label="Tracked symbols"
                    value={market.data.trackedSymbolCount}
                  />
                </dl>
              ) : (
                <EmptyState
                  title="No market data yet"
                  description="Run the daily ingestion job to populate prices."
                />
              )}
            </QueryState>
          </Card>
        </div>

        <div className="grid gap-6 xl:grid-cols-2">
          <Card>
            <CardHeader title="Top gainers" />
            <QueryState
              isLoading={market.isLoading}
              error={market.error}
              skeleton={<Skeleton className="m-5 h-48" />}
            >
              <MoversTable stocks={market.data?.topGainers ?? []} />
            </QueryState>
          </Card>

          <Card>
            <CardHeader
              title="Latest predictions"
              action={
                <LinkButton to="/recommendations" variant="ghost" size="sm">
                  All
                </LinkButton>
              }
            />
            <QueryState
              isLoading={recommendations.isLoading}
              error={recommendations.error}
              skeleton={<Skeleton className="m-5 h-48" />}
            >
              {recommendations.data?.hasPredictions ? (
                <Table>
                  <Thead>
                    <Tr>
                      <Th>Symbol</Th>
                      <Th>Signal</Th>
                      <Th numeric>Target</Th>
                      <Th numeric>Upside</Th>
                    </Tr>
                  </Thead>
                  <Tbody>
                    {recommendations.data.predictions.items
                      .slice(0, 6)
                      .map((prediction) => (
                        <Tr key={prediction.predictionId}>
                          <Td>
                            <Link
                              to={`/stocks/${prediction.symbol}`}
                              className="text-content hover:text-brand font-medium"
                            >
                              {prediction.symbol}
                            </Link>
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
                  title="No predictions yet"
                  description="The ML pipeline writes into these tables. Once it has run, its output appears here."
                />
              )}
            </QueryState>
          </Card>
        </div>
      </div>
    </>
  )
}

function BreadthRow({
  label,
  value,
  tone,
}: {
  label: string
  value: number
  tone?: 'gain' | 'loss'
}) {
  return (
    <div className="flex items-center justify-between px-5 py-3">
      <dt className="text-content-muted text-sm">{label}</dt>
      <dd
        className={
          tone === 'gain'
            ? 'text-gain tabular text-sm font-semibold'
            : tone === 'loss'
              ? 'text-loss tabular text-sm font-semibold'
              : 'text-content tabular text-sm font-semibold'
        }
      >
        {value.toLocaleString('en-US')}
      </dd>
    </div>
  )
}
