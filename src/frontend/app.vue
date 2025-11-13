<template>
  <div class="min-h-screen bg-gray-50 dark:bg-gray-900">
    <!-- App Header -->
    <header class="sticky top-0 z-50 border-b border-gray-200 dark:border-gray-700 bg-white/95 dark:bg-gray-800/95 backdrop-blur-sm">
      <div class="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
        <div class="flex items-center justify-between h-16">
          <!-- Logo/Brand -->
          <NuxtLink to="/" class="flex items-center gap-3 hover:opacity-80 transition-opacity">
            <div class="w-9 h-9 bg-blue-600 dark:bg-blue-400 rounded-lg flex items-center justify-center">
              <UIcon name="i-heroicons-newspaper" class="w-5 h-5 text-white" />
            </div>
            <span class="text-xl font-bold text-gray-900 dark:text-gray-100">
              Zeitung
            </span>
          </NuxtLink>

          <!-- Actions -->
          <div class="flex items-center gap-2">
            <UButton
              to="/feeds"
              icon="i-heroicons-rss"
              color="gray"
              variant="ghost"
              size="md"
              class="hidden sm:flex text-gray-700 dark:text-gray-300"
            >
              Feeds
            </UButton>
            <UButton
              to="/tags"
              icon="i-heroicons-tag"
              color="gray"
              variant="ghost"
              size="md"
              class="hidden sm:flex text-gray-700 dark:text-gray-300"
            >
              Tags
            </UButton>
            
            <!-- Color Mode Toggle -->
            <ClientOnly>
              <UButton
                :icon="isDark ? 'i-heroicons-moon' : 'i-heroicons-sun'"
                color="gray"
                variant="ghost"
                size="md"
                aria-label="Toggle color mode"
                class="text-gray-700 dark:text-gray-300"
                @click="toggleColorMode"
              />
            </ClientOnly>
          </div>
        </div>
      </div>
    </header>

    <!-- Main Content -->
    <main>
      <NuxtPage />
    </main>

    <!-- Footer -->
    <footer class="mt-16 border-t border-gray-200 dark:border-gray-700 py-8 bg-white dark:bg-gray-800">
      <div class="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
        <div class="text-center space-y-2">
          <div class="flex items-center justify-center gap-2">
            <UIcon name="i-heroicons-newspaper" class="w-5 h-5 text-gray-500 dark:text-gray-400" />
            <span class="font-semibold text-gray-900 dark:text-gray-100">Zeitung</span>
          </div>
          <p class="text-sm text-gray-600 dark:text-gray-400">
            Smart RSS Feed Reader with AI-powered recommendations
          </p>
        </div>
      </div>
    </footer>
  </div>
</template>

<script setup lang="ts">
const colorMode = useColorMode()

const isDark = computed(() => colorMode.value === 'dark')

const toggleColorMode = () => {
  colorMode.preference = colorMode.value === 'dark' ? 'light' : 'dark'
}
</script>
