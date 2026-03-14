<template>
  <div class="user-menu" ref="menuRef">
    <!-- Trigger -->
    <button class="user-trigger" @click="toggleMenu" :aria-expanded="open">
      <span class="user-avatar" :style="avatarStyle">
        <img v-if="avatarUrl" :src="avatarUrl" alt="avatar" class="avatar-img" />
        <span v-else>{{ initial }}</span>
      </span>
      <span class="user-trigger-name">{{ username }}</span>
      <svg class="chevron-icon" :class="{ open }" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5" stroke-linecap="round" stroke-linejoin="round">
        <polyline points="6 9 12 15 18 9"/>
      </svg>
    </button>

    <!-- Dropdown -->
    <Transition name="dropdown">
      <div v-if="open" class="dropdown-panel">

        <!-- Sección perfil -->
        <div class="dropdown-section">
          <!-- Avatar upload -->
          <div class="avatar-section">
            <div class="avatar-large" :style="avatarStyle">
              <img v-if="avatarUrl" :src="avatarUrl" alt="avatar" class="avatar-img" />
              <span v-else>{{ initial }}</span>
            </div>
            <label class="avatar-upload-btn" title="Cambiar foto">
              <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
                <path d="M23 19a2 2 0 0 1-2 2H3a2 2 0 0 1-2-2V8a2 2 0 0 1 2-2h4l2-3h6l2 3h4a2 2 0 0 1 2 2z"/>
                <circle cx="12" cy="13" r="4"/>
              </svg>
              <input type="file" accept="image/*" class="sr-only" @change="handleAvatarChange" />
            </label>
          </div>

          <!-- Cambiar nombre -->
          <div v-if="!editingName" class="username-row">
            <span class="username-display">{{ username }}</span>
            <button class="icon-btn" title="Cambiar nombre" @click="startEditName">
              <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
                <path d="M11 4H4a2 2 0 0 0-2 2v14a2 2 0 0 0 2 2h14a2 2 0 0 0 2-2v-7"/>
                <path d="M18.5 2.5a2.121 2.121 0 0 1 3 3L12 15l-4 1 1-4 9.5-9.5z"/>
              </svg>
            </button>
          </div>
          <div v-else class="name-edit-row">
            <input
              ref="nameInputRef"
              v-model="newName"
              class="name-input"
              placeholder="Nuevo nombre"
              maxlength="30"
              @keydown.enter="saveName"
              @keydown.escape="cancelEditName"
            />
            <button class="icon-btn confirm-btn" @click="saveName" :disabled="savingName" title="Guardar">
              <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5" stroke-linecap="round" stroke-linejoin="round"><polyline points="20 6 9 17 4 12"/></svg>
            </button>
            <button class="icon-btn cancel-btn" @click="cancelEditName" title="Cancelar">
              <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5" stroke-linecap="round" stroke-linejoin="round"><line x1="18" y1="6" x2="6" y2="18"/><line x1="6" y1="6" x2="18" y2="18"/></svg>
            </button>
          </div>
          <p v-if="nameError" class="field-error">{{ nameError }}</p>
        </div>

        <div class="dropdown-divider" />

        <!-- Mis listas -->
        <div class="dropdown-section">
          <button class="dropdown-item" @click="openPanel('favorites')">
            <span class="item-icon">❤️</span>
            Mis favoritos
            <span class="item-count" v-if="favCount !== null">{{ favCount }}</span>
          </button>
          <button class="dropdown-item" @click="openPanel('purchases')">
            <span class="item-icon">✅</span>
            Mis compras
            <span class="item-count" v-if="buyCount !== null">{{ buyCount }}</span>
          </button>
        </div>

        <div class="dropdown-divider" />

        <!-- Grupo y salir -->
        <div class="dropdown-section">
          <RouterLink to="/grupo" class="dropdown-item" @click="open = false">
            <span class="item-icon">👥</span>
            Mi grupo
          </RouterLink>
          <button class="dropdown-item danger" @click="handleLogout">
            <span class="item-icon">🚪</span>
            Cerrar sesión
          </button>
        </div>
      </div>
    </Transition>
  </div>

  <!-- Panel lateral: favoritos / compras -->
  <Teleport to="body">
    <Transition name="panel">
      <div v-if="panelType" class="panel-overlay" @click.self="closePanel">
        <div class="side-panel">
          <div class="panel-header">
            <button class="panel-back" @click="backToMenu" title="Volver al menú">
              <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5" stroke-linecap="round" stroke-linejoin="round"><polyline points="15 18 9 12 15 6"/></svg>
            </button>
            <h3 class="panel-title">
              {{ panelType === 'favorites' ? '❤️ Mis favoritos' : '✅ Mis compras' }}
            </h3>
            <button class="panel-close" @click="closePanel">
              <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5" stroke-linecap="round" stroke-linejoin="round"><line x1="18" y1="6" x2="6" y2="18"/><line x1="6" y1="6" x2="18" y2="18"/></svg>
            </button>
          </div>

          <div v-if="panelLoading" class="panel-spinner">
            <div class="spinner-border" role="status" />
          </div>
          <div v-else-if="panelItems.length === 0" class="panel-empty">
            Sin juegos en esta lista aún.
          </div>
          <ul v-else class="panel-list">
            <li v-for="game in panelItems" :key="game.gameId" class="panel-item">
              <img v-if="game.coverImageUrl" :src="game.coverImageUrl" :alt="game.gameName" class="panel-thumb" />
              <span v-else class="panel-thumb-ph">🎮</span>
              <span class="panel-game-name">{{ game.gameName }}</span>
              <button class="panel-item-remove" @click="removeFromPanel(game.gameId)" title="Quitar">
                <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5" stroke-linecap="round" stroke-linejoin="round"><line x1="18" y1="6" x2="6" y2="18"/><line x1="6" y1="6" x2="18" y2="18"/></svg>
              </button>
            </li>
          </ul>
        </div>
      </div>
    </Transition>
  </Teleport>
