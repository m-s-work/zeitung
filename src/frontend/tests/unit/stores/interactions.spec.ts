import { describe, it, expect, beforeEach } from 'vitest'
import { setActivePinia, createPinia } from 'pinia'
import { useInteractionsStore } from '../../../stores/interactions'

describe('Interactions Store', () => {
  beforeEach(() => {
    setActivePinia(createPinia())
  })

  it('should initialize with empty state', () => {
    const store = useInteractionsStore()
    expect(store.interactions).toEqual({})
    expect(store.activeArticle).toBeNull()
  })

  it('should record article open', () => {
    const store = useInteractionsStore()
    const articleId = 'test-article-1'
    
    store.recordArticleOpen(articleId)
    
    expect(store.interactions[articleId]).toBeDefined()
    expect(store.interactions[articleId].articleId).toBe(articleId)
    expect(store.interactions[articleId].openedAt).toBeDefined()
    expect(store.activeArticle).toBeDefined()
    expect(store.activeArticle?.id).toBe(articleId)
  })

  it('should record feedback', () => {
    const store = useInteractionsStore()
    const articleId = 'test-article-2'
    
    store.recordFeedback(articleId, true)
    
    expect(store.interactions[articleId]).toBeDefined()
    expect(store.interactions[articleId].likedRecommendation).toBe(true)
  })

  it('should record rating', () => {
    const store = useInteractionsStore()
    const articleId = 'test-article-3'
    const rating = 4
    
    store.recordRating(articleId, rating)
    
    expect(store.interactions[articleId]).toBeDefined()
    expect(store.interactions[articleId].rating).toBe(rating)
  })

  it('should only record read time if greater than 5 seconds', () => {
    const store = useInteractionsStore()
    const articleId = 'test-article-4'
    
    // Simulate opening article
    store.recordArticleOpen(articleId)
    
    // Simulate returning after only 3 seconds
    const startTime = store.activeArticle!.startTime
    store.activeArticle!.startTime = new Date(Date.now() - 3000)
    
    store.recordArticleReturn(articleId)
    
    expect(store.interactions[articleId].readTimeSeconds).toBeUndefined()
  })
})
