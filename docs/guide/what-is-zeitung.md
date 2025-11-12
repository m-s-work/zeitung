# What is Zeitung?

Zeitung (German for "newspaper") is a smart RSS feed reader that helps you stay informed about topics you care about. Unlike traditional RSS readers, Zeitung uses AI to understand your interests and recommend relevant content.

## The Problem

Modern RSS readers face several challenges:
- Information overload from too many feeds
- Difficulty discovering new relevant content
- Static interests that don't evolve with you
- No personalization or recommendations

## The Solution

Zeitung solves these problems by:

### Intelligent Tagging
Every article is automatically tagged using Large Language Models (LLMs) via OpenRouter. This creates a rich semantic understanding of your content.

### Personalized Recommendations
The system learns your preferences through:
- **Explicit interests**: Tags you mark as interesting
- **Ignored tags**: Content you want to avoid
- **Click tracking**: Articles you click show implicit interest
- **Likes**: Strong signals of what you enjoy

### Dynamic Interest Evolution
Your interests naturally change over time. Zeitung's tag decay system ensures that:
- Old interests fade as new ones emerge
- Tags with more interactions decay more slowly
- The system adapts to your evolving preferences

### Community Curation
- Users can add their own personal feeds
- Moderators and admins can promote quality feeds to the global list
- Everyone benefits from community-vetted content

## How It Works

### Feed Management
1. Add RSS feeds to your personal collection
2. Subscribe to globally approved feeds
3. Moderators promote quality feeds for everyone

### Content Discovery
1. The worker service fetches new articles
2. Each article is tagged using AI
3. Articles are recommended based on your tag preferences
4. Tag relationships help find similar content

### Personalization
1. Click articles you're interested in
2. Like content you love
3. Mark tags as interesting or ignored
4. The system learns and adapts

## Technical Overview

Zeitung is built with modern technologies:

- **Backend**: .NET 9 with ASP.NET Core and Aspire
- **Frontend**: Nuxt 4 with Vue 3
- **Database**: PostgreSQL for reliable data storage
- **Cache**: Redis for performance
- **Search**: Elasticsearch for advanced queries (planned)
- **AI**: OpenRouter for article tagging

The architecture is designed for scalability, with separate services for the API, worker, and frontend.
