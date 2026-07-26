import { z } from 'zod'

/** Mirrors the server-side FluentValidation rules so bad input never reaches the network. */
export const portfolioSchema = z.object({
  name: z
    .string()
    .trim()
    .min(1, 'Enter a name.')
    .max(120, 'Names are limited to 120 characters.'),
  description: z
    .string()
    .trim()
    .max(500, 'Descriptions are limited to 500 characters.'),
  isDefault: z.boolean(),
})

export type PortfolioValues = z.infer<typeof portfolioSchema>

/**
 * Numeric fields are registered with `valueAsNumber`, so an empty box arrives as NaN rather
 * than an empty string — hence the explicit invalid-type messages.
 */
export const holdingSchema = z.object({
  symbol: z
    .string()
    .trim()
    .min(1, 'Enter a symbol.')
    .max(16, 'Symbols are limited to 16 characters.')
    .transform((value) => value.toUpperCase()),
  quantity: z
    .number({ error: 'Enter a quantity.' })
    .positive('Quantity must be greater than zero.'),
  averageCost: z
    .number({ error: 'Enter an average cost.' })
    .nonnegative('Average cost cannot be negative.'),
  openedOn: z.string(),
  notes: z.string().trim().max(500, 'Notes are limited to 500 characters.'),
})

export type HoldingValues = z.input<typeof holdingSchema>
export type HoldingPayloadValues = z.output<typeof holdingSchema>
