---
layout: home

hero:
  name: "Zeitung"
  text: "Smart RSS Feed Reader"
  tagline: AI-powered RSS reader with personalized recommendations
  actions:
    - theme: brand
      text: Get Started
      link: /guide/getting-started
    - theme: alt
      text: View on GitHub
      link: https://github.com/m-s-work/zeitung

features:
  - title: Personalized Feeds
    details: AI-powered tagging and recommendations based on your reading habits and preferences
  - title: Global Feed List
    details: Community-curated feeds approved by moderators for quality content
  - title: Smart Tag System
    details: Automatic article tagging with LLM, tag decay for evolving interests
  - title: Modern Architecture
    details: Built with .NET 9, Nuxt 4, PostgreSQL, and Redis for performance and scalability
---

## Overview

Zeitung is a modern RSS feed reader that uses AI to help you discover and read content that matters to you. It combines the simplicity of RSS with intelligent recommendations based on your reading patterns.

## Key Features

- **Personal & Global Feeds**: Manage your own feeds and discover community-approved content
- **AI-Powered Tagging**: Automatic article tagging using LLMs via OpenRouter
- **Smart Recommendations**: Learn your preferences through clicks, likes, and explicit interests
- **Tag Decay System**: Your interests naturally evolve as you interact with content
- **Modern Tech Stack**: .NET backend, Nuxt frontend, PostgreSQL database

## Quick Start

```bash
# Clone the repository
git clone https://github.com/m-s-work/zeitung.git
cd zeitung

# Start with Docker Compose
docker-compose up
```

Visit the [Getting Started](/guide/getting-started) guide for detailed setup instructions.
