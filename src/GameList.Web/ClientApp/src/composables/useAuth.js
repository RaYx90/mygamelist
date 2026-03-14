/**
 * Composable de autenticación — estado global reactivo de la sesión.
 *
 * El JWT vive en una cookie HttpOnly (el JS no puede leerlo).
 * Este módulo solo gestiona los datos de display: userId, username, email, groupId.
 * La verificación de sesión se hace en init() llamando a GET /api/auth/me.
 */
import { reactive, computed } from 'vue'

const state = reactive({
  userId: null,
  username: null,
  email: null,
  groupId: null
})

export function useAuth() {
  /**
   * Verifica la sesión activa llamando a /api/auth/me.
   * Si la cookie gl_token es válida, popula el estado; si no (401), lo limpia.
   * Se llama una sola vez en main.js antes de montar la app.
   */
  async function init() {
    try {
      const res = await fetch('/api/auth/me', { credentials: 'same-origin' })
      if (res.ok) {
        const data = await res.json()
        setState(data)
      } else {
        clearState()
      }
    } catch {
      clearState()
    }
  }

  /** Persiste los datos de usuario en el estado reactivo tras login/register. */
  function login(data) {
    setState(data)
  }

  /** Llama al endpoint de logout (borra la cookie en el servidor) y limpia el estado. */
  async function logout() {
    try {
      await fetch('/api/auth/logout', { method: 'POST', credentials: 'same-origin' })
    } finally {
      clearState()
    }
  }

  /**
   * Actualiza el groupId en el estado sin necesidad de re-login.
   * Se llama tras crear o unirse a un grupo.
   */
  function updateGroupId(groupId) {
    state.groupId = groupId
  }

  function updateUsername(newUsername) {
    state.username = newUsername
  }

  function setState(data) {
    state.userId = data.userId ?? null
    state.username = data.username ?? null
    state.email = data.email ?? null
    state.groupId = data.groupId ?? null
  }

  function clearState() {
    state.userId = null
    state.username = null
    state.email = null
    state.groupId = null
  }

  return {
    userId: computed(() => state.userId),
    username: computed(() => state.username),
    email: computed(() => state.email),
    groupId: computed(() => state.groupId),
    isLoggedIn: computed(() => !!state.userId),
    init,
    login,
    logout,
    updateGroupId,
    updateUsername
  }
}
