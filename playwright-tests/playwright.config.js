import { defineConfig, devices } from '@playwright/test';

export default defineConfig({
  testDir: './tests',
  fullyParallel: true,
  reporter: 'list',
  use: {
    baseURL: 'http://127.0.0.1:5173',
    trace: 'on-first-retry',
    screenshot: 'only-on-failure',
    video: 'retain-on-failure',
  },
  webServer: [
    {
      command:
        'dotnet run --project ../CreativesIntegration.Api/CreativesIntegration.Api.csproj',
      url: 'http://127.0.0.1:5068',
      reuseExistingServer: true,
      timeout: 120000,
    },
    {
      command: 'npm run dev -- --host 127.0.0.1',
      cwd: '../creatives-integration-frontend',
      url: 'http://127.0.0.1:5173',
      reuseExistingServer: true,
      timeout: 120000,
    },
  ],
  projects: [
    {
      name: 'chromium',
      use: { ...devices['Desktop Chrome'] },
    },
  ],
});
