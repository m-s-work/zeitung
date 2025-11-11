<template>
  <div class="min-h-screen">
    <UContainer class="py-12">
      <div class="space-y-12">
        <!-- Hero Section -->
        <div class="text-center max-w-4xl mx-auto space-y-6">
          <div class="inline-block">
            <div class="flex items-center justify-center w-20 h-20 bg-gradient-to-br from-blue-500 via-purple-500 to-pink-500 rounded-2xl shadow-2xl mb-6 mx-auto animate-pulse">
              <UIcon name="i-heroicons-newspaper" class="w-10 h-10 text-white" />
            </div>
          </div>
          <h1 class="text-5xl sm:text-6xl lg:text-7xl font-black">
            <span class="bg-gradient-to-r from-blue-600 via-purple-600 to-pink-600 bg-clip-text text-transparent">
              Your Personalized
            </span>
            <br />
            <span class="bg-gradient-to-r from-pink-600 via-purple-600 to-blue-600 bg-clip-text text-transparent">
              News Feed
            </span>
          </h1>
          <p class="text-xl sm:text-2xl text-gray-700 dark:text-gray-300 font-medium max-w-2xl mx-auto">
            Discover articles tailored to your interests with 
            <span class="text-transparent bg-clip-text bg-gradient-to-r from-blue-600 to-purple-600 font-bold">AI-powered recommendations</span>
          </p>
        </div>

        <!-- Loading State -->
        <div v-if="pending" class="space-y-6 max-w-5xl mx-auto">
          <div v-for="i in 3" :key="i" class="bg-white dark:bg-gray-800 rounded-2xl p-6 shadow-xl animate-pulse">
            <div class="space-y-4">
              <div class="h-6 bg-gray-200 dark:bg-gray-700 rounded w-3/4"></div>
              <div class="h-4 bg-gray-200 dark:bg-gray-700 rounded w-1/2"></div>
              <div class="h-20 bg-gray-200 dark:bg-gray-700 rounded"></div>
            </div>
          </div>
        </div>

        <!-- Error State -->
        <UAlert
          v-else-if="error"
          color="red"
          variant="soft"
          title="Failed to load articles"
          :description="error.message"
          icon="i-heroicons-exclamation-circle"
          class="max-w-5xl mx-auto shadow-xl"
        />

        <!-- Articles List -->
        <div v-else-if="articles.length > 0" class="space-y-6 max-w-5xl mx-auto">
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
          <div v-if="totalPages > 1" class="flex justify-center pt-8">
            <UPagination
              v-model="currentPage"
              :total="totalArticles"
              :page-count="pageSize"
              :ui="{ rounded: 'rounded-full' }"
            />
          </div>
        </div>

        <!-- Empty State -->
        <div v-else class="max-w-3xl mx-auto">
          <div class="bg-gradient-to-br from-white to-blue-50 dark:from-gray-900 dark:to-blue-950 rounded-3xl shadow-2xl p-12 text-center border border-blue-200/50 dark:border-blue-800/50">
            <div class="w-32 h-32 mx-auto mb-8 bg-gradient-to-br from-blue-500 via-purple-500 to-pink-500 rounded-full flex items-center justify-center shadow-2xl animate-bounce">
              <UIcon name="i-heroicons-newspaper" class="w-16 h-16 text-white" />
            </div>
            <h3 class="text-3xl font-black text-gray-900 dark:text-white mb-4">
              No articles yet
            </h3>
            <p class="text-lg text-gray-600 dark:text-gray-400 mb-8 max-w-md mx-auto leading-relaxed">
              Start your reading journey by adding some RSS feeds to discover interesting content tailored just for you
            </p>
            <UButton 
              to="/feeds" 
              color="blue" 
              size="xl"
              icon="i-heroicons-plus"
              class="font-bold shadow-xl hover:shadow-2xl transform hover:scale-105 transition-all"
            >
              Add Your First Feed
            </UButton>
            
            <!-- Feature highlights -->
            <div class="grid grid-cols-1 md:grid-cols-3 gap-6 mt-12">
              <div class="text-center p-4">
                <div class="w-12 h-12 bg-blue-500 rounded-xl flex items-center justify-center mx-auto mb-3 shadow-lg">
                  <UIcon name="i-heroicons-sparkles" class="w-6 h-6 text-white" />
                </div>
                <h4 class="font-bold text-gray-900 dark:text-white mb-1">AI-Powered</h4>
                <p class="text-sm text-gray-600 dark:text-gray-400">Smart recommendations</p>
              </div>
              <div class="text-center p-4">
                <div class="w-12 h-12 bg-purple-500 rounded-xl flex items-center justify-center mx-auto mb-3 shadow-lg">
                  <UIcon name="i-heroicons-bolt" class="w-6 h-6 text-white" />
                </div>
                <h4 class="font-bold text-gray-900 dark:text-white mb-1">Lightning Fast</h4>
                <p class="text-sm text-gray-600 dark:text-gray-400">Instant updates</p>
              </div>
              <div class="text-center p-4">
                <div class="w-12 h-12 bg-pink-500 rounded-xl flex items-center justify-center mx-auto mb-3 shadow-lg">
                  <UIcon name="i-heroicons-heart" class="w-6 h-6 text-white" />
                </div>
                <h4 class="font-bold text-gray-900 dark:text-white mb-1">Personalized</h4>
                <p class="text-sm text-gray-600 dark:text-gray-400">Just for you</p>
              </div>
            </div>
          </div>
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
