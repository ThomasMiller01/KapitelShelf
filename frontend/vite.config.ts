import react from '@vitejs/plugin-react'
import { defineConfig } from 'vite'
import runtimeEnv from 'vite-plugin-runtime-env';

import pkg from "./package.json";

// https://vite.dev/config/
export default defineConfig({
  plugins: [
    react(),
    runtimeEnv(),
  ],
  define: {
    __APP_VERSION__: JSON.stringify(pkg.version)
  }
})
