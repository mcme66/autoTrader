import type { ButtonHTMLAttributes, ReactNode } from 'react'

import {
  buttonClasses,
  type ButtonSize,
  type ButtonVariant,
} from './button-styles'
import { Spinner } from './Spinner'

export interface ButtonProps extends ButtonHTMLAttributes<HTMLButtonElement> {
  variant?: ButtonVariant
  size?: ButtonSize
  loading?: boolean
  icon?: ReactNode
}

export function Button({
  variant = 'primary',
  size = 'md',
  loading = false,
  icon,
  className,
  children,
  disabled,
  type = 'button',
  ...props
}: ButtonProps) {
  return (
    <button
      type={type}
      // A button that is busy is also unusable; deriving this here means no caller can forget
      // and allow a double submit.
      disabled={disabled ?? loading}
      aria-busy={loading}
      className={buttonClasses(variant, size, className)}
      {...props}
    >
      {loading ? <Spinner className="size-4" /> : icon}
      {children}
    </button>
  )
}
