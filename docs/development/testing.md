# Testing

This guide covers testing strategies and practices for Zeitung.

## Backend Testing

### Running Tests

```bash
cd src/backend

# Run all tests
dotnet test Zeitung.sln

# Run with coverage
dotnet test Zeitung.sln --collect:"XPlat Code Coverage"

# Run specific test project
dotnet test Zeitung.Api.Tests

# Run tests excluding integration tests
dotnet test Zeitung.sln --filter "TestCategory!=IntegrationTest"

# Run only integration tests
dotnet test Zeitung.sln --filter "TestCategory=IntegrationTest"
```

### Test Categories

#### Unit Tests
Located in `*.Tests` projects. These test individual components in isolation.

```csharp
[Fact]
public void TagService_CalculateDecay_AppliesCorrectFormula()
{
    // Arrange
    var service = new TagService();
    
    // Act
    var result = service.CalculateDecay(10.0, 5, 10);
    
    // Assert
    Assert.InRange(result, 3.0, 4.0);
}
```

#### Integration Tests
Located in `*.Tests` projects with `[Trait("Category", "IntegrationTest")]`.

```csharp
[Fact]
[Trait("Category", "IntegrationTest")]
public async Task FeedController_AddFeed_StoresInDatabase()
{
    // Uses real database (test instance)
    // Tests full request/response cycle
}
```

### Test Structure

```
src/backend/
├── Zeitung.Api.Tests/
│   ├── Controllers/
│   ├── Services/
│   └── Integration/
├── Zeitung.Worker.Tests/
│   ├── Services/
│   └── Integration/
└── Zeitung.AppHost.Tests/
    └── Integration/
```

### Writing Backend Tests

Follow these patterns:

```csharp
public class TagServiceTests
{
    [Fact]
    public void MethodName_Scenario_ExpectedBehavior()
    {
        // Arrange - Set up test data
        var service = new TagService();
        var input = new Tag { Name = "test" };
        
        // Act - Execute the method
        var result = service.ProcessTag(input);
        
        // Assert - Verify the outcome
        Assert.NotNull(result);
        Assert.Equal("test", result.Name);
    }
}
```

## Frontend Testing

### Running Tests

```bash
cd src/frontend

# Run unit tests
npm test

# Run tests in watch mode
npm run test:watch

# Run with coverage
npm run test:coverage

# Run E2E tests
npm run test:e2e

# Run E2E in headed mode
npm run test:e2e -- --headed

# Run E2E in debug mode
npm run test:e2e -- --debug
```

### Unit Tests (Vitest)

Located in `tests/unit/` and `components/**/*.test.ts`.

```typescript
import { describe, it, expect } from 'vitest'
import { mount } from '@vue/test-utils'
import ArticleCard from '~/components/ArticleCard.vue'

describe('ArticleCard', () => {
  it('renders article title', () => {
    const wrapper = mount(ArticleCard, {
      props: {
        article: {
          title: 'Test Article',
          link: 'https://example.com',
          description: 'Test description'
        }
      }
    })
    
    expect(wrapper.text()).toContain('Test Article')
  })
})
```

### E2E Tests (Playwright)

