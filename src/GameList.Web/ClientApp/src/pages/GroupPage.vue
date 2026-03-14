<template>
  <div class="group-container">

    <!-- Sin grupo -->
    <div v-if="!groupId" class="group-actions">
      <h2 class="group-title">Mi Grupo</h2>
      <div class="action-card">
        <h4>Crear un grupo nuevo</h4>
        <form @submit.prevent="handleCreate" class="d-flex gap-2">
          <input v-model="groupName" type="text" class="form-control" placeholder="Nombre del grupo" required minlength="3" />
          <button type="submit" class="btn btn-primary" :disabled="creating">
            <span v-if="creating" class="spinner-border spinner-border-sm" />
            <span v-else>Crear</span>
          </button>
        </form>
        <div v-if="createError" class="alert alert-danger mt-2 py-2">{{ createError }}</div>
      </div>

      <div class="action-card mt-3">
        <h4>Unirse con código de invitación</h4>
        <form @submit.prevent="handleJoin" class="d-flex gap-2">
          <input v-model="inviteCode" type="text" class="form-control" placeholder="Código (ej: AB12CD34)" required maxlength="8" style="text-transform: uppercase" />
          <button type="submit" class="btn btn-outline-primary" :disabled="joining">
            <span v-if="joining" class="spinner-border spinner-border-sm" />
            <span v-else>Unirse</span>
          </button>
        </form>
        <div v-if="joinError" class="alert alert-danger mt-2 py-2">{{ joinError }}</div>
      </div>
    </div>

    <!-- Con grupo -->
    <template v-else-if="groupId">
      <!-- Cabecera compacta -->
      <div v-if="groupInfo" class="group-topbar">
        <div class="group-topbar-name">
          <span class="group-icon">👥</span>
          <span class="group-name-text">{{ groupInfo.name }}</span>
          <span class="member-pill">{{ groupInfo.memberUsernames.length }} miembros</span>
        </div>
        <button class="invite-btn" @click="copyInviteCode" :title="copied ? 'Copiado' : 'Copiar código'">
          <span class="invite-code-text">{{ groupInfo.inviteCode }}</span>
          <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
            <rect x="9" y="9" width="13" height="13" rx="2" ry="2"/>
            <path d="M5 15H4a2 2 0 0 1-2-2V4a2 2 0 0 1 2-2h9a2 2 0 0 1 2 2v1"/>
          </svg>
          <span class="copied-badge" :class="{ visible: copied }">Copiado</span>
        </button>
      </div>

      <!-- Tabs -->
      <div class="tabs-bar">
        <button class="tab-btn" :class="{ active: activeTab === 'members' }" @click="activeTab = 'members'">
          Miembros
          <span class="tab-badge">{{ memberGames.length }}</span>
        </button>
        <button class="tab-btn" :class="{ active: activeTab === 'insights' }" @click="activeTab = 'insights'">
          Coincidencias
          <span class="tab-badge" :class="{ highlight: insights.length > 0 }">{{ insights.length }}</span>
        </button>
      </div>

      <div v-if="loadingInsights" class="text-center py-5">
        <div class="spinner-border" role="status" />
      </div>

      <!-- Tab: Miembros -->
      <div v-else-if="activeTab === 'members'" class="tab-content">
        <div v-if="memberGames.length === 0" class="empty-state">
          <p>Aún no hay juegos marcados en el grupo.</p>
        </div>
        <div v-else class="accordion-list">
          <div
            v-for="member in memberGames"
            :key="member.username"
            class="accordion-item"
            :class="{ open: expandedMember === member.username }"
          >
            <button class="accordion-header" @click="toggleMember(member.username)">
              <span class="member-avatar">{{ member.username[0].toUpperCase() }}</span>
              <span class="member-name">{{ member.username }}</span>
              <div class="member-counts">
                <span class="count-badge fav" title="Favoritos">❤️ {{ member.favorites.length }}</span>
                <span class="count-badge buy" title="Comprado">✅ {{ member.purchases.length }}</span>
              </div>
              <svg class="chevron" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5" stroke-linecap="round" stroke-linejoin="round">
                <polyline points="6 9 12 15 18 9"/>
              </svg>
            </button>

            <div class="accordion-body">
              <div class="accordion-body-inner">
                <div v-if="member.favorites.length > 0" class="member-section">
                  <p class="section-label">❤️ Favoritos</p>
                  <div class="game-chips">
                    <div v-for="g in member.favorites" :key="g.gameId" class="game-chip">
                      <img v-if="g.coverImageUrl" :src="g.coverImageUrl" :alt="g.gameName" class="chip-thumb" />
                      <span v-else class="chip-thumb-ph">🎮</span>
                      <span class="chip-name">{{ g.gameName }}</span>
                    </div>
                  </div>
                </div>
                <div v-else class="member-empty">Sin favoritos</div>

                <div v-if="member.purchases.length > 0" class="member-section mt-2">
                  <p class="section-label">✅ Comprado</p>
                  <div class="game-chips">
                    <div v-for="g in member.purchases" :key="g.gameId" class="game-chip">
                      <img v-if="g.coverImageUrl" :src="g.coverImageUrl" :alt="g.gameName" class="chip-thumb" />
                      <span v-else class="chip-thumb-ph">🎮</span>
                      <span class="chip-name">{{ g.gameName }}</span>
                    </div>
                  </div>
                </div>
                <div v-else class="member-empty">Sin compras</div>
              </div>
            </div>
          </div>
        </div>
      </div>

      <!-- Tab: Coincidencias -->
      <div v-else-if="activeTab === 'insights'" class="tab-content">
        <div v-if="insights.length === 0" class="empty-state">
          <p>No hay juegos que varios miembros compartan aún.</p>
        </div>
        <div v-else class="insights-grid">
          <div v-for="game in insights" :key="game.gameId" class="insight-card">
            <img v-if="game.coverImageUrl" :src="game.coverImageUrl" :alt="game.gameName" class="insight-cover" />
            <div v-else class="insight-cover-placeholder">🎮</div>
            <div class="insight-info">
              <h6 class="insight-name">{{ game.gameName }}</h6>
              <p v-if="game.releaseDate" class="insight-date">{{ formatDateShort(game.releaseDate) }}</p>
              <div class="insight-badges">
                <span class="badge bg-danger me-1">❤️ {{ game.wantedBy.length }}</span>
                <span class="badge bg-success">✅ {{ game.purchasedBy.length }}</span>
              </div>
              <p v-if="game.wantedBy.length" class="insight-users">Quieren: {{ game.wantedBy.join(', ') }}</p>
              <p v-if="game.purchasedBy.length" class="insight-users">Comprado por: {{ game.purchasedBy.join(', ') }}</p>
            </div>
          </div>
        </div>
      </div>
    </template>
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import { createGroup, joinGroup, getGroupInsights, getMyGroup, getGroupMembersGames } from '../api/socialApi.js'
import { useAuth } from '../composables/useAuth.js'
import { useFormatDate } from '../composables/useFormatDate.js'

