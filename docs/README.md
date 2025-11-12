# Zeitung Documentation

This directory contains the documentation for Zeitung, built with [VitePress](https://vitepress.dev/).

## 🚀 Quick Start

### Prerequisites

- Node.js 22 or higher
- npm

### Development

```bash
# Install dependencies
npm install

# Start dev server
npm run docs:dev
```

Visit http://localhost:5173 to see the documentation.

### Building

```bash
# Build the documentation
npm run docs:build

# Preview the built documentation
npm run docs:preview
```

## 📁 Structure

```
docs/
├── .vitepress/
│   └── config.mts          # VitePress configuration
├── api/                    # API reference documentation
│   ├── overview.md
│   ├── authentication.md
│   ├── feeds.md
│   ├── articles.md
│   ├── tags.md
│   └── users.md
├── guide/                  # User guides
│   ├── what-is-zeitung.md
│   ├── getting-started.md
│   └── architecture.md
├── development/            # Developer documentation
│   ├── contributing.md
│   └── testing.md
└── index.md               # Homepage

```

## 📝 Writing Documentation

### Adding a New Page

1. Create a new `.md` file in the appropriate directory
2. Add front matter if needed
3. Write your content in Markdown
4. Add the page to `.vitepress/config.mts` sidebar

### Markdown Features

VitePress supports:
- Standard Markdown
- GitHub-flavored Markdown
- Custom containers (tip, warning, danger, etc.)
- Code syntax highlighting
- Line highlighting in code blocks
- Code groups/tabs

Example:

```md
::: tip
This is a tip
:::

::: warning
This is a warning
:::

::: danger
This is a dangerous warning
:::
```

### Code Blocks

```js
// This is a JavaScript code block
const hello = 'world'
```

With line highlighting:

```js{2,4-5}
function example() {
  const highlighted = true  // This line is highlighted
  const normal = false
  const alsoHighlighted = true  // These lines are highlighted
  const stillHighlighted = true
}
```

## 🔧 Configuration

VitePress configuration is in `.vitepress/config.mts`. Key settings:

- **title**: Site title
- **description**: Site description
- **base**: Base URL (for GitHub Pages)
- **themeConfig**: Theme settings
  - **nav**: Top navigation
  - **sidebar**: Sidebar navigation
  - **socialLinks**: Social media links

## 🚢 Deployment

Documentation is automatically deployed to GitHub Pages when changes are pushed to the `main` branch.

The deployment workflow:
1. Triggers on push to `main` with changes in `docs/**`
2. Builds the documentation with VitePress
3. Deploys to GitHub Pages

### Manual Deployment

To trigger a manual deployment, use the GitHub Actions UI and run the "Deploy Documentation" workflow.

## 📚 Resources

- [VitePress Documentation](https://vitepress.dev/)
- [Markdown Guide](https://www.markdownguide.org/)
- [GitHub Pages Documentation](https://docs.github.com/en/pages)

## 🤝 Contributing

When contributing to the documentation:

1. Follow the existing structure and style
2. Use clear, concise language
3. Include code examples where appropriate
4. Test your changes locally before committing
5. Ensure all links work correctly

## 📄 License

Same license as the main Zeitung project.
