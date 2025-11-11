<template>
  <UCard 
    class="hover:shadow-xl dark:hover:shadow-2xl transition-all duration-300 cursor-pointer group overflow-hidden border-0 shadow-md dark:shadow-lg"
    :ui="{ 
      body: { padding: 'p-0' },
      ring: 'ring-1 ring-gray-200 dark:ring-gray-800',
      background: 'bg-white dark:bg-gray-900'
    }"
  >
    <div class="p-5 md:p-6 space-y-4">
      <!-- Article Header -->
      <div class="flex items-start gap-4">
        <div class="flex-1 min-w-0 space-y-2">
          <div class="flex items-center gap-2 text-xs">
            <UBadge color="primary" variant="soft" size="xs" class="font-medium">
              {{ article.feedName }}
            </UBadge>
            <span class="text-gray-400 dark:text-gray-600">•</span>
            <time :datetime="article.publishedDate" class="text-gray-500 dark:text-gray-400">
              {{ formatDate(article.publishedDate) }}
            </time>
          </div>
          
          <h3 class="text-lg md:text-xl font-bold text-gray-900 dark:text-white line-clamp-2 group-hover:text-primary-600 dark:group-hover:text-primary-400 transition-colors">
            {{ article.title }}
          </h3>
        </div>

        <!-- Quick Like Action -->
        <UButton
          :icon="article.isLiked ? 'i-heroicons-heart-solid' : 'i-heroicons-heart'"
          size="md"
          :color="article.isLiked ? 'red' : 'gray'"
          variant="ghost"
          class="shrink-0"
          @click.stop="handleLike"
          :loading="liking"
        />
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
          color="gray"
          variant="subtle"
          size="xs"
          class="font-normal"
        >
          <UIcon name="i-heroicons-hashtag" class="w-3 h-3 mr-0.5" />
          {{ tag.name }}
          <span v-if="showConfidence" class="ml-1 opacity-60 text-xs">
            {{ Math.round(tag.confidence * 100) }}%
          </span>
        </UBadge>
        <UBadge
          v-if="article.tags.length > 5"
          color="gray"
          variant="subtle"
          size="xs"
          class="font-normal"
        >
          +{{ article.tags.length - 5 }}
        </UBadge>
      </div>

      <!-- Actions Bar -->
      <div class="flex flex-wrap items-center justify-between gap-3 pt-3 border-t border-gray-100 dark:border-gray-800">
        <div class="flex gap-2">
          <UButton
            icon="i-heroicons-hand-thumb-up"
            size="sm"
            :color="article.userVote === 1 ? 'primary' : 'gray'"
            :variant="article.userVote === 1 ? 'soft' : 'ghost'"
            @click.stop="handleVote(1)"
            :loading="voting"
            class="font-medium"
          >
            Upvote
          </UButton>
          <UButton
            icon="i-heroicons-hand-thumb-down"
            size="sm"
            :color="article.userVote === -1 ? 'red' : 'gray'"
            :variant="article.userVote === -1 ? 'soft' : 'ghost'"
            @click.stop="handleVote(-1)"
            :loading="voting"
            class="font-medium"
          >
            Downvote
          </UButton>
        </div>

        <UButton
          label="Read Article"
          trailing-icon="i-heroicons-arrow-right"
          size="sm"
          color="primary"
          @click.stop="handleRead"
          class="font-medium"
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
