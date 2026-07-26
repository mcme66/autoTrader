import { zodResolver } from '@hookform/resolvers/zod'
import { useState } from 'react'
import { useForm } from 'react-hook-form'

import { Alert } from '@/components/ui/Alert'
import { Button } from '@/components/ui/Button'
import { Input, Textarea } from '@/components/ui/Input'
import { Modal } from '@/components/ui/Modal'
import { errorMessage } from '@/lib/api-client'
import type { Portfolio } from '@/types/api'
import { useCreatePortfolio, useUpdatePortfolio } from './portfolio-api'
import { portfolioSchema, type PortfolioValues } from './schemas'

/** Create and edit share a form; the presence of `portfolio` picks the mutation. */
export function PortfolioFormModal({
  open,
  onClose,
  portfolio,
}: {
  open: boolean
  onClose: () => void
  portfolio?: Portfolio
}) {
  return (
    <Modal
      open={open}
      onClose={onClose}
      title={portfolio ? 'Edit portfolio' : 'New portfolio'}
    >
      {/* Mounted only while open, so the form starts from the current values every time and
          no effect is needed to reset it. */}
      <PortfolioForm onClose={onClose} portfolio={portfolio} />
    </Modal>
  )
}

function PortfolioForm({
  onClose,
  portfolio,
}: {
  onClose: () => void
  portfolio?: Portfolio
}) {
  const [submitError, setSubmitError] = useState<string | null>(null)
  const create = useCreatePortfolio()
  const update = useUpdatePortfolio(portfolio?.id ?? '')

  const {
    register,
    handleSubmit,
    formState: { errors, isSubmitting },
  } = useForm<PortfolioValues>({
    resolver: zodResolver(portfolioSchema),
    defaultValues: {
      name: portfolio?.name ?? '',
      description: portfolio?.description ?? '',
      isDefault: portfolio?.isDefault ?? false,
    },
  })

  const onSubmit = handleSubmit(async (values) => {
    setSubmitError(null)
    const payload = {
      name: values.name,
      description: values.description || null,
      isDefault: values.isDefault,
    }

    try {
      if (portfolio) {
        await update.mutateAsync(payload)
      } else {
        await create.mutateAsync({ ...payload, baseCurrency: 'USD' })
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
        label="Name"
        autoFocus
        error={errors.name?.message}
        {...register('name')}
      />

      <Textarea
        label="Description"
        error={errors.description?.message}
        {...register('description')}
      />

      <label className="text-content flex items-center gap-2 text-sm">
        <input
          type="checkbox"
          className="accent-brand size-4"
          {...register('isDefault')}
        />
        Use as my default portfolio
      </label>

      <div className="flex justify-end gap-2 pt-2">
        <Button variant="secondary" onClick={onClose}>
          Cancel
        </Button>
        <Button type="submit" loading={isSubmitting}>
          {portfolio ? 'Save changes' : 'Create'}
        </Button>
      </div>
    </form>
  )
}
