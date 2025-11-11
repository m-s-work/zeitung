<template>
  <div>
    <UContainer class="py-6 md:py-8">
      <div class="space-y-6">
        <!-- Header -->
        <div class="flex items-center justify-between">
          <div>
            <h1 class="text-2xl md:text-3xl font-bold text-gray-900 dark:text-white">
              Feed Management
            </h1>
            <p class="mt-1 text-sm md:text-base text-gray-600 dark:text-gray-400">
              Manage your RSS feed subscriptions
            </p>
          </div>

          <UButton
            icon="i-heroicons-plus"
            color="primary"
            size="lg"
            @click="showAddModal = true"
            class="shadow-md"
          >
            <span class="hidden sm:inline">Add Feed</span>
          </UButton>
        </div>

        <!-- Tabs -->
        <UTabs v-model="activeTab" :items="tabs">
          <!-- My Feeds -->
          <template #feeds>
            <div v-if="loadingFeeds" class="space-y-4 pt-4">
              <USkeleton v-for="i in 3" :key="i" class="h-32" />
            </div>

            <div v-else-if="myFeeds.length > 0" class="space-y-4 pt-4">
              <FeedCard
                v-for="feed in myFeeds"
                :key="feed.id"
                :feed="feed"
                :can-promote="false"
                :loading="actionLoading === feed.id"
                @unsubscribe="handleUnsubscribe"
                @delete="handleDelete"
              />
            </div>

            <UCard v-else class="mt-4">
              <div class="text-center py-12">
                <div class="w-20 h-20 mx-auto mb-4 rounded-full bg-gradient-to-br from-primary-100 to-primary-200 dark:from-primary-900 dark:to-primary-800 flex items-center justify-center">
                  <UIcon name="i-heroicons-rss" class="w-10 h-10 text-primary-600 dark:text-primary-400" />
                </div>
                <h3 class="text-lg font-semibold text-gray-900 dark:text-white mb-2">
                  No feeds yet
                </h3>
                <p class="text-gray-600 dark:text-gray-400 mb-4">
                  Start by adding your first feed
                </p>
                <UButton color="primary" @click="showAddModal = true" icon="i-heroicons-plus">
                  Add Feed
                </UButton>
              </div>
            </UCard>
          </template>

          <!-- Recommendations -->
          <template #recommendations>
            <div v-if="loadingRecommendations" class="space-y-4 pt-4">
              <USkeleton v-for="i in 3" :key="i" class="h-32" />
            </div>

            <div v-else-if="recommendations.length > 0" class="space-y-4 pt-4">
              <FeedCard
                v-for="feed in recommendations"
                :key="feed.id"
                :feed="feed"
                :is-recommendation="true"
                :loading="actionLoading === feed.id"
                @subscribe="handleSubscribe"
              />
            </div>

            <UCard v-else class="mt-4">
              <div class="text-center py-12">
                <div class="w-20 h-20 mx-auto mb-4 rounded-full bg-gradient-to-br from-amber-100 to-amber-200 dark:from-amber-900 dark:to-amber-800 flex items-center justify-center">
                  <UIcon name="i-heroicons-light-bulb" class="w-10 h-10 text-amber-600 dark:text-amber-400" />
                </div>
                <h3 class="text-lg font-semibold text-gray-900 dark:text-white mb-2">
                  No recommendations yet
                </h3>
                <p class="text-gray-600 dark:text-gray-400">
                  Add some feeds and like articles to get personalized recommendations
                </p>
              </div>
            </UCard>
          </template>
        </UTabs>
      </div>

      <!-- Add Feed Modal -->
      <UModal v-model="showAddModal">
        <UCard>
          <template #header>
            <h3 class="text-lg font-semibold">Add New Feed</h3>
          </template>

          <div class="space-y-4">
            <UFormGroup label="Feed URL" required>
              <UInput
                v-model="newFeed.url"
                placeholder="https://example.com/feed.xml"
                type="url"
              />
            </UFormGroup>

            <UFormGroup label="Name" required>
              <UInput
                v-model="newFeed.name"
                placeholder="My Awesome Blog"
              />
            </UFormGroup>

            <UFormGroup label="Description">
              <UTextarea
                v-model="newFeed.description"
                placeholder="Optional description"
                :rows="3"
              />
            </UFormGroup>
          </div>

          <template #footer>
            <div class="flex justify-end gap-2">
              <UButton color="gray" variant="ghost" @click="showAddModal = false">
                Cancel
              </UButton>
              <UButton
                color="primary"
                @click="handleAddFeed"
                :loading="adding"
                :disabled="!newFeed.url || !newFeed.name"
              >
                Add Feed
              </UButton>
            </div>
          </template>
        </UCard>
      </UModal>
    </UContainer>
  </div>
