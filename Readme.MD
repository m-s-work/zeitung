# Smart RSS Feed Reader

Zeitung (German for "newspaper") is a smart RSS feed reader that uses AI to tag and recommend articles based on user interests.

## 📚 Documentation

**[View Full Documentation](https://m-s-work.github.io/zeitung/)** (GitHub Pages)

## Platforms

- **Web**: Nuxt 4 frontend with .NET backend
- **Desktop**: Native Windows and Linux apps via Tauri 2
- **Mobile**: Android app via Tauri 2

See [docs/tauri-setup.md](docs/tauri-setup.md) for desktop and mobile app development.


# How it works

## Feed List
Each user can add feeds to their personal feed list.

Admins, Mods and Super Mods can approve feeds and therefore promote it to the global feed list.
The global feed list is fetched by the server.

## Feed Ingest
The server fetches new feeds. Each fetched feed is tagged with an LLM via openrouter.

## Tags
Tags are stored in the DB. Tags can related to articles. Tags are also counted on how often they appear together so we can calculate similarity between tags.

There are multiple categories the user can "like" tags. These categories are tracked separately.:
- Users explicitly marks tags they are interested in.
- Users explicitly set ignore tags.
- If the user clicks an article, the tags related to that article are marked as interesting for that user.
- If the user likes an article, the tags related to that article are marked as interesting for that user.

Tags should decay over time so that the system can adapt to changing user interests.

There should be a "summary" of the tags. Here the decay is factored in. The TagSummary is used for article recommendation.



# Technical

## Architecture

The system consists of a C# backend and a Nuxt frontend. For ingesting feeds we use a separate worker service.
As database we use PostgreSQL, cache is Redis, recommendation is based on Elasticsearch (or/and maybe a vector DB).

### Desktop and Mobile Apps

Desktop and mobile applications are built using [Tauri v2](https://v2.tauri.app), which bundles the Nuxt frontend with a native Rust backend. This provides:
- Native performance and small binary sizes
- Cross-platform support (Windows, Linux, Android)
- Local-first capabilities with optional backend sync
- Access to native OS features

For more details, see:
- [Tauri Setup Guide](docs/tauri-setup.md)
- [C# Integration Analysis](docs/csharp-integration.md)