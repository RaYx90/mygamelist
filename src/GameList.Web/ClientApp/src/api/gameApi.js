import { createHttpClient } from './httpClient.js'

const client = createHttpClient('/api')

export function getReleases(year, month, platformId) {
  return client.getWithParams('/releases', { year, month, platformId })
}

export function getPlatforms() {
  return client.get('/platforms')
}

export function getGameDetail(id) {
  return client.get(`/games/${id}`)
}