</template>

<script setup>
import { ref, computed, onMounted, onBeforeUnmount, nextTick } from 'vue'
import { useRouter } from 'vue-router'
import { useAuth } from '../../composables/useAuth.js'
import { changeUsername } from '../../api/authApi.js'
import { getMyFavorites, getMyPurchases, removeFavorite, unmarkPurchased } from '../../api/socialApi.js'

const router = useRouter()
const { username, isLoggedIn, logout, updateUsername } = useAuth()

// ── Estado del dropdown ────────────────────────────────────────────────────
const open = ref(false)
const menuRef = ref(null)

function toggleMenu() { open.value = !open.value }

function handleOutsideClick(e) {
  if (menuRef.value && !menuRef.value.contains(e.target)) open.value = false
}
function onGameStatusChanged(e) {
  const { type, added } = e.detail
  if (type === 'favorite') favCount.value = (favCount.value ?? 0) + (added ? 1 : -1)
  else if (type === 'purchase') buyCount.value = (buyCount.value ?? 0) + (added ? 1 : -1)
}

onMounted(() => {
  document.addEventListener('mousedown', handleOutsideClick)
  window.addEventListener('game-status-changed', onGameStatusChanged)
})
onBeforeUnmount(() => {
  document.removeEventListener('mousedown', handleOutsideClick)
  window.removeEventListener('game-status-changed', onGameStatusChanged)
})

// ── Avatar ─────────────────────────────────────────────────────────────────
const AVATAR_KEY = computed(() => `gl_avatar_${username.value}`)
const avatarUrl = ref(null)

onMounted(() => {
  avatarUrl.value = localStorage.getItem(AVATAR_KEY.value) ?? null
})

