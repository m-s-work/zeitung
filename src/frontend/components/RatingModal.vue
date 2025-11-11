<template>
  <UModal v-model="isOpen" :ui="{ width: 'max-w-md' }">
    <UCard class="border-0 shadow-2xl">
      <template #header>
        <div class="flex items-center justify-between">
          <h3 class="text-xl font-semibold text-gray-900 dark:text-white">Rate This Article</h3>
          <UButton
            color="gray"
            variant="ghost"
            icon="i-heroicons-x-mark"
            @click="close"
            size="sm"
          />
        </div>
      </template>

      <div class="space-y-6">
        <div>
          <p class="text-sm text-gray-600 dark:text-gray-400 mb-4">
            You spent <span class="font-semibold text-gray-900 dark:text-white">{{ formatReadingTime(readingTime) }}</span> reading this article.
          </p>
          <p class="text-base font-medium text-gray-900 dark:text-white mb-4">How would you rate it?</p>
          
          <!-- Star Rating -->
          <div class="flex gap-3 justify-center py-6">
            <button
              v-for="star in 5"
              :key="star"
              @click="rating = star"
              @mouseenter="hoverRating = star"
              @mouseleave="hoverRating = 0"
              class="transition-all duration-200 hover:scale-125 focus:outline-none focus:ring-2 focus:ring-primary focus:ring-offset-2 dark:focus:ring-offset-gray-900 rounded-full p-1"
            >
              <UIcon
                :name="(hoverRating || rating) >= star ? 'i-heroicons-star-solid' : 'i-heroicons-star'"
                :class="[
                  'w-12 h-12 transition-colors',
                  (hoverRating || rating) >= star
                    ? 'text-yellow-400'
                    : 'text-gray-300 dark:text-gray-700'
                ]"
              />
            </button>
          </div>

          <div class="text-center text-base font-medium text-gray-700 dark:text-gray-300 min-h-[28px]">
            <template v-if="rating === 0">Select a rating</template>
            <template v-else-if="rating === 1">
              <span class="text-red-500">😞 Poor</span>
            </template>
            <template v-else-if="rating === 2">
              <span class="text-orange-500">😐 Fair</span>
            </template>
            <template v-else-if="rating === 3">
              <span class="text-yellow-500">🙂 Good</span>
            </template>
            <template v-else-if="rating === 4">
              <span class="text-blue-500">😊 Very Good</span>
            </template>
            <template v-else-if="rating === 5">
              <span class="text-green-500">🤩 Excellent</span>
            </template>
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
