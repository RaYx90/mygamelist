/**
 * Composable que calcula los datos sociales (favoritos/compras) para un juego dado.
 * Elimina la duplicación de la misma función en DayCell y DayReleasesModal.
 */
export function useGameSocialData(gameStatus) {
  function getForGame(gameId) {
    if (!gameStatus.value) return { favCount: 0, purchasedCount: 0, isFavorite: false, isPurchased: false }
    const isFavorite = gameStatus.value.myFavorites?.includes(gameId) ?? false
    const isPurchased = gameStatus.value.myPurchases?.includes(gameId) ?? false
    const favoritedBy = gameStatus.value.favoritedByInGroup?.[gameId] ?? []
    const purchasedByGroup = gameStatus.value.purchasedByInGroup?.[gameId] ?? []
    return {
      favCount: (isFavorite ? 1 : 0) + favoritedBy.length,
      purchasedCount: (isPurchased ? 1 : 0) + purchasedByGroup.length,
      isFavorite,
      isPurchased
    }
  }
  return { getForGame }
}
