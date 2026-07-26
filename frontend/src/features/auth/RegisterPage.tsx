import { zodResolver } from '@hookform/resolvers/zod'
import { useState } from 'react'
import { useForm } from 'react-hook-form'
import { Link, useNavigate } from 'react-router'

import { AuthLayout } from '@/components/layout/AuthLayout'
import { Alert } from '@/components/ui/Alert'
import { Button } from '@/components/ui/Button'
import { Input } from '@/components/ui/Input'
import { errorMessage } from '@/lib/api-client'
import {
  MIN_PASSWORD_LENGTH,
  registerSchema,
  type RegisterValues,
} from './schemas'
import { useAuth } from './useAuth'

export function RegisterPage() {
  const { register: createAccount } = useAuth()
  const navigate = useNavigate()
  const [submitError, setSubmitError] = useState<string | null>(null)

  const {
    register,
    handleSubmit,
    formState: { errors, isSubmitting },
  } = useForm<RegisterValues>({
    resolver: zodResolver(registerSchema),
    defaultValues: { email: '', displayName: '', password: '' },
  })

  const onSubmit = handleSubmit(async (values) => {
    setSubmitError(null)

    try {
      await createAccount(values.email, values.password, values.displayName)
      void navigate('/', { replace: true })
    } catch (error) {
      setSubmitError(errorMessage(error))
    }
  })

  return (
    <AuthLayout
      title="Create an account"
      description="The first account created becomes the administrator."
      footer={
        <>
          Already registered?{' '}
          <Link to="/login" className="text-brand font-medium">
            Sign in
          </Link>
        </>
      }
    >
      <form onSubmit={(event) => void onSubmit(event)} className="space-y-4">
        {submitError ? <Alert tone="error">{submitError}</Alert> : null}

        <Input
          label="Display name"
          autoComplete="name"
          autoFocus
          error={errors.displayName?.message}
          {...register('displayName')}
        />

        <Input
          label="Email"
          type="email"
          autoComplete="email"
          error={errors.email?.message}
          {...register('email')}
        />

        <Input
          label="Password"
          type="password"
          autoComplete="new-password"
          hint={`At least ${MIN_PASSWORD_LENGTH} characters.`}
          error={errors.password?.message}
          {...register('password')}
        />

        <Button type="submit" loading={isSubmitting} className="w-full">
          Create account
        </Button>
      </form>
    </AuthLayout>
  )
}
