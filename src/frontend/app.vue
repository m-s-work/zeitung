<template>
  <div class="min-h-screen bg-gray-50 dark:bg-gray-950">
    <!-- Header Navigation -->
    <header class="sticky top-0 z-50 w-full border-b border-gray-200 dark:border-gray-800 bg-white/95 dark:bg-gray-900/95 backdrop-blur supports-[backdrop-filter]:bg-white/60 dark:supports-[backdrop-filter]:bg-gray-900/60">
      <UContainer>
        <nav class="flex items-center justify-between h-16">
          <!-- Logo and Brand -->
          <NuxtLink to="/" class="flex items-center gap-2 hover:opacity-80 transition-opacity">
            <Icon name="heroicons:newspaper" class="w-7 h-7 text-primary-500" />
            <span class="text-xl font-bold bg-gradient-to-r from-primary-500 to-primary-600 dark:from-primary-400 dark:to-primary-500 bg-clip-text text-transparent">
              Zeitung
            </span>
          </NuxtLink>

          <!-- Desktop Navigation -->
          <div class="hidden md:flex items-center gap-2">
            <UButton
              to="/"
              variant="ghost"
              color="gray"
              size="sm"
            >
              <Icon name="heroicons:newspaper-20-solid" class="w-4 h-4" />
              Articles
            </UButton>
            <UButton
              to="/feeds"
              variant="ghost"
              color="gray"
              size="sm"
            >
              <Icon name="heroicons:rss-20-solid" class="w-4 h-4" />
              Feeds
            </UButton>
            <UButton
              to="/tags"
              variant="ghost"
              color="gray"
              size="sm"
            >
              <Icon name="heroicons:tag-20-solid" class="w-4 h-4" />
              Tags
            </UButton>
            
            <div class="w-px h-6 bg-gray-300 dark:bg-gray-700 mx-2"></div>
            
            <!-- Color Mode Toggle -->
            <ClientOnly>
              <UButton
                :icon="isDark ? 'i-heroicons-moon-20-solid' : 'i-heroicons-sun-20-solid'"
                color="gray"
                variant="ghost"
                size="sm"
                @click="toggleColorMode"
                aria-label="Toggle color mode"
              />
            </ClientOnly>
          </div>

          <!-- Mobile Menu Button -->
          <div class="md:hidden flex items-center gap-2">
            <ClientOnly>
              <UButton
                :icon="isDark ? 'i-heroicons-moon-20-solid' : 'i-heroicons-sun-20-solid'"
                color="gray"
                variant="ghost"
                size="sm"
                @click="toggleColorMode"
                aria-label="Toggle color mode"
              />
            </ClientOnly>
            <UButton
              icon="i-heroicons-bars-3-20-solid"
              color="gray"
              variant="ghost"
              size="sm"
              @click="isMenuOpen = !isMenuOpen"
            />
          </div>
        </nav>

        <!-- Mobile Menu -->
        <div v-if="isMenuOpen" class="md:hidden pb-4 space-y-2">
          <UButton
            to="/"
            block
            variant="ghost"
            color="gray"
            @click="isMenuOpen = false"
          >
            <Icon name="heroicons:newspaper-20-solid" class="w-4 h-4" />
            Articles
          </UButton>
          <UButton
            to="/feeds"
            block
            variant="ghost"
            color="gray"
            @click="isMenuOpen = false"
          >
            <Icon name="heroicons:rss-20-solid" class="w-4 h-4" />
            Feeds
          </UButton>
          <UButton
            to="/tags"
            block
            variant="ghost"
            color="gray"
            @click="isMenuOpen = false"
          >
            <Icon name="heroicons:tag-20-solid" class="w-4 h-4" />
            Tags
          </UButton>
        </div>
      </UContainer>
    </header>

    <!-- Main Content -->
    <main>
      <NuxtPage />
    </main>

    <!-- Footer -->
    <footer class="border-t border-gray-200 dark:border-gray-800 mt-12">
      <UContainer class="py-8">
        <div class="flex flex-col md:flex-row items-center justify-between gap-4 text-sm text-gray-600 dark:text-gray-400">
          <div class="flex items-center gap-2">
            <Icon name="heroicons:newspaper" class="w-5 h-5" />
            <span class="font-medium">Zeitung RSS Reader</span>
          </div>
          <p>Smart RSS reader with AI-powered recommendations</p>
        </div>
      </UContainer>
    </footer>
  </div>
</template>

<script setup lang="ts">
const colorMode = useColorMode()
const isMenuOpen = ref(false)

const isDark = computed(() => colorMode.value === 'dark')

const toggleColorMode = () => {
  colorMode.preference = colorMode.value === 'dark' ? 'light' : 'dark'
}

// Set dark as default on mount
onMounted(() => {
  if (!colorMode.preference || colorMode.preference === 'system') {
    colorMode.preference = 'dark'
  }
})
</script>
