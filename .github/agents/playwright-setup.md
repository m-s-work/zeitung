# Playwright Configuration for GitHub Copilot Agent Mode

## Important: System Browser Requirement

When running Playwright tests in **GitHub Copilot agent mode**, you **must configure Playwright to use the system browser** instead of the downloaded Playwright browsers.

### Current Configuration

The project is already configured to use the system Chrome browser. See `playwright.config.ts`:

```typescript
projects: [
  {
    name: 'chromium',
    use: { 
      ...devices['Desktop Chrome'],
      // Use system Chrome/Chromium instead of downloading Playwright browsers
      channel: 'chrome',
    },
  },
],
```

### Why This Is Required

GitHub Copilot agents run in containerized environments where:
- Playwright's bundled browsers may not be available
- System browsers are pre-installed and accessible
- Using `channel: 'chrome'` ensures Playwright uses the system Chrome/Chromium

### Installation

Before running tests, ensure Chrome/Chromium is installed on your system:

**On Ubuntu/Debian:**
```bash
sudo apt-get update
sudo apt-get install -y chromium-browser
```

**On macOS:**
```bash
brew install chromium
```

**On Windows:**
Download and install Chrome from https://www.google.com/chrome/

### Verification

To verify your configuration is working:

```bash
cd src/frontend
npm run test:e2e
```

### Alternative Configuration

If you need to use downloaded Playwright browsers instead (not recommended for agent mode):

```typescript
projects: [
  {
    name: 'chromium',
    use: { 
      ...devices['Desktop Chrome'],
      // Remove the channel property to use Playwright's bundled browser
    },
  },
],
```

Then install Playwright browsers:
```bash
npx playwright install chromium
```
