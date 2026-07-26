import { zodResolver } from '@hookform/resolvers/zod'
import { useState } from 'react'
import { useForm } from 'react-hook-form'

import { Alert } from '@/components/ui/Alert'
import { Badge } from '@/components/ui/Badge'
import { Button } from '@/components/ui/Button'
import { Card, CardBody, CardHeader } from '@/components/ui/Card'
import { Input } from '@/components/ui/Input'
import { PageHeader } from '@/components/ui/PageHeader'
import { errorMessage } from '@/lib/api-client'
import { formatDateTime } from '@/lib/format'
import { authApi } from '@/features/auth/auth-api'
import {
  changePasswordSchema,
  MIN_PASSWORD_LENGTH,
  profileSchema,
  type ChangePasswordValues,
  type ProfileValues,
} from '@/features/auth/schemas'
import { useAuth } from '@/features/auth/useAuth'
import { useTheme } from '@/features/theme/useTheme'
import type { ThemePreference } from '@/features/theme/theme-context'

export function SettingsPage() {
  return (
    <>
      <PageHeader
        title="Settings"
        description="Your profile, password, and how this app looks."
      />

      <div className="grid max-w-4xl gap-6">
        <ProfileCard />
        <PasswordCard />
        <AppearanceCard />
        <AccountCard />
      </div>
    </>
  )
}

function ProfileCard() {
  const { user, setUser } = useAuth()
  const [status, setStatus] = useState<string | null>(null)
  const [submitError, setSubmitError] = useState<string | null>(null)

  const {
    register,
    handleSubmit,
    reset,
    formState: { errors, isSubmitting, isDirty },
  } = useForm<ProfileValues>({
    resolver: zodResolver(profileSchema),
    defaultValues: { displayName: user?.displayName ?? '' },
  })

  const onSubmit = handleSubmit(async (values) => {
    setStatus(null)
    setSubmitError(null)

    try {
      setUser(await authApi.updateProfile(values.displayName))
      reset(values)
      setStatus('Profile updated.')
    } catch (error) {
      setSubmitError(errorMessage(error))
    }
  })

  return (
    <Card>
      <CardHeader title="Profile" description="How you appear in the app." />
      <CardBody>
        <form
          onSubmit={(event) => void onSubmit(event)}
          className="max-w-md space-y-4"
        >
          {submitError ? <Alert tone="error">{submitError}</Alert> : null}
          {status ? <Alert tone="success">{status}</Alert> : null}

          <Input
            label="Display name"
            error={errors.displayName?.message}
            {...register('displayName')}
          />

          <Input
            label="Email"
            value={user?.email ?? ''}
            readOnly
            disabled
            hint="Email changes are not supported yet."
          />

          <Button type="submit" loading={isSubmitting} disabled={!isDirty}>
            Save changes
          </Button>
        </form>
      </CardBody>
    </Card>
  )
}

function PasswordCard() {
  const [status, setStatus] = useState<string | null>(null)
  const [submitError, setSubmitError] = useState<string | null>(null)

  const {
    register,
    handleSubmit,
    reset,
    formState: { errors, isSubmitting },
  } = useForm<ChangePasswordValues>({
    resolver: zodResolver(changePasswordSchema),
    defaultValues: {
      currentPassword: '',
      newPassword: '',
      confirmPassword: '',
    },
  })

  const onSubmit = handleSubmit(async (values) => {
    setStatus(null)
    setSubmitError(null)

    try {
      await authApi.changePassword({
        currentPassword: values.currentPassword,
        newPassword: values.newPassword,
      })
      reset()
      setStatus(
        'Password changed. Other sessions have been signed out.',
      )
    } catch (error) {
      setSubmitError(errorMessage(error))
    }
  })

  return (
    <Card>
      <CardHeader
        title="Password"
        description="Changing your password revokes every refresh token except this session's."
      />
      <CardBody>
        <form
          onSubmit={(event) => void onSubmit(event)}
          className="max-w-md space-y-4"
        >
          {submitError ? <Alert tone="error">{submitError}</Alert> : null}
          {status ? <Alert tone="success">{status}</Alert> : null}

          <Input
            label="Current password"
            type="password"
            autoComplete="current-password"
            error={errors.currentPassword?.message}
            {...register('currentPassword')}
          />

          <Input
            label="New password"
            type="password"
            autoComplete="new-password"
            hint={`At least ${MIN_PASSWORD_LENGTH} characters.`}
            error={errors.newPassword?.message}
            {...register('newPassword')}
          />

          <Input
            label="Confirm new password"
            type="password"
            autoComplete="new-password"
            error={errors.confirmPassword?.message}
            {...register('confirmPassword')}
          />

          <Button type="submit" loading={isSubmitting}>
            Change password
          </Button>
        </form>
      </CardBody>
    </Card>
  )
}

const themeOptions: { value: ThemePreference; label: string }[] = [
  { value: 'light', label: 'Light' },
  { value: 'dark', label: 'Dark' },
  { value: 'system', label: 'Match system' },
]

function AppearanceCard() {
  const { preference, setPreference } = useTheme()

  return (
    <Card>
      <CardHeader
        title="Appearance"
        description="Stored in this browser only."
      />
      <CardBody>
        <div className="flex flex-wrap gap-2">
          {themeOptions.map((option) => (
            <Button
              key={option.value}
              variant={preference === option.value ? 'primary' : 'secondary'}
              size="sm"
              aria-pressed={preference === option.value}
              onClick={() => setPreference(option.value)}
            >
              {option.label}
            </Button>
          ))}
        </div>
      </CardBody>
    </Card>
  )
}

function AccountCard() {
  const { user } = useAuth()

  return (
    <Card>
      <CardHeader title="Account" />
      <CardBody>
        <dl className="grid gap-4 text-sm sm:grid-cols-2">
          <div>
            <dt className="text-content-muted text-xs">Roles</dt>
            <dd className="mt-1 flex flex-wrap gap-1.5">
              {user?.roles.length ? (
                user.roles.map((role) => (
                  <Badge key={role} tone="brand">
                    {role}
                  </Badge>
                ))
              ) : (
                <span className="text-content-subtle">None</span>
              )}
            </dd>
          </div>
          <div>
            <dt className="text-content-muted text-xs">Member since</dt>
            <dd className="text-content mt-1">
              {formatDateTime(user?.createdAt)}
            </dd>
          </div>
          <div>
            <dt className="text-content-muted text-xs">Last sign-in</dt>
            <dd className="text-content mt-1">
              {formatDateTime(user?.lastLoginAt)}
            </dd>
          </div>
        </dl>
      </CardBody>
    </Card>
  )
}
