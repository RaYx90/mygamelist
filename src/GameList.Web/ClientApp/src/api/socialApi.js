import { createHttpClient } from './httpClient.js'

const client = createHttpClient('/api/social')

// ── Favoritos ────────────────────────────────────────────────────────────────

/** Obtiene el estado (favoritos/compras) del usuario para los gameIds indicados. */
export function getStatus(gameIds = []) {
  // Los IDs se envían como CSV en el query param: ?gameIds=1,2,3
  const q = gameIds.length ? `?gameIds=${gameIds.join(',')}` : ''
  return client.get(`/status${q}`)
}

/** Añade el juego a favoritos del usuario. */
export function addFavorite(gameId) {
  return client.post(`/favorites/${gameId}`)
}

/** Elimina el juego de favoritos del usuario. */
export function removeFavorite(gameId) {
  return client.delete(`/favorites/${gameId}`)
}

// ── Compras ──────────────────────────────────────────────────────────────────

/** Marca el juego como comprado. */
export function markPurchased(gameId) {
  return client.post(`/purchases/${gameId}`)
}

/** Desmarca el juego como comprado. */
export function unmarkPurchased(gameId) {
  return client.delete(`/purchases/${gameId}`)
}

// ── Grupos ───────────────────────────────────────────────────────────────────

/** Crea un nuevo grupo con el nombre indicado. Devuelve el GroupDto con el código de invitación. */
export function createGroup(name) {
  return client.postJson('/groups', { name })
}

/**
 * Intenta unirse al grupo con el código de invitación dado.
 * Lanza ApiError(400) si el código es inválido.
 */
export function joinGroup(inviteCode) {
  return client.postJson('/groups/join', { inviteCode })
}

/** Obtiene los insights del grupo: lista de juegos deseados/comprados por los miembros. */
export function getGroupInsights() {
  return client.get('/group')
}

/** Obtiene la información básica del grupo del usuario: nombre, código de invitación y miembros. */
export function getMyGroup() {
  return client.get('/group/info')
}

/** Obtiene la vista por miembro: favoritos y compras de cada integrante del grupo. */
export function getGroupMembersGames() {
  return client.get('/group/members')
}
