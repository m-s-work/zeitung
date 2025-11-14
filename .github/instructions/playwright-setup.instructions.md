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
- Playwright's bundled browsers are not be available (network access is restricted by firewall)
- System browsers are pre-installed and accessible
- Using `channel: 'chrome'` ensures Playwright uses the system Chrome/Chromium
