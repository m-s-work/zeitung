<template>
  <NuxtLayout>
    <div class="space-y-6">
      <!-- Header -->
      <div class="flex items-center justify-between">
        <div>
          <h1 class="text-3xl font-bold text-gray-900 dark:text-white">
            My Feeds
          </h1>
          <p class="text-gray-600 dark:text-gray-400 mt-1">
            Manage your RSS feed subscriptions
          </p>
        </div>
        <UButton
          icon="i-heroicons-plus"
          label="Add Feed"
          color="primary"
          @click="showAddModal = true"
        />
      </div>

      <!-- Loading State -->
      <div v-if="pending && !feeds.length" class="space-y-4">
        <USkeleton class="h-24" v-for="i in 3" :key="i" />
      </div>

      <!-- Error State -->
      <UAlert
        v-else-if="error"
        icon="i-heroicons-exclamation-triangle"
        color="red"
        variant="soft"
        title="Failed to load feeds"
        :description="error.message"
      />

      <!-- Feeds List -->
      <div v-else class="space-y-4">
        <!-- Global Feeds Section -->
        <div>
          <h2 class="text-xl font-semibold text-gray-900 dark:text-white mb-4">
            Global Feeds
          </h2>
          <div class="grid gap-4 md:grid-cols-2 lg:grid-cols-3">
            <FeedCard
              v-for="feed in globalFeeds"
              :key="feed.id"
              :feed="feed"
              :show-promote="false"
              @delete="onDeleteFeed"
            />
          </div>
        </div>

        <!-- Personal Feeds Section -->
        <div v-if="personalFeeds.length > 0">
          <h2 class="text-xl font-semibold text-gray-900 dark:text-white mb-4">
            My Personal Feeds
          </h2>
          <div class="grid gap-4 md:grid-cols-2 lg:grid-cols-3">
            <FeedCard
              v-for="feed in personalFeeds"
              :key="feed.id"
              :feed="feed"
              :show-promote="true"
              @promote="onPromoteFeed"
              @delete="onDeleteFeed"
            />
          </div>
        </div>

        <!-- Empty State for Personal Feeds -->
        <div v-else class="text-center py-12 border border-dashed border-gray-300 dark:border-gray-700 rounded-lg">
          <UIcon name="i-heroicons-rss" class="w-16 h-16 mx-auto text-gray-400 mb-4" />
          <h3 class="text-lg font-semibold text-gray-900 dark:text-white mb-2">
            No personal feeds yet
          </h3>
          <p class="text-gray-600 dark:text-gray-400 mb-4">
            Add your first RSS feed to get started
          </p>
          <UButton
            label="Add Feed"
            color="primary"
            @click="showAddModal = true"
          />
        </div>
      </div>

      <!-- Add Feed Modal -->
      <AddFeedModal
        v-model="showAddModal"
        @added="onFeedAdded"
      />
    </div>
  </NuxtLayout>
</template>

<script setup lang="ts">
// Fetch feeds
const { data: feedsData, pending, error, refresh: fetchFeeds } = await useFetch('/api/feeds')

const feeds = computed(() => (feedsData.value as any) || [])
const globalFeeds = computed(() => feeds.value.filter((f: any) => f.isGlobal))
const personalFeeds = computed(() => feeds.value.filter((f: any) => !f.isGlobal))

const showAddModal = ref(false)

const onFeedAdded = async () => {
  await fetchFeeds()
}

const onPromoteFeed = async (feedId: string) => {
  try {
    await $fetch(`/api/feeds/${feedId}/promote`, {
      method: 'POST'
    })
    await fetchFeeds()
  } catch (e) {
    console.error('Failed to promote feed', e)
  }
}

const onDeleteFeed = async (feedId: string) => {
  try {
    await $fetch(`/api/feeds/${feedId}`, {
      method: 'DELETE'
    })
    await fetchFeeds()
  } catch (e) {
    console.error('Failed to delete feed', e)
  }
}
</script>
