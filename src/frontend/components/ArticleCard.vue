<template>
  <UCard 
    class="hover:shadow-xl dark:hover:shadow-2xl transition-shadow duration-300 overflow-hidden"
    :ui="{ 
      body: { padding: 'p-6' },
      ring: 'ring-1 ring-gray-200 dark:ring-gray-800 hover:ring-primary-500 dark:hover:ring-primary-600',
      background: 'bg-white dark:bg-gray-900',
      rounded: 'rounded-2xl'
    }"
  >
    <div class="space-y-4">
      <!-- Article Header -->
      <div class="flex items-start gap-4">
        <div class="flex-1 min-w-0 space-y-2">
          <div class="flex items-center gap-2 text-xs">
            <UBadge color="primary" variant="soft" size="xs">
              {{ article.feedName }}
            </UBadge>
            <span class="text-gray-400 dark:text-gray-600">•</span>
            <time :datetime="article.publishedDate" class="text-gray-500 dark:text-gray-400">
              {{ formatDate(article.publishedDate) }}
            </time>
          </div>
          
          <h3 class="text-xl font-bold text-gray-900 dark:text-white line-clamp-2 hover:text-primary-600 dark:hover:text-primary-400 transition-colors">
            {{ article.title }}
          </h3>
        </div>

        <!-- Quick Like Action -->
        <UButton
          :icon="article.isLiked ? 'i-heroicons-heart-solid' : 'i-heroicons-heart'"
          size="md"
          :color="article.isLiked ? 'red' : 'gray'"
          variant="ghost"
          @click="handleLike"
          :loading="liking"
        />
      </div>

      <!-- Description -->
      <p v-if="article.description" class="text-base text-gray-700 dark:text-gray-300 line-clamp-3 leading-relaxed">
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
        >
          <Icon name="heroicons:hashtag-20-solid" class="w-3 h-3" />
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
        >
          +{{ article.tags.length - 5 }}
        </UBadge>
      </div>

      <!-- Actions Bar -->
      <div class="flex flex-wrap items-center justify-between gap-3 pt-3 border-t border-gray-200 dark:border-gray-800">
        <div class="flex gap-2">
          <UButton
            icon="i-heroicons-hand-thumb-up-20-solid"
            size="sm"
            :color="article.userVote === 1 ? 'primary' : 'gray'"
            :variant="article.userVote === 1 ? 'soft' : 'ghost'"
            @click="handleVote(1)"
            :loading="voting"
          >
            Upvote
          </UButton>
          <UButton
            icon="i-heroicons-hand-thumb-down-20-solid"
            size="sm"
            :color="article.userVote === -1 ? 'red' : 'gray'"
            :variant="article.userVote === -1 ? 'soft' : 'ghost'"
            @click="handleVote(-1)"
            :loading="voting"
          >
            Downvote
          </UButton>
        </div>

        <UButton
          size="sm"
          color="primary"
          @click="handleRead"
        >
          Read Article
          <Icon name="heroicons:arrow-right-20-solid" class="w-4 h-4" />
        </UButton>
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
