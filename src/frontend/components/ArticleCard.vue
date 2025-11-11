<template>
  <div class="bg-gradient-to-br from-white to-blue-50 dark:from-gray-900 dark:to-blue-950 rounded-2xl shadow-xl hover:shadow-2xl transition-all duration-300 hover:scale-[1.01] border border-blue-200/50 dark:border-blue-800/50 overflow-hidden">
    <div class="p-6 space-y-4">
      <!-- Article Header -->
      <div class="flex items-start justify-between gap-4">
        <div class="flex-1 min-w-0">
          <h3 class="text-2xl font-black text-gray-900 dark:text-white line-clamp-2 mb-3 hover:text-blue-600 dark:hover:text-blue-400 transition-colors cursor-pointer">
            {{ article.title }}
          </h3>
          <div class="flex items-center gap-2 text-sm text-gray-600 dark:text-gray-400 flex-wrap">
            <UBadge 
              color="blue" 
              variant="soft" 
              size="sm"
              class="font-bold"
            >
              {{ article.feedName }}
            </UBadge>
            <span class="text-gray-400 dark:text-gray-600">•</span>
            <time :datetime="article.publishedDate" class="flex items-center gap-1 font-medium">
              <UIcon name="i-heroicons-clock" class="w-4 h-4" />
              {{ formatDate(article.publishedDate) }}
            </time>
          </div>
        </div>

        <!-- Quick Like Action -->
        <UButton
          :icon="article.isLiked ? 'i-heroicons-heart-solid' : 'i-heroicons-heart'"
          size="lg"
          :color="article.isLiked ? 'red' : 'gray'"
          :variant="article.isLiked ? 'soft' : 'ghost'"
          @click="handleLike"
          :loading="liking"
          class="shrink-0 hover:scale-110 transition-transform"
        />
      </div>

      <!-- Description -->
      <p v-if="article.description" class="text-base text-gray-700 dark:text-gray-300 line-clamp-3 leading-relaxed">
        {{ article.description }}
      </p>

      <!-- Tags -->
      <div v-if="article.tags && article.tags.length > 0" class="flex flex-wrap gap-2">
        <UBadge
          v-for="tag in article.tags"
          :key="tag.id"
          color="purple"
          variant="soft"
          size="sm"
          class="font-semibold"
        >
          <UIcon name="i-heroicons-hashtag" class="w-3 h-3" />
          {{ tag.name }}
          <span v-if="showConfidence" class="ml-1 opacity-70 text-xs">
            {{ Math.round(tag.confidence * 100) }}%
          </span>
        </UBadge>
      </div>

      <!-- Actions Bar -->
      <div class="flex items-center justify-between pt-4 border-t border-blue-200/50 dark:border-blue-800/50">
        <div class="flex gap-2">
          <UButton
            icon="i-heroicons-hand-thumb-up"
            size="sm"
            :color="article.userVote === 1 ? 'blue' : 'gray'"
            :variant="article.userVote === 1 ? 'solid' : 'soft'"
            @click="handleVote(1)"
            :loading="voting"
            class="font-bold"
          >
            <span class="hidden sm:inline">Upvote</span>
          </UButton>
          <UButton
            icon="i-heroicons-hand-thumb-down"
            size="sm"
            :color="article.userVote === -1 ? 'red' : 'gray'"
            :variant="article.userVote === -1 ? 'solid' : 'soft'"
            @click="handleVote(-1)"
            :loading="voting"
            class="font-bold"
          >
            <span class="hidden sm:inline">Downvote</span>
          </UButton>
        </div>

        <UButton
          label="Read Article"
          size="md"
          color="blue"
          variant="solid"
          icon="i-heroicons-arrow-top-right-on-square"
          trailing
          @click="handleRead"
          class="font-bold shadow-lg hover:shadow-xl transform hover:scale-105 transition-all"
        />
      </div>
    </div>
  </div>
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