const AVATAR_COLORS = ['#6366f1', '#8b5cf6', '#ec4899', '#f59e0b', '#10b981', '#3b82f6']
const avatarColor = computed(() => {
  const code = (username.value ?? '?').charCodeAt(0)
  return AVATAR_COLORS[code % AVATAR_COLORS.length]
})
const avatarStyle = computed(() => avatarUrl.value ? {} : { background: avatarColor.value })
const initial = computed(() => (username.value ?? '?')[0].toUpperCase())

function handleAvatarChange(e) {
  const file = e.target.files?.[0]
  if (!file) return
  const reader = new FileReader()
  reader.onload = (ev) => {
    const dataUrl = ev.target.result
    localStorage.setItem(AVATAR_KEY.value, dataUrl)
    avatarUrl.value = dataUrl
  }
  reader.readAsDataURL(file)
}

// ── Cambiar nombre ─────────────────────────────────────────────────────────
const editingName = ref(false)
const newName = ref('')
const savingName = ref(false)
const nameError = ref('')
const nameInputRef = ref(null)

function startEditName() {
  newName.value = username.value ?? ''
  nameError.value = ''
  editingName.value = true
  nextTick(() => nameInputRef.value?.focus())
}
function cancelEditName() {
  editingName.value = false
  nameError.value = ''
}
async function saveName() {
  if (!newName.value.trim() || newName.value.trim().length < 3) {
    nameError.value = 'Mínimo 3 caracteres'
    return
  }
  if (newName.value.trim() === username.value) { cancelEditName(); return }
  savingName.value = true
  nameError.value = ''
  try {
    const data = await changeUsername(newName.value.trim())
    updateUsername(data.username)
    editingName.value = false
  } catch (e) {
    nameError.value = e.message
  } finally {
    savingName.value = false
  }
}

// ── Contadores ──────────────────────────────────────────────────────────────
const favCount = ref(null)
const buyCount = ref(null)

onMounted(async () => {
  if (!isLoggedIn.value) return
  try {
    const [favs, buys] = await Promise.all([getMyFavorites(), getMyPurchases()])
    favCount.value = favs.length
    buyCount.value = buys.length
  } catch { /* silencioso */ }
})

// ── Panel lateral ──────────────────────────────────────────────────────────
const panelType = ref(null)
const panelItems = ref([])
const panelLoading = ref(false)

async function openPanel(type) {
  open.value = false
  panelType.value = type
  panelLoading.value = true
  panelItems.value = []
  try {
    panelItems.value = type === 'favorites' ? await getMyFavorites() : await getMyPurchases()
    // Actualiza contadores
    if (type === 'favorites') favCount.value = panelItems.value.length
    else buyCount.value = panelItems.value.length
  } catch { panelItems.value = [] }
  finally { panelLoading.value = false }
}
function closePanel() { panelType.value = null }
function backToMenu() { panelType.value = null; open.value = true }

async function removeFromPanel(gameId) {
  try {
    if (panelType.value === 'favorites') {
      await removeFavorite(gameId)
      window.dispatchEvent(new CustomEvent('game-status-changed', { detail: { type: 'favorite', gameId, added: false } }))
    } else {
      await unmarkPurchased(gameId)
      window.dispatchEvent(new CustomEvent('game-status-changed', { detail: { type: 'purchase', gameId, added: false } }))
    }
    panelItems.value = panelItems.value.filter(g => g.gameId !== gameId)
  } catch (e) {
    console.error('Error al quitar item del panel', e)
  }
}

// ── Logout ─────────────────────────────────────────────────────────────────
async function handleLogout() {
  open.value = false
  await logout()
  router.push('/login')
}
</script>

<style scoped>
/* ── Trigger ─────────────────────────────────────────────────────────────── */
.user-menu {
  position: relative;
}
.user-trigger {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  background: rgba(255,255,255,0.06);
  border: 1px solid rgba(255,255,255,0.1);
  border-radius: 20px;
  padding: 0.3rem 0.75rem 0.3rem 0.3rem;
  cursor: pointer;
  color: inherit;
  transition: background 0.15s, border-color 0.15s;
}
.user-trigger:hover { background: rgba(255,255,255,0.12); }

