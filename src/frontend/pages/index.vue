<template>
  <div class="min-h-screen">
    <div class="max-w-4xl mx-auto px-4 sm:px-6 lg:px-8 py-8 sm:py-12">
      <div class="space-y-10">
        <!-- Hero Section -->
        <div class="text-center space-y-4">
          <h1 class="text-3xl sm:text-4xl font-bold text-gray-900 dark:text-gray-100">
            Your Personalized Feed
          </h1>
          <p class="text-base text-gray-600 dark:text-gray-400 max-w-2xl mx-auto">
            Discover articles tailored to your interests with AI-powered recommendations
          </p>
        </div>

        <!-- Loading State -->
        <div v-if="pending" class="space-y-4">
          <USkeleton v-for="i in 3" :key="i" class="h-48 rounded-xl" />
        </div>

        <!-- Error State -->
        <UAlert
          v-else-if="error"
          color="red"
          variant="soft"
          title="Failed to load articles"
          :description="error.message"
          icon="i-heroicons-exclamation-circle"
        />

        <!-- Articles List -->
        <div v-else-if="articles.length > 0" class="space-y-4">
          <ArticleCard
            v-for="article in articles"
            :key="article.id"
            :article="article"
            :show-confidence="true"
            @like="handleLike"
            @vote="handleVote"
            @click="handleClick"
            @read="handleRead"
          />

          <!-- Pagination -->
          <div v-if="totalPages > 1" class="flex justify-center pt-6">
            <UPagination
              v-model="currentPage"
              :total="totalArticles"
              :page-count="pageSize"
            />
          </div>
        </div>

        <!-- Empty State -->
        <div v-else>
          <UCard class="shadow-md">
            <div class="text-center py-12">
              <div class="w-16 h-16 mx-auto mb-6 bg-gray-100 dark:bg-gray-800 rounded-full flex items-center justify-center">
                <UIcon name="i-heroicons-newspaper" class="w-8 h-8 text-gray-500 dark:text-gray-400" />
              </div>
              <h3 class="text-xl font-bold text-gray-900 dark:text-gray-100 mb-3">
                No articles yet
              </h3>
              <p class="text-base text-gray-600 dark:text-gray-400 mb-6">
                Start your reading journey by adding some RSS feeds
              </p>
              <UButton 
                to="/feeds" 
                color="primary"
                size="lg"
                icon="i-heroicons-plus"
              >
                Add Your First Feed
              </UButton>
            </div>
          </UCard>
        </div>
      </div>

      <!-- Rating Modal -->
      <RatingModal
        v-if="ratingArticle"
        v-model="showRatingModal"
        :article-id="ratingArticle.id"
        :reading-time="ratingArticle.readingTime"
        @rated="handleRated"
      />
    </div>
  </div>
</template>

<script setup lang="ts">
import type { Article } from '~/types'

const { userId } = useUser()
const { startReading, stopReading, getReadingTime } = useReadingTracker()

const currentPage = ref(1)
const pageSize = ref(20)
const articles = ref<Article[]>([])
const totalArticles = ref(0)
const pending = ref(false)
const error = ref<Error | null>(null)

const showRatingModal = ref(false)
const ratingArticle = ref<{ id: number; readingTime: number } | null>(null)

const totalPages = computed(() => Math.ceil(totalArticles.value / pageSize.value))

// Fetch articles
const fetchArticles = async () => {
  pending.value = true
  error.value = null
  try {
    const { data, error: fetchError } = await useZeitungApi('/api/articles', {
      query: {
        userId: userId.value,
        page: currentPage.value,
        pageSize: pageSize.value,
      },
    })

    if (fetchError.value) {
      error.value = new Error(fetchError.value.message || 'Failed to fetch articles')
    } else if (data.value) {
      // Parse the response - adjust based on actual API response structure
      const response = data.value as any
      articles.value = response.articles || []
      totalArticles.value = response.totalCount || 0
    }
  } catch (e) {
    error.value = e as Error
  } finally {
    pending.value = false
  }
}

// Handle like action
const handleLike = async (articleId: number) => {
  try {
    const { error: likeError } = await useZeitungApi('/api/articles/{id}/like', {
      method: 'POST',
      path: { id: articleId },
      query: { userId: userId.value },
    })

    if (!likeError.value) {
      // Update local state
      const article = articles.value.find(a => a.id === articleId)
      if (article) {
        article.isLiked = !article.isLiked
      }
    }
  } catch (e) {
    console.error('Failed to like article:', e)
  }
}

// Handle vote action
const handleVote = async (articleId: number, value: number) => {
  try {
    const { error: voteError } = await useZeitungApi('/api/articles/{id}/vote', {
      method: 'POST',
      path: { id: articleId },
      query: { userId: userId.value },
      body: { value },
    })

    if (!voteError.value) {
      // Update local state
      const article = articles.value.find(a => a.id === articleId)
      if (article) {
        article.userVote = value
      }
    }
  } catch (e) {
    console.error('Failed to vote:', e)
  }
}

// Handle click tracking
const handleClick = async (articleId: number) => {
  try {
    await useZeitungApi('/api/articles/{id}/click', {
      method: 'POST',
      path: { id: articleId },
      query: { userId: userId.value },
    })
  } catch (e) {
    console.error('Failed to track click:', e)
  }
}

// Handle read action with reading time tracking
const handleRead = (articleId: number) => {
  startReading(articleId)
  
  // Listen for when the user returns to rate the article
  const handleVisibilityChange = () => {
    if (!document.hidden) {
      // User returned to the tab
      const readingTime = getReadingTime(articleId)
      if (readingTime > 10) { // Only show rating if user spent more than 10 seconds
        stopReading(articleId)
        ratingArticle.value = { id: articleId, readingTime }
        showRatingModal.value = true
      }
      document.removeEventListener('visibilitychange', handleVisibilityChange)
    }
  }
  
  document.addEventListener('visibilitychange', handleVisibilityChange)
}

// Handle rating submission
const handleRated = async (articleId: number, rating: number) => {
  try {
    await useZeitungApi('/api/articles/{id}/vote', {
      method: 'POST',
      path: { id: articleId },
      query: { userId: userId.value },
      body: { value: rating > 3 ? 1 : -1 }, // Convert 5-star to upvote/downvote
    })
    
    // Update local state
    const article = articles.value.find(a => a.id === articleId)
    if (article) {
      article.userVote = rating > 3 ? 1 : -1
    }
  } catch (e) {
    console.error('Failed to submit rating:', e)
  } finally {
    ratingArticle.value = null
  }
}

// Watch page changes
watch(currentPage, () => {
  fetchArticles()
})

// Initial fetch
onMounted(() => {
  fetchArticles()
})

// Set page metadata
useSeoMeta({
  title: 'Zeitung - Smart RSS Feed Reader',
  description: 'AI-powered RSS reader with personalized recommendations',
})
</script>
