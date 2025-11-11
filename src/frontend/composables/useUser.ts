// Composable for user state management (temporary userId until auth is implemented)
export const useUser = () => {
  // For now, use a hardcoded userId. This will be replaced with proper authentication
  const userId = useState<number>('userId', () => 1)

  const setUserId = (id: number) => {
    userId.value = id
  }

  return {
    userId: readonly(userId),
    setUserId,
  }
}
