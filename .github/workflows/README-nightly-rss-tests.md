# Nightly RSS Feed Tests Workflow

This workflow automatically tests all RSS feeds configured in Zeitung to ensure they remain accessible and parseable.

## Schedule

- **Nightly**: Runs every day at 2:00 AM UTC
- **Manual**: Can be triggered manually via GitHub Actions UI

## What It Does

1. **Builds the application**: Compiles the backend with all tests
2. **Runs integration tests**: Executes tests marked with `TestCategory=NightlyFeedTest`
3. **Parses test results**: Extracts information about failed feeds from TRX test results
4. **Manages GitHub issues**:
   - Creates issues for newly failing feeds
   - Updates existing issues with new errors (if different)
   - Assigns all issues to `@copilot` for automated handling

## Test Results

Test results are:
- Published as GitHub Actions check results
- Uploaded as workflow artifacts (retained for 30 days)
- Used to create/update issues for failed feeds

## Issue Management

### Issue Format

Failed feeds trigger issues with:
- **Title**: `RSS Feed Failure: [Feed Name]`
- **Labels**: `rss-feed-failure`, `automated`, `bug`
- **Assignee**: `@copilot`
- **Body**: Includes error details, timestamp, and link to test run

### Issue Updates

- If an issue already exists for a feed, a comment is added instead of creating a new issue
- Comments are only added if the error message differs from the last comment
- This prevents spam from identical errors

### Issue Lifecycle

1. **Created**: When a feed first fails
2. **Updated**: When the feed continues to fail with different errors
3. **Manual closure**: When the feed is fixed (should be closed by a maintainer)

## Feeds Tested

Currently tested feeds:
- **BBC News**: http://feeds.bbci.co.uk/news/rss.xml
- **Heise Online**: https://www.heise.de/rss/heise-atom.xml
- **Golem.de**: https://rss.golem.de/rss.php?feed=RSS2.0
- **ORF.at**: https://rss.orf.at/news.xml

## Manual Triggering

To manually trigger the workflow:

1. Go to **Actions** tab in GitHub
2. Select **Nightly RSS Feed Tests**
3. Click **Run workflow**
4. Select branch (usually `main` or `develop`)
5. Click **Run workflow**

## Permissions

This workflow requires:
- `contents: read` - To checkout the repository
- `issues: write` - To create and update issues

## Customization

### Changing Schedule

To change when tests run, modify the cron expression:

```yaml
on:
  schedule:
    - cron: '0 2 * * *'  # Current: 2 AM UTC daily
```

Common schedules:
- `0 */6 * * *` - Every 6 hours
- `0 0 * * 1` - Every Monday at midnight
- `0 2 * * 1,4` - Monday and Thursday at 2 AM

### Adding Feeds

Add new feeds to test in `src/backend/Zeitung.AppHost.Tests/NightlyRssFeedTests.cs`:

```csharp
public static IEnumerable<TestCaseData> RssFeedTestCases
{
    get
    {
        var feeds = new[]
        {
            // ... existing feeds ...
            new RssFeed
            {
                Name = "New Feed Name",
                Url = "https://example.com/rss.xml",
                Description = "Feed description"
            }
        };
        // ...
    }
}
```

### Disabling PR Runs

The workflow is configured to run nightly and can be manually triggered. It does NOT run on pull requests by default. If you want to enable PR runs for testing, add:

```yaml
on:
  schedule:
    - cron: '0 2 * * *'
  workflow_dispatch:
  pull_request:  # Add this to enable PR runs
    paths:
      - '.github/workflows/nightly-rss-feed-tests.yml'
      - 'src/backend/Zeitung.AppHost.Tests/NightlyRssFeedTests.cs'
```

## Troubleshooting

### Tests Fail in CI but Pass Locally

- Check network accessibility in CI environment
- Verify timeouts are sufficient for slow feeds
- Check if feeds require specific User-Agent headers

### Too Many Issues Created

- Verify error comparison logic in the workflow
- Check that identical errors are being detected correctly
- Adjust error message truncation if needed

### Missing Test Results

- Check that TRX logger is configured correctly
- Verify test output path matches what workflow expects
- Ensure tests are properly categorized with `TestCategory=NightlyFeedTest`

## Related Files

- **Test Implementation**: `src/backend/Zeitung.AppHost.Tests/NightlyRssFeedTests.cs`
- **Feed Parser**: `src/backend/Zeitung.Worker/Services/RssFeedParser.cs`
- **Feed Configuration**: `src/backend/Zeitung.Worker/appsettings.json`
- **Documentation**: `docs/development/testing.md`
