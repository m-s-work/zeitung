import { defineConfig, devices } from '@playwright/test';

/**
 * Playwright Test Configuration for Zeitung Frontend
 * 
 * BROWSER CONFIGURATION:
 * =====================
 * 
 * Local Development / Agent Mode:
 * - Uses Playwright-managed Chromium browser
 * - Install with: npm run test:e2e:install
 * - Browser is downloaded to ~/.cache/ms-playwright/
 * - No system Chrome/Chromium required
 * 
 * CI/CD Mode:
 * - Same configuration - uses Playwright-managed Chromium
 * - Set CI=true environment variable
 * - Automatically retries failed tests (2 retries)
 * - Runs tests serially (workers: 1) for stability
 * 
 * See https://playwright.dev/docs/test-configuration.
 */
export default defineConfig({
  testDir: './tests/e2e',
  /* Run tests in files in parallel */
  fullyParallel: true,
  /* Fail the build on CI if you accidentally left test.only in the source code. */
  forbidOnly: !!process.env.CI,
  /* Retry on CI only */
  retries: process.env.CI ? 2 : 0,
  /* Opt out of parallel tests on CI. */
  workers: process.env.CI ? 1 : undefined,
  /* Reporter to use. See https://playwright.dev/docs/test-reporters */
  reporter: [
    ['html'],
    ['junit', { outputFile: 'test-results/junit.xml' }],
    ['list']
  ],
  /* Shared settings for all the projects below. See https://playwright.dev/docs/api/class-testoptions. */
  use: {
    /* Base URL to use in actions like `await page.goto('/')`. */
    baseURL: process.env.BASE_URL || 'http://127.0.0.1:3000',
    /* Collect trace when retrying the failed test. See https://playwright.dev/docs/trace-viewer */
    trace: 'on-first-retry',
  },

  /* Configure projects for major browsers */
  projects: [
    {
      name: 'chromium',
      use: { 
        ...devices['Desktop Chrome'],
        // Uses Playwright-managed Chromium (not system Chrome)
        // This ensures consistent browser version across environments
      },
    },
  ],

  /* Run your local dev server before starting the tests */
  webServer: {
    command: 'npm run preview',
    url: 'http://127.0.0.1:3000',
    reuseExistingServer: !process.env.CI,
    timeout: 120 * 1000,
  },
});
