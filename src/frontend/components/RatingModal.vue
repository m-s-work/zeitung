<template>
  <UModal v-model="isOpen" :ui="{ width: 'max-w-md' }">
    <UCard>
      <template #header>
        <div class="flex items-center justify-between">
          <h3 class="text-lg font-semibold">Rate This Article</h3>
          <UButton
            color="gray"
            variant="ghost"
            icon="i-heroicons-x-mark"
            @click="close"
          />
        </div>
      </template>

      <div class="space-y-4">
        <div>
          <p class="text-sm text-gray-600 dark:text-gray-400 mb-3">
            You spent {{ formatReadingTime(readingTime) }} reading this article.
          </p>
          <p class="text-sm font-medium mb-2">How would you rate it?</p>
          
          <!-- Star Rating -->
          <div class="flex gap-2 justify-center py-4">
            <button
              v-for="star in 5"
              :key="star"
              @click="rating = star"
              @mouseenter="hoverRating = star"
              @mouseleave="hoverRating = 0"
              class="transition-transform hover:scale-110"
            >
              <UIcon
                name="i-heroicons-star"
                :class="[
                  'w-10 h-10',
                  (hoverRating || rating) >= star
                    ? 'text-yellow-400'
                    : 'text-gray-300 dark:text-gray-600'
                ]"
              />
            </button>
          </div>

          <div class="text-center text-sm text-gray-600 dark:text-gray-400">
            <template v-if="rating === 0">Select a rating</template>
            <template v-else-if="rating === 1">Poor</template>
            <template v-else-if="rating === 2">Fair</template>
            <template v-else-if="rating === 3">Good</template>
            <template v-else-if="rating === 4">Very Good</template>
            <template v-else-if="rating === 5">Excellent</template>
          </div>
        </div>
      </div>

      <template #footer>
        <div class="flex justify-end gap-2">
          <UButton color="gray" variant="ghost" @click="close">
            Skip
          </UButton>
          <UButton 
            color="primary" 
            @click="submitRating"
            :disabled="rating === 0"
            :loading="submitting"
          >
            Submit Rating
          </UButton>
        </div>
      </template>
    </UCard>
  </UModal>
</template>

<script setup lang="ts">
const props = defineProps<{
  articleId: number
  readingTime: number
}>()

const emit = defineEmits<{
  rated: [articleId: number, rating: number]
}>()

const isOpen = defineModel<boolean>()
const rating = ref(0)
const hoverRating = ref(0)
const submitting = ref(false)

const formatReadingTime = (seconds: number) => {
  if (seconds < 60) {
    return `${seconds} seconds`
  }
  const minutes = Math.floor(seconds / 60)
  const remainingSeconds = seconds % 60
  if (remainingSeconds === 0) {
    return `${minutes} minute${minutes !== 1 ? 's' : ''}`
  }
  return `${minutes}m ${remainingSeconds}s`
}

const submitRating = async () => {
  if (rating.value === 0) return
  
  submitting.value = true
  try {
    emit('rated', props.articleId, rating.value)
    close()
  } finally {
    submitting.value = false
  }
}

const close = () => {
  isOpen.value = false
  // Reset after animation completes
  setTimeout(() => {
    rating.value = 0
    hoverRating.value = 0
  }, 300)
}
</script>
