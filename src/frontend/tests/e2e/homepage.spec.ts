import { test, expect } from '@playwright/test';

test.describe('Frontend Homepage', () => {
  test('should load the homepage', async ({ page }) => {
    await page.goto('/');
    
    // Check that the page loads successfully by verifying the heading is present
    await expect(page.locator('h1')).toContainText('Welcome to Zeitung');
  });

  test('should render the main app component', async ({ page }) => {
    await page.goto('/');
    
    // Check that the page has some content
    const content = await page.textContent('body');
    expect(content).toBeTruthy();
    expect(content).toContain('RSS Feed Reader');
  });
});
