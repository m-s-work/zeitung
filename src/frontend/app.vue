<template>
  <div class="min-h-screen bg-gradient-to-br from-blue-50 via-indigo-50 to-purple-50 dark:from-gray-950 dark:via-blue-950 dark:to-indigo-950">
    <!-- App Header -->
    <header class="sticky top-0 z-50 border-b border-blue-200/50 dark:border-blue-900/50 bg-white/90 dark:bg-gray-900/90 backdrop-blur-xl shadow-sm">
      <UContainer>
        <div class="flex items-center justify-between h-16">
          <!-- Logo/Brand -->
          <NuxtLink to="/" class="flex items-center gap-3 hover:scale-105 transition-transform">
            <div class="w-10 h-10 bg-gradient-to-br from-blue-500 to-purple-600 rounded-xl flex items-center justify-center shadow-lg">
              <UIcon name="i-heroicons-newspaper" class="w-6 h-6 text-white" />
            </div>
            <span class="text-2xl font-black bg-gradient-to-r from-blue-600 via-purple-600 to-pink-600 bg-clip-text text-transparent">
              Zeitung
            </span>
          </NuxtLink>

          <!-- Actions -->
          <div class="flex items-center gap-3">
            <UButton
              to="/feeds"
              icon="i-heroicons-rss"
              color="blue"
              variant="soft"
              size="md"
              class="hidden sm:flex font-semibold"
            >
              Feeds
            </UButton>
            <UButton
              to="/tags"
              icon="i-heroicons-tag"
              color="purple"
              variant="soft"
              size="md"
              class="hidden sm:flex font-semibold"
            >
              Tags
            </UButton>
            
            <!-- Color Mode Toggle -->
            <ClientOnly>
              <UButton
                :icon="colorMode.value === 'dark' ? 'i-heroicons-moon' : 'i-heroicons-sun'"
                color="gray"
                variant="soft"
                size="md"
                aria-label="Toggle color mode"
                @click="toggleColorMode"
                class="w-10 h-10"
              />
            </ClientOnly>
          </div>
        </div>
      </UContainer>
    </header>

    <!-- Main Content -->
    <main>
      <NuxtPage />
    </main>

    <!-- Footer -->
    <footer class="mt-16 border-t border-blue-200/50 dark:border-blue-900/50 bg-white/50 dark:bg-gray-900/50 backdrop-blur-lg py-12">
      <UContainer>
        <div class="text-center space-y-4">
          <div class="flex items-center justify-center gap-2">
            <div class="w-8 h-8 bg-gradient-to-br from-blue-500 to-purple-600 rounded-lg flex items-center justify-center">
              <UIcon name="i-heroicons-newspaper" class="w-5 h-5 text-white" />
            </div>
            <span class="text-lg font-bold text-gray-900 dark:text-white">Zeitung</span>
          </div>
          <p class="text-gray-600 dark:text-gray-400">
            Smart RSS Feed Reader with AI-powered recommendations
          </p>
          <div class="flex items-center justify-center gap-4 text-sm text-gray-500 dark:text-gray-500">
            <span>Made with ❤️</span>
            <span>•</span>
            <span>Open Source</span>
          </div>
        </div>
      </UContainer>
    </footer>
  </div>
</template>

<script setup lang="ts">
const colorMode = useColorMode()

const toggleColorMode = () => {
  colorMode.preference = colorMode.value === 'dark' ? 'light' : 'dark'
}
</script>
