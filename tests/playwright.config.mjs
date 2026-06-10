import { defineConfig } from '@playwright/test';

export default defineConfig({
  testDir: './e2e',
  timeout: 60000,
  use: {
    baseURL: 'http://127.0.0.1:8787',
    viewport: { width: 1000, height: 800 },
  },
  webServer: {
    command: 'node tools/serve.mjs',
    url: 'http://127.0.0.1:8787/websim/index.html',
    reuseExistingServer: true,
  },
  reporter: [['list']],
});
