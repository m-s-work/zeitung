<template>
  <UCard 
    :ui="{ 
      body: { padding: 'p-6' },
      header: { padding: 'px-6 py-4' }
    }"
    class="hover:shadow-lg transition-shadow duration-200"
  >
    <!-- Article Header -->
    <template #header>
      <div class="flex items-start justify-between">
        <div class="flex-1">
          <h3 class="text-lg font-semibold text-gray-900 dark:text-white line-clamp-2">
            {{ article.title }}
          </h3>
          <div class="flex items-center gap-2 mt-2 text-sm text-gray-600 dark:text-gray-400">
            <span v-if="article.feedTitle">{{ article.feedTitle }}</span>
            <span v-if="article.author">• {{ article.author }}</span>
            <span v-if="article.publishedAt">• {{ formatDate(article.publishedAt) }}</span>
          </div>
        </div>
        
        <!-- Recommendation Score Badge -->
        <div 
          v-if="recommendationScore"
          class="ml-4 flex items-center gap-1 px-2 py-1 bg-primary-50 dark:bg-primary-900/20 rounded text-primary-600 dark:text-primary-400 text-sm font-medium"
        >
          <UIcon name="i-heroicons-star" class="w-4 h-4" />
          {{ Math.round(recommendationScore * 100) }}%
        </div>
      </div>
    </template>

    <!-- Article Content -->
    <div class="space-y-4">
      <!-- Content Preview -->
      <p v-if="article.content" class="text-gray-700 dark:text-gray-300 line-clamp-3">
        {{ article.content }}
      </p>

      <!-- Tags -->
      <div v-if="article.tags && article.tags.length > 0" class="flex flex-wrap gap-2">
        <UBadge 
          v-for="tag in article.tags" 
          :key="tag"
          color="gray"
          variant="subtle"
          size="sm"
        >
          {{ tag }}
        </UBadge>
      </div>

      <!-- Actions -->
      <div class="flex items-center justify-between pt-4 border-t border-gray-200 dark:border-gray-700">
        <!-- Feedback Buttons (for recommendations) -->
        <div v-if="showFeedback" class="flex items-center gap-2">
          <UTooltip text="Like this recommendation">
            <UButton
              :icon="interaction?.likedRecommendation === true ? 'i-heroicons-hand-thumb-up-solid' : 'i-heroicons-hand-thumb-up'"
              :color="interaction?.likedRecommendation === true ? 'primary' : 'gray'"
              variant="ghost"
              size="sm"
              @click="onFeedback(true)"
            />
          </UTooltip>
          <UTooltip text="Dislike this recommendation">
            <UButton
              :icon="interaction?.likedRecommendation === false ? 'i-heroicons-hand-thumb-down-solid' : 'i-heroicons-hand-thumb-down'"
              :color="interaction?.likedRecommendation === false ? 'red' : 'gray'"
              variant="ghost"
              size="sm"
              @click="onFeedback(false)"
            />
          </UTooltip>
        </div>
        <div v-else />

        <!-- Action Buttons -->
        <div class="flex items-center gap-2">
          <UButton
            label="Details"
            variant="ghost"
            size="sm"
            @click="$emit('view-details', article.id)"
          />
          <UButton
            label="Read Article"
            color="primary"
            size="sm"
            @click="onReadArticle"
          />
        </div>
      </div>
    </div>
  </UCard>
</template>

<script setup lang="ts">
interface Article {
  id: string
  title: string
  url: string
  content?: string
  author?: string
  feedTitle?: string
  publishedAt?: string
  tags?: string[]
}

interface Props {
  article: Article
  recommendationScore?: number
  showFeedback?: boolean
  interaction?: any
}

const props = defineProps<Props>()
const emit = defineEmits<{
  (e: 'view-details', id: string): void
  (e: 'feedback', data: { articleId: string; liked: boolean }): void
}>()

const interactionsStore = useInteractionsStore()

const formatDate = (dateStr: string) => {
  const date = new Date(dateStr)
  const now = new Date()
  const diffMs = now.getTime() - date.getTime()
  const diffMins = Math.floor(diffMs / 60000)
  
  if (diffMins < 60) return `${diffMins}m ago`
  
  const diffHours = Math.floor(diffMins / 60)
  if (diffHours < 24) return `${diffHours}h ago`
  
  const diffDays = Math.floor(diffHours / 24)
  if (diffDays < 7) return `${diffDays}d ago`
  
  return date.toLocaleDateString()
}

const onReadArticle = () => {
  // Record that article was opened
  interactionsStore.recordArticleOpen(props.article.id)
  
  // Open article in new tab
  window.open(props.article.url, '_blank')
}

const onFeedback = (liked: boolean) => {
  interactionsStore.recordFeedback(props.article.id, liked)
  emit('feedback', { articleId: props.article.id, liked })
}
</script>
