import { Brain } from 'lucide-react'
import { useState } from 'react'
import { Link } from 'react-router'

import { Badge } from '@/components/ui/Badge'
import { Card, CardHeader } from '@/components/ui/Card'
import { ChangePill } from '@/components/ui/ChangePill'
import { EmptyState } from '@/components/ui/EmptyState'
import { Select } from '@/components/ui/Input'
import { PageHeader } from '@/components/ui/PageHeader'
import { Pagination } from '@/components/ui/Pagination'
import { QueryState } from '@/components/ui/QueryState'
import { SkeletonTable } from '@/components/ui/Skeleton'
import { StatCard } from '@/components/ui/StatCard'
import { Table, Tbody, Td, Th, Thead, Tr } from '@/components/ui/Table'
import { formatCurrency, formatDate, formatNumber } from '@/lib/format'
import { useRecommendations } from './recommendations-api'
import { SignalBadge } from './SignalBadge'

export function RecommendationsPage() {
  const [model, setModel] = useState<string | null>(null)
  const [page, setPage] = useState(1)
  const { data, isPending, error, refetch } = useRecommendations(model, page)

  return (
    <>
      <PageHeader
        title="ML Recommendations"
        description="Read-only output from the separate ML pipeline. This service never writes to these tables."
      />

      <QueryState
        isLoading={isPending}
        error={error}
        onRetry={() => void refetch()}
        skeleton={
          <Card>
            <SkeletonTable rows={8} columns={6} />
          </Card>
        }
      >
        {data?.hasPredictions ? (
          <div className="space-y-6">
            {data.accuracy.length > 0 ? (
              <div className="grid gap-4 sm:grid-cols-2 xl:grid-cols-3">
                {data.accuracy.map((entry) => (
                  <StatCard
                    key={`${entry.modelKey}:${entry.modelVersion}`}
                    label={`${entry.modelName} · v${entry.modelVersion}`}
                    value={
                      entry.directionalAccuracyPercent == null
                        ? '—'
                        : `${formatNumber(entry.directionalAccuracyPercent, 1)}%`
                    }
                    footnote={`Directional accuracy over ${entry.evaluatedCount.toLocaleString('en-US')} scored predictions · MAE ${formatNumber(entry.meanAbsoluteError)}`}
                  />
                ))}
              </div>
            ) : null}

            <Card>
              <CardHeader
                title="Latest predictions"
                description="One row per symbol, newest prediction date first."
                action={
                  data.models.length > 1 ? (
                    <Select
                      aria-label="Filter by model"
                      value={model ?? ''}
                      onChange={(event) => {
                        setModel(event.target.value || null)
                        setPage(1)
                      }}
                      className="h-8 w-52 text-xs"
                    >
                      <option value="">All models</option>
                      {data.models.map((entry) => (
                        <option key={entry.key} value={entry.key}>
                          {entry.name} v{entry.version}
                        </option>
                      ))}
                    </Select>
                  ) : undefined
                }
              />

              {data.predictions.items.length === 0 ? (
                <EmptyState
                  title="No predictions match this filter"
                  description="Choose a different model to see its output."
                />
              ) : (
                <>
                  <Table>
                    <Thead>
                      <Tr>
                        <Th>Symbol</Th>
                        <Th>Signal</Th>
                        <Th>Model</Th>
                        <Th>Target date</Th>
                        <Th numeric>Last close</Th>
                        <Th numeric>Predicted</Th>
                        <Th numeric>Upside</Th>
                        <Th numeric>Confidence</Th>
                      </Tr>
                    </Thead>
                    <Tbody>
                      {data.predictions.items.map((prediction) => (
                        <Tr key={prediction.predictionId}>
                          <Td>
                            <Link
                              to={`/stocks/${prediction.symbol}`}
                              className="text-content hover:text-brand font-medium"
                            >
                              {prediction.symbol}
                            </Link>
                            <span className="text-content-subtle ml-2 hidden text-xs lg:inline">
                              {prediction.companyName}
                            </span>
                          </Td>
                          <Td>
                            <SignalBadge signal={prediction.signal} />
                          </Td>
                          <Td className="text-content-muted text-xs">
                            {prediction.modelName} v{prediction.modelVersion}
                          </Td>
                          <Td className="text-content-muted">
                            {formatDate(prediction.targetDate)}
                            <span className="text-content-subtle ml-1 text-xs">
                              ({prediction.horizonDays}d)
                            </span>
                          </Td>
                          <Td numeric>
                            {formatCurrency(prediction.latestClose)}
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
                          <Td numeric>
                            {prediction.confidence == null ? (
                              <span className="text-content-subtle">—</span>
                            ) : (
                              <Badge tone="brand">
                                {formatNumber(prediction.confidence * 100, 0)}%
                              </Badge>
                            )}
                          </Td>
                        </Tr>
                      ))}
                    </Tbody>
                  </Table>

                  <Pagination
                    page={data.predictions.page}
                    totalPages={data.predictions.totalPages}
                    totalCount={data.predictions.totalCount}
                    hasPreviousPage={data.predictions.hasPreviousPage}
                    hasNextPage={data.predictions.hasNextPage}
                    onChange={setPage}
                  />
                </>
              )}
            </Card>
          </div>
        ) : (
          <Card>
            <EmptyState
              icon={Brain}
              title="Predictions haven't been generated yet"
              description="The ML pipeline writes into these tables. Once MLPipeline_Jordan has trained a model and published its output, recommendations and accuracy metrics will appear here automatically — no change to this application is needed."
            />
          </Card>
        )}
      </QueryState>
    </>
  )
}
