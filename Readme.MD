# Smart RSS Feed Reader

Zeitung (German for "newspaper") is a smart RSS feed reader that uses AI to help you discover and read content that matters to you.

## 📚 Documentation

**[View Full Documentation](https://m-s-work.github.io/zeitung/)** (GitHub Pages)

- [Getting Started](https://m-s-work.github.io/zeitung/guide/getting-started.html)
- [API Reference](https://m-s-work.github.io/zeitung/api/overview.html)
- [Architecture Guide](https://m-s-work.github.io/zeitung/guide/architecture.html)
- [Contributing](https://m-s-work.github.io/zeitung/development/contributing.html)



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

The system consists of a c# backend and a nuxt frontend. For ingesting feeds we use a separate worker service.
As database we use postgresql, cache is redis, recommendation is based on elasticsearch (or/and maybe and vector DB).