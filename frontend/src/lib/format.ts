import { format, parseISO } from 'date-fns'

const currencyFormatters = new Map<string, Intl.NumberFormat>()

function currencyFormatter(currency: string): Intl.NumberFormat {
  let formatter = currencyFormatters.get(currency)

  if (!formatter) {
    formatter = new Intl.NumberFormat('en-US', {
      style: 'currency',
      currency,
      minimumFractionDigits: 2,
      maximumFractionDigits: 2,
    })
    currencyFormatters.set(currency, formatter)
  }

  return formatter
}

const compactNumber = new Intl.NumberFormat('en-US', {
  notation: 'compact',
  maximumFractionDigits: 1,
})

const percent = new Intl.NumberFormat('en-US', {
  minimumFractionDigits: 2,
  maximumFractionDigits: 2,
})

/** Em dash for absent values, so an empty cell is never mistaken for a zero. */
export const EMPTY = '—'

export function formatCurrency(
  value: number | null | undefined,
  currency = 'USD',
): string {
  return value == null ? EMPTY : currencyFormatter(currency).format(value)
}

export function formatNumber(
  value: number | null | undefined,
  fractionDigits = 2,
): string {
  return value == null
    ? EMPTY
    : value.toLocaleString('en-US', {
        minimumFractionDigits: fractionDigits,
        maximumFractionDigits: fractionDigits,
      })
}

export function formatCompact(value: number | null | undefined): string {
  return value == null ? EMPTY : compactNumber.format(value)
}

/** Always signed, because the sign is the point on a change column. */
export function formatPercent(value: number | null | undefined): string {
  if (value == null) {
    return EMPTY
  }

  const sign = value > 0 ? '+' : ''
  return `${sign}${percent.format(value)}%`
}

export function formatSignedCurrency(
  value: number | null | undefined,
  currency = 'USD',
): string {
  if (value == null) {
    return EMPTY
  }

  const sign = value > 0 ? '+' : ''
  return `${sign}${currencyFormatter(currency).format(value)}`
}

export function formatDate(value: string | null | undefined): string {
  return value == null ? EMPTY : format(parseISO(value), 'MMM d, yyyy')
}

export function formatShortDate(value: string | null | undefined): string {
  return value == null ? EMPTY : format(parseISO(value), 'MMM d')
}

export function formatDateTime(value: string | null | undefined): string {
  return value == null ? EMPTY : format(parseISO(value), 'MMM d, yyyy HH:mm')
}

/**
 * Maps a change to a semantic colour. Null is deliberately neutral rather than red: "no
 * price yet" is not a loss.
 */
export function changeTone(
  value: number | null | undefined,
): 'gain' | 'loss' | 'neutral' {
  if (value == null || value === 0) {
    return 'neutral'
  }

  return value > 0 ? 'gain' : 'loss'
}

export function changeTextClass(value: number | null | undefined): string {
  switch (changeTone(value)) {
    case 'gain':
      return 'text-gain'
    case 'loss':
      return 'text-loss'
    default:
      return 'text-content-muted'
  }
}

/** ISO date (yyyy-MM-dd) as the API expects for date-only query parameters. */
export function toIsoDate(date: Date): string {
  return format(date, 'yyyy-MM-dd')
}
