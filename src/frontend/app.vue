<template>
  <div class="min-h-screen bg-gray-50 dark:bg-gray-950">
    <!-- Header Navigation -->
    <header class="sticky top-0 z-50 w-full border-b border-gray-200 dark:border-gray-800 bg-white/80 dark:bg-gray-900/80 backdrop-blur-lg">
      <UContainer class="py-3">
        <nav class="flex items-center justify-between">
          <!-- Logo and Brand -->
          <NuxtLink to="/" class="flex items-center gap-2 hover:opacity-80 transition-opacity">
            <UIcon name="i-heroicons-newspaper" class="w-8 h-8 text-primary" />
            <span class="text-xl font-bold bg-gradient-to-r from-primary-500 to-primary-600 bg-clip-text text-transparent">
              Zeitung
            </span>
          </NuxtLink>

          <!-- Desktop Navigation -->
          <div class="hidden md:flex items-center gap-4">
            <UButton
              to="/"
              variant="ghost"
              color="gray"
              :class="{ 'bg-gray-100 dark:bg-gray-800': $route.path === '/' }"
            >
              Articles
            </UButton>
            <UButton
              to="/feeds"
              variant="ghost"
              color="gray"
              :class="{ 'bg-gray-100 dark:bg-gray-800': $route.path === '/feeds' }"
            >
              Feeds
            </UButton>
            <UButton
              to="/tags"
              variant="ghost"
              color="gray"
              :class="{ 'bg-gray-100 dark:bg-gray-800': $route.path === '/tags' }"
            >
              Tags
            </UButton>
            
            <UDivider orientation="vertical" class="h-6" />
            
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

          <!-- Mobile Menu Button -->
          <div class="md:hidden flex items-center gap-2">
            <ClientOnly>
              <UButton
                :icon="colorMode.value === 'dark' ? 'i-heroicons-moon' : 'i-heroicons-sun'"
                color="gray"
                variant="ghost"
                size="sm"
                aria-label="Toggle color mode"
                @click="toggleColorMode"
              />
            </ClientOnly>
            <UButton
              icon="i-heroicons-bars-3"
              color="gray"
              variant="ghost"
              @click="mobileMenuOpen = true"
            />
          </div>
        </nav>
      </UContainer>
    </header>

    <!-- Mobile Menu Slide-over -->
    <USlideover v-model="mobileMenuOpen">
      <UCard class="flex flex-col flex-1" :ui="{ body: { base: 'flex-1', padding: '' }, ring: '', divide: '' }">
        <template #header>
          <div class="flex items-center justify-between">
            <div class="flex items-center gap-2">
              <UIcon name="i-heroicons-newspaper" class="w-6 h-6 text-primary" />
              <span class="text-lg font-bold">Zeitung</span>
            </div>
            <UButton
              icon="i-heroicons-x-mark"
              color="gray"
              variant="ghost"
              @click="mobileMenuOpen = false"
            />
          </div>
        </template>

        <nav class="flex flex-col gap-2 p-4">
          <UButton
            to="/"
            block
            variant="ghost"
            color="gray"
            size="lg"
            :class="{ 'bg-gray-100 dark:bg-gray-800': $route.path === '/' }"
            @click="mobileMenuOpen = false"
          >
            <template #leading>
              <UIcon name="i-heroicons-newspaper" class="w-5 h-5" />
            </template>
            Articles
          </UButton>
          <UButton
            to="/feeds"
            block
            variant="ghost"
            color="gray"
            size="lg"
            :class="{ 'bg-gray-100 dark:bg-gray-800': $route.path === '/feeds' }"
            @click="mobileMenuOpen = false"
          >
            <template #leading>
              <UIcon name="i-heroicons-rss" class="w-5 h-5" />
            </template>
            Feeds
          </UButton>
          <UButton
            to="/tags"
            block
            variant="ghost"
            color="gray"
            size="lg"
            :class="{ 'bg-gray-100 dark:bg-gray-800': $route.path === '/tags' }"
            @click="mobileMenuOpen = false"
          >
            <template #leading>
              <UIcon name="i-heroicons-tag" class="w-5 h-5" />
            </template>
            Tags
          </UButton>
        </nav>
      </UCard>
    </USlideover>

    <!-- Main Content -->
    <main>
      <NuxtPage />
    </main>

    <!-- Footer -->
    <footer class="border-t border-gray-200 dark:border-gray-800 mt-auto">
      <UContainer class="py-8">
        <div class="flex flex-col md:flex-row items-center justify-between gap-4 text-sm text-gray-600 dark:text-gray-400">
          <div class="flex items-center gap-2">
            <UIcon name="i-heroicons-newspaper" class="w-5 h-5" />
            <span>Zeitung RSS Reader</span>
          </div>
          <p>Smart RSS reader with AI-powered recommendations</p>
        </div>
      </UContainer>
    </footer>
  </div>
</template>

<script setup lang="ts">
const colorMode = useColorMode()
const mobileMenuOpen = ref(false)

const toggleColorMode = () => {
  colorMode.preference = colorMode.value === 'dark' ? 'light' : 'dark'
}

// Set dark as default
onMounted(() => {
  if (!colorMode.preference) {
    colorMode.preference = 'dark'
  }
})
</script>
