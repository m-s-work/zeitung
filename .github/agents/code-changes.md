# Code Change Guidelines

When making changes to the Zeitung codebase, follow these principles to maintain code quality and consistency.

## Minimal Changes

- Make the smallest possible changes to achieve the goal
- Only modify what's necessary to implement the feature or fix the bug
- Avoid refactoring unrelated code unless it's part of the task
- Keep the scope of changes focused and surgical

## Preserve Existing Code

- **DO NOT remove existing comments** - Comments provide valuable context and documentation
- Keep existing code structure unless changes are required
- Maintain backward compatibility when possible
- Only remove code that is clearly obsolete or replaced

## Helper Functions and Utilities

- Place helper functions in **related files**, not in separate utility files unless they're truly generic
- Avoid duplicating helper functions across files
- If a helper is used in multiple places, consider extracting it to a shared location
- Keep helpers close to where they're used for better maintainability

## File Organization

- **One class per file** - Each file should contain a single class or component
- Name the file after the class it contains (e.g., `UserService.cs` contains `UserService` class)
- Group related files in appropriate directories
- Follow the existing project structure

## Code Style

- Follow the existing code style in the file you're editing
- Use consistent naming conventions (PascalCase for C#, camelCase for TypeScript/JavaScript)
- Add comments only when they add value and match the style of existing comments
- Keep functions and methods focused on a single responsibility

## Testing

- Add tests for new functionality
- Update existing tests when behavior changes
- Don't remove or modify working tests unless necessary
- Follow existing test patterns and conventions
