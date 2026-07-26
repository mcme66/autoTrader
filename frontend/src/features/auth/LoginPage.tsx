import { zodResolver } from '@hookform/resolvers/zod'
import { useState } from 'react'
import { useForm } from 'react-hook-form'
import { Link, useLocation, useNavigate } from 'react-router'

import { AuthLayout } from '@/components/layout/AuthLayout'
import { Alert } from '@/components/ui/Alert'
import { Button } from '@/components/ui/Button'
import { Input } from '@/components/ui/Input'
import { errorMessage } from '@/lib/api-client'
import { loginSchema, type LoginValues } from './schemas'
import { useAuth } from './useAuth'

export function LoginPage() {
  const { login } = useAuth()
  const navigate = useNavigate()
  const location = useLocation()
  const [submitError, setSubmitError] = useState<string | null>(null)

  const {
    register,
    handleSubmit,
    formState: { errors, isSubmitting },
  } = useForm<LoginValues>({
    resolver: zodResolver(loginSchema),
    defaultValues: { email: '', password: '' },
  })

  const onSubmit = handleSubmit(async (values) => {
    setSubmitError(null)

    try {
      await login(values.email, values.password)
      // Return the user to the page that bounced them here, if there was one.
      const from = (location.state as { from?: string } | null)?.from
      void navigate(from ?? '/', { replace: true })
    } catch (error) {
      setSubmitError(errorMessage(error))
    }
  })

  return (
    <AuthLayout
      title="Sign in"
      description="Access your portfolios and market data."
      footer={
        <>
          No account yet?{' '}
          <Link to="/register" className="text-brand font-medium">
            Create one
          </Link>
        </>
      }
    >
      <form onSubmit={(event) => void onSubmit(event)} className="space-y-4">
        {submitError ? <Alert tone="error">{submitError}</Alert> : null}

        <Input
          label="Email"
          type="email"
          autoComplete="email"
          autoFocus
          error={errors.email?.message}
          {...register('email')}
        />

        <Input
          label="Password"
          type="password"
          autoComplete="current-password"
          error={errors.password?.message}
          {...register('password')}
        />

        <Button type="submit" loading={isSubmitting} className="w-full">
          Sign in
        </Button>
      </form>
    </AuthLayout>
  )
}
