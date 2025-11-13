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
 * 
 * CI/CD Mode (GitHub Actions):
 * - Uses pre-installed Chrome from the runner OS
 * - No browser installation required (faster pipeline)
 * - GitHub Actions ubuntu-latest includes Chrome
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
    ['playwright-ctrf-json-reporter', { outputFile: 'ctrf-report.json', outputDir: 'test-results' }],
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
        // In CI/CD: Use system Chrome (pre-installed on GitHub Actions runners)
        // Locally: Use Playwright-managed Chromium (install with npm run test:e2e:install)
        ...(process.env.CI && { channel: 'chrome' }),
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
