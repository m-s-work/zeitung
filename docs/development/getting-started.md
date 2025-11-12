# Getting Started

This guide will help you set up Zeitung on your local machine for development or testing.

## Prerequisites

Before you begin, ensure you have the following installed:

- [Podman Desktop](https://podman-desktop.io/) (alternative to Docker)
- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- [Node.js 22](https://nodejs.org/) (LTS recommended)



### Frontend

```bash
cd src/frontend

# Start development server
npm run dev
```

The frontend will be available at http://localhost:3000


## Configuration


### OpenRouter API

For AI tagging, you'll need an OpenRouter API key:

1. Sign up at [OpenRouter](https://openrouter.ai/)
2. Add your API key to environment variables or configuration


## Next Steps

- Read the [Architecture Guide](/guide/architecture) to understand how Zeitung works
- Explore the [API Reference](/api/overview) for available endpoints
- Check the [Contributing Guide](/development/contributing) if you want to contribute


