import type { ReactNode } from 'react'
import { Link, type LinkProps } from 'react-router'

import {
  buttonClasses,
  type ButtonSize,
  type ButtonVariant,
} from './button-styles'

/**
 * A router link styled as a button. Kept separate from `Button` rather than adding an
 * `asChild` escape hatch, because navigation and actions should stay distinguishable in the
 * markup as well as to assistive technology.
 */
export function LinkButton({
  variant = 'primary',
  size = 'md',
  icon,
  className,
  children,
  ...props
}: LinkProps & {
  variant?: ButtonVariant
  size?: ButtonSize
  icon?: ReactNode
}) {
  return (
    <Link className={buttonClasses(variant, size, className)} {...props}>
      {icon}
      {children}
    </Link>
  )
}
