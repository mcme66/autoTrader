import {
  Briefcase,
  Pencil,
  Plus,
  Trash2,
  Wallet,
} from 'lucide-react'
import { useState } from 'react'
import { Link } from 'react-router'
import { Cell, Pie, PieChart, ResponsiveContainer, Tooltip } from 'recharts'

import { Button } from '@/components/ui/Button'
import { Card, CardBody, CardHeader } from '@/components/ui/Card'
import { ChangePill } from '@/components/ui/ChangePill'
import { EmptyState } from '@/components/ui/EmptyState'
import { Select } from '@/components/ui/Input'
import { PageHeader } from '@/components/ui/PageHeader'
import { QueryState } from '@/components/ui/QueryState'
import { Skeleton, SkeletonCards } from '@/components/ui/Skeleton'
import { StatCard } from '@/components/ui/StatCard'
import { Table, Tbody, Td, Th, Thead, Tr } from '@/components/ui/Table'
import { chartNumber } from '@/lib/chart'
import {
  formatCurrency,
  formatDate,
  formatNumber,
  formatPercent,
  formatSignedCurrency,
} from '@/lib/format'
import type { Holding } from '@/types/api'
import { HoldingFormModal } from './HoldingFormModal'
import { PortfolioFormModal } from './PortfolioFormModal'
import {
  useDeletePortfolio,
  usePortfolios,
  usePortfolioSummary,
  useRemoveHolding,
} from './portfolio-api'

/** Distinct enough to tell eleven sectors apart without leaving the brand palette entirely. */
const allocationColors = [
  'oklch(55% 0.2 264)',
  'oklch(62% 0.16 152)',
  'oklch(66% 0.17 22)',
  'oklch(70% 0.15 75)',
  'oklch(60% 0.16 300)',
  'oklch(64% 0.14 200)',
  'oklch(58% 0.18 340)',
  'oklch(68% 0.13 120)',
  'oklch(56% 0.15 240)',
  'oklch(72% 0.12 95)',
  'oklch(50% 0.14 20)',
]