.user-avatar {
  width: 28px;
  height: 28px;
  border-radius: 50%;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 0.85rem;
  font-weight: 700;
  color: #fff;
  flex-shrink: 0;
  overflow: hidden;
}
.avatar-img { width: 100%; height: 100%; object-fit: cover; }
.user-trigger-name { font-size: 0.85rem; font-weight: 600; }
.chevron-icon {
  width: 0.8rem;
  height: 0.8rem;
  transition: transform 0.2s;
  opacity: 0.6;
}
.chevron-icon.open { transform: rotate(180deg); }

/* ── Dropdown panel ──────────────────────────────────────────────────────── */
.dropdown-panel {
  position: absolute;
  top: calc(100% + 8px);
  right: 0;
  width: 260px;
  background: #1a1a2e;
  border: 1px solid #333;
  border-radius: 14px;
  box-shadow: 0 8px 32px rgba(0,0,0,0.5);
  overflow: hidden;
  z-index: 1000;
}
.dropdown-transition-enter-active,
.dropdown-transition-leave-active { transition: opacity 0.15s, transform 0.15s; }
.dropdown-transition-enter-from,
.dropdown-transition-leave-to { opacity: 0; transform: translateY(-6px); }

.dropdown-enter-active,
.dropdown-leave-active { transition: opacity 0.15s, transform 0.15s; }
.dropdown-enter-from,
.dropdown-leave-to { opacity: 0; transform: translateY(-6px); }

