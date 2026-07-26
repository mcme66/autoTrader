import { useId, type InputHTMLAttributes, type ReactNode } from 'react'

import { cn } from '@/lib/cn'

const fieldClasses =
  'bg-surface border-border text-content placeholder:text-content-subtle h-10 w-full rounded-lg border px-3 text-sm transition-colors disabled:opacity-60 aria-[invalid=true]:border-loss'

export interface InputProps extends InputHTMLAttributes<HTMLInputElement> {
  label?: string
  error?: string
  hint?: ReactNode
  leading?: ReactNode
}

export function Input({
  label,
  error,
  hint,
  leading,
  className,
  id,
  ...props
}: InputProps) {
  const generatedId = useId()
  const inputId = id ?? generatedId
  const describedBy = error ? `${inputId}-error` : hint ? `${inputId}-hint` : undefined

  return (
    <div className="w-full">
      {label ? (
        <label
          htmlFor={inputId}
          className="text-content mb-1.5 block text-xs font-medium"
        >
          {label}
        </label>
      ) : null}

      <div className="relative">
        {leading ? (
          <span className="text-content-subtle pointer-events-none absolute top-1/2 left-3 -translate-y-1/2">
            {leading}
          </span>
        ) : null}
        <input
          id={inputId}
          aria-invalid={error ? true : undefined}
          aria-describedby={describedBy}
          className={cn(fieldClasses, leading && 'pl-9', className)}
          {...props}
        />
      </div>

      {error ? (
        <p id={`${inputId}-error`} className="text-loss mt-1 text-xs">
          {error}
        </p>
      ) : hint ? (
        <p id={`${inputId}-hint`} className="text-content-subtle mt-1 text-xs">
          {hint}
        </p>
      ) : null}
    </div>
  )
}

export interface SelectProps
  extends React.SelectHTMLAttributes<HTMLSelectElement> {
  label?: string
  error?: string
}

export function Select({
  label,
  error,
  className,
  id,
  children,
  ...props
}: SelectProps) {
  const generatedId = useId()
  const selectId = id ?? generatedId

  return (
    <div className="w-full">
      {label ? (
        <label
          htmlFor={selectId}
          className="text-content mb-1.5 block text-xs font-medium"
        >
          {label}
        </label>
      ) : null}
      <select
        id={selectId}
        aria-invalid={error ? true : undefined}
        className={cn(fieldClasses, 'pr-8', className)}
        {...props}
      >
        {children}
      </select>
      {error ? <p className="text-loss mt-1 text-xs">{error}</p> : null}
    </div>
  )
}

export interface TextareaProps
  extends React.TextareaHTMLAttributes<HTMLTextAreaElement> {
  label?: string
  error?: string
}

export function Textarea({
  label,
  error,
  className,
  id,
  ...props
}: TextareaProps) {
  const generatedId = useId()
  const textareaId = id ?? generatedId

  return (
    <div className="w-full">
      {label ? (
        <label
          htmlFor={textareaId}
          className="text-content mb-1.5 block text-xs font-medium"
        >
          {label}
        </label>
      ) : null}
      <textarea
        id={textareaId}
        aria-invalid={error ? true : undefined}
        className={cn(fieldClasses, 'h-auto min-h-20 py-2', className)}
        {...props}
      />
      {error ? <p className="text-loss mt-1 text-xs">{error}</p> : null}
    </div>
  )
}
