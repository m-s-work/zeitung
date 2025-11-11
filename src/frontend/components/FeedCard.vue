<template>
  <UCard :ui="{ 
    ring: 'ring-1 ring-gray-200 dark:ring-gray-800',
    rounded: 'rounded-xl',
    shadow: 'shadow-sm hover:shadow-md transition-shadow duration-200'
  }">
    <template #header>
      <div class="flex items-start justify-between gap-3">
        <div class="flex-1 min-w-0">
          <h3 class="text-lg font-semibold text-gray-900 dark:text-white mb-2">{{ feed.name }}</h3>
        </div>
        <div class="flex gap-2 flex-shrink-0">
          <UBadge v-if="feed.isApproved" color="green" variant="soft" size="sm">
            <UIcon name="i-heroicons-check-circle" class="w-3 h-3" />
            Approved
          </UBadge>
          <UBadge v-if="feed.isSubscribed" color="primary" variant="soft" size="sm">
            <UIcon name="i-heroicons-bell" class="w-3 h-3" />
            Subscribed
          </UBadge>
        </div>
      </div>
    </template>

    <div class="space-y-3">
      <p v-if="feed.description" class="text-sm text-gray-700 dark:text-gray-300 leading-relaxed">
        {{ feed.description }}
      </p>

      <div class="text-xs text-gray-600 dark:text-gray-400 space-y-2">
        <div class="flex items-center gap-2">
          <UIcon name="i-heroicons-link" class="w-4 h-4 flex-shrink-0" />
          <a :href="feed.url" target="_blank" class="hover:text-primary-600 dark:hover:text-primary-400 hover:underline truncate transition-colors">
            {{ feed.url }}
          </a>
        </div>
        <div v-if="feed.lastFetchedAt" class="flex items-center gap-2">
          <UIcon name="i-heroicons-clock" class="w-4 h-4 flex-shrink-0" />
          <span>Last updated: {{ formatDate(feed.lastFetchedAt) }}</span>
        </div>
      </div>

      <!-- Relevant Tags for Recommendations -->
      <div v-if="isRecommendation && feed.relevantTags?.length" class="flex flex-wrap gap-2 pt-2">
        <UBadge
          v-for="tag in feed.relevantTags"
          :key="tag"
          color="primary"
          variant="soft"
          size="xs"
        >
          {{ tag }}
        </UBadge>
      </div>
    </div>

    <template #footer>
      <div class="flex justify-between items-center">
        <div v-if="isRecommendation" class="text-sm text-gray-600 dark:text-gray-400">
          Match: {{ Math.round((feed.relevanceScore || 0) * 100) }}%
        </div>
        <div v-else class="text-xs text-gray-500">
          Added {{ formatDate(feed.createdAt) }}
        </div>

        <div class="flex gap-2">
          <UButton
            v-if="!feed.isSubscribed"
            icon="i-heroicons-plus"
            size="xs"
            color="primary"
            @click="$emit('subscribe', feed.id)"
            :loading="loading"
          >
            Subscribe
          </UButton>
          <UButton
            v-else
            icon="i-heroicons-minus"
            size="xs"
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
            size="xs"
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
            size="xs"
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