.dropdown-section { padding: 0.75rem; }
.dropdown-divider { height: 1px; background: #2a2a3e; }

/* ── Avatar section ──────────────────────────────────────────────────────── */
.avatar-section {
  display: flex;
  align-items: flex-end;
  gap: 0.6rem;
  margin-bottom: 0.75rem;
}
.avatar-large {
  width: 52px;
  height: 52px;
  border-radius: 50%;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 1.4rem;
  font-weight: 700;
  color: #fff;
  overflow: hidden;
  flex-shrink: 0;
}
.avatar-upload-btn {
  display: flex;
  align-items: center;
  justify-content: center;
  width: 26px;
  height: 26px;
  background: #2a2a40;
  border: 1px solid #444;
  border-radius: 50%;
  cursor: pointer;
  color: #aaa;
  transition: background 0.15s, color 0.15s;
}
.avatar-upload-btn:hover { background: #35355a; color: #fff; }
.avatar-upload-btn svg { width: 0.85rem; height: 0.85rem; }
.sr-only { position: absolute; width: 1px; height: 1px; overflow: hidden; clip: rect(0,0,0,0); }

/* ── Username ────────────────────────────────────────────────────────────── */
.username-row {
  display: flex;
  align-items: center;
  gap: 0.4rem;
}
.username-display {
  font-size: 0.95rem;
  font-weight: 700;
  flex: 1;
  color: #f1f5f9;
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
}
.name-edit-row {
  display: flex;
  align-items: center;
  gap: 0.3rem;
}
.name-input {
  flex: 1;
  background: #0f0f1a;
  border: 1px solid #4a4a80;
  border-radius: 6px;
  color: #f1f5f9;
  font-size: 0.85rem;
  padding: 0.3rem 0.5rem;
  outline: none;
}
.icon-btn {
  background: none;
  border: none;
  cursor: pointer;
  color: #888;
  padding: 0.2rem;
  display: flex;
  align-items: center;
  border-radius: 4px;
  transition: color 0.15s, background 0.15s;
}
.icon-btn:hover { color: #fff; background: rgba(255,255,255,0.08); }
.icon-btn svg { width: 0.9rem; height: 0.9rem; }
.confirm-btn:hover { color: #4ade80; }
.cancel-btn:hover { color: #f87171; }
.icon-btn:disabled { opacity: 0.4; pointer-events: none; }
.field-error { font-size: 0.72rem; color: #f87171; margin: 0.25rem 0 0; }

/* ── Dropdown items ──────────────────────────────────────────────────────── */
.dropdown-item {
  display: flex;
  align-items: center;
  gap: 0.6rem;
  width: 100%;
  padding: 0.5rem 0.5rem;
  background: none;
  border: none;
  border-radius: 8px;
  color: #ccc;
  font-size: 0.88rem;
  cursor: pointer;
  text-decoration: none;
  transition: background 0.12s, color 0.12s;
}
.dropdown-item:hover { background: rgba(255,255,255,0.07); color: #fff; }
.dropdown-item.danger:hover { background: rgba(248,113,113,0.1); color: #f87171; }
.item-icon { font-size: 1rem; }
.item-count {
  margin-left: auto;
  background: #2a2a40;
  color: #aaa;
  font-size: 0.72rem;
  font-weight: 600;
  border-radius: 10px;
  padding: 0.1rem 0.45rem;
}

/* ── Panel lateral ───────────────────────────────────────────────────────── */
.panel-overlay {
  position: fixed;
  inset: 0;
  background: rgba(0,0,0,0.5);
  z-index: 2000;
  display: flex;
  justify-content: flex-end;
}
.side-panel {
  width: min(360px, 92vw);
  height: 100%;
  background: #1a1a2e;
  border-left: 1px solid #333;
  display: flex;
  flex-direction: column;
  overflow: hidden;
}
.panel-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 1.1rem 1.25rem;
  border-bottom: 1px solid #2a2a3e;
}
.panel-title { font-size: 1rem; font-weight: 700; margin: 0; }
.panel-back,
.panel-close {
  background: none;
  border: none;
  cursor: pointer;
  color: #888;
  display: flex;
  padding: 0.2rem;
  border-radius: 4px;
  transition: color 0.15s;
}
.panel-back:hover,
.panel-close:hover { color: #fff; }
.panel-back svg,
.panel-close svg { width: 1.1rem; height: 1.1rem; }

.panel-spinner, .panel-empty {
  flex: 1;
  display: flex;
  align-items: center;
  justify-content: center;
  color: #888;
  font-size: 0.9rem;
}
.panel-list {
  list-style: none;
  padding: 0.75rem;
  margin: 0;
  overflow-y: auto;
  flex: 1;
  display: flex;
  flex-direction: column;
  gap: 0.4rem;
}
.panel-item {
  display: flex;
  align-items: center;
  gap: 0.65rem;
  padding: 0.5rem 0.5rem;
  border-radius: 8px;
  transition: background 0.12s;
}
.panel-item:hover { background: rgba(255,255,255,0.04); }
.panel-thumb {
  width: 30px;
  height: 40px;
  object-fit: cover;
  border-radius: 4px;
  flex-shrink: 0;
}
.panel-thumb-ph {
  width: 30px;
  height: 40px;
  background: #2a2a3e;
  border-radius: 4px;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 0.9rem;
  flex-shrink: 0;
}
.panel-game-name {
  font-size: 0.85rem;
  color: #e2e8f0;
  line-height: 1.3;
}
.panel-item-remove {
  margin-left: auto;
  flex-shrink: 0;
  background: none;
  border: none;
  cursor: pointer;
  color: #9ca3af;
  padding: 4px;
  border-radius: 4px;
  display: flex;
  align-items: center;
  opacity: 0;
  transition: opacity 0.15s, color 0.15s;
}
.panel-item-remove svg { width: 14px; height: 14px; }
.panel-item:hover .panel-item-remove { opacity: 1; }
.panel-item-remove:hover { color: #ef4444; }

.panel-enter-active, .panel-leave-active { transition: opacity 0.2s; }
.panel-enter-active .side-panel, .panel-leave-active .side-panel { transition: transform 0.25s ease; }
.panel-enter-from, .panel-leave-to { opacity: 0; }
.panel-enter-from .side-panel, .panel-leave-to .side-panel { transform: translateX(100%); }
</style>
