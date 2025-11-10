<template>
  <div class="min-h-screen bg-gray-50 dark:bg-gray-900">
    <UContainer>
      <!-- Header -->
      <header class="sticky top-0 z-50 bg-white/80 dark:bg-gray-900/80 backdrop-blur-sm border-b border-gray-200 dark:border-gray-800">
        <div class="flex items-center justify-between h-16 px-4">
          <div class="flex items-center space-x-4">
            <NuxtLink to="/" class="text-2xl font-bold text-primary">
              Zeitung
            </NuxtLink>
            <nav class="hidden md:flex space-x-4">
              <UButton 
                to="/" 
                variant="ghost" 
                label="Feed"
                :class="$route.path === '/' ? 'text-primary' : ''"
              />
              <UButton 
                to="/feeds" 
                variant="ghost" 
                label="My Feeds"
                :class="$route.path === '/feeds' ? 'text-primary' : ''"
              />
            </nav>
          </div>
          
          <div class="flex items-center space-x-2">
            <ClientOnly>
              <UButton
                icon="i-heroicons-moon"
                variant="ghost"
                @click="toggleColorMode"
                v-if="colorMode.value === 'light'"
              />
              <UButton
                icon="i-heroicons-sun"
                variant="ghost"
                @click="toggleColorMode"
                v-else
              />
            </ClientOnly>
          </div>
        </div>
      </header>

      <!-- Main content -->
      <main class="py-8">
        <slot />
      </main>

      <!-- Footer -->
      <footer class="border-t border-gray-200 dark:border-gray-800 py-6 mt-12">
        <div class="text-center text-sm text-gray-600 dark:text-gray-400">
          <p>Zeitung - Smart RSS Feed Reader with AI-powered tagging</p>
          <p v-if="lastSync" class="mt-2">
            Last sync: {{ formatLastSync(lastSync) }}
          </p>
        </div>
      </footer>
    </UContainer>
  </div>
</template>

<script setup lang="ts">
const colorMode = useColorMode()
const syncStore = useSyncStore()

const lastSync = computed(() => syncStore.lastSync)

const toggleColorMode = () => {
  colorMode.preference = colorMode.value === 'dark' ? 'light' : 'dark'
}

const formatLastSync = (date: Date) => {
  const now = new Date()
  const diffMs = now.getTime() - date.getTime()
  const diffMins = Math.floor(diffMs / 60000)
  
  if (diffMins < 1) return 'Just now'
  if (diffMins < 60) return `${diffMins} minute${diffMins !== 1 ? 's' : ''} ago`
  
  const diffHours = Math.floor(diffMins / 60)
  if (diffHours < 24) return `${diffHours} hour${diffHours !== 1 ? 's' : ''} ago`
  
  const diffDays = Math.floor(diffHours / 24)
  return `${diffDays} day${diffDays !== 1 ? 's' : ''} ago`
}
</script>
