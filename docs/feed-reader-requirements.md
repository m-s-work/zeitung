# Feed Reader UI - Requirements Specification

## Overview
This document outlines the requirements and implementation details for the Zeitung Feed Reader UI, a Nuxt 4-based frontend application for consuming and managing RSS feeds with AI-powered recommendations.

## Table of Contents
1. [Functional Requirements](#functional-requirements)
2. [Technical Architecture](#technical-architecture)
3. [UI/UX Design Decisions](#uiux-design-decisions)
4. [Data Models](#data-models)
5. [API Endpoints](#api-endpoints)
6. [State Management](#state-management)
7. [Tracking Strategy](#tracking-strategy)
8. [Future Enhancements](#future-enhancements)

## Functional Requirements

### 1. Feed Viewing
- **FR-1.1**: Users can view a personalized feed of recommended articles
- **FR-1.2**: Articles display title, author, publication date, feed source, and content preview
- **FR-1.3**: Articles show recommendation score as a percentage
- **FR-1.4**: Articles display associated tags as badges
- **FR-1.5**: Feed supports infinite scroll/pagination for loading more articles

### 2. Article Interaction
- **FR-2.1**: Users can like or dislike recommendations using thumbs up/down buttons
- **FR-2.2**: Users can click "Read Article" to open the article in a new tab
- **FR-2.3**: System tracks when users open articles
- **FR-2.4**: System tracks time spent reading articles (with visibility API)
- **FR-2.5**: Users can rate articles on a 5-star scale after reading
- **FR-2.6**: Rating modal automatically appears after user returns from reading (>30 seconds read time)

### 3. Article Details
- **FR-3.1**: Users can view detailed article information in a modal
- **FR-3.2**: Article details show full content, tags, and metadata
- **FR-3.3**: Users can vote on individual tags (interested/ignored)
- **FR-3.4**: Article details display user's interaction history (opened, read time, rating)

### 4. Feed Management
- **FR-4.1**: Users can view all their subscribed feeds (personal + global)
- **FR-4.2**: Users can add new RSS feeds by providing a URL
- **FR-4.3**: Users can delete their personal feeds
- **FR-4.4**: Users with permissions can promote personal feeds to global
- **FR-4.5**: Feed cards display title, URL, description, and last fetch time
- **FR-4.6**: Feeds are categorized into "Global Feeds" and "My Personal Feeds"

### 5. Tag Management
- **FR-5.1**: Users can vote on tags to indicate interest or disinterest
- **FR-5.2**: Tag votes update user preferences for better recommendations
- **FR-5.3**: Tag voting is available both in article cards and detail views
- **FR-5.4**: Visual feedback shows current vote state (green for interested, red for ignored)

## Technical Architecture

### Technology Stack
- **Framework**: Nuxt 4
- **UI Library**: Nuxt UI (with Tailwind CSS)
- **State Management**: Pinia
- **Type Safety**: TypeScript
- **API Integration**: nuxt-open-fetch (with OpenAPI schema)
- **Icons**: Heroicons (via @nuxt/icon)
- **Testing**: Vitest (unit), Playwright (E2E)

### Project Structure
```
src/frontend/
├── components/          # Reusable Vue components
│   ├── ArticleCard.vue
│   ├── ArticleDetailsModal.vue
│   ├── RatingModal.vue
│   ├── TagVoting.vue
│   ├── FeedCard.vue
│   └── AddFeedModal.vue
├── layouts/            # Application layouts
│   └── default.vue
├── pages/              # Route pages
│   ├── index.vue       # Recommended feed
│   └── feeds.vue       # Feed management
├── stores/             # Pinia stores
│   ├── interactions.ts # Article interaction tracking
│   └── sync.ts         # Sync state management
├── composables/        # Composable functions
├── types/              # TypeScript type definitions
└── utils/              # Utility functions
```

## UI/UX Design Decisions

### Design Library Choice: Nuxt UI
**Rationale**:
- Native Nuxt 4 integration
- Built on Tailwind CSS (highly customizable)
- Comprehensive component library
- Dark mode support out of the box
- Accessible components (WCAG compliant)
- TypeScript support
- Active maintenance

### Mobile Interaction Strategy
**Like/Dislike on Mobile**:
- Desktop: Hover shows thumbs up/down buttons
- Mobile: Buttons are always visible but styled as ghost buttons
- Touch targets are appropriately sized (min 44x44px)
- Visual feedback on tap

### Color Mode
- Default: Light mode
- Users can toggle between light/dark modes
- Preference persisted in browser

## Data Models

### Feed
```typescript
{
  id: string
  title: string
  url: string
  description?: string
  isGlobal: boolean
  userId?: string
  createdAt: Date
  lastFetchedAt?: Date
}
```

### Article
```typescript
{
  id: string
  title: string
  url: string
  content?: string
  author?: string
  feedId: string
  feedTitle?: string
  publishedAt: Date
  createdAt: Date
  tags: string[]
  recommendationScore?: number
}
```

### RecommendedArticle
```typescript
{
  article: Article
  score: number
  reason?: string
  interaction?: UserArticleInteraction
}
```

### UserArticleInteraction
```typescript
{
  articleId: string
  userId: string
  likedRecommendation?: boolean
  opened: boolean
  openedAt?: Date
  readTimeSeconds?: number
  rating?: number
  createdAt: Date
  updatedAt: Date
}
```

### Tag
```typescript
{
  id: string
  name: string
  category?: string
  usageCount: number
  createdAt: Date
}
```

### UserTagPreference
```typescript
{
  tagId: string
  tagName: string
  userId: string
  preferenceType: 'Explicit' | 'Ignored' | 'InferredFromClick' | 'InferredFromLike'
  strength: number  // 0-1, with decay applied
  createdAt: Date
  updatedAt: Date
}
```

## API Endpoints

### Feed Endpoints
- `GET /api/feeds` - Get all feeds (personal + global)
- `GET /api/feeds/{id}` - Get specific feed
- `POST /api/feeds` - Create new feed
- `POST /api/feeds/{id}/promote` - Promote feed to global
- `DELETE /api/feeds/{id}` - Delete feed

### Article Endpoints
- `GET /api/articles/recommended` - Get recommended articles
- `GET /api/articles/{id}` - Get article details
- `POST /api/articles/{id}/feedback` - Record like/dislike
- `POST /api/articles/{id}/open` - Record article open
- `POST /api/articles/{id}/readtime` - Record read time
- `POST /api/articles/{id}/rating` - Rate article
- `GET /api/articles/{id}/interaction` - Get user interaction

### Tag Endpoints
- `GET /api/tags` - Get all tags
- `GET /api/tags/preferences` - Get user preferences
- `POST /api/tags/vote` - Vote on tag
- `POST /api/tags/articles/{id}/votes` - Vote on article tags

## State Management

### Interaction Store (`stores/interactions.ts`)
**Purpose**: Track user interactions with articles for analytics and recommendations

**State**:
- `interactions`: Record of all article interactions
- `activeArticle`: Currently active article being read

**Actions**:
- `recordArticleOpen(articleId)`: Track when article is opened
- `recordArticleReturn(articleId)`: Calculate read time when user returns
- `recordFeedback(articleId, liked)`: Record like/dislike
- `recordRating(articleId, rating)`: Record star rating
- `persistInteractions()`: Save to localStorage
- `loadInteractions()`: Load from localStorage

**Persistence**: All interactions stored in `localStorage` as `zeitung_interactions`

### Sync Store (`stores/sync.ts`)
**Purpose**: Manage sync state for offline capability

**State**:
- `lastSync`: Timestamp of last sync
- `isSyncing`: Boolean indicating sync in progress

**Actions**:
- `updateLastSync()`: Update sync timestamp
- `loadLastSync()`: Load from localStorage
- `sync()`: Perform sync operation (placeholder for future implementation)

**Persistence**: Last sync time stored in `localStorage` as `zeitung_last_sync`

## Tracking Strategy

### Read Time Tracking

#### Implementation
Uses the **Page Visibility API** to accurately track reading time:

1. When user clicks "Read Article":
   - Article opens in new tab
   - `recordArticleOpen()` is called
   - Start time is recorded in `activeArticle`

2. When user returns to Zeitung tab:
   - `visibilitychange` event fires
   - If user was away >5 seconds, read time is calculated
   - `recordArticleReturn()` saves the read time

#### False Tracking Prevention

**Minimum Read Time**: Only tracks if user spent >5 seconds
- Prevents accidental clicks
- Filters out quick tab switches
- Ensures meaningful engagement

**Visibility API Benefits**:
- Only counts time when tab is active
- Stops counting when user switches tabs
- Pauses on browser minimize
- Resumes on tab focus

**Edge Cases Handled**:
- User closes tab without returning: No read time recorded (acceptable loss)
- User switches between tabs: Only counts active time
- User leaves browser open but inactive: Not counted
- Multiple articles open: Tracks last opened article

#### Rating Trigger
- Rating modal appears if read time >30 seconds
- Only shows if article hasn't been rated yet
- User can skip rating

### Feedback Tracking
- Immediate feedback to API when like/dislike clicked
- Visual state persisted in localStorage
- No duplicate submissions

### Tag Voting
- Votes sent to API immediately
- Visual feedback instant
- Can toggle votes (click again to remove)

## Future Enhancements

### Phase 1: Enhanced Offline Support
- **Service Worker**: Cache articles for offline reading
- **Background Sync**: Queue interactions when offline, sync when online
- **Conflict Resolution**: Handle sync conflicts intelligently

### Phase 2: Mobile App
- **PWA**: Convert to installable Progressive Web App
- **Native Apps**: Build with Capacitor for iOS/Android
- **Push Notifications**: Alert users of new recommended articles
- **Biometric Auth**: Support fingerprint/face recognition

### Phase 3: Advanced Features
- **Article Collections**: Save articles to custom collections
- **Reading Lists**: Create and share reading lists
- **Notes & Highlights**: Annotate articles
- **Social Features**: Share recommendations with friends
- **Reading Statistics**: Dashboard showing reading habits

### Phase 4: Sync Infrastructure
- **Real-time Sync**: WebSocket-based live sync
- **Multi-device**: Seamless experience across devices
- **Cloud Storage**: Sync settings, interactions, and collections
- **Conflict Resolution**: Intelligent merge strategies

### Potential Libraries for State Syncing
1. **tRPC**: Type-safe API calls with automatic sync
2. **Socket.io**: WebSocket for real-time sync
3. **IndexedDB**: Local database for offline storage
4. **Workbox**: Service worker management
5. **Amplify DataStore**: AWS solution with offline-first approach

## Implementation Notes

### Single User Implementation
- Current implementation assumes single user with full permissions
- All API calls use default user context
- Authentication/authorization framework ready for future multi-user support

### Security Considerations
- XSS protection via Vue's template escaping
- External URLs opened in new tabs (security)
- Input validation with Zod schemas
- CORS configured on backend

### Performance Optimizations
- Lazy loading of modals
- Virtual scrolling for large article lists (future)
- Image lazy loading
- Component-level code splitting

### Accessibility
- Keyboard navigation support
- ARIA labels on interactive elements
- Color contrast meets WCAG AA
- Focus management in modals
- Screen reader friendly

## Testing Strategy

### Unit Tests
- Component rendering
- Store actions and state
- Utility functions
- Input validation

### Integration Tests
- Component interactions
- API integration
- Store integration with components

### E2E Tests
- Full user workflows
- Feed management flow
- Article interaction flow
- Rating and feedback flow

## Deployment Considerations

### Build Configuration
- Production builds optimize bundle size
- Tree-shaking removes unused code
- CSS purging reduces stylesheet size
- Asset optimization (images, fonts)

### Environment Variables
- API base URL
- Feature flags
- Analytics keys
- CDN configurations

### Monitoring
- Error tracking (Sentry)
- Performance monitoring
- User analytics
- API usage metrics

## Maintenance

### Code Quality
- TypeScript for type safety
- ESLint for code standards
- Prettier for formatting
- Git hooks for pre-commit checks

### Documentation
- Component documentation
- API documentation (OpenAPI)
- Setup instructions
- Contributing guidelines

### Updates
- Regular dependency updates
- Security patch monitoring
- Nuxt version migrations
- Breaking change management
