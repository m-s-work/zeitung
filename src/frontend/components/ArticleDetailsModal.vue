<template>
  <UModal v-model="isOpen" :ui="{ width: 'sm:max-w-2xl' }">
    <UCard v-if="article">
      <template #header>
        <div class="flex items-start justify-between">
          <div class="flex-1">
            <h2 class="text-xl font-bold text-gray-900 dark:text-white">
              {{ article.title }}
            </h2>
            <div class="flex items-center gap-2 mt-2 text-sm text-gray-600 dark:text-gray-400">
              <span v-if="article.feedTitle">{{ article.feedTitle }}</span>
              <span v-if="article.author">• {{ article.author }}</span>
              <span v-if="article.publishedAt">• {{ formatDate(article.publishedAt) }}</span>
            </div>
          </div>
          <UButton
            icon="i-heroicons-x-mark"
            variant="ghost"
            color="gray"
            @click="isOpen = false"
          />
        </div>
      </template>

      <div class="space-y-6">
        <!-- Content -->
        <div v-if="article.content" class="text-gray-700 dark:text-gray-300">
          {{ article.content }}
        </div>

        <!-- Tags with Voting -->
        <div v-if="article.tags && article.tags.length > 0">
          <TagVoting
            :tags="article.tags"
            :initial-votes="tagVotes"
            @vote="onTagVote"
          />
        </div>

        <!-- User Interaction Info -->
        <div v-if="interaction" class="border-t border-gray-200 dark:border-gray-700 pt-4">
          <h4 class="text-sm font-medium text-gray-900 dark:text-white mb-2">
            Your Interaction
          </h4>
          <div class="text-sm text-gray-600 dark:text-gray-400 space-y-1">
            <p v-if="interaction.openedAt">
              Opened: {{ formatDate(interaction.openedAt) }}
            </p>
            <p v-if="interaction.readTimeSeconds">
              Read time: {{ formatReadTime(interaction.readTimeSeconds) }}
            </p>
            <p v-if="interaction.rating">
              Your rating: {{ '⭐'.repeat(interaction.rating) }}
            </p>
          </div>
        </div>
      </div>

      <template #footer>
        <div class="flex justify-between items-center">
          <UButton
            v-if="!interaction?.rating"
            label="Rate Article"
            variant="outline"
            @click="showRating"
          />
          <div v-else />
          <UButton
            label="Read Full Article"
            color="primary"
            @click="openArticle"
          />
        </div>
      </template>
    </UCard>

    <UCard v-else-if="loading">
      <USkeleton class="h-64" />
    </UCard>

    <UCard v-else-if="error">
      <UAlert
        icon="i-heroicons-exclamation-triangle"
        color="red"
        variant="soft"
        title="Failed to load article"
        :description="error.message"
      />
    </UCard>
  </UModal>
</template>

<script setup lang="ts">
interface Props {
  modelValue: boolean
  articleId: string
}

const props = defineProps<Props>()
const emit = defineEmits<{
  (e: 'update:modelValue', value: boolean): void
  (e: 'close'): void
}>()

const isOpen = computed({
  get: () => props.modelValue,
  set: (value) => {
    emit('update:modelValue', value)
    if (!value) emit('close')
  }
})

const interactionsStore = useInteractionsStore()

// Fetch article details
const { data: article, pending: loading, error } = await useFetch(() => `/api/articles/${props.articleId}`, {
  watch: [() => props.articleId],
  immediate: computed(() => props.modelValue && !!props.articleId)
})

const interaction = computed(() => interactionsStore.interactions[props.articleId])
const tagVotes = ref<Record<string, 'interested' | 'ignored' | null>>({})

const formatDate = (dateStr: string | Date) => {
  const date = typeof dateStr === 'string' ? new Date(dateStr) : dateStr
  return date.toLocaleDateString('en-US', { 
    year: 'numeric', 
    month: 'short', 
    day: 'numeric',
    hour: '2-digit',
    minute: '2-digit'
  })
}

const formatReadTime = (seconds: number) => {
  if (seconds < 60) return `${seconds} seconds`
  const minutes = Math.floor(seconds / 60)
  return `${minutes} minute${minutes !== 1 ? 's' : ''}`
}

const onTagVote = async ({ tag, vote }: { tag: string; vote: 'interested' | 'ignored' | null }) => {
  tagVotes.value[tag] = vote
  
  // Send to API
  try {
    await $fetch('/api/tags/vote', {
      method: 'POST',
      body: {
        tag,
        voteType: vote === 'interested' ? 'Interested' : vote === 'ignored' ? 'Ignored' : 'Neutral'
      }
    })
  } catch (e) {
    console.error('Failed to record tag vote', e)
  }
}

const showRating = () => {
  // Close this modal and the parent will show the rating modal
  isOpen.value = false
  // TODO: Trigger rating modal from parent
}

const openArticle = () => {
  if (article.value) {
    interactionsStore.recordArticleOpen(props.articleId)
    window.open((article.value as any).url, '_blank')
  }
}
</script>
