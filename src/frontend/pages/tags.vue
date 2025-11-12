<template>
  <div class="min-h-screen bg-gradient-to-br from-gray-50 via-white to-gray-50 dark:from-gray-950 dark:via-gray-900 dark:to-gray-950">
    <UContainer class="py-8">
      <div class="space-y-8">
        <!-- Header -->
        <div class="max-w-4xl mx-auto">
          <div class="flex items-center gap-3 mb-4">
            <UButton
              to="/"
              icon="i-heroicons-arrow-left"
              color="gray"
              variant="ghost"
              size="sm"
              class="text-gray-700 dark:text-gray-200"
            >
              <span class="hidden sm:inline">Back</span>
            </UButton>
          </div>
          <h1 class="text-3xl sm:text-4xl font-bold text-gray-900 dark:text-white">
            Tag Preferences
          </h1>
          <p class="mt-2 text-gray-600 dark:text-gray-400">
            Customize your interests to receive better article recommendations
          </p>
        </div>

        <!-- User Tag Stats -->
        <UCard v-if="!loadingUserTags && userTags.length > 0" class="max-w-4xl mx-auto shadow-lg" :ui="{ rounded: 'rounded-xl' }">
          <template #header>
            <div class="flex items-center gap-2">
              <UIcon name="i-heroicons-star" class="w-5 h-5 text-primary-500" />
              <h3 class="text-lg font-bold">Your Tag Interests</h3>
            </div>
          </template>

          <div class="space-y-3">
            <div
              v-for="tag in userTags"
              :key="tag.tagId"
              class="flex items-center justify-between p-4 rounded-lg bg-gray-50 dark:bg-gray-800/50 hover:bg-gray-100 dark:hover:bg-gray-800 transition-colors"
            >
              <div class="flex-1">
                <div class="flex items-center gap-3 flex-wrap">
                  <UBadge
                    :color="getInteractionColor(tag.interactionType)"
                    variant="soft"
                    size="md"
                  >
                    <UIcon name="i-heroicons-hashtag" class="w-3 h-3" />
                    {{ tag.tagName }}
                  </UBadge>
                  <span class="text-xs font-medium text-gray-600 dark:text-gray-400 uppercase tracking-wide">
                    {{ tag.interactionType }}
                  </span>
                </div>
                <div class="mt-2 flex items-center gap-4 text-xs text-gray-500 dark:text-gray-400">
                  <span class="flex items-center gap-1">
                    <UIcon name="i-heroicons-chart-bar" class="w-3 h-3" />
                    Score: {{ tag.score.toFixed(2) }}
                  </span>
                  <span class="flex items-center gap-1">
                    <UIcon name="i-heroicons-cursor-arrow-ripple" class="w-3 h-3" />
                    {{ tag.interactionCount }} interactions
                  </span>
                </div>
              </div>

              <UButton
                v-if="tag.interactionType !== 'Ignored'"
                icon="i-heroicons-x-mark"
                size="sm"
                color="red"
                variant="ghost"
                @click="handleIgnoreTag(tag.tagId)"
              />
            </div>
          </div>
        </UCard>

        <!-- All Tags -->
        <UCard class="max-w-4xl mx-auto shadow-lg" :ui="{ rounded: 'rounded-xl' }">
          <template #header>
            <div class="flex items-center justify-between gap-4 flex-wrap">
              <div class="flex items-center gap-2">
                <UIcon name="i-heroicons-tag" class="w-5 h-5 text-primary-500" />
                <h3 class="text-lg font-bold">All Tags</h3>
              </div>
              <UInput
                v-model="searchQuery"
                placeholder="Search tags..."
                icon="i-heroicons-magnifying-glass"
                size="md"
                class="w-full sm:w-64"
              />
            </div>
          </template>

          <div v-if="loadingTags" class="space-y-3">
            <USkeleton v-for="i in 10" :key="i" class="h-16 rounded-lg" />
          </div>

          <div v-else-if="filteredTags.length > 0" class="space-y-2">
            <div
              v-for="tag in paginatedTags"
              :key="tag.id"
              class="flex items-center justify-between p-4 rounded-lg hover:bg-gray-50 dark:hover:bg-gray-800/50 transition-all duration-200 group"
            >
              <div class="flex items-center gap-4">
                <UBadge color="gray" variant="soft" size="md">
                  <UIcon name="i-heroicons-hashtag" class="w-3 h-3" />
                  {{ tag.name }}
                </UBadge>
                <span class="text-sm text-gray-500 dark:text-gray-400">
                  {{ tag.usageCount }} articles
                </span>
              </div>

              <div class="flex gap-2">
                <UButton
                  icon="i-heroicons-heart"
                  size="sm"
                  :color="isTagInterested(tag.id) ? 'primary' : 'gray'"
                  :variant="isTagInterested(tag.id) ? 'soft' : 'ghost'"
                  @click="handleMarkInterested(tag.id)"
                  :loading="actionLoading === tag.id"
                >
                  <span class="hidden sm:inline">Interested</span>
                </UButton>
                <UButton
                  icon="i-heroicons-x-mark"
                  size="sm"
                  :color="isTagIgnored(tag.id) ? 'red' : 'gray'"
                  :variant="isTagIgnored(tag.id) ? 'soft' : 'ghost'"
                  @click="handleMarkIgnored(tag.id)"
                  :loading="actionLoading === tag.id"
                >
                  <span class="hidden sm:inline">Ignore</span>
                </UButton>
              </div>
            </div>

            <!-- Pagination -->
            <div v-if="totalTagPages > 1" class="flex justify-center pt-6">
              <UPagination
                v-model="currentTagPage"
                :total="filteredTags.length"
                :page-count="tagsPerPage"
                :ui="{ rounded: 'rounded-full' }"
              />
            </div>
          </div>

          <div v-else class="text-center py-12">
            <UIcon name="i-heroicons-magnifying-glass" class="w-12 h-12 mx-auto text-gray-400 mb-3" />
            <p class="text-gray-500 dark:text-gray-400">No tags found</p>
          </div>
        </UCard>
      </div>
    </UContainer>
  </div>
