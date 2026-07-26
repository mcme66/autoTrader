/// <reference types="vite/client" />

/** Typed build-time configuration. Untyped `import.meta.env` reads are `any` and defeat strict mode. */
interface ImportMetaEnv {
  /** Overrides the API origin. Left unset in both run modes, where `/api` is same-origin. */
  readonly VITE_API_BASE_URL?: string
  /** Dev-only: where the Vite proxy forwards `/api`. */
  readonly VITE_DEV_API_PROXY_TARGET?: string
}

interface ImportMeta {
  readonly env: ImportMetaEnv
}
