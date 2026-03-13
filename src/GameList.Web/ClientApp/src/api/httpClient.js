/**
 * Cliente HTTP centralizado.
 * La cookie gl_token (HttpOnly) se envía automáticamente por el navegador
 * al ser same-origin, sin necesidad de gestionar tokens manualmente.
 */

export class ApiError extends Error {
  constructor(status, message) {
    super(message || `Error HTTP ${status}`)
    this.status = status
  }
}

async function request(url, options = {}) {
  const headers = {
    ...(options.body ? { 'Content-Type': 'application/json' } : {}),
    ...(options.headers ?? {})
  }

  const res = await fetch(url, { ...options, headers, credentials: 'same-origin' })

  if (res.status === 204) return null
  if (!res.ok) {
    const text = await res.text().catch(() => '')
    throw new ApiError(res.status, text)
  }
  return res.json()
}

export function createHttpClient(base) {
  return {
    get: (path) => request(`${base}${path}`),
    post: (path) => request(`${base}${path}`, { method: 'POST' }),
    postJson: (path, body) => request(`${base}${path}`, { method: 'POST', body: JSON.stringify(body) }),
    delete: (path) => request(`${base}${path}`, { method: 'DELETE' }),
    getWithParams: (path, params) => {
      const qs = new URLSearchParams(
        Object.entries(params).filter(([, v]) => v != null).map(([k, v]) => [k, String(v)])
      ).toString()
      return request(`${base}${path}${qs ? '?' + qs : ''}`)
    }
  }
}
