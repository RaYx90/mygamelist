const BASE = '/api'

export async function getReleases(year, month, platformId, authHeader = {}) {
  const params = new URLSearchParams({ year, month })
  if (platformId != null) params.set('platformId', platformId)
  const res = await fetch(`${BASE}/releases?${params}`, { headers: { ...authHeader } })
  if (!res.ok) throw new Error(`getReleases failed: ${res.status}`)
  return res.json()
}

export async function getPlatforms(authHeader = {}) {
  const res = await fetch(`${BASE}/platforms`, { headers: { ...authHeader } })
  if (!res.ok) throw new Error(`getPlatforms failed: ${res.status}`)
  return res.json()
}

export async function getGameDetail(id, authHeader = {}) {
  const res = await fetch(`${BASE}/games/${id}`, { headers: { ...authHeader } })
  if (!res.ok) throw new Error(`getGameDetail failed: ${res.status}`)
  return res.json()
}