export function PortfolioPage() {
  const portfolios = usePortfolios()
  const [chosenId, setChosenId] = useState<string | null>(null)
  const [portfolioModalOpen, setPortfolioModalOpen] = useState(false)
  const [editingPortfolio, setEditingPortfolio] = useState(false)
  const [holdingModalOpen, setHoldingModalOpen] = useState(false)
  const [editingHolding, setEditingHolding] = useState<Holding | undefined>()

  // Derived rather than stored: an explicit choice wins, but falls back to the default
  // portfolio, which also means a deleted selection recovers on its own with no effect to
  // keep the two in sync.
  const list = portfolios.data ?? []
  const selected =
    list.find((item) => item.id === chosenId) ??
    list.find((item) => item.isDefault) ??
    list[0]
  const selectedId = selected?.id ?? null

  const summary = usePortfolioSummary(selectedId ?? undefined)
  const deletePortfolio = useDeletePortfolio()
  const removeHolding = useRemoveHolding(selectedId ?? '')

  const currency = summary.data?.portfolio.baseCurrency ?? 'USD'

  const handleDeletePortfolio = async () => {
    if (!selected) {
      return
    }

    const confirmed = window.confirm(
      `Delete "${selected.name}" and its holdings? Price history is not affected.`,
    )

    if (confirmed) {
      await deletePortfolio.mutateAsync(selected.id)
      setChosenId(null)
    }
  }

  const handleRemoveHolding = async (holding: Holding) => {
    if (window.confirm(`Remove ${holding.symbol} from this portfolio?`)) {
      await removeHolding.mutateAsync(holding.id)
    }
  }

  return (
    <>
      <PageHeader
        title="Portfolio"
        description="Positions valued at the most recent stored close."
        actions={
          <>
            {list.length > 1 ? (
              <Select
                aria-label="Select portfolio"
                value={selectedId ?? ''}
                onChange={(event) => setChosenId(event.target.value)}
                className="w-52"
              >
                {list.map((item) => (
                  <option key={item.id} value={item.id}>
                    {item.name}
                    {item.isDefault ? ' (default)' : ''}
                  </option>
                ))}
              </Select>
            ) : null}
            <Button
              variant="secondary"
              icon={<Plus className="size-4" />}
              onClick={() => {
                setEditingPortfolio(false)
                setPortfolioModalOpen(true)
              }}
            >
              New portfolio
            </Button>
          </>
        }
      />

      <QueryState
        isLoading={portfolios.isLoading}
        error={portfolios.error}
        onRetry={() => void portfolios.refetch()}
        skeleton={<SkeletonCards />}
      >
        {list.length === 0 ? (
          <Card>
            <EmptyState
              icon={Briefcase}
              title="No portfolios yet"
              description="Create one to start tracking positions. You can keep several — one is marked as your default and drives the dashboard."
              action={
                <Button
                  icon={<Plus className="size-4" />}
                  onClick={() => {
                    setEditingPortfolio(false)
                    setPortfolioModalOpen(true)
                  }}
                >
                  Create a portfolio
                </Button>
              }
            />
          </Card>
        ) : (
          <QueryState
            isLoading={summary.isPending}
            error={summary.error}
            onRetry={() => void summary.refetch()}
            skeleton={
              <div className="space-y-6">
                <SkeletonCards />
                <Skeleton className="h-64" />
              </div>
            }
          >
            {summary.data ? (
              <div className="space-y-6">
                <div className="grid gap-4 sm:grid-cols-2 xl:grid-cols-4">
                  <StatCard
                    label="Market value"
                    value={formatCurrency(
                      summary.data.totalMarketValue,
                      currency,
                    )}
                    icon={Wallet}
                    footnote={
                      summary.data.valuedAsOf
                        ? `As of ${formatDate(summary.data.valuedAsOf)}`
                        : 'Awaiting price data'
                    }
                  />
                  <StatCard
                    label="Cost basis"
                    value={formatCurrency(
                      summary.data.totalCostBasis,
                      currency,
                    )}
                  />
                  <StatCard
                    label="Unrealised gain"
                    value={formatSignedCurrency(
                      summary.data.totalUnrealizedGain,
                      currency,
                    )}
                    deltaValue={summary.data.totalUnrealizedGain}
                    delta={formatPercent(
                      summary.data.totalUnrealizedGainPercent,
                    )}
                  />
                  <StatCard
                    label="Day change"
                    value={formatSignedCurrency(
                      summary.data.dayChange,
                      currency,
                    )}
                    deltaValue={summary.data.dayChange}
                    delta={formatPercent(summary.data.dayChangePercent)}
                  />
                </div>

                <div className="grid gap-6 xl:grid-cols-3">
                  <Card className="xl:col-span-2">
                    <CardHeader
                      title={summary.data.portfolio.name}
                      description={
                        summary.data.portfolio.description ?? undefined
                      }
                      action={
                        <div className="flex gap-2">
                          <Button
                            variant="ghost"
                            size="sm"
                            icon={<Pencil className="size-3.5" />}
                            onClick={() => {
                              setEditingPortfolio(true)
                              setPortfolioModalOpen(true)
                            }}
                          >
                            Edit
                          </Button>
                          <Button
                            variant="ghost"
                            size="sm"
                            icon={<Trash2 className="size-3.5" />}
                            onClick={() => void handleDeletePortfolio()}
                          >
                            Delete
                          </Button>
                          <Button
                            size="sm"
                            icon={<Plus className="size-3.5" />}
                            onClick={() => {
                              setEditingHolding(undefined)
                              setHoldingModalOpen(true)
                            }}
                          >
                            Add holding
                          </Button>
                        </div>
                      }
                    />

                    {summary.data.holdings.length === 0 ? (
                      <EmptyState
                        title="No holdings"
                        description="Add a position and it will be valued against the latest daily close."
                      />
                    ) : (
                      <Table>
                        <Thead>
                          <Tr>
                            <Th>Symbol</Th>
                            <Th numeric>Qty</Th>
                            <Th numeric>Avg cost</Th>
                            <Th numeric>Last</Th>
                            <Th numeric>Value</Th>
                            <Th numeric>Gain</Th>
                            <Th numeric>Weight</Th>
                            <Th />
                          </Tr>
                        </Thead>
                        <Tbody>
                          {summary.data.holdings.map((holding) => (
                            <Tr key={holding.id}>
                              <Td>
                                <Link
                                  to={`/stocks/${holding.symbol}`}
                                  className="text-content hover:text-brand font-medium"
                                >
                                  {holding.symbol}
                                </Link>
                                <span className="text-content-subtle ml-2 hidden text-xs lg:inline">
                                  {holding.companyName}
                                </span>
                              </Td>
                              <Td numeric>
                                {formatNumber(holding.quantity, 2)}
                              </Td>
                              <Td numeric>
                                {formatCurrency(holding.averageCost, currency)}
                              </Td>
                              <Td numeric>
                                {formatCurrency(holding.latestClose, currency)}
                              </Td>
                              <Td numeric>
                                {formatCurrency(holding.marketValue, currency)}
                              </Td>
                              <Td numeric>
                                <div className="flex flex-col items-end">
                                  <span
                                    className={
                                      (holding.unrealizedGain ?? 0) >= 0
                                        ? 'text-gain tabular text-sm'
                                        : 'text-loss tabular text-sm'
                                    }
                                  >
                                    {formatSignedCurrency(
                                      holding.unrealizedGain,
                                      currency,
                                    )}
                                  </span>
                                  <ChangePill
                                    value={holding.unrealizedGainPercent}
                                    showIcon={false}
                                    className="text-xs"
                                  />
                                </div>
                              </Td>
                              <Td numeric className="text-content-muted">
                                {holding.weight == null
                                  ? '—'
                                  : `${holding.weight.toFixed(1)}%`}
                              </Td>
                              <Td numeric>
                                <div className="flex justify-end gap-1">
                                  <Button
                                    variant="ghost"
                                    size="sm"
                                    aria-label={`Edit ${holding.symbol}`}
                                    onClick={() => {
                                      setEditingHolding(holding)
                                      setHoldingModalOpen(true)
                                    }}
                                  >
                                    <Pencil className="size-3.5" />
                                  </Button>
                                  <Button
                                    variant="ghost"
                                    size="sm"
                                    aria-label={`Remove ${holding.symbol}`}
                                    onClick={() =>
                                      void handleRemoveHolding(holding)
                                    }
                                  >
                                    <Trash2 className="size-3.5" />
                                  </Button>
                                </div>
                              </Td>
                            </Tr>
                          ))}
                        </Tbody>
                      </Table>
                    )}
                  </Card>

                  <Card>
                    <CardHeader
                      title="Sector allocation"
                      description="By market value."
                    />
                    <CardBody>
                      {summary.data.sectorAllocation.length === 0 ? (
                        <EmptyState
                          title="Nothing allocated"
                          description="Allocation appears once holdings have been priced."
                        />
                      ) : (
                        <>
                          <ResponsiveContainer width="100%" height={200}>
                            <PieChart>
                              <Pie
                                data={summary.data.sectorAllocation}
                                dataKey="marketValue"
                                nameKey="sectorName"
                                innerRadius={50}
                                outerRadius={80}
                                paddingAngle={2}
                              >
                                {summary.data.sectorAllocation.map(
                                  (slice, index) => (
                                    <Cell
                                      key={slice.sectorKey}
                                      fill={
                                        allocationColors[
                                          index % allocationColors.length
                                        ]
                                      }
                                    />
                                  ),
                                )}
                              </Pie>
                              <Tooltip
                                formatter={(value) =>
                                  formatCurrency(chartNumber(value), currency)
                                }
                                contentStyle={{
                                  background: 'var(--color-surface)',
                                  border: '1px solid var(--color-border)',
                                  borderRadius: '0.5rem',
                                  fontSize: '0.75rem',
                                }}
                              />
                            </PieChart>
                          </ResponsiveContainer>

                          <ul className="mt-4 space-y-2">
                            {summary.data.sectorAllocation.map(
                              (slice, index) => (
                                <li
                                  key={slice.sectorKey}
                                  className="flex items-center gap-2 text-xs"
                                >
                                  <span
                                    className="size-2.5 shrink-0 rounded-full"
                                    style={{
                                      background:
                                        allocationColors[
                                          index % allocationColors.length
                                        ],
                                    }}
                                  />
                                  <span className="text-content-muted flex-1 truncate">
                                    {slice.sectorName}
                                  </span>
                                  <span className="text-content tabular font-medium">
                                    {slice.weight.toFixed(1)}%
                                  </span>
                                </li>
                              ),
                            )}
                          </ul>
                        </>
                      )}
                    </CardBody>
                  </Card>
                </div>
              </div>
            ) : null}
          </QueryState>
        )}
      </QueryState>

      <PortfolioFormModal
        open={portfolioModalOpen}
        onClose={() => setPortfolioModalOpen(false)}
        portfolio={editingPortfolio ? selected : undefined}
      />

      {selectedId ? (
        <HoldingFormModal
          open={holdingModalOpen}
          onClose={() => setHoldingModalOpen(false)}
          portfolioId={selectedId}
          holding={editingHolding}
        />
      ) : null}
    </>
  )
}
