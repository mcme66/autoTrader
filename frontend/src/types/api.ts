/**
 * Mirrors of the API response contracts.
 *
 * Hand-written rather than generated: the surface is small and stable, and a generator would
 * add a build step and a stale-artifact failure mode for no real benefit at this size. If the
 * API grows substantially, generating these from the OpenAPI document is the upgrade path.
 */

export interface PagedResult<T> {
  items: T[]
  page: number
  pageSize: number
  totalCount: number
  totalPages: number
  hasPreviousPage: boolean
  hasNextPage: boolean
}

export interface AuthenticatedUser {
  id: string
  email: string
  displayName: string
  roles: string[]
  createdAt: string
  lastLoginAt?: string | null
}

export interface AuthenticationResponse {
  accessToken: string
  accessTokenExpiresAt: string
  user: AuthenticatedUser
}

export interface Sector {
  key: string
  name: string
  displayOrder: number
}

export interface Stock {
  symbol: string
  companyName: string
  sectorKey?: string | null
  sectorName?: string | null
  industryName?: string | null
  exchange?: string | null
  currencyCode: string
  isTracked: boolean
  latestTradeDate?: string | null
  latestClose?: number | null
  previousClose?: number | null
  changeAmount?: number | null
  changePercent?: number | null
  latestVolume?: number | null
}

export interface PriceStatistics {
  barCount: number
  firstTradeDate: string
  lastTradeDate: string
  periodHigh: number
  periodLow: number
  averageClose: number
  averageVolume: number
  periodChangePercent?: number | null
}

export interface StockDetail {
  summary: Stock
  description?: string | null
  homepageUrl?: string | null
  countryCode?: string | null
  employeeCount?: number | null
  listedOn?: string | null
  delistedOn?: string | null
  statistics?: PriceStatistics | null
}

export interface PriceBar {
  tradeDate: string
  open: number
  high: number
  low: number
  close: number
  volume: number
  volumeWeightedAveragePrice?: number | null
}

export interface PriceHistory {
  symbol: string
  from: string
  to: string
  bars: PriceBar[]
  statistics?: PriceStatistics | null
}

export interface MarketBreadth {
  tradeDate?: string | null
  advancers: number
  decliners: number
  unchanged: number
  totalVolume: number
}

export interface SectorPerformance {
  sectorKey: string
  sectorName: string
  stockCount: number
  averageChangePercent?: number | null
  totalVolume: number
}

export interface MarketOverview {
  breadth: MarketBreadth
  sectors: SectorPerformance[]
  topGainers: Stock[]
  topLosers: Stock[]
  mostActive: Stock[]
  trackedSymbolCount: number
}

export interface Portfolio {
  id: string
  name: string
  description?: string | null
  baseCurrency: string
  isDefault: boolean
  holdingCount: number
  createdAt: string
  updatedAt: string
}

export interface Holding {
  id: string
  symbol: string
  companyName: string
  sectorKey?: string | null
  sectorName?: string | null
  quantity: number
  averageCost: number
  costBasis: number
  latestClose?: number | null
  priceAsOf?: string | null
  marketValue?: number | null
  unrealizedGain?: number | null
  unrealizedGainPercent?: number | null
  dayChange?: number | null
  dayChangePercent?: number | null
  weight?: number | null
  openedOn?: string | null
  notes?: string | null
}

export interface SectorAllocation {
  sectorKey: string
  sectorName: string
  marketValue: number
  weight: number
}

export interface PortfolioSummary {
  portfolio: Portfolio
  totalCostBasis: number
  totalMarketValue?: number | null
  totalUnrealizedGain?: number | null
  totalUnrealizedGainPercent?: number | null
  dayChange?: number | null
  dayChangePercent?: number | null
  valuedAsOf?: string | null
  holdings: Holding[]
  sectorAllocation: SectorAllocation[]
}

export type PredictionSignal =
  | 'StrongSell'
  | 'Sell'
  | 'Hold'
  | 'Buy'
  | 'StrongBuy'

export type PredictionDirection = 'Down' | 'Flat' | 'Up'

export interface Recommendation {
  predictionId: number
  symbol: string
  companyName: string
  sectorName?: string | null
  modelKey: string
  modelName: string
  modelVersion: string
  predictionDate: string
  targetDate: string
  horizonDays: number
  predictedClose?: number | null
  predictedReturn?: number | null
  direction: PredictionDirection
  signal: PredictionSignal
  confidence?: number | null
  latestClose?: number | null
  impliedUpsidePercent?: number | null
}

export interface MlModel {
  key: string
  name: string
  version: string
  description?: string | null
  isActive: boolean
  createdAt: string
}

export interface ModelAccuracy {
  modelKey: string
  modelName: string
  modelVersion: string
  evaluatedCount: number
  meanAbsoluteError?: number | null
  meanAbsolutePercentageError?: number | null
  directionalAccuracyPercent?: number | null
}

export interface Recommendations {
  hasPredictions: boolean
  predictions: PagedResult<Recommendation>
  models: MlModel[]
  accuracy: ModelAccuracy[]
}

/** RFC 9457 problem document, optionally carrying per-field validation errors. */
export interface ProblemDetails {
  type?: string
  title?: string
  status?: number
  detail?: string
  instance?: string
  traceId?: string
  errors?: Record<string, string[]>
}
