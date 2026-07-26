import { clsx, type ClassValue } from 'clsx'
import { twMerge } from 'tailwind-merge'

/**
 * Merges class names, letting a caller's utility win over a component's default rather than
 * both landing in the class list and the cascade deciding arbitrarily.
 */
export function cn(...inputs: ClassValue[]): string {
  return twMerge(clsx(inputs))
}
