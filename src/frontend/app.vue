<template>
  <div class="min-h-screen bg-white dark:bg-gray-950 transition-colors duration-200">
    <!-- App Header -->
    <header class="sticky top-0 z-50 border-b border-gray-200 dark:border-gray-800 bg-white/80 dark:bg-gray-950/80 backdrop-blur-md">
      <UContainer>
        <div class="flex items-center justify-between h-16">
          <!-- Logo/Brand -->
          <NuxtLink to="/" class="flex items-center gap-3 hover:opacity-80 transition-opacity">
            <UIcon name="i-heroicons-newspaper" class="w-8 h-8 text-primary" />
            <span class="text-xl font-bold bg-gradient-to-r from-primary-500 to-primary-700 bg-clip-text text-transparent">
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
              size="sm"
              class="hidden sm:flex"
            >
              Feeds
            </UButton>
            <UButton
              to="/tags"
              icon="i-heroicons-tag"
              color="gray"
              variant="ghost"
              size="sm"
              class="hidden sm:flex"
            >
              Tags
            </UButton>
            
            <!-- Color Mode Toggle -->
            <ClientOnly>
              <UButton
                :icon="colorMode.value === 'dark' ? 'i-heroicons-moon' : 'i-heroicons-sun'"
                color="gray"
                variant="ghost"
                aria-label="Toggle color mode"
                @click="toggleColorMode"
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
    <footer class="mt-auto border-t border-gray-200 dark:border-gray-800 py-8">
      <UContainer>
        <div class="text-center text-sm text-gray-600 dark:text-gray-400">
          <p>Zeitung - Smart RSS Feed Reader with AI-powered recommendations</p>
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
