import {
  Area,
  AreaChart,
  CartesianGrid,
  ResponsiveContainer,
  Tooltip,
  XAxis,
  YAxis,
} from 'recharts'

import { chartLabel, chartNumber } from '@/lib/chart'
import { formatCurrency, formatShortDate } from '@/lib/format'
import type { PriceBar } from '@/types/api'

/**
 * Closing price over the selected window. Deliberately a line rather than candlesticks: at a
 * daily granularity over months, candles are unreadable and the OHLC detail is already in the
 * table below.
 */
export function PriceChart({ bars }: { bars: PriceBar[] }) {
  const first = bars.at(0)
  const last = bars.at(-1)
  const rising = first && last ? last.close >= first.close : true
  const stroke = rising ? 'var(--color-gain)' : 'var(--color-loss)'

  return (
    <ResponsiveContainer width="100%" height={320}>
      <AreaChart data={bars} margin={{ top: 8, right: 8, bottom: 0, left: 8 }}>
        <defs>
          <linearGradient id="price-fill" x1="0" y1="0" x2="0" y2="1">
            <stop offset="0%" stopColor={stroke} stopOpacity={0.25} />
            <stop offset="100%" stopColor={stroke} stopOpacity={0} />
          </linearGradient>
        </defs>
        <CartesianGrid
          stroke="var(--color-border)"
          strokeDasharray="3 3"
          vertical={false}
        />
        <XAxis
          dataKey="tradeDate"
          tickFormatter={formatShortDate}
          minTickGap={48}
          tick={{ fontSize: 11, fill: 'var(--color-content-subtle)' }}
          axisLine={false}
          tickLine={false}
        />
        <YAxis
          domain={['auto', 'auto']}
          width={64}
          tickFormatter={(value: number) => `$${value.toFixed(0)}`}
          tick={{ fontSize: 11, fill: 'var(--color-content-subtle)' }}
          axisLine={false}
          tickLine={false}
        />
        <Tooltip
          labelFormatter={(label) => formatShortDate(chartLabel(label))}
          formatter={(value) => [formatCurrency(chartNumber(value)), 'Close']}
          contentStyle={{
            background: 'var(--color-surface)',
            border: '1px solid var(--color-border)',
            borderRadius: '0.5rem',
            fontSize: '0.75rem',
          }}
        />
        <Area
          type="monotone"
          dataKey="close"
          stroke={stroke}
          strokeWidth={2}
          fill="url(#price-fill)"
          dot={false}
        />
      </AreaChart>
    </ResponsiveContainer>
  )
}
