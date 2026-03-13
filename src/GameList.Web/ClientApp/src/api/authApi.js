const BASE = '/api/auth'

export async function register(username, email, password, inviteCode) {
  const res = await fetch(`${BASE}/register`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ username, email, password, inviteCode }),
    credentials: 'same-origin'
  })
  if (res.status === 409) {
    const err = await res.json()
    throw new Error(err.error ?? 'El usuario ya existe')
  }
  if (res.status === 400) {
    const msg = await res.text()
    throw new Error(msg || 'Error al registrar')
  }
  if (!res.ok) throw new Error('Error al registrar')
  return res.json()
}

export async function login(email, password) {
  const res = await fetch(`${BASE}/login`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ email, password }),
    credentials: 'same-origin'
  })
  if (res.status === 401) throw new Error('Email o contraseña incorrectos')
  if (!res.ok) throw new Error('Error al iniciar sesión')
  return res.json()
}
