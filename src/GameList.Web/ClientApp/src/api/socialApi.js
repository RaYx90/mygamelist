// Cliente HTTP para los endpoints sociales de /api/social.
// Todas las funciones requieren authHeader = { Authorization: 'Bearer <token>' }.
const BASE = '/api/social'

/**
 * Helper interno que añade el header de autorización y lanza error si la respuesta no es OK.
 * Devuelve null para respuestas 204 (sin contenido).
 */
async function authFetch(url, authHeader, options = {}) {
  const res = await fetch(url, {
    ...options,
    headers: { ...authHeader, ...(options.headers ?? {}) }
  })
  if (!res.ok) throw new Error(`Request failed: ${res.status}`)
  if (res.status === 204) return null
  return res.json()
}

// ── Favoritos ────────────────────────────────────────────────────────────────

/** Obtiene el estado (favoritos/compras) del usuario para los gameIds indicados. */
export async function getStatus(authHeader, gameIds = []) {
  // Los IDs se envían como CSV en el query param: ?gameIds=1,2,3
  const q = gameIds.length ? `?gameIds=${gameIds.join(',')}` : ''
  return authFetch(`${BASE}/status${q}`, authHeader)
}

/** Añade el juego a favoritos del usuario. */
export async function addFavorite(gameId, authHeader) {
  return authFetch(`${BASE}/favorites/${gameId}`, authHeader, { method: 'POST' })
}

/** Elimina el juego de favoritos del usuario. */
export async function removeFavorite(gameId, authHeader) {
  return authFetch(`${BASE}/favorites/${gameId}`, authHeader, { method: 'DELETE' })
}

// ── Compras ──────────────────────────────────────────────────────────────────

/** Marca el juego como comprado. */
export async function markPurchased(gameId, authHeader) {
  return authFetch(`${BASE}/purchases/${gameId}`, authHeader, { method: 'POST' })
}

/** Desmarca el juego como comprado. */
export async function unmarkPurchased(gameId, authHeader) {
  return authFetch(`${BASE}/purchases/${gameId}`, authHeader, { method: 'DELETE' })
}

// ── Grupos ───────────────────────────────────────────────────────────────────

/** Crea un nuevo grupo con el nombre indicado. Devuelve el GroupDto con el código de invitación. */
export async function createGroup(name, authHeader) {
  return authFetch(`${BASE}/groups`, authHeader, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ name })
  })
}

/**
 * Intenta unirse al grupo con el código de invitación dado.
 * Lanza error si el código es inválido (400 Bad Request).
 */
export async function joinGroup(inviteCode, authHeader) {
  return authFetch(`${BASE}/groups/join`, authHeader, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ inviteCode })
  })
}

/** Obtiene los insights del grupo: lista de juegos deseados/comprados por los miembros, ordenada por popularidad. */
export async function getGroupInsights(authHeader) {
  return authFetch(`${BASE}/group`, authHeader)
}

/** Obtiene la información básica del grupo del usuario: nombre, código de invitación y lista de miembros. */
export async function getMyGroup(authHeader) {
  return authFetch(`${BASE}/group/info`, authHeader)
}

/** Obtiene la vista por miembro: favoritos y compras de cada integrante del grupo. */
export async function getGroupMembersGames(authHeader) {
  return authFetch(`${BASE}/group/members`, authHeader)
}
