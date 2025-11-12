# GitHub Pages Setup Instructions

After merging this PR, follow these steps to enable GitHub Pages deployment:

## Step 1: Enable GitHub Pages

1. Go to your repository: https://github.com/m-s-work/zeitung
2. Click on **Settings** (top navigation)
3. In the left sidebar, click on **Pages** (under "Code and automation")

## Step 2: Configure Source

1. Under "Build and deployment"
2. For **Source**, select **GitHub Actions** from the dropdown
3. Save the configuration (should auto-save)

## Step 3: Verify Deployment

1. Go to **Actions** tab in your repository
2. You should see the "Deploy Documentation" workflow
3. Click on a workflow run to see the deployment status
4. Once complete, the documentation will be available at:
   
   **https://m-s-work.github.io/zeitung/**

## Step 4: Test the Documentation

Visit the documentation URL and verify:
- [ ] Home page loads correctly
- [ ] Navigation works
- [ ] Search functionality works
- [ ] All pages are accessible
- [ ] Links work correctly

## Troubleshooting

### Documentation doesn't load

If you see a 404 error:
1. Check that GitHub Pages is enabled in Settings > Pages
2. Verify the source is set to "GitHub Actions"
3. Check the Actions tab for any failed deployments
4. Ensure the workflow has completed successfully

### Base URL issues

If pages load but navigation doesn't work:
1. Check `.vitepress/config.mts` - `base` should be `/zeitung/`
2. This matches your repository name
3. Rebuild and redeploy if changed

### Workflow doesn't trigger

If the workflow doesn't run:
1. Check that you've pushed changes to the `docs/` folder
2. Verify the workflow file exists: `.github/workflows/deploy-docs.yml`
3. Check the "Actions" tab for any errors

## Manual Trigger

You can manually trigger the deployment:
1. Go to **Actions** tab
2. Select "Deploy Documentation" workflow
3. Click **Run workflow**
4. Select the `main` branch
5. Click **Run workflow**

## Updating Documentation

After initial setup, documentation will automatically deploy when:
- Changes are pushed to `main` branch
- Changes are in the `docs/` folder or workflow file

## Custom Domain (Optional)

To use a custom domain:
1. Go to Settings > Pages
2. Under "Custom domain", enter your domain
3. Follow GitHub's instructions to configure DNS
4. Update `base` in `.vitepress/config.mts` to `/`

## Support

If you encounter any issues:
1. Check the Actions tab for error logs
2. Review the workflow file: `.github/workflows/deploy-docs.yml`
3. See VitePress documentation: https://vitepress.dev/
4. Open an issue in the repository
