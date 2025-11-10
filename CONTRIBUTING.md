# Contributing to Zeitung

Thank you for your interest in contributing to Zeitung! This document provides guidelines and best practices for contributing to the project.

## Pull Request Process

### Conventional Commits

**All PRs and Commits must follow [Conventional Commits](https://www.conventionalcommits.org/)** because we squash all PRs. Your PR title becomes the final commit message in the main branch.

#### Format

```
<type>(<scope>): <description>
```

#### Types

- `feat`: New feature
- `fix`: Bug fix
- `docs`: Documentation changes
- `style`: Code style changes (formatting, etc.)
- `refactor`: Code refactoring
- `perf`: Performance improvements
- `test`: Test additions or modifications
- `build`: Build system changes
- `ci`: CI/CD changes
- `chore`: Other changes

#### Examples

```
feat(api): add user authentication endpoint
fix(frontend): resolve login form validation issue
docs(readme): update installation instructions
```

### Before Submitting

1. Ensure all tests pass
2. Follow the code change guidelines (see below)
3. Update documentation if needed
4. Write a clear PR description

## Code Change Guidelines

### Minimal Changes

- Make the **smallest possible changes** to achieve your goal
- Only modify what's necessary
- Avoid refactoring unrelated code

### Preserve Existing Code

- **DO NOT remove existing comments** in code
- Keep existing code structure unless changes are required
- Maintain backward compatibility when possible

### Code Organization

- **One class per file** - Each file should contain a single class or component
- Avoid duplicating helper functions
- Follow the existing project structure

### Code Style

- Follow existing code conventions in the file you're editing
- Add comments only when they add value
- Keep functions focused on a single responsibility
- Use consistent naming:
  - PascalCase for C# classes and methods
  - camelCase for TypeScript/JavaScript

## Development Setup

### Backend (.NET)

```bash
cd src/backend
dotnet restore Zeitung.sln
dotnet build Zeitung.sln
dotnet test Zeitung.sln
```

### Frontend (Nuxt)

```bash
cd src/frontend
npm ci
npm run dev
npm test
```


## Project Structure

```
zeitung/
├── src/
│   ├── backend/          # .NET backend (ASP.NET Core + Aspire)
│   │   ├── Zeitung.Api/          # Web API
│   │   ├── Zeitung.AppHost/      # Aspire orchestration
│   │   └── Zeitung.ServiceDefaults/
│   └── frontend/         # Nuxt 4 frontend
├── .github/
│   ├── agents/          # GitHub Copilot agent instructions
│   └── workflows/       # CI/CD workflows
└── docker-compose.yml   # Docker orchestration
```

## Testing

- Add tests for new functionality
- Update existing tests when behavior changes
- Follow existing test patterns
- Don't remove or modify working tests unless necessary

## Questions?

If you have questions or need help, please open an issue for discussion.