</template>

<script setup lang="ts">
import type { Feed, FeedRecommendation } from '~/types'

const { userId } = useUser()

const activeTab = ref(0)
const tabs = [
  { label: 'My Feeds', slot: 'feeds' },
  { label: 'Recommendations', slot: 'recommendations' },
]

const myFeeds = ref<Feed[]>([])
const recommendations = ref<FeedRecommendation[]>([])
const loadingFeeds = ref(false)
const loadingRecommendations = ref(false)
const actionLoading = ref<number | null>(null)

const showAddModal = ref(false)
const adding = ref(false)
const newFeed = ref({
  url: '',
  name: '',
  description: '',
})

// Fetch user's feeds
const fetchMyFeeds = async () => {
  loadingFeeds.value = true
  try {
    const { data, error } = await useZeitungApi('/api/feeds', {
      query: { userId: userId.value },
    })

    if (!error.value && data.value) {
      myFeeds.value = (data.value as any) || []
    }
  } catch (e) {
    console.error('Failed to fetch feeds:', e)
  } finally {
    loadingFeeds.value = false
  }
}

// Fetch feed recommendations
const fetchRecommendations = async () => {
  loadingRecommendations.value = true
  try {
    const { data, error } = await useZeitungApi('/api/feeds/recommendations', {
      query: { userId: userId.value },
    })

    if (!error.value && data.value) {
      recommendations.value = (data.value as any) || []
    }
  } catch (e) {
    console.error('Failed to fetch recommendations:', e)
  } finally {
    loadingRecommendations.value = false
  }
}

// Add new feed
const handleAddFeed = async () => {
  adding.value = true
  try {
    const { error } = await useZeitungApi('/api/feeds', {
      method: 'POST',
      query: { userId: userId.value },
      body: newFeed.value,
    })

    if (!error.value) {
      showAddModal.value = false
      newFeed.value = { url: '', name: '', description: '' }
      await fetchMyFeeds()
    }
  } catch (e) {
    console.error('Failed to add feed:', e)
  } finally {
    adding.value = false
  }
}

// Subscribe to a feed
const handleSubscribe = async (feedId: number) => {
  actionLoading.value = feedId
  try {
    // For now, we'll just add it as a personal feed
    // This might need adjustment based on actual API structure
    await fetchMyFeeds()
  } catch (e) {
    console.error('Failed to subscribe:', e)
  } finally {
    actionLoading.value = null
  }
}

// Unsubscribe from a feed
const handleUnsubscribe = async (feedId: number) => {
  actionLoading.value = feedId
  try {
    // Implementation depends on API structure
    // For approved feeds, just unsubscribe; for personal feeds, delete
    const feed = myFeeds.value.find(f => f.id === feedId)
    if (feed && !feed.isApproved) {
      await handleDelete(feedId)
    }
  } catch (e) {
    console.error('Failed to unsubscribe:', e)
  } finally {
    actionLoading.value = null
  }
}

// Delete a feed
const handleDelete = async (feedId: number) => {
  actionLoading.value = feedId
  try {
    const { error } = await useZeitungApi('/api/feeds/{id}', {
      method: 'DELETE',
      path: { id: feedId },
      query: { userId: userId.value },
    })

    if (!error.value) {
      await fetchMyFeeds()
    }
  } catch (e) {
    console.error('Failed to delete feed:', e)
  } finally {
    actionLoading.value = null
  }
}

// Watch tab changes
watch(activeTab, (newTab) => {
  if (newTab === 1 && recommendations.value.length === 0) {
    fetchRecommendations()
  }
})

// Initial fetch
onMounted(() => {
  fetchMyFeeds()
})

useSeoMeta({
  title: 'Feed Management - Zeitung',
  description: 'Manage your RSS feed subscriptions',
})
</script>
