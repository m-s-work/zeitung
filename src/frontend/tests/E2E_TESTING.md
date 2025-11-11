# E2E Testing Guide

This document explains how to run E2E tests for the Zeitung frontend in different environments.

## Quick Start

```bash
# Install dependencies
npm install

# Install Playwright browsers
npm run test:e2e:install

# Build the app
npm run build

# Run E2E tests
npm run test:e2e
```

## Browser Configuration

### Standard Setup (Local & CI/CD)

The project uses **Playwright-managed Chromium** for all environments. This ensures:
- ✅ Consistent browser version across all environments
- ✅ No system browser dependencies
- ✅ Automatic browser updates with Playwright
- ✅ Works in sandboxed/restricted environments

**Installation:**
```bash
npm run test:e2e:install
```

This downloads Chromium to `~/.cache/ms-playwright/` (Linux/Mac) or `%USERPROFILE%\AppData\Local\ms-playwright\` (Windows).

### Local Development / Agent Mode

When running tests locally or in GitHub Copilot agent mode:

```bash
npm run test:e2e
```

**Behavior:**
- Tests run in parallel for speed
- No automatic retries
- Uses multiple workers
- Reuses existing dev server if running

### CI/CD Mode

When running in CI/CD pipelines (GitHub Actions, GitLab CI, etc.):

```bash
CI=true npm run test:e2e
```

**Behavior:**
- Tests run serially (`workers: 1`) for stability
- Automatic retries on failure (`retries: 2`)
- Fails if `test.only` is found in code
- Starts fresh preview server

## Test Structure

```
tests/e2e/
├── homepage.spec.ts     # Tests for main article feed (4 tests)
├── feeds.spec.ts        # Tests for feed management (6 tests)
└── tags.spec.ts         # Tests for tag preferences (5 tests)
```

**Total: 15 E2E tests**

## Test Coverage

### Homepage (`homepage.spec.ts`)
- ✅ Page loads with correct title
- ✅ Main content renders
- ✅ Navigation buttons visible
- ✅ Empty state displays when no articles

### Feed Management (`feeds.spec.ts`)
- ✅ Page loads correctly
- ✅ Navigation works
- ✅ Add feed button visible
- ✅ Tabs for My Feeds and Recommendations
- ✅ Empty state displays
- ✅ Modal opens on button click

### Tag Preferences (`tags.spec.ts`)
- ✅ Page loads correctly
- ✅ Navigation works
- ✅ All tags section visible
- ✅ Search functionality present
- ✅ Empty state displays

## Common Issues

### Icon Loading Errors

You may see errors like:
```
[Icon] failed to load icon `heroicons:newspaper`
```

**This is expected** in sandboxed/restricted environments where external CDNs are blocked. Icons won't display in tests, but functionality is unaffected. Tests verify structure and behavior, not visual appearance.

### Browser Not Found

If you get "chromium not found":
```bash
npm run test:e2e:install
```

### Tests Timeout

If tests timeout during server startup:
- Increase timeout in `playwright.config.ts` → `webServer.timeout`
- Ensure you've run `npm run build` first
- Check if port 3000 is already in use

### OpenAPI Types Missing

If you see errors about missing OpenAPI types:
```bash
cd ../backend
./generate-openapi.sh
cd ../frontend
npm run postinstall
```

## Environment Variables

| Variable | Purpose | Default |
|----------|---------|---------|
| `CI` | Enable CI mode (retries, serial execution) | `false` |
| `BASE_URL` | Base URL for tests | `http://127.0.0.1:3000` |

## Debugging Tests

### View Test Report

After tests run:
```bash
npx playwright show-report
```

### Run Tests in Debug Mode

```bash
npx playwright test --debug
```

### Run Specific Test File

```bash
npx playwright test tests/e2e/homepage.spec.ts
```

### Run Specific Test

```bash
npx playwright test -g "should load the homepage"
```

### View Traces

When tests fail in CI, traces are saved:
```bash
npx playwright show-trace test-results/*/trace.zip
```

## CI/CD Integration

### GitHub Actions Example

The repository includes a complete CI/CD workflow in `.github/workflows/ci-cd.yml`. Key steps for E2E tests:

```yaml
- name: Setup .NET
  uses: actions/setup-dotnet@v4
  with:
    dotnet-version: '9.0.x'

- name: Generate OpenAPI schema
  working-directory: src/backend
  run: |
    chmod +x generate-openapi.sh
    ./generate-openapi.sh

- name: Install frontend dependencies
  working-directory: src/frontend
  run: npm ci

- name: Install Playwright browsers
  working-directory: src/frontend
  run: npx playwright install --with-deps chromium

- name: Build frontend
  working-directory: src/frontend
  run: npm run build

- name: Run Playwright tests
  working-directory: src/frontend
  run: npm run test:e2e
  env:
    CI: true
```

**Important:** The OpenAPI schema must be generated before building the frontend, as the frontend depends on the generated types.

## Key Differences: Agent vs CI Mode

| Aspect | Agent Mode | CI Mode |
|--------|-----------|---------|
| Browser | Playwright Chromium | Playwright Chromium |
| Installation | `npm run test:e2e:install` | `npm run test:e2e:install` |
| Execution | Parallel | Serial |
| Retries | None | 2 retries |
| Workers | Multiple | 1 |
| Server | Reuses if running | Starts fresh |

**Note:** Both modes use the same Playwright-managed Chromium browser. No system Chrome/Chromium installation required.
