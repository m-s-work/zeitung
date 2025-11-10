import { describe, it, expect, beforeEach } from 'vitest'
import { setActivePinia, createPinia } from 'pinia'
import { useSyncStore } from '../../../stores/sync'

describe('Sync Store', () => {
  beforeEach(() => {
    setActivePinia(createPinia())
  })

  it('should initialize with null lastSync', () => {
    const store = useSyncStore()
    expect(store.lastSync).toBeNull()
    expect(store.isSyncing).toBe(false)
  })

  it('should update last sync timestamp', () => {
    const store = useSyncStore()
    const beforeUpdate = new Date()
    
    store.updateLastSync()
    
    expect(store.lastSync).toBeDefined()
    expect(store.lastSync!.getTime()).toBeGreaterThanOrEqual(beforeUpdate.getTime())
  })

  it('should set syncing state during sync', async () => {
    const store = useSyncStore()
    
    const syncPromise = store.sync()
    expect(store.isSyncing).toBe(true)
    
    await syncPromise
    expect(store.isSyncing).toBe(false)
  })

  it('should update lastSync after successful sync', async () => {
    const store = useSyncStore()
    
    await store.sync()
    
    expect(store.lastSync).toBeDefined()
  })
})
