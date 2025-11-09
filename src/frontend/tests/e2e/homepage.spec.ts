import { test, expect } from '@playwright/test';

test.describe('Frontend Homepage', () => {
  test('should load the homepage', async ({ page }) => {
    await page.goto('/');
    
    // Check that the page loads successfully
    await expect(page).toHaveTitle(/Zeitung|Nuxt/);
  });

  test('should render the main app component', async ({ page }) => {
    await page.goto('/');
    
    // Check that the page has some content
    const content = await page.textContent('body');
    expect(content).toBeTruthy();
  });
});