</template>

<script setup lang="ts">
import type { Tag, UserTag } from '~/types'

const { userId } = useUser()

const userTags = ref<UserTag[]>([])
const allTags = ref<Tag[]>([])
const loadingUserTags = ref(false)
const loadingTags = ref(false)
const actionLoading = ref<number | null>(null)

const searchQuery = ref('')
const currentTagPage = ref(1)
const tagsPerPage = ref(20)

const filteredTags = computed(() => {
  if (!searchQuery.value) return allTags.value
  
  const query = searchQuery.value.toLowerCase()
  return allTags.value.filter(tag => 
    tag.name.toLowerCase().includes(query)
  )
})

const paginatedTags = computed(() => {
  const start = (currentTagPage.value - 1) * tagsPerPage.value
  const end = start + tagsPerPage.value
  return filteredTags.value.slice(start, end)
})

const totalTagPages = computed(() => 
  Math.ceil(filteredTags.value.length / tagsPerPage.value)
)

const getInteractionColor = (type: string) => {
  switch (type) {
    case 'Explicit': return 'primary'
    case 'Ignored': return 'red'
    case 'Liked': return 'green'
    case 'Clicked': return 'blue'
    default: return 'gray'
  }
}

const isTagInterested = (tagId: number) => {
  return userTags.value.some(
    ut => ut.tagId === tagId && ut.interactionType === 'Explicit'
  )
}

const isTagIgnored = (tagId: number) => {
  return userTags.value.some(
    ut => ut.tagId === tagId && ut.interactionType === 'Ignored'
  )
}

// Fetch user's tag preferences
const fetchUserTags = async () => {
  loadingUserTags.value = true
  try {
    const { data, error } = await useZeitungApi('/api/tags/user', {
      query: { userId: userId.value },
    })

    if (!error.value && data.value) {
      userTags.value = (data.value as any) || []
    }
  } catch (e) {
    console.error('Failed to fetch user tags:', e)
  } finally {
    loadingUserTags.value = false
  }
}

// Fetch all tags
const fetchAllTags = async () => {
  loadingTags.value = true
  try {
    const { data, error } = await useZeitungApi('/api/tags', {
      query: {
        page: 1,
        pageSize: 500, // Get a large set of tags
      },
    })

    if (!error.value && data.value) {
      const response = data.value as any
      allTags.value = response.tags || []
    }
  } catch (e) {
    console.error('Failed to fetch tags:', e)
  } finally {
    loadingTags.value = false
  }
}

// Mark tags as interesting
const handleMarkInterested = async (tagId: number) => {
  actionLoading.value = tagId
  try {
    const { error } = await useZeitungApi('/api/tags/interest', {
      method: 'POST',
      query: { userId: userId.value },
      body: { tagIds: [tagId] },
    })

    if (!error.value) {
      await fetchUserTags()
    }
  } catch (e) {
    console.error('Failed to mark tag as interesting:', e)
  } finally {
    actionLoading.value = null
  }
}

// Mark tags as ignored
const handleMarkIgnored = async (tagId: number) => {
  actionLoading.value = tagId
  try {
    const { error } = await useZeitungApi('/api/tags/ignore', {
      method: 'POST',
      query: { userId: userId.value },
      body: { tagIds: [tagId] },
    })

    if (!error.value) {
      await fetchUserTags()
    }
  } catch (e) {
    console.error('Failed to mark tag as ignored:', e)
  } finally {
    actionLoading.value = null
  }
}

// Ignore a tag from user tags list
const handleIgnoreTag = async (tagId: number) => {
  await handleMarkIgnored(tagId)
}

// Watch search query changes
watch(searchQuery, () => {
  currentTagPage.value = 1
})

// Initial fetch
onMounted(() => {
  fetchUserTags()
  fetchAllTags()
})

useSeoMeta({
  title: 'Tag Preferences - Zeitung',
  description: 'Manage your tag interests and preferences',
})
</script>
