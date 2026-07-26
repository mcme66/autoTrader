import { zodResolver } from '@hookform/resolvers/zod'
import { useState } from 'react'
import { useForm } from 'react-hook-form'

import { Alert } from '@/components/ui/Alert'
import { Button } from '@/components/ui/Button'
import { Input, Textarea } from '@/components/ui/Input'
import { Modal } from '@/components/ui/Modal'
import { errorMessage } from '@/lib/api-client'
import type { Holding } from '@/types/api'
import { useAddHolding, useUpdateHolding } from './portfolio-api'
import {
  holdingSchema,
  type HoldingPayloadValues,
  type HoldingValues,
} from './schemas'

export function HoldingFormModal({
  open,
  onClose,
  portfolioId,
  holding,
}: {
  open: boolean
  onClose: () => void
  portfolioId: string
  holding?: Holding
}) {
  return (
    <Modal
      open={open}
      onClose={onClose}
      title={holding ? `Edit ${holding.symbol}` : 'Add holding'}
    >
      <HoldingForm
        onClose={onClose}
        portfolioId={portfolioId}
        holding={holding}
      />
    </Modal>
  )
}

function HoldingForm({
  onClose,
  portfolioId,
  holding,
}: {
  onClose: () => void
  portfolioId: string
  holding?: Holding
}) {
  const [submitError, setSubmitError] = useState<string | null>(null)
  const add = useAddHolding(portfolioId)
  const update = useUpdateHolding(portfolioId)

  const {
    register,
    handleSubmit,
    formState: { errors, isSubmitting },
  } = useForm<HoldingValues, unknown, HoldingPayloadValues>({
    resolver: zodResolver(holdingSchema),
    defaultValues: {
      symbol: holding?.symbol ?? '',
      quantity: holding?.quantity ?? 0,
      averageCost: holding?.averageCost ?? 0,
      openedOn: holding?.openedOn ?? '',
      notes: holding?.notes ?? '',
    },
  })

  const onSubmit = handleSubmit(async (values) => {
    setSubmitError(null)
    const payload = {
      quantity: values.quantity,
      averageCost: values.averageCost,
      openedOn: values.openedOn || null,
      notes: values.notes || null,
    }

    try {
      if (holding) {
        await update.mutateAsync({ holdingId: holding.id, payload })
      } else {
        await add.mutateAsync({ symbol: values.symbol, ...payload })
      }

      onClose()
    } catch (error) {
      setSubmitError(errorMessage(error))
    }
  })

  return (
    <form onSubmit={(event) => void onSubmit(event)} className="space-y-4">
      {submitError ? <Alert tone="error">{submitError}</Alert> : null}

      <Input
        label="Symbol"
        // The symbol identifies the position, so changing it would be a different holding.
        disabled={Boolean(holding)}
        autoFocus={!holding}
        placeholder="AAPL"
        error={errors.symbol?.message}
        {...register('symbol')}
      />

      <div className="grid gap-4 sm:grid-cols-2">
        <Input
          label="Quantity"
          type="number"
          step="any"
          min="0"
          error={errors.quantity?.message}
          {...register('quantity', { valueAsNumber: true })}
        />
        <Input
          label="Average cost"
          type="number"
          step="any"
          min="0"
          error={errors.averageCost?.message}
          {...register('averageCost', { valueAsNumber: true })}
        />
      </div>

      <Input
        label="Opened on"
        type="date"
        error={errors.openedOn?.message}
        {...register('openedOn')}
      />

      <Textarea
        label="Notes"
        error={errors.notes?.message}
        {...register('notes')}
      />

      <div className="flex justify-end gap-2 pt-2">
        <Button variant="secondary" onClick={onClose}>
          Cancel
        </Button>
        <Button type="submit" loading={isSubmitting}>
          {holding ? 'Save changes' : 'Add'}
        </Button>
      </div>
    </form>
  )
}
