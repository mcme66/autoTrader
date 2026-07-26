import { X } from 'lucide-react'
import { useEffect, useRef, type ReactNode } from 'react'

import { Button } from './Button'

/**
 * Wraps the native `<dialog>` element, which gives focus trapping, Escape handling, and the
 * top layer for free — all things a hand-rolled modal gets subtly wrong.
 */
export function Modal({
  open,
  onClose,
  title,
  children,
  footer,
}: {
  open: boolean
  onClose: () => void
  title: string
  children: ReactNode
  footer?: ReactNode
}) {
  const dialogRef = useRef<HTMLDialogElement>(null)

  useEffect(() => {
    const dialog = dialogRef.current
    if (!dialog) {
      return
    }

    if (open && !dialog.open) {
      dialog.showModal()
    } else if (!open && dialog.open) {
      dialog.close()
    }
  }, [open])

  return (
    <dialog
      ref={dialogRef}
      onClose={onClose}
      onCancel={onClose}
      className="bg-surface text-content border-border m-auto w-[min(32rem,calc(100vw-2rem))] rounded-card border p-0 shadow-xl backdrop:bg-black/50"
    >
      {open ? (
        <>
          <div className="border-border flex items-center justify-between border-b px-5 py-4">
            <h2 className="text-sm font-semibold">{title}</h2>
            <Button
              variant="ghost"
              size="sm"
              onClick={onClose}
              aria-label="Close"
              className="-mr-2"
            >
              <X className="size-4" />
            </Button>
          </div>
          <div className="p-5">{children}</div>
          {footer ? (
            <div className="border-border flex justify-end gap-2 border-t px-5 py-4">
              {footer}
            </div>
          ) : null}
        </>
      ) : null}
    </dialog>
  )
}
