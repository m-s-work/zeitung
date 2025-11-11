<template>
  <div class="min-h-screen bg-gray-50 dark:bg-gray-950">
    <!-- Header Navigation -->
    <header class="sticky top-0 z-50 border-b border-gray-200 dark:border-gray-800 bg-white/80 dark:bg-gray-900/80 backdrop-blur-lg">
      <UContainer>
        <div class="flex items-center justify-between h-16">
          <!-- Logo and Brand -->
          <NuxtLink to="/" class="flex items-center gap-3 hover:opacity-80 transition-opacity">
            <UIcon name="i-heroicons-newspaper" class="w-8 h-8 text-primary" />
            <div class="hidden sm:block">
              <h1 class="text-xl font-bold text-gray-900 dark:text-white">Zeitung</h1>
              <p class="text-xs text-gray-500 dark:text-gray-400">Smart RSS Reader</p>
            </div>
          </NuxtLink>

          <!-- Desktop Navigation -->
          <nav class="hidden md:flex items-center gap-1">
            <UButton
              to="/"
              :variant="route.path === '/' ? 'soft' : 'ghost'"
              color="gray"
              icon="i-heroicons-home"
            >
              Articles
            </UButton>
            <UButton
              to="/feeds"
              :variant="route.path === '/feeds' ? 'soft' : 'ghost'"
              color="gray"
              icon="i-heroicons-rss"
            >
              Feeds
            </UButton>
            <UButton
              to="/tags"
              :variant="route.path === '/tags' ? 'soft' : 'ghost'"
              color="gray"
              icon="i-heroicons-tag"
            >
              Tags
            </UButton>
          </nav>

          <!-- Actions -->
          <div class="flex items-center gap-2">
            <!-- Theme Toggle -->
            <UButton
              :icon="isDark ? 'i-heroicons-moon' : 'i-heroicons-sun'"
              color="gray"
              variant="ghost"
              aria-label="Toggle theme"
              @click="toggleTheme"
            />

            <!-- Mobile Menu Toggle -->
            <UButton
              class="md:hidden"
              :icon="mobileMenuOpen ? 'i-heroicons-x-mark' : 'i-heroicons-bars-3'"
              color="gray"
              variant="ghost"
              aria-label="Toggle menu"
              @click="mobileMenuOpen = !mobileMenuOpen"
            />
          </div>
        </div>

        <!-- Mobile Navigation -->
        <Transition
          enter-active-class="transition duration-200 ease-out"
          enter-from-class="opacity-0 -translate-y-2"
          enter-to-class="opacity-100 translate-y-0"
          leave-active-class="transition duration-150 ease-in"
          leave-from-class="opacity-100 translate-y-0"
          leave-to-class="opacity-0 -translate-y-2"
        >
          <nav v-if="mobileMenuOpen" class="md:hidden py-4 space-y-1 border-t border-gray-200 dark:border-gray-800">
            <UButton
              to="/"
              :variant="route.path === '/' ? 'soft' : 'ghost'"
              color="gray"
              icon="i-heroicons-home"
              block
              @click="mobileMenuOpen = false"
            >
              Articles
            </UButton>
            <UButton
              to="/feeds"
              :variant="route.path === '/feeds' ? 'soft' : 'ghost'"
              color="gray"
              icon="i-heroicons-rss"
              block
              @click="mobileMenuOpen = false"
            >
              Feeds
            </UButton>
            <UButton
              to="/tags"
              :variant="route.path === '/tags' ? 'soft' : 'ghost'"
              color="gray"
              icon="i-heroicons-tag"
              block
              @click="mobileMenuOpen = false"
            >
              Tags
            </UButton>
          </nav>
        </Transition>
      </UContainer>
    </header>

    <!-- Main Content -->
    <main>
      <slot />
    </main>

    <!-- Footer -->
    <footer class="mt-auto border-t border-gray-200 dark:border-gray-800 bg-white dark:bg-gray-900">
      <UContainer>
        <div class="py-8 text-center text-sm text-gray-600 dark:text-gray-400">
          <p>Zeitung RSS Reader - AI-Powered Personalized News</p>
        </div>
      </UContainer>
    </footer>
  </div>
</template>

<script setup lang="ts">
const route = useRoute()
const colorMode = useColorMode()
const mobileMenuOpen = ref(false)

const isDark = computed({
  get() {
    return colorMode.value === 'dark'
  },
  set() {
    colorMode.preference = colorMode.value === 'dark' ? 'light' : 'dark'
  }
})

const toggleTheme = () => {
  colorMode.preference = colorMode.value === 'dark' ? 'light' : 'dark'
}

// Close mobile menu on route change
watch(() => route.path, () => {
  mobileMenuOpen.value = false
})
</script>
