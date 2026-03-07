import { reactive } from 'vue'

const STORAGE_KEY = 'gl_user'

const state = reactive({
  token: null,
  userId: null,
  username: null,
  email: null,
  groupId: null
})

export function useAuth() {
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
      clear()
    }
  }

  function login(data) {
    state.token = data.token
    state.userId = data.userId
    state.username = data.username
    state.email = data.email
    state.groupId = data.groupId ?? null
    localStorage.setItem(STORAGE_KEY, JSON.stringify(data))
  }

  function logout() {
    clear()
  }

  function updateGroupId(groupId) {
    state.groupId = groupId
    const raw = localStorage.getItem(STORAGE_KEY)
    if (raw) {
      const data = JSON.parse(raw)
      data.groupId = groupId
      localStorage.setItem(STORAGE_KEY, JSON.stringify(data))
    }
  }

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
    get isLoggedIn() { return !!state.token },
    get authHeader() { return state.token ? { Authorization: `Bearer ${state.token}` } : {} }
  }
}
