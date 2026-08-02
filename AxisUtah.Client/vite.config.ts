import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'

// https://vite.dev/config/
export default defineConfig({
  plugins: [react()],
  css: {
    devSourcemap: true,
  },
  server: {
    sourcemapIgnoreList: () => false, // Prevents Vite from hiding sourcemaps from VS Code
    proxy: {
      '/admin': 'http://localhost:5173',
    },
  },
})
