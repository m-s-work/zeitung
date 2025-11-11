<template>
  <UCard 
    :ui="{ 
      body: { padding: 'p-0' },
      ring: 'ring-1 ring-gray-200 dark:ring-gray-800',
      rounded: 'rounded-xl',
      shadow: 'shadow-lg hover:shadow-xl',
      base: 'transition-all duration-200'
    }"
  >
    <div class="p-5 sm:p-6 space-y-4">
      <!-- Article Header -->
      <div class="flex items-start gap-4">
        <div class="flex-1 min-w-0 space-y-2">
          <h3 class="text-lg sm:text-xl font-semibold text-gray-900 dark:text-white line-clamp-2 leading-tight">
            {{ article.title }}
          </h3>
          <div class="flex items-center gap-2 text-xs sm:text-sm text-gray-600 dark:text-gray-400">
            <UBadge color="gray" variant="subtle" size="xs" class="font-medium">
              {{ article.feedName }}
            </UBadge>
            <span class="text-gray-400 dark:text-gray-600">•</span>
            <time :datetime="article.publishedDate" class="whitespace-nowrap">
              {{ formatDate(article.publishedDate) }}
            </time>
          </div>
        </div>

        <!-- Quick Actions -->
        <div class="flex gap-1">
          <UButton
            :icon="article.isLiked ? 'i-heroicons-heart-solid' : 'i-heroicons-heart'"
            size="sm"
            :color="article.isLiked ? 'red' : 'gray'"
            variant="ghost"
            @click="handleLike"
            :loading="liking"
            class="hover:scale-110 transition-transform"
          />
        </div>
      </div>

      <!-- Description -->
      <p v-if="article.description" class="text-sm sm:text-base text-gray-700 dark:text-gray-300 line-clamp-3 leading-relaxed">
        {{ article.description }}
      </p>

      <!-- Tags -->
      <div v-if="article.tags && article.tags.length > 0" class="flex flex-wrap gap-2">
        <UBadge
          v-for="tag in article.tags.slice(0, 5)"
          :key="tag.id"
          color="primary"
          variant="soft"
          size="xs"
          class="font-medium"
        >
          <span class="flex items-center gap-1">
            {{ tag.name }}
            <span v-if="showConfidence" class="opacity-60 text-xs">
              {{ Math.round(tag.confidence * 100) }}%
            </span>
          </span>
        </UBadge>
        <UBadge
          v-if="article.tags.length > 5"
          color="gray"
          variant="soft"
          size="xs"
        >
          +{{ article.tags.length - 5 }}
        </UBadge>
      </div>

      <!-- Actions Bar -->
      <div class="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-3 pt-3 border-t border-gray-200 dark:border-gray-800">
        <div class="flex gap-2">
          <UButton
            icon="i-heroicons-hand-thumb-up"
            size="sm"
            :color="article.userVote === 1 ? 'primary' : 'gray'"
            :variant="article.userVote === 1 ? 'solid' : 'soft'"
            @click="handleVote(1)"
            :loading="voting"
          >
            Upvote
          </UButton>
          <UButton
            icon="i-heroicons-hand-thumb-down"
            size="sm"
            :color="article.userVote === -1 ? 'red' : 'gray'"
            :variant="article.userVote === -1 ? 'solid' : 'soft'"
            @click="handleVote(-1)"
            :loading="voting"
          >
            Downvote
          </UButton>
        </div>

        <UButton
          label="Read Article"
          size="sm"
          color="primary"
          trailing-icon="i-heroicons-arrow-top-right-on-square"
          @click="handleRead"
          class="w-full sm:w-auto"
        />
      </div>
    </div>
  </UCard>
</template>

<script setup lang="ts">
import type { Article } from '~/types'

const props = defineProps<{
  article: Article
  showConfidence?: boolean
}>()

const emit = defineEmits<{
  like: [articleId: number]
  vote: [articleId: number, value: number]
  click: [articleId: number]
  read: [articleId: number]
}>()

const { userId } = useUser()
const liking = ref(false)
const voting = ref(false)

const handleLike = async () => {
  liking.value = true
  try {
    emit('like', props.article.id)
  } finally {
    liking.value = false
  }
}

const handleVote = async (value: number) => {
  voting.value = true
  try {
    emit('vote', props.article.id, value)
  } finally {
    voting.value = false
  }
}

const handleClick = () => {
  emit('click', props.article.id)
  window.open(props.article.link, '_blank', 'noopener,noreferrer')
}

const handleRead = () => {
  emit('read', props.article.id)
  window.open(props.article.link, '_blank', 'noopener,noreferrer')
}

const formatDate = (dateString: string) => {
  const date = new Date(dateString)
  const now = new Date()
  const diffMs = now.getTime() - date.getTime()
  const diffMins = Math.floor(diffMs / 60000)
  const diffHours = Math.floor(diffMs / 3600000)
  const diffDays = Math.floor(diffMs / 86400000)

  if (diffMins < 60) {
    return `${diffMins}m ago`
  } else if (diffHours < 24) {
    return `${diffHours}h ago`
  } else if (diffDays < 7) {
    return `${diffDays}d ago`
  } else {
    return date.toLocaleDateString()
  }
}
</script>
