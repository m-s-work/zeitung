<template>
  <div class="min-h-screen">
    <UContainer class="py-6 md:py-10">
      <div class="space-y-6 md:space-y-8">
        <!-- Header -->
        <div class="text-center md:text-left space-y-3">
          <h1 class="text-3xl md:text-4xl font-bold bg-gradient-to-r from-gray-900 to-gray-700 dark:from-white dark:to-gray-300 bg-clip-text text-transparent">
            Tag Preferences
          </h1>
          <p class="text-base md:text-lg text-gray-600 dark:text-gray-400">
            Customize your interests to improve article recommendations
          </p>
        </div>

        <!-- User Tag Stats -->
        <UCard 
          v-if="!loadingUserTags && userTags.length > 0"
          class="shadow-lg border-0"
          :ui="{ 
            ring: 'ring-1 ring-gray-200 dark:ring-gray-800',
            background: 'bg-white dark:bg-gray-900'
          }"
        >
          <template #header>
            <div class="flex items-center gap-2">
              <UIcon name="i-heroicons-star" class="w-5 h-5 text-primary-500" />
              <h3 class="text-lg md:text-xl font-bold">Your Tag Interests</h3>
            </div>
          </template>

          <div class="grid grid-cols-1 md:grid-cols-2 gap-3">
            <div
              v-for="tag in userTags"
              :key="tag.tagId"
              class="flex items-center justify-between p-4 rounded-xl bg-gray-50 dark:bg-gray-800/50 hover:bg-gray-100 dark:hover:bg-gray-800 transition-colors"
            >
              <div class="flex-1 min-w-0">
                <div class="flex items-center gap-2 flex-wrap">
                  <UBadge
                    :color="getInteractionColor(tag.interactionType)"
                    variant="soft"
                    size="md"
                  >
                    <UIcon name="i-heroicons-hashtag" class="w-3 h-3 mr-0.5" />
                    {{ tag.tagName }}
                  </UBadge>
                  <span class="text-xs text-gray-500 dark:text-gray-400 font-medium">
                    {{ tag.interactionType }}
                  </span>
                </div>
                <div class="mt-2 flex items-center gap-3 text-xs text-gray-600 dark:text-gray-400">
                  <span class="flex items-center gap-1">
                    <UIcon name="i-heroicons-chart-bar" class="w-3 h-3" />
                    Score: {{ tag.score.toFixed(2) }}
                  </span>
                  <span class="flex items-center gap-1">
                    <UIcon name="i-heroicons-cursor-arrow-rays" class="w-3 h-3" />
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
                aria-label="Ignore tag"
              />
            </div>
          </div>
        </UCard>

        <!-- All Tags -->
        <UCard
          class="shadow-lg border-0"
          :ui="{ 
            ring: 'ring-1 ring-gray-200 dark:ring-gray-800',
            background: 'bg-white dark:bg-gray-900'
          }"
        >
          <template #header>
            <div class="flex flex-col md:flex-row md:items-center justify-between gap-4">
              <div class="flex items-center gap-2">
                <UIcon name="i-heroicons-hashtag" class="w-5 h-5 text-gray-500" />
                <h3 class="text-lg md:text-xl font-bold">All Tags</h3>
              </div>
              <UInput
                v-model="searchQuery"
                placeholder="Search tags..."
                icon="i-heroicons-magnifying-glass"
                size="md"
                class="md:w-64"
              />
            </div>
          </template>

          <div v-if="loadingTags" class="grid grid-cols-1 md:grid-cols-2 gap-3">
            <USkeleton v-for="i in 10" :key="i" class="h-16 rounded-xl" />
          </div>

          <div v-else-if="filteredTags.length > 0" class="space-y-3">
            <div class="grid grid-cols-1 md:grid-cols-2 gap-3">
              <div
                v-for="tag in paginatedTags"
                :key="tag.id"
                class="flex items-center justify-between p-4 rounded-xl hover:bg-gray-50 dark:hover:bg-gray-800/50 transition-colors border border-gray-200 dark:border-gray-800"
              >
                <div class="flex items-center gap-3 flex-1 min-w-0">
                  <UBadge color="gray" variant="subtle" size="md">
                    <UIcon name="i-heroicons-hashtag" class="w-3 h-3 mr-0.5" />
                    {{ tag.name }}
                  </UBadge>
                  <span class="text-xs text-gray-500 dark:text-gray-400">
                    {{ tag.usageCount }} articles
                  </span>
                </div>

                <div class="flex gap-2 shrink-0">
                  <UButton
                    icon="i-heroicons-heart"
                    size="sm"
                    :color="isTagInterested(tag.id) ? 'primary' : 'gray'"
                    :variant="isTagInterested(tag.id) ? 'soft' : 'ghost'"
                    @click="handleMarkInterested(tag.id)"
                    :loading="actionLoading === tag.id"
                    aria-label="Mark as interested"
                  />
                  <UButton
                    icon="i-heroicons-x-mark"
                    size="sm"
                    :color="isTagIgnored(tag.id) ? 'red' : 'gray'"
                    :variant="isTagIgnored(tag.id) ? 'soft' : 'ghost'"
                    @click="handleMarkIgnored(tag.id)"
                    :loading="actionLoading === tag.id"
                    aria-label="Ignore tag"
                  />
                </div>
              </div>
            </div>

            <!-- Pagination -->
            <div v-if="totalTagPages > 1" class="flex justify-center pt-4">
              <UPagination
                v-model="currentTagPage"
                :total="filteredTags.length"
                :page-count="tagsPerPage"
                :ui="{
                  rounded: 'rounded-full'
                }"
              />
            </div>
          </div>

          <div v-else class="text-center py-12 text-gray-500 dark:text-gray-400">
            <UIcon name="i-heroicons-magnifying-glass" class="w-12 h-12 mx-auto mb-3 opacity-50" />
            <p>No tags found</p>
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
