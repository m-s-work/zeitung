// Composable for managing reading sessions and tracking article reading time
import type { ReadingSession } from '~/types'

export const useReadingTracker = () => {
  const sessions = useState<Map<number, ReadingSession>>('readingSessions', () => new Map())
  const currentSession = useState<ReadingSession | null>('currentSession', () => null)

  const startReading = (articleId: number) => {
    const session: ReadingSession = {
      articleId,
      startTime: Date.now(),
    }
    currentSession.value = session
    sessions.value.set(articleId, session)
  }

  const stopReading = (articleId: number, rating?: number) => {
    const session = sessions.value.get(articleId)
    if (session) {
      session.endTime = Date.now()
      if (rating !== undefined) {
        session.rating = rating
      }
      sessions.value.set(articleId, session)
    }
    if (currentSession.value?.articleId === articleId) {
      currentSession.value = null
    }
  }

  const getReadingTime = (articleId: number): number => {
    const session = sessions.value.get(articleId)
    if (!session) return 0
    const endTime = session.endTime || Date.now()
    return Math.floor((endTime - session.startTime) / 1000) // Return in seconds
  }

  const getSession = (articleId: number): ReadingSession | undefined => {
    return sessions.value.get(articleId)
  }

  return {
    startReading,
    stopReading,
    getReadingTime,
    getSession,
    currentSession: readonly(currentSession),
  }
}