const { groupId, updateGroupId } = useAuth()
const { formatDateShort } = useFormatDate()

const groupName = ref('')
const inviteCode = ref('')
const createError = ref('')
const joinError = ref('')
const creating = ref(false)
const joining = ref(false)
const copied = ref(false)

const groupInfo = ref(null)
const insights = ref([])
const memberGames = ref([])
const loadingInsights = ref(false)
const activeTab = ref('members')
const expandedMember = ref(null)

onMounted(async () => {
  if (groupId.value) await loadGroupData()
})

async function loadGroupData() {
  loadingInsights.value = true
  try {
    const [info, ins, members] = await Promise.all([
      getMyGroup(),
      getGroupInsights(),
      getGroupMembersGames()
    ])
    groupInfo.value = info
    insights.value = ins ?? []
    memberGames.value = members ?? []
  } catch {
    insights.value = []
    memberGames.value = []
  } finally {
    loadingInsights.value = false
  }
}

function toggleMember(username) {
  expandedMember.value = expandedMember.value === username ? null : username
}

async function copyInviteCode() {
  if (!groupInfo.value?.inviteCode) return
  try {
    await navigator.clipboard.writeText(groupInfo.value.inviteCode)
    copied.value = true
    setTimeout(() => { copied.value = false }, 2000)
  } catch {
    // fallback silencioso
  }
}

