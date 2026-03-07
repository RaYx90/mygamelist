const BASE = '/api/social'

async function authFetch(url, authHeader, options = {}) {
  const res = await fetch(url, {
    ...options,
    headers: { ...authHeader, ...(options.headers ?? {}) }
  })
  if (!res.ok) throw new Error(`Request failed: ${res.status}`)
  if (res.status === 204) return null
  return res.json()
}

export async function getStatus(authHeader, gameIds = []) {
  const q = gameIds.length ? `?gameIds=${gameIds.join(',')}` : ''
  return authFetch(`${BASE}/status${q}`, authHeader)
}

export async function addFavorite(gameId, authHeader) {
  return authFetch(`${BASE}/favorites/${gameId}`, authHeader, { method: 'POST' })
}

export async function removeFavorite(gameId, authHeader) {
  return authFetch(`${BASE}/favorites/${gameId}`, authHeader, { method: 'DELETE' })
}

export async function markPurchased(gameId, authHeader) {
  return authFetch(`${BASE}/purchases/${gameId}`, authHeader, { method: 'POST' })
}

export async function unmarkPurchased(gameId, authHeader) {
  return authFetch(`${BASE}/purchases/${gameId}`, authHeader, { method: 'DELETE' })
}

export async function createGroup(name, authHeader) {
  return authFetch(`${BASE}/groups`, authHeader, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ name })
  })
}

export async function joinGroup(inviteCode, authHeader) {
  return authFetch(`${BASE}/groups/join`, authHeader, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ inviteCode })
  })
}

export async function getGroupInsights(authHeader) {
  return authFetch(`${BASE}/group`, authHeader)
}

export async function getMyGroup(authHeader) {
  return authFetch(`${BASE}/group/info`, authHeader)
}

export async function getGroupMembersGames(authHeader) {
  return authFetch(`${BASE}/group/members`, authHeader)
}
