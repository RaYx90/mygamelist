/**
 * Store de autenticación (patrón composable de Vue 3, similar a Pinia).
 * Gestiona el estado de sesión del usuario y su sincronización con localStorage.
 *
 * El estado es reactivo y compartido entre todos los componentes que llamen a useAuth().
 * Se persiste en localStorage bajo la clave 'gl_user' para sobrevivir recargas de página.
 */
import { reactive } from 'vue'

// Clave de localStorage donde se guarda la sesión del usuario.
const STORAGE_KEY = 'gl_user'

// Estado global reactivo — compartido entre todos los consumidores de useAuth().
const state = reactive({
  token: null,      // JWT Bearer token para llamadas a la API
  userId: null,     // ID del usuario en la BD
  username: null,   // Nombre de usuario para mostrar en la UI
  email: null,
  groupId: null     // ID del grupo al que pertenece el usuario (null si no tiene grupo)
})

export function useAuth() {
  /**
   * Restaura la sesión desde localStorage al iniciar la app (se llama en App.vue onMounted).
   * Si los datos están corruptos, limpia el estado para forzar nuevo login.
   */
  function init() {
    try {
      const raw = localStorage.getItem(STORAGE_KEY)
      if (raw) {
        const data = JSON.parse(raw)
        state.token = data.token ?? null
        state.userId = data.userId ?? null
        state.username = data.username ?? null
        state.email = data.email ?? null
        state.groupId = data.groupId ?? null
      }
    } catch {
      clear() // JSON corrupto — se limpia todo para evitar estado inconsistente.
    }
  }

  /** Persiste la sesión recibida del backend (respuesta del endpoint /api/auth/login). */
  function login(data) {
    state.token = data.token
    state.userId = data.userId
    state.username = data.username
    state.email = data.email
    state.groupId = data.groupId ?? null
    localStorage.setItem(STORAGE_KEY, JSON.stringify(data))
  }

  /** Cierra sesión limpiando estado y localStorage. */
  function logout() {
    clear()
  }

  /**
   * Actualiza el groupId en el estado reactivo y en localStorage sin invalidar el token.
   * Se llama tras crear o unirse a un grupo para que la UI refleje el cambio inmediatamente.
   */
  function updateGroupId(groupId) {
    state.groupId = groupId
    const raw = localStorage.getItem(STORAGE_KEY)
    if (raw) {
      const data = JSON.parse(raw)
      data.groupId = groupId
      localStorage.setItem(STORAGE_KEY, JSON.stringify(data))
    }
  }

  /** Limpia todo el estado y elimina la sesión de localStorage. */
  function clear() {
    state.token = null
    state.userId = null
    state.username = null
    state.email = null
    state.groupId = null
    localStorage.removeItem(STORAGE_KEY)
  }

  return {
    state,
    init,
    login,
    logout,
    updateGroupId,
    /** true si el usuario tiene un token activo. */
    get isLoggedIn() { return !!state.token },
    /** Objeto header listo para pasar a fetch: { Authorization: 'Bearer <token>' } o {} si no hay sesión. */
    get authHeader() { return state.token ? { Authorization: `Bearer ${state.token}` } : {} }
  }
}
