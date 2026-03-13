/**
 * Composable que gestiona el estado social del usuario (favoritos/compras)
 * para los juegos visibles en el calendario.
 */
import { ref } from 'vue'
import { getStatus, addFavorite, removeFavorite, markPurchased, unmarkPurchased } from '../api/socialApi.js'
import { useAuth } from './useAuth.js'

export function useGameStatus() {
  const gameStatus = ref(null)
  const auth = useAuth()

  async function loadStatus(gameIds) {
    if (!auth.isLoggedIn.value) { gameStatus.value = null; return }
    try {
      gameStatus.value = await getStatus(gameIds)
    } catch {
      gameStatus.value = null
    }
  }

  async function toggleFavorite(gameId) {
    if (!auth.isLoggedIn.value) return
    const isFav = gameStatus.value?.myFavorites?.includes(gameId)
    try {
      if (isFav) {
        await removeFavorite(gameId)
        gameStatus.value = { ...gameStatus.value, myFavorites: gameStatus.value.myFavorites.filter(id => id !== gameId) }
      } else {
        await addFavorite(gameId)
        gameStatus.value = { ...gameStatus.value, myFavorites: [...(gameStatus.value?.myFavorites ?? []), gameId] }
      }
    } catch (e) {
      console.error('Error al cambiar favorito', e)
    }
  }

  async function togglePurchase(gameId) {
    if (!auth.isLoggedIn.value) return
    const isPurchased = gameStatus.value?.myPurchases?.includes(gameId)
    try {
      if (isPurchased) {
        await unmarkPurchased(gameId)
        gameStatus.value = { ...gameStatus.value, myPurchases: gameStatus.value.myPurchases.filter(id => id !== gameId) }
      } else {
        await markPurchased(gameId)
        gameStatus.value = { ...gameStatus.value, myPurchases: [...(gameStatus.value?.myPurchases ?? []), gameId] }
      }
    } catch (e) {
      console.error('Error al cambiar compra', e)
    }
  }

  return { gameStatus, loadStatus, toggleFavorite, togglePurchase }
}
