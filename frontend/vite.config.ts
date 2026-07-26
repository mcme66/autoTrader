import { fileURLToPath, URL } from 'node:url'

import tailwindcss from '@tailwindcss/vite'
import react from '@vitejs/plugin-react'
import { loadEnv } from 'vite'
import { defineConfig } from 'vitest/config'

export default defineConfig(({ mode }) => {
  const env = loadEnv(mode, process.cwd(), '')
  const apiTarget = env.VITE_DEV_API_PROXY_TARGET ?? 'http://localhost:5088'

  return {
    plugins: [react(), tailwindcss()],
    resolve: {
      alias: {
        '@': fileURLToPath(new URL('./src', import.meta.url)),
      },
    },
    server: {
      port: 5173,
      strictPort: true,
      // Proxying keeps dev same-origin with production, so auth cookies and
      // relative `/api` URLs behave identically in both modes.
      proxy: {
        '/api': {
          target: apiTarget,
          changeOrigin: true,
        },
      },
    },
    build: {
      outDir: 'dist',
      sourcemap: true,
      rollupOptions: {
        output: {
          // Recharts and d3 are a large, rarely changing dependency; splitting them out keeps
          // the app chunk small and cacheable across deploys.
          manualChunks: (id: string) =>
            id.includes('node_modules') &&
            (id.includes('recharts') || id.includes('d3-'))
              ? 'charts'
              : undefined,
        },
      },
    },
    test: {
      environment: 'jsdom',
      globals: true,
      setupFiles: ['./vitest.setup.ts'],
      css: false,
    },
  }
})
