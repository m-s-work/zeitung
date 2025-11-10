<template>
  <UCard>
    <template #header>
      <div class="flex items-start justify-between">
        <div class="flex-1">
          <h3 class="text-lg font-semibold text-gray-900 dark:text-white">
            {{ feed.title }}
          </h3>
          <div class="flex items-center gap-2 mt-1">
            <UBadge
              v-if="feed.isGlobal"
              color="primary"
              variant="subtle"
              size="xs"
            >
              Global
            </UBadge>
            <UBadge
              v-else
              color="gray"
              variant="subtle"
              size="xs"
            >
              Personal
            </UBadge>
          </div>
        </div>
        <UDropdown :items="menuItems" :popper="{ placement: 'bottom-end' }">
          <UButton
            icon="i-heroicons-ellipsis-vertical"
            variant="ghost"
            color="gray"
            size="sm"
          />
        </UDropdown>
      </div>
    </template>

    <div class="space-y-3">
      <!-- URL -->
      <div class="text-sm">
        <a 
          :href="feed.url" 
          target="_blank"
          class="text-primary-600 hover:text-primary-700 dark:text-primary-400 dark:hover:text-primary-300 truncate block"
        >
          {{ feed.url }}
        </a>
      </div>

      <!-- Description -->
      <p v-if="feed.description" class="text-sm text-gray-600 dark:text-gray-400 line-clamp-2">
        {{ feed.description }}
      </p>

      <!-- Last Fetched -->
      <div v-if="feed.lastFetchedAt" class="text-xs text-gray-500 dark:text-gray-500">
        Last updated: {{ formatDate(feed.lastFetchedAt) }}
      </div>
    </div>
  </UCard>
</template>

<script setup lang="ts">
interface Feed {
  id: string
  title: string
  url: string
  description?: string
  isGlobal: boolean
  lastFetchedAt?: string
}

interface Props {
  feed: Feed
  showPromote?: boolean
}

const props = defineProps<Props>()
const emit = defineEmits<{
  (e: 'promote', feedId: string): void
  (e: 'delete', feedId: string): void
}>()

const menuItems = computed(() => {
  const items = [[
    {
      label: 'Open Feed URL',
      icon: 'i-heroicons-arrow-top-right-on-square',
      click: () => window.open(props.feed.url, '_blank')
    }
  ]]

  if (props.showPromote && !props.feed.isGlobal) {
    items[0].push({
      label: 'Promote to Global',
      icon: 'i-heroicons-arrow-up-circle',
      click: () => emit('promote', props.feed.id)
    })
  }

  if (!props.feed.isGlobal) {
    items.push([{
      label: 'Delete',
      icon: 'i-heroicons-trash',
      click: () => emit('delete', props.feed.id)
    }])
  }

  return items
})

const formatDate = (dateStr: string) => {
  const date = new Date(dateStr)
  const now = new Date()
  const diffMs = now.getTime() - date.getTime()
  const diffMins = Math.floor(diffMs / 60000)
  
  if (diffMins < 60) return `${diffMins} minute${diffMins !== 1 ? 's' : ''} ago`
  
  const diffHours = Math.floor(diffMins / 60)
  if (diffHours < 24) return `${diffHours} hour${diffHours !== 1 ? 's' : ''} ago`
  
  const diffDays = Math.floor(diffHours / 24)
  return `${diffDays} day${diffDays !== 1 ? 's' : ''} ago`
}
</script>