async function handleCreate() {
  createError.value = ''
  creating.value = true
  try {
    const group = await createGroup(groupName.value)
    updateGroupId(group.id)
    await loadGroupData()
  } catch (e) {
    createError.value = e.message
  } finally {
    creating.value = false
  }
}

async function handleJoin() {
  joinError.value = ''
  joining.value = true
  try {
    const group = await joinGroup(inviteCode.value.toUpperCase())
    updateGroupId(group.id)
    await loadGroupData()
  } catch {
    joinError.value = 'Código inválido o grupo no encontrado'
  } finally {
    joining.value = false
  }
}
</script>

<style scoped>
.group-container {
  max-width: 760px;
  margin: 0 auto;
  padding: 1.5rem 1rem 3rem;
}
.group-title {
  font-size: 1.75rem;
  font-weight: 700;
  margin-bottom: 1.5rem;
}
.action-card {
  background: var(--card-bg, #1e1e2e);
  border: 1px solid var(--border-color, #333);
  border-radius: 12px;
  padding: 1.5rem;
}

/* Topbar */
.group-topbar {
  display: flex;
  align-items: center;
  justify-content: space-between;
  flex-wrap: wrap;
  gap: 0.75rem;
  background: var(--card-bg, #1e1e2e);
  border: 1px solid #333;
  border-radius: 12px;
  padding: 0.85rem 1.1rem;
  margin-bottom: 1.25rem;
}
.group-topbar-name {
  display: flex;
  align-items: center;
  gap: 0.6rem;
}
.group-icon { font-size: 1.2rem; }
.group-name-text {
  font-size: 1.05rem;
  font-weight: 700;
  color: #f1f5f9;
}
.member-pill {
  font-size: 0.72rem;
  background: #2a2a40;
  color: #aaa;
  border-radius: 20px;
  padding: 0.15rem 0.6rem;
}
.invite-btn {
  display: flex;
  align-items: center;
  gap: 0.4rem;
  background: #2a2a40;
  border: 1px solid #444;
  border-radius: 8px;
  padding: 0.35rem 0.75rem;
  color: #ccc;
  cursor: pointer;
  font-size: 0.85rem;
  font-family: monospace;
  letter-spacing: 0.08em;
  transition: background 0.15s;
  position: relative;
}
.invite-btn:hover { background: #35355a; color: #fff; }
.invite-btn svg { width: 0.95rem; height: 0.95rem; flex-shrink: 0; }
.invite-code-text { font-weight: 600; }
.copied-badge {
  position: absolute;
  top: -1.8rem;
  left: 50%;
  transform: translateX(-50%);
  background: #4a4a80;
  color: #fff;
  font-size: 0.7rem;
  padding: 0.15rem 0.5rem;
  border-radius: 6px;
  opacity: 0;
  pointer-events: none;
  transition: opacity 0.2s;
  white-space: nowrap;
  font-family: sans-serif;
  letter-spacing: 0;
}
.copied-badge.visible { opacity: 1; }

/* Tabs */
.tabs-bar {
  display: flex;
  gap: 4px;
  background: #16161e;
  border: 1px solid #333;
  border-radius: 10px;
  padding: 3px;
  margin-bottom: 1.25rem;
}
.tab-btn {
  flex: 1;
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 0.45rem;
  padding: 0.45rem 0.75rem;
  border: none;
  border-radius: 7px;
  background: transparent;
  color: #888;
  font-size: 0.9rem;
  font-weight: 500;
  cursor: pointer;
  transition: background 0.15s, color 0.15s;
}
.tab-btn.active { background: #4a4a80; color: #fff; }
.tab-badge {
  background: rgba(255,255,255,0.1);
  color: #aaa;
  border-radius: 20px;
  padding: 0.05rem 0.45rem;
  font-size: 0.75rem;
  font-weight: 600;
}
.tab-btn.active .tab-badge { background: rgba(255,255,255,0.2); color: #fff; }
.tab-badge.highlight { background: #7c3aed44; color: #a78bfa; }

.tab-content { }

/* Accordion */
.accordion-list {
  display: flex;
  flex-direction: column;
  gap: 0.5rem;
}
.accordion-item {
  background: var(--card-bg, #1e1e2e);
  border: 1px solid #333;
  border-radius: 10px;
  overflow: hidden;
  transition: border-color 0.15s;
}
.accordion-item.open { border-color: #4a4a80; }
.accordion-header {
  width: 100%;
  display: flex;
  align-items: center;
  gap: 0.75rem;
  padding: 0.75rem 1rem;
  background: none;
  border: none;
  cursor: pointer;
  text-align: left;
  color: inherit;
}
.accordion-header:hover { background: rgba(255,255,255,0.03); }
.member-avatar {
  width: 34px;
  height: 34px;
  background: #4a4a80;
  border-radius: 50%;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 0.95rem;
  font-weight: 700;
  color: #fff;
  flex-shrink: 0;
}
.member-name {
  font-size: 0.95rem;
  font-weight: 600;
  flex: 1;
}
.member-counts {
  display: flex;
  gap: 0.4rem;
}
.count-badge {
  font-size: 0.78rem;
  padding: 0.15rem 0.5rem;
  border-radius: 20px;
}
.count-badge.fav { background: rgba(220,38,38,0.15); color: #f87171; }
.count-badge.buy { background: rgba(34,197,94,0.12); color: #4ade80; }
.chevron {
  width: 1rem;
  height: 1rem;
  color: #666;
  flex-shrink: 0;
  transition: transform 0.2s;
}
.accordion-item.open .chevron { transform: rotate(180deg); }

.accordion-body {
  display: grid;
  grid-template-rows: 0fr;
  transition: grid-template-rows 0.25s ease;
}
.accordion-item.open .accordion-body {
  grid-template-rows: 1fr;
}
.accordion-body-inner {
  overflow: hidden;
  padding: 0 1rem;
}
.accordion-item.open .accordion-body-inner {
  padding: 0 1rem 0.85rem;
}

.member-section {
  background: rgba(255,255,255,0.03);
  border-radius: 8px;
  padding: 0.5rem 0.6rem;
}
.section-label {
  font-size: 0.75rem;
  font-weight: 600;
  color: #aaa;
  margin-bottom: 0.4rem;
}
.member-empty {
  font-size: 0.75rem;
  color: #555;
  padding: 0.2rem 0;
}
.game-chips {
  display: flex;
  flex-direction: column;
  gap: 0.3rem;
}
.game-chip {
  display: flex;
  align-items: center;
  gap: 0.5rem;
}
.chip-thumb {
  width: 24px;
  height: 32px;
  object-fit: cover;
  border-radius: 3px;
  flex-shrink: 0;
}
.chip-thumb-ph {
  width: 24px;
  height: 32px;
  background: #2a2a3e;
  border-radius: 3px;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 0.75rem;
  flex-shrink: 0;
}
.chip-name {
  font-size: 0.8rem;
  color: #ddd;
  line-height: 1.3;
}

/* Insights */
.insights-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(260px, 1fr));
  gap: 0.75rem;
}
.insight-card {
  background: var(--card-bg, #1e1e2e);
  border: 1px solid #333;
  border-radius: 10px;
  display: flex;
  gap: 0.75rem;
  padding: 0.75rem;
  align-items: flex-start;
}
.insight-cover {
  width: 56px;
  height: 75px;
  object-fit: cover;
  border-radius: 6px;
  flex-shrink: 0;
}
.insight-cover-placeholder {
  width: 56px;
  height: 75px;
  background: #2a2a3e;
  border-radius: 6px;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 1.4rem;
  flex-shrink: 0;
}
.insight-name {
  font-size: 0.88rem;
  font-weight: 600;
  margin-bottom: 0.2rem;
}
.insight-date {
  font-size: 0.72rem;
  color: #aaa;
  margin-bottom: 0.25rem;
}
.insight-users {
  font-size: 0.72rem;
  color: #ccc;
  margin-top: 0.2rem;
  margin-bottom: 0;
}
.empty-state {
  text-align: center;
  color: #888;
  padding: 3rem 0;
}
</style>
