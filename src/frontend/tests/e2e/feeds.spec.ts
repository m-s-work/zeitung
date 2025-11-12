import { test, expect } from '@playwright/test';

test.describe('Feed Management Page', () => {
  test('should load the feeds page', async ({ page }) => {
    await page.goto('/feeds');
    
    // Check that the page loads successfully
    await expect(page.locator('h1')).toContainText('Feed Management');
    await expect(page.getByText(/Manage your RSS feed subscriptions/i)).toBeVisible();
  });

  test('should have tabs for my feeds and recommendations', async ({ page }) => {
    await page.goto('/feeds');
    
    await expect(page.getByRole('tab', { name: /my feeds/i })).toBeVisible();
    await expect(page.getByRole('tab', { name: /recommendations/i })).toBeVisible();
  });

  test('should show page content', async ({ page }) => {
    await page.goto('/feeds');
    
    // Default tab is "My Feeds", wait for content to load
    await page.waitForTimeout(1000);
    
    // The page should load and show tabs
    // We just verify the tabs component is present
    const tabsList = page.locator('[role="tablist"]');
    await expect(tabsList).toBeVisible();
  });
});
