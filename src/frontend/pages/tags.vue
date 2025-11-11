<template>
  <div>
    <UContainer class="py-6 md:py-8">
      <div class="space-y-6">
        <!-- Header -->
        <div>
          <h1 class="text-2xl md:text-3xl font-bold text-gray-900 dark:text-white">
            Tag Preferences
          </h1>
          <p class="mt-1 text-sm md:text-base text-gray-600 dark:text-gray-400">
            Manage your interests and improve recommendations
          </p>
        </div>

        <!-- User Tag Stats -->
        <UCard v-if="!loadingUserTags && userTags.length > 0" :ui="{ 
          ring: 'ring-1 ring-gray-200 dark:ring-gray-800',
          rounded: 'rounded-xl',
          shadow: 'shadow-sm'
        }">
          <template #header>
            <div class="flex items-center gap-2">
              <UIcon name="i-heroicons-heart" class="w-5 h-5 text-primary-600" />
              <h3 class="text-lg font-semibold">Your Tag Interests</h3>
            </div>
          </template>

          <div class="space-y-3">
            <div
              v-for="tag in userTags"
              :key="tag.tagId"
              class="flex items-center justify-between p-3 rounded-lg bg-gradient-to-r from-gray-50 to-transparent dark:from-gray-800 dark:to-transparent border border-gray-200 dark:border-gray-700"
            >
              <div class="flex-1 min-w-0">
                <div class="flex items-center gap-2 mb-1">
                  <UBadge
                    :color="getInteractionColor(tag.interactionType)"
                    variant="soft"
                    size="sm"
                  >
                    <UIcon name="i-heroicons-hashtag" class="w-3 h-3 -ml-1" />
                    {{ tag.tagName }}
                  </UBadge>
                  <span class="text-xs text-gray-500 capitalize">
                    {{ tag.interactionType }}
                  </span>
                </div>
                <div class="text-xs text-gray-600 dark:text-gray-400">
                  Score: {{ tag.score.toFixed(2) }} • 
                  Interactions: {{ tag.interactionCount }}
                </div>
              </div>

              <UButton
                v-if="tag.interactionType !== 'Ignored'"
                icon="i-heroicons-x-mark"
                size="xs"
                color="red"
                variant="ghost"
                @click="handleIgnoreTag(tag.tagId)"
                square
              />
            </div>
          </div>
        </UCard>

        <!-- All Tags -->
        <UCard :ui="{ 
          ring: 'ring-1 ring-gray-200 dark:ring-gray-800',
          rounded: 'rounded-xl',
          shadow: 'shadow-sm'
        }">
          <template #header>
            <div class="flex flex-col sm:flex-row sm:items-center justify-between gap-3">
              <div class="flex items-center gap-2">
                <UIcon name="i-heroicons-tag" class="w-5 h-5 text-primary-600" />
                <h3 class="text-lg font-semibold">All Tags</h3>
              </div>
              <UInput
                v-model="searchQuery"
                placeholder="Search tags..."
                icon="i-heroicons-magnifying-glass"
                size="sm"
                class="sm:w-64"
              />
            </div>
          </template>

          <div v-if="loadingTags" class="space-y-2">
            <USkeleton v-for="i in 10" :key="i" class="h-10" />
          </div>

          <div v-else-if="filteredTags.length > 0" class="space-y-2">
            <div
              v-for="tag in paginatedTags"
              :key="tag.id"
              class="flex items-center justify-between p-3 rounded-lg hover:bg-gray-50 dark:hover:bg-gray-800/50 transition-colors border border-transparent hover:border-gray-200 dark:hover:border-gray-700"
            >
              <div class="flex items-center gap-3 flex-1 min-w-0">
                <UBadge color="gray" variant="soft" size="sm">
                  <UIcon name="i-heroicons-hashtag" class="w-3 h-3 -ml-1" />
                  {{ tag.name }}
                </UBadge>
                <span class="text-xs text-gray-500 flex items-center gap-1">
                  <UIcon name="i-heroicons-document-text" class="w-3 h-3" />
                  {{ tag.usageCount }}
                </span>
              </div>

              <div class="flex gap-2 flex-shrink-0">
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
            <div v-if="totalTagPages > 1" class="flex justify-center pt-4">
              <UPagination
                v-model="currentTagPage"
                :total="filteredTags.length"
                :page-count="tagsPerPage"
              />
            </div>
          </div>

          <div v-else class="text-center py-8 text-gray-500">
            No tags found
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
