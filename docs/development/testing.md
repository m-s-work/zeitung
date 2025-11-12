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

