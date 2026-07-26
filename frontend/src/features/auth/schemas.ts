import { z } from 'zod'

/**
 * Client-side mirrors of the server's FluentValidation rules.
 *
 * The server remains the authority — these exist to give immediate feedback, not to be
 * trusted. Where the two could drift (password length), the value is kept in one constant so
 * the mismatch is at least easy to spot and fix.
 */
export const MIN_PASSWORD_LENGTH = 12

export const loginSchema = z.object({
  email: z.email('Enter a valid email address.'),
  password: z.string().min(1, 'Enter your password.'),
})

export const registerSchema = z.object({
  email: z.email('Enter a valid email address.').max(256),
  displayName: z
    .string()
    .min(1, 'Enter a display name.')
    .max(128, 'Display names are limited to 128 characters.'),
  password: z
    .string()
    .min(
      MIN_PASSWORD_LENGTH,
      `Use at least ${MIN_PASSWORD_LENGTH} characters.`,
    )
    .max(256),
})

export const changePasswordSchema = z
  .object({
    currentPassword: z.string().min(1, 'Enter your current password.'),
    newPassword: z
      .string()
      .min(
        MIN_PASSWORD_LENGTH,
        `Use at least ${MIN_PASSWORD_LENGTH} characters.`,
      )
      .max(256),
    confirmPassword: z.string(),
  })
  .refine((values) => values.newPassword === values.confirmPassword, {
    message: 'The passwords do not match.',
    path: ['confirmPassword'],
  })
  .refine((values) => values.newPassword !== values.currentPassword, {
    message: 'The new password must differ from the current one.',
    path: ['newPassword'],
  })

export const profileSchema = z.object({
  displayName: z.string().min(1, 'Enter a display name.').max(128),
})

export type LoginValues = z.infer<typeof loginSchema>
export type RegisterValues = z.infer<typeof registerSchema>
export type ChangePasswordValues = z.infer<typeof changePasswordSchema>
export type ProfileValues = z.infer<typeof profileSchema>
