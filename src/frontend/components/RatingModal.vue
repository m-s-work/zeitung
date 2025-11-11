<template>
  <UModal 
    v-model="isOpen" 
    :ui="{ 
      width: 'sm:max-w-md',
      overlay: {
        background: 'bg-gray-950/75 dark:bg-gray-950/90'
      }
    }"
  >
    <UCard
      :ui="{
        ring: 'ring-1 ring-gray-200 dark:ring-gray-800',
        background: 'bg-white dark:bg-gray-900'
      }"
    >
      <template #header>
        <div class="flex items-center justify-between">
          <div class="flex items-center gap-3">
            <div class="flex items-center justify-center w-10 h-10 rounded-full bg-primary-100 dark:bg-primary-900/30">
              <UIcon name="i-heroicons-star" class="w-5 h-5 text-primary-600 dark:text-primary-400" />
            </div>
            <h3 class="text-xl font-bold">Rate This Article</h3>
          </div>
          <UButton
            color="gray"
            variant="ghost"
            icon="i-heroicons-x-mark"
            @click="close"
          />
        </div>
      </template>

      <div class="space-y-6">
        <div class="text-center">
          <div class="inline-flex items-center gap-2 px-4 py-2 rounded-full bg-primary-50 dark:bg-primary-900/20 text-primary-700 dark:text-primary-300 mb-4">
            <UIcon name="i-heroicons-clock" class="w-4 h-4" />
            <span class="text-sm font-medium">{{ formatReadingTime(readingTime) }} reading time</span>
          </div>
          
          <p class="text-base font-medium text-gray-900 dark:text-white mb-6">
            How would you rate this article?
          </p>
          
          <!-- Star Rating -->
          <div class="flex gap-3 justify-center py-6">
            <button
              v-for="star in 5"
              :key="star"
              @click="rating = star"
              @mouseenter="hoverRating = star"
              @mouseleave="hoverRating = 0"
              class="transition-all duration-200 hover:scale-125 active:scale-110"
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
            <p class="text-base font-semibold" :class="[
              rating === 0 ? 'text-gray-500 dark:text-gray-400' : 'text-gray-900 dark:text-white'
            ]">
              <template v-if="rating === 0">Select a rating</template>
              <template v-else-if="rating === 1">😞 Poor</template>
              <template v-else-if="rating === 2">😐 Fair</template>
              <template v-else-if="rating === 3">🙂 Good</template>
              <template v-else-if="rating === 4">😊 Very Good</template>
              <template v-else-if="rating === 5">🤩 Excellent</template>
            </p>
          </div>
        </div>
      </div>

      <template #footer>
        <div class="flex justify-end gap-3">
          <UButton color="gray" variant="ghost" size="lg" @click="close">
            Skip
          </UButton>
          <UButton 
            color="primary"
            size="lg"
            @click="submitRating"
            :disabled="rating === 0"
            :loading="submitting"
            icon="i-heroicons-paper-airplane"
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
