import { build, defineConfig } from 'vite'
import vue from '@vitejs/plugin-vue'

//
// Import certificate so that the dev server can serve on HTTPS
//
// https://alexpotter.dev/net-6-with-vue-3/
//
// If there is no certificate here, this code will do nothing.
// If you DO export your certificate as a pfx, and put it here,
// go ahead and uncomment the "https: httpsSettings" line below,
// and be sure to connect to the dev server using https.
//

import fs from 'node:fs';
import type { ServerOptions } from 'https';

const developmentCertificateName = 'localhost.pfx';

const httpsSettings: ServerOptions =
  fs.existsSync(developmentCertificateName)
    ? {
      pfx: fs.readFileSync(developmentCertificateName),
      passphrase: 'secret'    
    }
    : {}

// https://vitejs.dev/config/
export default defineConfig({
  build: {
    chunkSizeWarningLimit: 600
  },
  plugins: [vue()],
  server: {
    port: 5173,
    // Uncomment `https:` line to enable dev server to listen on HTTPS
    // Be sure you have a trusted certificate in place first 
    // (See above)
    // https: httpsSettings,
    proxy: {
      '/api': {
        target: 'http://localhost:5235/',
        changeOrigin: true
      }
    }
  }
})
