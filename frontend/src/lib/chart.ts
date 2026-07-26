/**
 * Recharts types formatter arguments loosely (`string | number | array | undefined`), because a
 * chart can carry heterogeneous payloads. Narrowing once here keeps every tooltip callback a
 * one-liner instead of repeating the same type guard.
 */
export function chartNumber(value: unknown): number | null {
  return typeof value === 'number' && Number.isFinite(value) ? value : null
}

export function chartLabel(value: unknown): string | null {
  return typeof value === 'string' ? value : null
}
