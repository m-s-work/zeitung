<template>
  <div class="min-h-screen bg-gradient-to-br from-gray-50 to-blue-50 dark:from-gray-900 dark:to-gray-800">
    <!-- Mobile Header -->
    <header class="sticky top-0 z-40 bg-white/80 dark:bg-gray-900/80 backdrop-blur-lg border-b border-gray-200 dark:border-gray-800">
      <UContainer>
        <div class="flex items-center justify-between h-16">
          <!-- Logo/Brand -->
          <NuxtLink to="/" class="flex items-center gap-3 font-bold text-xl text-gray-900 dark:text-white hover:text-primary-600 transition-colors">
            <UIcon name="i-heroicons-newspaper" class="w-8 h-8 text-primary-600" />
            <span class="hidden sm:block">Zeitung</span>
          </NuxtLink>

          <!-- Desktop Navigation -->
          <nav class="hidden md:flex items-center gap-2">
            <UButton
              to="/"
              :variant="route.path === '/' ? 'soft' : 'ghost'"
              color="gray"
              icon="i-heroicons-home"
              size="sm"
            >
              Feed
            </UButton>
            <UButton
              to="/feeds"
              :variant="route.path === '/feeds' ? 'soft' : 'ghost'"
              color="gray"
              icon="i-heroicons-rss"
              size="sm"
            >
              Manage Feeds
            </UButton>
            <UButton
              to="/tags"
              :variant="route.path === '/tags' ? 'soft' : 'ghost'"
              color="gray"
              icon="i-heroicons-tag"
              size="sm"
            >
              Tags
            </UButton>
          </nav>

          <!-- Mobile Menu Button -->
          <UButton
            class="md:hidden"
            icon="i-heroicons-bars-3"
            color="gray"
            variant="ghost"
            @click="mobileMenuOpen = true"
          />

          <!-- Theme Toggle (Desktop) -->
          <UButton
            class="hidden md:block"
            :icon="isDark ? 'i-heroicons-moon' : 'i-heroicons-sun'"
            color="gray"
            variant="ghost"
            @click="toggleTheme"
          />
        </div>
      </UContainer>
    </header>

    <!-- Mobile Menu Sidebar -->
    <USlideover v-model="mobileMenuOpen" side="right">
      <UCard class="flex flex-col h-full" :ui="{ body: { padding: '' }, ring: '', divide: 'divide-y divide-gray-100 dark:divide-gray-800' }">
        <template #header>
          <div class="flex items-center justify-between">
            <h2 class="text-lg font-semibold">Menu</h2>
            <UButton
              color="gray"
              variant="ghost"
              icon="i-heroicons-x-mark"
              @click="mobileMenuOpen = false"
            />
          </div>
        </template>

        <nav class="flex flex-col p-4 space-y-2">
          <UButton
            to="/"
            :variant="route.path === '/' ? 'soft' : 'ghost'"
            color="gray"
            icon="i-heroicons-home"
            size="lg"
            block
            @click="mobileMenuOpen = false"
          >
            Feed
          </UButton>
          <UButton
            to="/feeds"
            :variant="route.path === '/feeds' ? 'soft' : 'ghost'"
            color="gray"
            icon="i-heroicons-rss"
            size="lg"
            block
            @click="mobileMenuOpen = false"
          >
            Manage Feeds
          </UButton>
          <UButton
            to="/tags"
            :variant="route.path === '/tags' ? 'soft' : 'ghost'"
            color="gray"
            icon="i-heroicons-tag"
            size="lg"
            block
            @click="mobileMenuOpen = false"
          >
            Tag Preferences
          </UButton>
          
          <div class="pt-4 border-t border-gray-200 dark:border-gray-700">
            <UButton
              :icon="isDark ? 'i-heroicons-moon' : 'i-heroicons-sun'"
              color="gray"
              variant="ghost"
              size="lg"
              block
              @click="toggleTheme"
            >
              {{ isDark ? 'Dark' : 'Light' }} Mode
            </UButton>
          </div>
        </nav>
      </UCard>
    </USlideover>

    <!-- Main Content -->
    <main class="pb-16">
      <slot />
    </main>

    <!-- Mobile Bottom Navigation -->
    <nav class="md:hidden fixed bottom-0 left-0 right-0 z-30 bg-white dark:bg-gray-900 border-t border-gray-200 dark:border-gray-800 shadow-lg">
      <div class="flex items-center justify-around h-16 px-4">
        <NuxtLink
          to="/"
          class="flex flex-col items-center gap-1 px-4 py-2 rounded-lg transition-colors"
          :class="route.path === '/' ? 'text-primary-600 bg-primary-50 dark:bg-primary-950' : 'text-gray-600 dark:text-gray-400 hover:text-gray-900 dark:hover:text-white'"
        >
          <UIcon name="i-heroicons-home" class="w-6 h-6" />
          <span class="text-xs font-medium">Feed</span>
        </NuxtLink>
        
        <NuxtLink
          to="/feeds"
          class="flex flex-col items-center gap-1 px-4 py-2 rounded-lg transition-colors"
          :class="route.path === '/feeds' ? 'text-primary-600 bg-primary-50 dark:bg-primary-950' : 'text-gray-600 dark:text-gray-400 hover:text-gray-900 dark:hover:text-white'"
        >
          <UIcon name="i-heroicons-rss" class="w-6 h-6" />
          <span class="text-xs font-medium">Feeds</span>
        </NuxtLink>
        
        <NuxtLink
          to="/tags"
          class="flex flex-col items-center gap-1 px-4 py-2 rounded-lg transition-colors"
          :class="route.path === '/tags' ? 'text-primary-600 bg-primary-50 dark:bg-primary-950' : 'text-gray-600 dark:text-gray-400 hover:text-gray-900 dark:hover:text-white'"
        >
          <UIcon name="i-heroicons-tag" class="w-6 h-6" />
          <span class="text-xs font-medium">Tags</span>
        </NuxtLink>
      </div>
    </nav>
  </div>
</template>

<script setup lang="ts">
const route = useRoute()
const colorMode = useColorMode()
const mobileMenuOpen = ref(false)

const isDark = computed(() => colorMode.value === 'dark')

const toggleTheme = () => {
  colorMode.preference = isDark.value ? 'light' : 'dark'
}
</script>
