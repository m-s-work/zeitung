<template>
  <UModal v-model="isOpen" :ui="{ width: 'sm:max-w-md' }">
    <UCard>
      <template #header>
        <h3 class="text-lg font-semibold">Add RSS Feed</h3>
      </template>

      <UForm :state="state" :schema="schema" @submit="onSubmit" class="space-y-4">
        <UFormGroup label="Feed URL" name="url" required>
          <UInput
            v-model="state.url"
            placeholder="https://example.com/feed.xml"
            icon="i-heroicons-rss"
          />
        </UFormGroup>

        <UFormGroup label="Title (Optional)" name="title">
          <UInput
            v-model="state.title"
            placeholder="Will be fetched from feed if not provided"
          />
        </UFormGroup>

        <UAlert
          v-if="errorMessage"
          icon="i-heroicons-exclamation-triangle"
          color="red"
          variant="soft"
          :title="errorMessage"
        />

        <div class="flex justify-end gap-2 pt-4">
          <UButton
            label="Cancel"
            variant="ghost"
            @click="isOpen = false"
          />
          <UButton
            type="submit"
            label="Add Feed"
            color="primary"
            :loading="submitting"
          />
        </div>
      </UForm>
    </UCard>
  </UModal>
</template>

<script setup lang="ts">
import { z } from 'zod'

interface Props {
  modelValue: boolean
}

const props = defineProps<Props>()
const emit = defineEmits<{
  (e: 'update:modelValue', value: boolean): void
  (e: 'added'): void
}>()

const isOpen = computed({
  get: () => props.modelValue,
  set: (value) => emit('update:modelValue', value)
})

const schema = z.object({
  url: z.string().url('Please enter a valid URL'),
  title: z.string().optional()
})

const state = reactive({
  url: '',
  title: ''
})

const submitting = ref(false)
const errorMessage = ref('')

const onSubmit = async () => {
  submitting.value = true
  errorMessage.value = ''
  
  try {
    await $fetch('/api/feeds', {
      method: 'POST',
      body: {
        url: state.url,
        title: state.title || undefined
      }
    })
    
    // Reset form
    state.url = ''
    state.title = ''
    
    // Close modal and notify parent
    isOpen.value = false
    emit('added')
  } catch (e: any) {
    errorMessage.value = e.data?.message || 'Failed to add feed. Please try again.'
  } finally {
    submitting.value = false
  }
}

// Reset form when modal opens
watch(isOpen, (newValue) => {
  if (newValue) {
    state.url = ''
    state.title = ''
    errorMessage.value = ''
  }
})
</script>
