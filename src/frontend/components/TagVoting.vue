<template>
  <div class="space-y-4">
    <div>
      <h4 class="text-sm font-medium text-gray-900 dark:text-white mb-3">
        Vote on tags to improve recommendations
      </h4>
      <p class="text-xs text-gray-600 dark:text-gray-400">
        Help us understand your interests better by voting on these tags
      </p>
    </div>

    <div class="flex flex-wrap gap-2">
      <div
        v-for="tag in tags"
        :key="tag"
        class="inline-flex items-center gap-1 px-3 py-1.5 rounded-full border transition-colors"
        :class="getTagClasses(tag)"
      >
        <span class="text-sm">{{ tag }}</span>
        <div class="flex items-center gap-0.5 ml-1">
          <button
            @click="onVote(tag, 'interested')"
            class="p-0.5 rounded hover:bg-green-100 dark:hover:bg-green-900/30 transition-colors"
            :class="getVote(tag) === 'interested' ? 'text-green-600' : 'text-gray-400'"
          >
            <UIcon name="i-heroicons-arrow-up" class="w-4 h-4" />
          </button>
          <button
            @click="onVote(tag, 'ignored')"
            class="p-0.5 rounded hover:bg-red-100 dark:hover:bg-red-900/30 transition-colors"
            :class="getVote(tag) === 'ignored' ? 'text-red-600' : 'text-gray-400'"
          >
            <UIcon name="i-heroicons-arrow-down" class="w-4 h-4" />
          </button>
        </div>
      </div>
    </div>

    <!-- Vote Summary -->
    <div v-if="Object.keys(votes).length > 0" class="text-xs text-gray-600 dark:text-gray-400">
      <div class="flex gap-4">
        <span v-if="interestedCount > 0" class="text-green-600">
          ↑ {{ interestedCount }} interested
        </span>
        <span v-if="ignoredCount > 0" class="text-red-600">
          ↓ {{ ignoredCount }} ignored
        </span>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
interface Props {
  tags: string[]
  initialVotes?: Record<string, 'interested' | 'ignored' | null>
}

const props = defineProps<Props>()
const emit = defineEmits<{
  (e: 'vote', data: { tag: string; vote: 'interested' | 'ignored' | null }): void
}>()

const votes = ref<Record<string, 'interested' | 'ignored' | null>>({
  ...(props.initialVotes || {})
})

const interestedCount = computed(() => {
  return Object.values(votes.value).filter(v => v === 'interested').length
})

const ignoredCount = computed(() => {
  return Object.values(votes.value).filter(v => v === 'ignored').length
})

const getVote = (tag: string) => {
  return votes.value[tag] || null
}

const getTagClasses = (tag: string) => {
  const vote = getVote(tag)
  if (vote === 'interested') {
    return 'border-green-300 bg-green-50 dark:border-green-700 dark:bg-green-900/20'
  } else if (vote === 'ignored') {
    return 'border-red-300 bg-red-50 dark:border-red-700 dark:bg-red-900/20'
  }
  return 'border-gray-300 bg-white dark:border-gray-700 dark:bg-gray-800'
}

const onVote = (tag: string, voteType: 'interested' | 'ignored') => {
  // Toggle vote if clicking the same type
  if (votes.value[tag] === voteType) {
    votes.value[tag] = null
  } else {
    votes.value[tag] = voteType
  }
  
  emit('vote', { tag, vote: votes.value[tag] })
}
</script>
