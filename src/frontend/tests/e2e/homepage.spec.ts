import { test, expect } from '@playwright/test';

test.describe('Frontend Homepage', () => {
  test('should load the homepage', async ({ page }) => {
    await page.goto('/');
    
    // Check that the page loads successfully by verifying the heading is present
    await expect(page.locator('h1')).toContainText('Zeitung Feed Reader');
  });

  test('should render the main app component', async ({ page }) => {
    await page.goto('/');
    
    // Check that the page has some content
    const content = await page.textContent('body');
    expect(content).toBeTruthy();
    expect(content).toContain('Smart RSS reader with AI-powered recommendations');
  });

  test('should have navigation buttons', async ({ page }) => {
    await page.goto('/');
    
    // Check for navigation buttons
    await expect(page.getByRole('link', { name: /manage feeds/i })).toBeVisible();
    await expect(page.getByRole('link', { name: /tag preferences/i })).toBeVisible();
  });

  test('should show empty state when no articles', async ({ page }) => {
    await page.goto('/');
    
    // Check for empty state message
    await expect(page.getByText('No articles yet')).toBeVisible();
    await expect(page.getByText('Add some feeds to start reading articles')).toBeVisible();
  });
});
