import { defineStore } from 'pinia'

export const useSyncStore = defineStore('sync', {
  state: () => ({
    lastSync: null as Date | null,
    isSyncing: false,
  }),
  
  actions: {
    updateLastSync() {
      this.lastSync = new Date()
      // Persist to localStorage for offline capability
      if (process.client) {
        localStorage.setItem('zeitung_last_sync', this.lastSync.toISOString())
      }
    },
    
    loadLastSync() {
      if (process.client) {
        const stored = localStorage.getItem('zeitung_last_sync')
        if (stored) {
          this.lastSync = new Date(stored)
        }
      }
    },
    
    async sync() {
      this.isSyncing = true
      try {
        // TODO: Implement actual sync logic
        // This would sync local data with the server
        await new Promise(resolve => setTimeout(resolve, 1000))
        this.updateLastSync()
      } finally {
        this.isSyncing = false
      }
    }
  }
})
