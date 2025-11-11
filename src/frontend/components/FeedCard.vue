<template>
  <UCard 
    class="hover:shadow-xl dark:hover:shadow-2xl transition-all duration-300 overflow-hidden border-0 shadow-md"
    :ui="{ 
      ring: 'ring-1 ring-gray-200 dark:ring-gray-800',
      background: 'bg-white dark:bg-gray-900',
      body: { padding: 'p-6' }
    }"
  >
    <template #header>
      <div class="flex items-start justify-between gap-3">
        <div class="flex-1 min-w-0">
          <h3 class="text-lg md:text-xl font-bold text-gray-900 dark:text-white mb-2">
            {{ feed.name }}
          </h3>
          <div class="flex flex-wrap gap-2">
            <UBadge v-if="feed.isApproved" color="green" variant="soft">
              <UIcon name="i-heroicons-check-circle" class="w-3 h-3 mr-1" />
              Approved
            </UBadge>
            <UBadge v-if="feed.isSubscribed" color="primary" variant="soft">
              <UIcon name="i-heroicons-rss" class="w-3 h-3 mr-1" />
              Subscribed
            </UBadge>
            <UBadge v-if="isRecommendation" color="yellow" variant="soft">
              <UIcon name="i-heroicons-sparkles" class="w-3 h-3 mr-1" />
              Recommended
            </UBadge>
          </div>
        </div>
      </div>
    </template>

    <div class="space-y-4">
      <p v-if="feed.description" class="text-sm md:text-base text-gray-700 dark:text-gray-300 line-clamp-2">
        {{ feed.description }}
      </p>

      <div class="space-y-2 text-xs md:text-sm text-gray-600 dark:text-gray-400">
        <div class="flex items-center gap-2">
          <UIcon name="i-heroicons-link" class="w-4 h-4 shrink-0" />
          <a :href="feed.url" target="_blank" class="hover:text-primary-500 transition-colors truncate">
            {{ feed.url }}
          </a>
        </div>
        <div v-if="feed.lastFetchedAt" class="flex items-center gap-2">
          <UIcon name="i-heroicons-clock" class="w-4 h-4 shrink-0" />
          <span>Last updated: {{ formatDate(feed.lastFetchedAt) }}</span>
        </div>
      </div>

      <!-- Relevant Tags for Recommendations -->
      <div v-if="isRecommendation && feed.relevantTags?.length" class="flex flex-wrap gap-2 pt-2 border-t border-gray-100 dark:border-gray-800">
        <span class="text-xs font-medium text-gray-700 dark:text-gray-300 w-full mb-1">Relevant topics:</span>
        <UBadge
          v-for="tag in feed.relevantTags"
          :key="tag"
          color="primary"
          variant="subtle"
          size="xs"
        >
          <UIcon name="i-heroicons-hashtag" class="w-3 h-3 mr-0.5" />
          {{ tag }}
        </UBadge>
      </div>
    </div>

    <template #footer>
      <div class="flex flex-wrap items-center justify-between gap-3">
        <div class="flex items-center gap-2">
          <div v-if="isRecommendation" class="text-sm font-medium text-gray-700 dark:text-gray-300">
            <span class="text-primary-600 dark:text-primary-400">{{ Math.round((feed.relevanceScore || 0) * 100) }}%</span>
            <span class="text-gray-500 dark:text-gray-400 ml-1">match</span>
          </div>
          <div v-else class="text-xs text-gray-500 dark:text-gray-400">
            Added {{ formatDate(feed.createdAt) }}
          </div>
        </div>

        <div class="flex gap-2">
          <UButton
            v-if="!feed.isSubscribed"
            icon="i-heroicons-plus"
            size="sm"
            color="primary"
            @click="$emit('subscribe', feed.id)"
            :loading="loading"
          >
            Subscribe
          </UButton>
          <UButton
            v-else
            icon="i-heroicons-minus"
            size="sm"
            color="gray"
            variant="ghost"
            @click="$emit('unsubscribe', feed.id)"
            :loading="loading"
          >
            Unsubscribe
          </UButton>

          <UButton
            v-if="canPromote && !feed.isApproved"
            icon="i-heroicons-arrow-up-circle"
            size="sm"
            color="green"
            variant="soft"
            @click="$emit('promote', feed.id)"
            :loading="loading"
          >
            Promote
          </UButton>

          <UButton
            v-if="feed.isSubscribed && !feed.isApproved"
            icon="i-heroicons-trash"
            size="sm"
            color="red"
            variant="ghost"
            @click="$emit('delete', feed.id)"
            :loading="loading"
          />
        </div>
      </div>
    </template>
  </UCard>
</template>

<script setup lang="ts">
import type { Feed, FeedRecommendation } from '~/types'

defineProps<{
  feed: Feed | FeedRecommendation
  isRecommendation?: boolean
  canPromote?: boolean
  loading?: boolean
}>()

defineEmits<{
  subscribe: [feedId: number]
  unsubscribe: [feedId: number]
  promote: [feedId: number]
  delete: [feedId: number]
}>()

const formatDate = (dateString: string) => {
  const date = new Date(dateString)
  const now = new Date()
  const diffMs = now.getTime() - date.getTime()
  const diffDays = Math.floor(diffMs / 86400000)

  if (diffDays < 1) {
    return 'Today'
  } else if (diffDays === 1) {
    return 'Yesterday'
  } else if (diffDays < 7) {
    return `${diffDays} days ago`
  } else if (diffDays < 30) {
    return `${Math.floor(diffDays / 7)} weeks ago`
  } else {
    return date.toLocaleDateString()
  }
}
</script>
