<template>
  <div class="group-container">
    <h2 class="group-title">Mi Grupo</h2>

    <!-- Sin grupo -->
    <div v-if="!groupId" class="group-actions">
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
    <div v-else-if="groupId">
      <div v-if="groupInfo" class="group-header-card mb-4">
        <h4>{{ groupInfo.name }}</h4>
        <p class="text-muted mb-1">Miembros: {{ groupInfo.memberUsernames.join(', ') }}</p>
        <p class="invite-code">
          Código de invitación: <strong>{{ groupInfo.inviteCode }}</strong>
        </p>
      </div>

      <!-- Tarjetas por miembro -->
      <h4 class="section-heading mb-3">Listas de miembros</h4>
      <div v-if="loadingInsights" class="text-center py-4">
        <div class="spinner-border" role="status" />
      </div>
      <div v-else-if="memberGames.length === 0" class="empty-state mb-4">
        <p>Aún no hay juegos marcados en el grupo.</p>
      </div>
      <div v-else class="members-grid mb-5">
        <div v-for="member in memberGames" :key="member.username" class="member-card">
          <div class="member-card-header">
            <span class="member-avatar">{{ member.username[0].toUpperCase() }}</span>
            <span class="member-name">{{ member.username }}</span>
          </div>

          <div v-if="member.favorites.length > 0" class="member-section">
            <p class="section-label">❤️ Favoritos ({{ member.favorites.length }})</p>
            <ul class="game-list">
              <li v-for="g in member.favorites" :key="g.gameId" class="game-list-item">
                <img v-if="g.coverImageUrl" :src="g.coverImageUrl" :alt="g.gameName" class="game-list-thumb" />
                <span v-else class="game-list-thumb-placeholder">🎮</span>
                <span class="game-list-name">{{ g.gameName }}</span>
              </li>
            </ul>
          </div>
          <div v-else class="member-empty">Sin favoritos</div>

          <div v-if="member.purchases.length > 0" class="member-section mt-2">
            <p class="section-label">✅ Tiene ({{ member.purchases.length }})</p>
            <ul class="game-list">
              <li v-for="g in member.purchases" :key="g.gameId" class="game-list-item">
                <img v-if="g.coverImageUrl" :src="g.coverImageUrl" :alt="g.gameName" class="game-list-thumb" />
                <span v-else class="game-list-thumb-placeholder">🎮</span>
                <span class="game-list-name">{{ g.gameName }}</span>
              </li>
            </ul>
          </div>
          <div v-else class="member-empty">Sin compras</div>
        </div>
      </div>

      <!-- Coincidencias del grupo -->
      <h4 class="section-heading mb-3">Coincidencias del grupo</h4>
      <div v-if="loadingInsights" class="text-center py-4">
        <div class="spinner-border" role="status" />
      </div>
      <div v-else-if="insights.length === 0" class="empty-state">
        <p>Aún no hay juegos marcados en el grupo.</p>
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
            <p v-if="game.purchasedBy.length" class="insight-users">Tienen: {{ game.purchasedBy.join(', ') }}</p>
          </div>
        </div>
      </div>
    </div>
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

const groupInfo = ref(null)
const insights = ref([])
const memberGames = ref([])
const loadingInsights = ref(false)

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
  } catch (e) {
    joinError.value = 'Código inválido o grupo no encontrado'
  } finally {
    joining.value = false
  }
}
</script>

<style scoped>
.group-container {
  max-width: 960px;
  margin: 0 auto;
  padding: 2rem 1rem;
}
.group-title {
  font-size: 1.75rem;
  font-weight: 700;
  margin-bottom: 1.5rem;
}
.section-heading {
  font-size: 1.1rem;
  font-weight: 600;
  color: #ccc;
  border-bottom: 1px solid #333;
  padding-bottom: 0.4rem;
}
.action-card, .group-header-card {
  background: var(--card-bg, #1e1e2e);
  border: 1px solid var(--border-color, #333);
  border-radius: 12px;
  padding: 1.5rem;
}
.invite-code {
  font-size: 0.95rem;
  letter-spacing: 0.05em;
}

/* Members grid */
.members-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(280px, 1fr));
  gap: 1rem;
}
.member-card {
  background: var(--card-bg, #1e1e2e);
  border: 1px solid var(--border-color, #333);
  border-radius: 12px;
  padding: 1rem;
  display: flex;
  flex-direction: column;
  gap: 0.25rem;
}
.member-card-header {
  display: flex;
  align-items: center;
  gap: 0.6rem;
  margin-bottom: 0.75rem;
}
.member-avatar {
  width: 36px;
  height: 36px;
  background: #4a4a80;
  border-radius: 50%;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 1rem;
  font-weight: 700;
  color: #fff;
  flex-shrink: 0;
}
.member-name {
  font-size: 1rem;
  font-weight: 600;
}
.member-section {
  background: rgba(255,255,255,0.03);
  border-radius: 8px;
  padding: 0.5rem 0.6rem;
}
.section-label {
  font-size: 0.78rem;
  font-weight: 600;
  color: #aaa;
  margin-bottom: 0.4rem;
}
.member-empty {
  font-size: 0.75rem;
  color: #555;
  padding: 0.25rem 0;
}
.game-list {
  list-style: none;
  padding: 0;
  margin: 0;
  display: flex;
  flex-direction: column;
  gap: 0.35rem;
}
.game-list-item {
  display: flex;
  align-items: center;
  gap: 0.5rem;
}
.game-list-thumb {
  width: 28px;
  height: 36px;
  object-fit: cover;
  border-radius: 4px;
  flex-shrink: 0;
}
.game-list-thumb-placeholder {
  width: 28px;
  height: 36px;
  background: #2a2a3e;
  border-radius: 4px;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 0.9rem;
  flex-shrink: 0;
}
.game-list-name {
  font-size: 0.8rem;
  color: #ddd;
  line-height: 1.3;
}

/* Insights grid */
.insights-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(260px, 1fr));
  gap: 1rem;
}
.insight-card {
  background: var(--card-bg, #1e1e2e);
  border: 1px solid var(--border-color, #333);
  border-radius: 10px;
  display: flex;
  gap: 0.75rem;
  padding: 0.75rem;
  align-items: flex-start;
}
.insight-cover {
  width: 60px;
  height: 80px;
  object-fit: cover;
  border-radius: 6px;
  flex-shrink: 0;
}
.insight-cover-placeholder {
  width: 60px;
  height: 80px;
  background: #2a2a3e;
  border-radius: 6px;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 1.5rem;
  flex-shrink: 0;
}
.insight-name {
  font-size: 0.9rem;
  font-weight: 600;
  margin-bottom: 0.25rem;
}
.insight-date {
  font-size: 0.75rem;
  color: #aaa;
  margin-bottom: 0.25rem;
}
.insight-users {
  font-size: 0.75rem;
  color: #ccc;
  margin-top: 0.25rem;
  margin-bottom: 0;
}
.empty-state {
  text-align: center;
  color: #888;
  padding: 2rem;
}
</style>
