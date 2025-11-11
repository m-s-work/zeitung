// Type definitions for the Zeitung Feed Reader

export interface Article {
  id: number
  title: string
  link: string
  description?: string
  publishedDate: string
  feedName: string
  tags?: ArticleTag[]
  userVote?: number
  isLiked?: boolean
}

export interface ArticleTag {
  id: number
  name: string
  confidence: number
}

export interface Feed {
  id: number
  url: string
  name: string
  description?: string
  isApproved: boolean
  isSubscribed: boolean
  createdAt: string
  lastFetchedAt?: string
}

export interface FeedRecommendation extends Feed {
  relevanceScore: number
  relevantTags: string[]
}

export interface Tag {
  id: number
  name: string
  usageCount?: number
}

export interface UserTag {
  tagId: number
  tagName: string
  interactionType: 'Explicit' | 'Ignored' | 'Clicked' | 'Liked'
  score: number
  interactionCount: number
}

export interface PaginatedResponse<T> {
  items: T[]
  totalCount: number
  page: number
  pageSize: number
}

export interface ReadingSession {
  articleId: number
  startTime: number
  endTime?: number
  rating?: number
}
