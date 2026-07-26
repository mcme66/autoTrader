import { Badge, type BadgeTone } from '@/components/ui/Badge'
import type { PredictionSignal } from '@/types/api'

const tones: Record<PredictionSignal, BadgeTone> = {
  StrongBuy: 'gain',
  Buy: 'gain',
  Hold: 'neutral',
  Sell: 'loss',
  StrongSell: 'loss',
}

const labels: Record<PredictionSignal, string> = {
  StrongBuy: 'Strong buy',
  Buy: 'Buy',
  Hold: 'Hold',
  Sell: 'Sell',
  StrongSell: 'Strong sell',
}

export function SignalBadge({ signal }: { signal: PredictionSignal }) {
  return <Badge tone={tones[signal]}>{labels[signal]}</Badge>
}
