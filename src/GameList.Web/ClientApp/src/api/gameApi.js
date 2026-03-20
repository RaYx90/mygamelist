import { createHttpClient } from './httpClient.js'

const client = createHttpClient('/api')

export function getReleases(year, month, platformId, category) {
  return client.getWithParams('/releases', { year, month, platformId, category })
}

export function getPlatforms() {
  return client.get('/platforms')
}

export function getGameDetail(id) {
  return client.get(`/games/${id}`)
}

export function searchReleases(year, q) {
  return client.getWithParams('/releases/search', { year, q })
}
