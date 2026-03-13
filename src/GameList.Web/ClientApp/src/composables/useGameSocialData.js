/**
 * Composable que calcula los datos sociales (favoritos/compras) para un juego dado.
 * Elimina la duplicación de la misma función en DayCell y DayReleasesModal.
 */
export function useGameSocialData(gameStatus) {
  function getForGame(gameId) {
    if (!gameStatus.value) return { favCount: 0, purchasedCount: 0, isFavorite: false, isPurchased: false }
    const isFavorite = gameStatus.value.myFavorites?.includes(gameId) ?? false
    const isPurchased = gameStatus.value.myPurchases?.includes(gameId) ?? false
    const otherFavs = gameStatus.value.favoritedCountInGroup?.[gameId] ?? 0
    const otherPurchases = gameStatus.value.purchasedByInGroup?.[gameId]?.length ?? 0
    return {
      favCount: (isFavorite ? 1 : 0) + otherFavs,
      purchasedCount: (isPurchased ? 1 : 0) + otherPurchases,
      isFavorite,
      isPurchased
    }
  }
  return { getForGame }
}
