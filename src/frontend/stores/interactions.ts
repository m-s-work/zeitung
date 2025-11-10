import { defineStore } from 'pinia'

export interface ArticleInteraction {
  articleId: string
  openedAt?: Date
  likedRecommendation?: boolean
  readTimeSeconds?: number
  rating?: number
}

export const useInteractionsStore = defineStore('interactions', {
  state: () => ({
    interactions: {} as Record<string, ArticleInteraction>,
    activeArticle: null as { id: string; startTime: Date } | null,
  }),
  
  actions: {
    // Track when user opens an article
    recordArticleOpen(articleId: string) {
      if (!this.interactions[articleId]) {
        this.interactions[articleId] = { articleId }
      }
      this.interactions[articleId].openedAt = new Date()
      this.activeArticle = { id: articleId, startTime: new Date() }
      
      // Persist to localStorage
      this.persistInteractions()
      
      // Send to API
      this.sendArticleOpen(articleId)
    },
    
    // Track when user returns from reading
    recordArticleReturn(articleId: string) {
      if (this.activeArticle?.id === articleId) {
        const readTime = Math.floor((new Date().getTime() - this.activeArticle.startTime.getTime()) / 1000)
        
        // Only record if read time is > 5 seconds (to avoid false tracking)
        if (readTime > 5) {
          if (!this.interactions[articleId]) {
            this.interactions[articleId] = { articleId }
          }
          this.interactions[articleId].readTimeSeconds = readTime
          
          this.persistInteractions()
          this.sendReadTime(articleId, readTime)
        }
        
        this.activeArticle = null
      }
    },
    
    // Record recommendation feedback
    recordFeedback(articleId: string, liked: boolean) {
      if (!this.interactions[articleId]) {
        this.interactions[articleId] = { articleId }
      }
      this.interactions[articleId].likedRecommendation = liked
      
      this.persistInteractions()
      this.sendFeedback(articleId, liked)
    },
    
    // Record article rating
    recordRating(articleId: string, rating: number) {
      if (!this.interactions[articleId]) {
        this.interactions[articleId] = { articleId }
      }
      this.interactions[articleId].rating = rating
      
      this.persistInteractions()
      this.sendRating(articleId, rating)
    },
    
    // Persist to localStorage
    persistInteractions() {
      if (process.client) {
        localStorage.setItem('zeitung_interactions', JSON.stringify(this.interactions))
      }
    },
    
    // Load from localStorage
    loadInteractions() {
      if (process.client) {
        const stored = localStorage.getItem('zeitung_interactions')
        if (stored) {
          try {
            this.interactions = JSON.parse(stored)
          } catch (e) {
            console.error('Failed to load interactions', e)
          }
        }
      }
    },
    
    // API calls
    async sendArticleOpen(articleId: string) {
      try {
        await $fetch(`/api/articles/${articleId}/open`, {
          method: 'POST'
        })
      } catch (e) {
        console.error('Failed to record article open', e)
      }
    },
    
    async sendReadTime(articleId: string, readTimeSeconds: number) {
      try {
        await $fetch(`/api/articles/${articleId}/readtime`, {
          method: 'POST',
          body: { articleId, readTimeSeconds }
        })
      } catch (e) {
        console.error('Failed to record read time', e)
      }
    },
    
    async sendFeedback(articleId: string, liked: boolean) {
      try {
        await $fetch(`/api/articles/${articleId}/feedback`, {
          method: 'POST',
          body: { articleId, liked }
        })
      } catch (e) {
        console.error('Failed to record feedback', e)
      }
    },
    
    async sendRating(articleId: string, rating: number) {
      try {
        await $fetch(`/api/articles/${articleId}/rating`, {
          method: 'POST',
          body: { articleId, rating }
        })
      } catch (e) {
        console.error('Failed to record rating', e)
      }
    },
  }
})
