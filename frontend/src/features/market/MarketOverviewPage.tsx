import { Activity, ArrowDownRight, ArrowUpRight, Layers } from 'lucide-react'
import {
  Bar,
  BarChart,
  Cell,
  ResponsiveContainer,
  Tooltip,
  XAxis,
  YAxis,
} from 'recharts'

import { Card, CardBody, CardHeader } from '@/components/ui/Card'
import { EmptyState } from '@/components/ui/EmptyState'
import { PageHeader } from '@/components/ui/PageHeader'
import { QueryState } from '@/components/ui/QueryState'
import { Skeleton, SkeletonCards } from '@/components/ui/Skeleton'
import { StatCard } from '@/components/ui/StatCard'
import { chartNumber } from '@/lib/chart'
import { formatCompact, formatDate, formatPercent } from '@/lib/format'
import { useMarketOverview } from './market-api'
import { MoversTable } from './MoversTable'

export function MarketOverviewPage() {
  const { data, isLoading, error, refetch } = useMarketOverview(5)

  return (
    <>
      <PageHeader
        title="Market Overview"
        description={
          data?.breadth.tradeDate
            ? `Latest stored trading day: ${formatDate(data.breadth.tradeDate)}`
            : 'Aggregates across every tracked symbol.'
        }
      />

      <QueryState
        isLoading={isLoading}
        error={error}
        onRetry={() => void refetch()}
        skeleton={
          <div className="space-y-6">
            <SkeletonCards />
            <Skeleton className="h-72" />
          </div>
        }
      >
        {data ? (
          data.breadth.tradeDate === null ? (
            <Card>
              <EmptyState
                icon={Activity}
                title="No market data has been collected yet"
                description="Trigger the internal daily-prices endpoint, or wait for the scheduled task to run after the next close."
              />
            </Card>
          ) : (
            <div className="space-y-6">
              <div className="grid gap-4 sm:grid-cols-2 xl:grid-cols-4">
                <StatCard
                  label="Advancers"
                  value={data.breadth.advancers}
                  icon={ArrowUpRight}
                  footnote={`${data.trackedSymbolCount} symbols tracked`}
                />
                <StatCard
                  label="Decliners"
                  value={data.breadth.decliners}
                  icon={ArrowDownRight}
                />
                <StatCard
                  label="Unchanged"
                  value={data.breadth.unchanged}
                  icon={Layers}
                />
                <StatCard
                  label="Total volume"
                  value={formatCompact(data.breadth.totalVolume)}
                  icon={Activity}
                />
              </div>

              <Card>
                <CardHeader
                  title="Sector performance"
                  description="Average one-day change across each sector's tracked symbols."
                />
                <CardBody>
                  <SectorChart sectors={data.sectors} />
                </CardBody>
              </Card>

              <div className="grid gap-6 xl:grid-cols-3">
                <Card>
                  <CardHeader title="Top gainers" />
                  <MoversTable stocks={data.topGainers} />
                </Card>
                <Card>
                  <CardHeader title="Top losers" />
                  <MoversTable stocks={data.topLosers} />
                </Card>
                <Card>
                  <CardHeader title="Most active" />
                  <MoversTable stocks={data.mostActive} />
                </Card>
              </div>
            </div>
          )
        ) : null}
      </QueryState>
    </>
  )
}

function SectorChart({
  sectors,
}: {
  sectors: {
    sectorKey: string
    sectorName: string
    averageChangePercent?: number | null
  }[]
}) {
  const rows = sectors
    .filter((sector) => sector.averageChangePercent != null)
    .map((sector) => ({
      name: sector.sectorName,
      value: sector.averageChangePercent ?? 0,
    }))

  if (rows.length === 0) {
    return (
      <EmptyState
        title="No sector performance yet"
        description="Sector averages appear once at least two trading days have been collected."
      />
    )
  }

  return (
    <ResponsiveContainer width="100%" height={Math.max(240, rows.length * 28)}>
      <BarChart data={rows} layout="vertical" margin={{ left: 24, right: 24 }}>
        <XAxis
          type="number"
          tickFormatter={(value: number) => `${value.toFixed(1)}%`}
          tick={{ fontSize: 11, fill: 'var(--color-content-subtle)' }}
          axisLine={false}
          tickLine={false}
        />
        <YAxis
          type="category"
          dataKey="name"
          width={150}
          tick={{ fontSize: 11, fill: 'var(--color-content-muted)' }}
          axisLine={false}
          tickLine={false}
        />
        <Tooltip
          cursor={{ fill: 'var(--color-surface-muted)' }}
          formatter={(value) => formatPercent(chartNumber(value))}
          contentStyle={{
            background: 'var(--color-surface)',
            border: '1px solid var(--color-border)',
            borderRadius: '0.5rem',
            fontSize: '0.75rem',
          }}
        />
        <Bar dataKey="value" radius={[0, 4, 4, 0]}>
          {rows.map((row) => (
            <Cell
              key={row.name}
              fill={row.value >= 0 ? 'var(--color-gain)' : 'var(--color-loss)'}
            />
          ))}
        </Bar>
      </BarChart>
    </ResponsiveContainer>
  )
}
