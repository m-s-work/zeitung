<template>
  <UModal v-model="isOpen" :ui="{ width: 'sm:max-w-md' }">
    <UCard>
      <template #header>
        <h3 class="text-lg font-semibold">Rate this article</h3>
      </template>

      <div class="space-y-6">
        <div>
          <h4 class="font-medium text-gray-900 dark:text-white mb-2">{{ articleTitle }}</h4>
          <p class="text-sm text-gray-600 dark:text-gray-400">
            How well did this article match your interests?
          </p>
        </div>

        <!-- Star Rating -->
        <div class="flex justify-center gap-2">
          <button
            v-for="star in 5"
            :key="star"
            @click="selectedRating = star"
            @mouseenter="hoveredRating = star"
            @mouseleave="hoveredRating = 0"
            class="transition-transform hover:scale-110"
          >
            <UIcon
              name="i-heroicons-star"
              :class="[
                'w-10 h-10',
                star <= (hoveredRating || selectedRating)
                  ? 'text-yellow-400'
                  : 'text-gray-300 dark:text-gray-600'
              ]"
            />
          </button>
        </div>

        <!-- Rating Labels -->
        <div v-if="selectedRating > 0" class="text-center">
          <p class="text-sm font-medium text-gray-700 dark:text-gray-300">
            {{ ratingLabels[selectedRating - 1] }}
          </p>
        </div>

        <!-- Read Time Display -->
        <div v-if="readTimeSeconds" class="text-center text-sm text-gray-600 dark:text-gray-400">
          Read time: {{ formatReadTime(readTimeSeconds) }}
        </div>
      </div>

      <template #footer>
        <div class="flex justify-end gap-2">
          <UButton
            label="Skip"
            variant="ghost"
            @click="onSkip"
          />
          <UButton
            label="Submit Rating"
            color="primary"
            :disabled="selectedRating === 0"
            @click="onSubmit"
          />
        </div>
      </template>
    </UCard>
  </UModal>
</template>

<script setup lang="ts">
interface Props {
  modelValue: boolean
  articleId: string
  articleTitle: string
  readTimeSeconds?: number
}

const props = defineProps<Props>()
const emit = defineEmits<{
  (e: 'update:modelValue', value: boolean): void
  (e: 'submit', data: { articleId: string; rating: number }): void
}>()

const isOpen = computed({
  get: () => props.modelValue,
  set: (value) => emit('update:modelValue', value)
})

const selectedRating = ref(0)
const hoveredRating = ref(0)

const ratingLabels = [
  'Not relevant',
  'Somewhat relevant',
  'Relevant',
  'Very relevant',
  'Perfect match!'
]

const formatReadTime = (seconds: number) => {
  if (seconds < 60) return `${seconds} seconds`
  const minutes = Math.floor(seconds / 60)
  const remainingSeconds = seconds % 60
  if (remainingSeconds === 0) return `${minutes} minute${minutes !== 1 ? 's' : ''}`
  return `${minutes}m ${remainingSeconds}s`
}

const onSubmit = () => {
  if (selectedRating > 0) {
    emit('submit', { articleId: props.articleId, rating: selectedRating })
    isOpen.value = false
    selectedRating.value = 0
  }
}

const onSkip = () => {
  isOpen.value = false
  selectedRating.value = 0
}

// Reset rating when modal opens
watch(isOpen, (newValue) => {
  if (newValue) {
    selectedRating.value = 0
    hoveredRating.value = 0
  }
})
</script>
