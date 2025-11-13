# Pull Request Guidelines

Pull requests (titles) to the Zeitung repository should follow these conventions to maintain a clean and professional codebase.

## Conventional Commits

All PR **titles** must follow the [Conventional Commits](https://www.conventionalcommits.org/) specification because **we squash all PRs**. This means your PR title becomes the final commit message.

### Format

```
<type>(<scope>): <description>
```

### Types

- **feat**: A new feature
- **fix**: A bug fix
- **docs**: Documentation only changes
- **style**: Changes that don't affect the meaning of the code (formatting, missing semicolons, etc.)
- **refactor**: A code change that neither fixes a bug nor adds a feature
- **perf**: A code change that improves performance
- **test**: Adding missing tests or correcting existing tests
- **build**: Changes that affect the build system or external dependencies
- **ci**: Changes to CI configuration files and scripts
- **chore**: Other changes that don't modify src or test files
- **revert**: Reverts a previous commit

### Breaking Changes

- Indicate breaking changes by adding `!` after the type/scope, e.g., `feat(api)!: ...`
- Include a `BREAKING CHANGE:` section in the PR description explaining the change

### Examples

```
feat(api): add user authentication endpoint
fix(frontend): resolve login form validation issue
docs(readme): update installation instructions
refactor(backend): simplify feed processing logic
test(api): add unit tests for user service
```

### Scope (Optional)

The scope should be the area of the codebase affected:
- `frontend` - Nuxt frontend changes
- `backend` - .NET backend changes
- `api` - API-specific changes
- `worker` - Worker service changes
- `ci` - CI/CD pipeline changes
- `docs` - Documentation changes

## PR Description

- Provide a clear description of what the PR does
- Include any relevant context or background information
- List any breaking changes
- Reference related issues (e.g., "Closes #123")

## Code Review

- Address all review comments
- Keep changes minimal and focused
- Ensure tests pass before requesting review
- Update documentation if necessary