Located in `tests/e2e/`. See [E2E Testing Guide](https://github.com/m-s-work/zeitung/blob/main/src/frontend/tests/E2E_TESTING.md) for details.

```typescript
import { test, expect } from '@playwright/test'

test('user can view articles', async ({ page }) => {
  await page.goto('/')
  
  // Wait for articles to load
  await expect(page.locator('.article-card')).toBeVisible()
  
  // Check article count
  const articles = await page.locator('.article-card').count()
  expect(articles).toBeGreaterThan(0)
})
```

## CI/CD Testing

Tests run automatically on:
- Every pull request
- Push to main branch
- Push to develop branch

### GitHub Actions Workflow

```yaml
jobs:
  test-frontend:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
      - uses: actions/setup-node@v4
      - run: npm ci
      - run: npm test
      - run: npm run build

  test-backend:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
      - uses: actions/setup-dotnet@v4
      - run: dotnet test
```

## Test Data

### Backend Test Data

Use fixtures for consistent test data:

```csharp
public static class TestData
{
    public static User CreateTestUser() => new()
    {
        Id = 1,
        Email = "test@example.com",
        Username = "testuser",
        Role = UserRole.User
    };
    
    public static Feed CreateTestFeed() => new()
    {
        Id = 1,
        Url = "https://example.com/rss",
        Name = "Test Feed",
        IsApproved = true
    };
}
```

### Frontend Test Data

Use factories for test data:

```typescript
export function createTestArticle(overrides = {}) {
  return {
    id: 1,
    title: 'Test Article',
    link: 'https://example.com',
    description: 'Test description',
    publishedDate: '2024-01-01T00:00:00Z',
    tags: ['test'],
    ...overrides
  }
}
```

## Mocking

### Backend Mocking

Use Moq for mocking dependencies:

```csharp
[Fact]
public async Task GetArticles_ReturnsFilteredResults()
{
    // Arrange
    var mockRepo = new Mock<IArticleRepository>();
    mockRepo.Setup(r => r.GetArticlesAsync(It.IsAny<int>()))
            .ReturnsAsync(new List<Article> { /* test data */ });
    
    var controller = new ArticleController(mockRepo.Object);
    
    // Act & Assert
    var result = await controller.GetArticles(userId: 1);
    Assert.NotEmpty(result);
}
```

### Frontend Mocking

Use Vitest mocks:

```typescript
import { vi } from 'vitest'

// Mock API calls
vi.mock('~/composables/useApi', () => ({
  useApi: () => ({
    getArticles: vi.fn().mockResolvedValue([
      { id: 1, title: 'Test' }
    ])
  })
}))
```

## Test Coverage

### Coverage Reports

Backend coverage is tracked with Coverlet:

```bash
cd src/backend
dotnet test --collect:"XPlat Code Coverage"
```

Frontend coverage with Vitest:

```bash
cd src/frontend
npm run test:coverage
```

### Coverage Goals

- **Critical paths**: 90%+ coverage
- **Business logic**: 80%+ coverage
- **UI components**: 70%+ coverage
- **Overall**: 75%+ coverage

## Continuous Testing

Tests run automatically:
- On every pull request
- On merge to main

Monitor test results in GitHub Actions and fix failures promptly.

## Nightly RSS Feed Tests

### Overview

Zeitung includes automated nightly tests that verify all RSS feeds are working correctly. These tests run at 2 AM UTC every night and can also be triggered manually.

### What the Tests Do

The nightly feed tests:
- Fetch each configured RSS feed
- Parse the feed content
- Verify the feed returns articles
- Check that articles have required properties (title, link)

### Running Nightly Tests Locally

You can run the nightly RSS feed tests locally:

```bash
cd src/backend

# Run only the nightly feed tests
dotnet test Zeitung.sln --filter "TestCategory=NightlyFeedTest"

# Run with detailed output
dotnet test Zeitung.sln --filter "TestCategory=NightlyFeedTest" --logger "console;verbosity=detailed"
```

### Test Structure

Tests are located in `Zeitung.AppHost.Tests/NightlyRssFeedTests.cs` and use:
- **NUnit TestCaseSource**: Each feed has a separate test case
- **Aspire Testing Framework**: Tests use the full application host with worker and database
- **Integration Testing**: Tests fetch real RSS feeds from the internet

Example test case:

```csharp
[Test]
[TestCaseSource(nameof(RssFeedTestCases))]
public async Task RssFeed_CanBeFetchedAndParsed_Successfully(RssFeed feed)
{
    // Test implementation that fetches and parses the feed
}
```

### Automated Issue Creation

When a feed fails, the workflow automatically:

1. **Checks for existing issues**: Searches for open issues with the same feed name
2. **Creates new issues**: If no issue exists, creates one with:
   - Title: "RSS Feed Failure: [Feed Name]"
   - Labels: `rss-feed-failure`, `automated`, `bug`
   - Assignee: `@copilot`
   - Details: Error message, timestamp, link to test run
3. **Updates existing issues**: If an issue exists:
   - Adds a comment with the new error (if different from last error)
   - Skips comment if the error is the same

### Triggering Tests Manually

You can trigger the nightly tests manually:

1. Go to the **Actions** tab in GitHub
2. Select **Nightly RSS Feed Tests** workflow
3. Click **Run workflow**
4. Select the branch and click **Run workflow**

This is useful for:
- Testing changes to feed URLs
- Verifying fixes for feed failures
- Testing the workflow itself

### Adding New Feeds to Test

To add a new RSS feed to the nightly tests:

1. Add the feed to `RssFeedTestCases` in `NightlyRssFeedTests.cs`:

```csharp
new RssFeed
{
    Name = "New Feed",
    Url = "https://example.com/feed.xml",
    Description = "Feed description"
}
```

2. The test will automatically run for the new feed on the next nightly run

### Monitoring Feed Health

Feed health can be monitored through:
- **GitHub Actions**: View test results in the Actions tab
- **GitHub Issues**: Open issues indicate failing feeds
- **Test Artifacts**: Download test results for detailed analysis

### Troubleshooting Failed Feeds

When a feed fails:

1. **Check the issue**: Review the error message in the created issue
2. **Test manually**: Try fetching the feed URL directly
3. **Common issues**:
   - Feed URL changed or moved
   - Feed format changed
   - Feed temporarily unavailable
   - Network/firewall issues
4. **Fix the feed**: Update feed URL in configuration or fix parser if needed
5. **Verify**: Run tests manually to confirm the fix
6. **Close issue**: Once fixed, close the issue

