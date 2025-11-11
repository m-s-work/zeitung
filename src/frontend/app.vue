<template>
  <NuxtLayout>
    <NuxtPage />
  </NuxtLayout>
</template>

<script setup lang="ts">
// Set dark mode as default
const colorMode = useColorMode()

onMounted(() => {
  // Set dark as default if no preference is set
  if (!colorMode.preference || colorMode.preference === 'system') {
    colorMode.preference = 'dark'
  }
})
</script>
