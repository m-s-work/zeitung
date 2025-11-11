<template>
  <div class="min-h-screen">
    <UContainer class="py-6 md:py-10">
      <div class="space-y-6 md:space-y-8">
        <!-- Hero Section -->
        <div class="text-center md:text-left space-y-3">
          <h1 class="text-3xl md:text-4xl lg:text-5xl font-bold bg-gradient-to-r from-gray-900 to-gray-700 dark:from-white dark:to-gray-300 bg-clip-text text-transparent">
            Your Personalized Feed
          </h1>
          <p class="text-base md:text-lg text-gray-600 dark:text-gray-400 max-w-2xl">
            Discover articles tailored to your interests with AI-powered recommendations
          </p>
        </div>

        <!-- Loading State -->
        <div v-if="pending" class="grid grid-cols-1 gap-4 md:gap-6">
          <USkeleton v-for="i in 3" :key="i" class="h-56 md:h-64 rounded-xl" />
        </div>

        <!-- Error State -->
        <UAlert
          v-else-if="error"
          color="red"
          variant="subtle"
          title="Failed to load articles"
          :description="error.message"
          icon="i-heroicons-exclamation-triangle"
        />

        <!-- Articles Grid -->
        <div v-else-if="articles.length > 0" class="space-y-6">
          <div class="grid grid-cols-1 gap-4 md:gap-6">
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
          </div>

          <!-- Pagination -->
          <div v-if="totalPages > 1" class="flex justify-center pt-4">
            <UPagination
              v-model="currentPage"
              :total="totalArticles"
              :page-count="pageSize"
              :ui="{
                rounded: 'rounded-full',
                default: {
                  activeButton: {
                    variant: 'solid'
                  }
                }
              }"
            />
          </div>
        </div>

        <!-- Empty State -->
        <UCard v-else class="border-2 border-dashed border-gray-300 dark:border-gray-700">
          <div class="text-center py-16 md:py-20">
            <div class="mb-6 inline-flex items-center justify-center w-20 h-20 rounded-full bg-primary-50 dark:bg-primary-900/20">
              <UIcon name="i-heroicons-newspaper" class="w-10 h-10 text-primary-500" />
            </div>
            <h3 class="text-xl md:text-2xl font-semibold text-gray-900 dark:text-white mb-3">
              No articles yet
            </h3>
            <p class="text-gray-600 dark:text-gray-400 mb-6 max-w-md mx-auto">
              Get started by adding some RSS feeds to begin reading personalized articles
            </p>
            <UButton to="/feeds" color="primary" size="lg" icon="i-heroicons-plus">
              Add Your First Feed
            </UButton>
          </div>
        </UCard>
      </div>

      <!-- Rating Modal -->
      <RatingModal
        v-if="ratingArticle"
        v-model="showRatingModal"
        :article-id="ratingArticle.id"
        :reading-time="ratingArticle.readingTime"
        @rated="handleRated"
      />
    </UContainer>
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
