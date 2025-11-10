<template>
  <NuxtLayout>
    <div class="space-y-6">
      <!-- Header -->
      <div class="flex items-center justify-between">
        <div>
          <h1 class="text-3xl font-bold text-gray-900 dark:text-white">
            Recommended Feed
          </h1>
          <p class="text-gray-600 dark:text-gray-400 mt-1">
            Articles personalized for your interests
          </p>
        </div>
        <UButton
          icon="i-heroicons-arrow-path"
          label="Refresh"
          :loading="refreshing"
          @click="refresh"
        />
      </div>

      <!-- Loading State -->
      <div v-if="pending && !articles.length" class="space-y-4">
        <USkeleton class="h-64" v-for="i in 3" :key="i" />
      </div>

      <!-- Error State -->
      <UAlert
        v-else-if="error"
        icon="i-heroicons-exclamation-triangle"
        color="red"
        variant="soft"
        title="Failed to load articles"
        :description="error.message"
      />

      <!-- Articles List -->
      <div v-else class="space-y-6">
        <ArticleCard
          v-for="item in articles"
          :key="item.article.id"
          :article="item.article"
          :recommendation-score="item.score"
          :show-feedback="true"
          :interaction="interactions[item.article.id]"
          @view-details="showArticleDetails"
          @feedback="onFeedback"
        />

        <!-- Empty State -->
        <div v-if="articles.length === 0" class="text-center py-12">
          <UIcon name="i-heroicons-inbox" class="w-16 h-16 mx-auto text-gray-400 mb-4" />
          <h3 class="text-lg font-semibold text-gray-900 dark:text-white mb-2">
            No articles yet
          </h3>
          <p class="text-gray-600 dark:text-gray-400 mb-4">
            Add some feeds to get personalized recommendations
          </p>
          <UButton
            to="/feeds"
            label="Manage Feeds"
            color="primary"
          />
        </div>

        <!-- Load More -->
        <div v-if="articles.length > 0" class="text-center">
          <UButton
            label="Load More"
            variant="outline"
            :loading="loadingMore"
            @click="loadMore"
          />
        </div>
      </div>

      <!-- Article Details Modal -->
      <ArticleDetailsModal
        v-model="showDetailsModal"
        :article-id="selectedArticleId"
        @close="showDetailsModal = false"
      />

      <!-- Rating Modal -->
      <RatingModal
        v-model="showRatingModal"
        :article-id="ratingArticleId"
        :article-title="ratingArticleTitle"
        :read-time-seconds="ratingReadTime"
        @submit="onRatingSubmit"
      />
    </div>
  </NuxtLayout>
</template>

<script setup lang="ts">
const interactionsStore = useInteractionsStore()
const syncStore = useSyncStore()

// Fetch recommended articles
const { data: articlesData, pending, error, refresh: fetchArticles } = await useFetch('/api/articles/recommended', {
  query: {
    limit: 20,
    offset: 0
  }
})

const articles = computed(() => (articlesData.value as any) || [])
const interactions = computed(() => interactionsStore.interactions)

const refreshing = ref(false)
const loadingMore = ref(false)

// Modal states
const showDetailsModal = ref(false)
const selectedArticleId = ref('')
const showRatingModal = ref(false)
const ratingArticleId = ref('')
const ratingArticleTitle = ref('')
const ratingReadTime = ref(0)

// Load interactions from localStorage
onMounted(() => {
  interactionsStore.loadInteractions()
  syncStore.loadLastSync()
  
  // Listen for when user returns to tab (they might have finished reading)
  document.addEventListener('visibilitychange', handleVisibilityChange)
})

onUnmounted(() => {
  document.removeEventListener('visibilitychange', handleVisibilityChange)
})

const handleVisibilityChange = () => {
  if (!document.hidden && interactionsStore.activeArticle) {
    const articleId = interactionsStore.activeArticle.id
    interactionsStore.recordArticleReturn(articleId)
    
    // Show rating modal if article was read for more than 30 seconds
    const interaction = interactionsStore.interactions[articleId]
    if (interaction?.readTimeSeconds && interaction.readTimeSeconds > 30 && !interaction.rating) {
      const article = articles.value.find((a: any) => a.article.id === articleId)
      if (article) {
        ratingArticleId.value = articleId
        ratingArticleTitle.value = article.article.title
        ratingReadTime.value = interaction.readTimeSeconds
        showRatingModal.value = true
      }
    }
  }
}

const refresh = async () => {
  refreshing.value = true
  try {
    await fetchArticles()
    await syncStore.sync()
  } finally {
    refreshing.value = false
  }
}

const loadMore = async () => {
  loadingMore.value = true
  try {
    // TODO: Implement pagination
    await new Promise(resolve => setTimeout(resolve, 1000))
  } finally {
    loadingMore.value = false
  }
}

const showArticleDetails = (articleId: string) => {
  selectedArticleId.value = articleId
  showDetailsModal.value = true
}

const onFeedback = ({ articleId, liked }: { articleId: string; liked: boolean }) => {
  // Feedback is already recorded in ArticleCard component
  console.log('Feedback recorded:', articleId, liked)
}

const onRatingSubmit = ({ articleId, rating }: { articleId: string; rating: number }) => {
  interactionsStore.recordRating(articleId, rating)
}
</script>
