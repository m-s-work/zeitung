<template>
  <UModal v-model="isOpen" :ui="{ width: 'sm:max-w-md' }">
    <UCard :ui="{ rounded: 'rounded-2xl' }">
      <template #header>
        <div class="flex items-center justify-between">
          <h3 class="text-xl font-bold text-gray-900 dark:text-gray-100">Rate This Article</h3>
          <UButton
            color="gray"
            variant="ghost"
            icon="i-heroicons-x-mark"
            @click="close"
          />
        </div>
      </template>

      <div class="space-y-6">
        <div>
          <div class="flex items-center justify-center gap-2 mb-4 text-sm text-gray-600 dark:text-gray-400">
            <UIcon name="i-heroicons-clock" class="w-4 h-4" />
            <p>
              You spent <span class="font-semibold text-blue-600 dark:text-blue-400">{{ formatReadingTime(readingTime) }}</span> reading
            </p>
          </div>
          <p class="text-center text-base font-medium text-gray-900 dark:text-gray-100 mb-4">
            How would you rate it?
          </p>
          
          <!-- Star Rating -->
          <div class="flex gap-3 justify-center py-6">
            <button
              v-for="star in 5"
              :key="star"
              @click="rating = star"
              @mouseenter="hoverRating = star"
              @mouseleave="hoverRating = 0"
              class="transition-all duration-200 hover:scale-125 focus:outline-none focus:ring-2 focus:ring-blue-500 rounded-full"
            >
              <UIcon
                :name="(hoverRating || rating) >= star ? 'i-heroicons-star-solid' : 'i-heroicons-star'"
                :class="[
                  'w-12 h-12 transition-colors',
                  (hoverRating || rating) >= star
                    ? 'text-yellow-400'
                    : 'text-gray-300 dark:text-gray-600'
                ]"
              />
            </button>
          </div>

          <div class="text-center">
            <UBadge
              :color="rating > 3 ? 'green' : rating > 0 ? 'amber' : 'gray'"
              variant="soft"
              size="lg"
            >
              <template v-if="rating === 0">Select a rating</template>
              <template v-else-if="rating === 1">Poor</template>
              <template v-else-if="rating === 2">Fair</template>
              <template v-else-if="rating === 3">Good</template>
              <template v-else-if="rating === 4">Very Good</template>
              <template v-else-if="rating === 5">Excellent</template>
            </UBadge>
          </div>
        </div>
      </div>

      <template #footer>
        <div class="flex justify-end gap-3">
          <UButton color="gray" variant="ghost" @click="close" size="lg">
            Skip
          </UButton>
          <UButton 
            color="primary" 
            @click="submitRating"
            :disabled="rating === 0"
            :loading="submitting"
            size="lg"
            icon="i-heroicons-check"
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
