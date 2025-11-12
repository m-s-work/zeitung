import { test, expect } from '@playwright/test';

test.describe('Tag Preferences Page', () => {
  test('should load the tags page', async ({ page }) => {
    await page.goto('/tags');
    
    // Check that the page loads successfully
    await expect(page.locator('h1')).toContainText('Tag Preferences');
    await expect(page.getByText(/Customize your interests/i)).toBeVisible();
  });

  test('should have all tags section', async ({ page }) => {
    await page.goto('/tags');
    
    await expect(page.getByText('All Tags')).toBeVisible();
  });

  test('should have search input', async ({ page }) => {
    await page.goto('/tags');
    
    await expect(page.getByPlaceholder('Search tags...')).toBeVisible();
  });

  test('should show no tags message when empty', async ({ page }) => {
    await page.goto('/tags');
    
    // Check for empty state
    await expect(page.getByText('No tags found')).toBeVisible();
  });
});
