<template>
  <UCard :ui="{ body: { padding: 'p-4 sm:p-6' } }">
    <div class="space-y-3">
      <!-- Article Header -->
      <div class="flex items-start justify-between gap-3">
        <div class="flex-1 min-w-0">
          <h3 class="text-lg font-semibold text-gray-900 dark:text-white line-clamp-2">
            {{ article.title }}
          </h3>
          <div class="flex items-center gap-2 mt-1 text-sm text-gray-600 dark:text-gray-400">
            <UBadge color="gray" variant="subtle" size="xs">{{ article.feedName }}</UBadge>
            <span>•</span>
            <time :datetime="article.publishedDate">
              {{ formatDate(article.publishedDate) }}
            </time>
          </div>
        </div>

        <!-- Quick Actions -->
        <div class="flex gap-1">
          <UButton
            icon="i-heroicons-heart"
            size="sm"
            :color="article.isLiked ? 'red' : 'gray'"
            variant="ghost"
            @click="handleLike"
            :loading="liking"
          />
          <UButton
            icon="i-heroicons-arrow-top-right-on-square"
            size="sm"
            color="gray"
            variant="ghost"
            @click="handleClick"
          />
        </div>
      </div>

      <!-- Description -->
      <p v-if="article.description" class="text-sm text-gray-700 dark:text-gray-300 line-clamp-3">
        {{ article.description }}
      </p>

      <!-- Tags -->
      <div v-if="article.tags && article.tags.length > 0" class="flex flex-wrap gap-2">
        <UBadge
          v-for="tag in article.tags"
          :key="tag.id"
          color="primary"
          variant="soft"
          size="xs"
        >
          {{ tag.name }}
          <span v-if="showConfidence" class="ml-1 opacity-70">
            ({{ Math.round(tag.confidence * 100) }}%)
          </span>
        </UBadge>
      </div>

      <!-- Actions Bar -->
      <div class="flex items-center justify-between pt-2 border-t border-gray-200 dark:border-gray-700">
        <div class="flex gap-2">
          <UButton
            icon="i-heroicons-hand-thumb-up"
            size="xs"
            :color="article.userVote === 1 ? 'primary' : 'gray'"
            variant="soft"
            @click="handleVote(1)"
            :loading="voting"
          >
            Upvote
          </UButton>
          <UButton
            icon="i-heroicons-hand-thumb-down"
            size="xs"
            :color="article.userVote === -1 ? 'red' : 'gray'"
            variant="soft"
            @click="handleVote(-1)"
            :loading="voting"
          >
            Downvote
          </UButton>
        </div>

        <UButton
          label="Read Article"
          size="xs"
          color="primary"
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
