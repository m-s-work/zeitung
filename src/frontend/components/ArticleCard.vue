<template>
  <UCard :ui="{ 
    body: { padding: 'p-0' },
    ring: 'ring-1 ring-gray-200 dark:ring-gray-800',
    rounded: 'rounded-xl',
    shadow: 'shadow-sm hover:shadow-md transition-shadow duration-200'
  }">
    <div class="p-4 sm:p-6 space-y-4">
      <!-- Article Header -->
      <div class="flex items-start justify-between gap-3">
        <div class="flex-1 min-w-0">
          <h3 class="text-lg md:text-xl font-semibold text-gray-900 dark:text-white line-clamp-2 mb-2">
            {{ article.title }}
          </h3>
          <div class="flex items-center gap-2 flex-wrap text-xs md:text-sm text-gray-600 dark:text-gray-400">
            <UBadge color="primary" variant="subtle" size="xs" class="font-medium">
              {{ article.feedName }}
            </UBadge>
            <span class="hidden sm:inline">•</span>
            <time :datetime="article.publishedDate" class="flex items-center gap-1">
              <UIcon name="i-heroicons-clock" class="w-3 h-3" />
              {{ formatDate(article.publishedDate) }}
            </time>
          </div>
        </div>

        <!-- Quick Actions -->
        <div class="flex gap-1 flex-shrink-0">
          <UButton
            icon="i-heroicons-heart"
            size="sm"
            :color="article.isLiked ? 'red' : 'gray'"
            :variant="article.isLiked ? 'soft' : 'ghost'"
            @click="handleLike"
            :loading="liking"
            square
          />
          <UButton
            icon="i-heroicons-arrow-top-right-on-square"
            size="sm"
            color="gray"
            variant="ghost"
            @click="handleClick"
            square
          />
        </div>
      </div>

      <!-- Description -->
      <p v-if="article.description" class="text-sm md:text-base text-gray-700 dark:text-gray-300 line-clamp-3 leading-relaxed">
        {{ article.description }}
      </p>

      <!-- Tags -->
      <div v-if="article.tags && article.tags.length > 0" class="flex flex-wrap gap-2">
        <UBadge
          v-for="tag in article.tags.slice(0, 5)"
          :key="tag.id"
          color="blue"
          variant="soft"
          size="xs"
          class="font-medium"
        >
          <UIcon name="i-heroicons-hashtag" class="w-3 h-3 -ml-1" />
          {{ tag.name }}
          <span v-if="showConfidence" class="ml-1 opacity-70 text-xs">
            {{ Math.round(tag.confidence * 100) }}%
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
      <div class="flex items-center justify-between pt-3 border-t border-gray-200 dark:border-gray-700">
        <div class="flex gap-2">
          <UButton
            icon="i-heroicons-hand-thumb-up"
            size="sm"
            :color="article.userVote === 1 ? 'primary' : 'gray'"
            :variant="article.userVote === 1 ? 'soft' : 'ghost'"
            @click="handleVote(1)"
            :loading="voting"
          >
            <span class="hidden sm:inline">Upvote</span>
          </UButton>
          <UButton
            icon="i-heroicons-hand-thumb-down"
            size="sm"
            :color="article.userVote === -1 ? 'red' : 'gray'"
            :variant="article.userVote === -1 ? 'soft' : 'ghost'"
            @click="handleVote(-1)"
            :loading="voting"
          >
            <span class="hidden sm:inline">Downvote</span>
          </UButton>
        </div>

        <UButton
          label="Read Article"
          size="sm"
          color="primary"
          icon="i-heroicons-arrow-right"
          trailing
          @click="handleRead"
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
