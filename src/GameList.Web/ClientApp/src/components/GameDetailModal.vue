<template>
  <div class="modal-overlay" @click="emit('close')">
    <div class="modal-content" @click.stop>
      <button class="modal-close" @click="emit('close')">✕</button>
      <div class="modal-body">
        <div class="modal-cover">
          <img
            v-if="game.coverImageUrl"
            :src="game.coverImageUrl"
            :alt="`${game.gameName} cover`"
            class="detail-cover"
          />
          <div v-else class="detail-cover-placeholder">🎮</div>
        </div>
        <div class="modal-info">
          <h2 class="game-title">{{ game.gameName }}</h2>
          <div
            class="release-type-badge badge mb-2"
            :class="game.releaseType === 1 ? 'bg-warning text-dark' : 'bg-info text-dark'"
          >
            {{ game.releaseType === 1
              ? 'Exclusivo de plataforma'
              : `Multiplataforma (${game.allPlatformLabels.join(', ')})` }}
          </div>
          <div class="detail-row">
            <span class="detail-label">Plataforma:</span>
            <span class="detail-value">{{ game.allPlatformLabels.join(', ') }}</span>
          </div>
          <div class="detail-row">
            <span class="detail-label">Fecha de lanzamiento:</span>
            <span class="detail-value">{{ formatDate(game.releaseDate) }}</span>
          </div>
          <div v-if="game.region" class="detail-row">
            <span class="detail-label">Región:</span>
            <span class="detail-value">{{ game.region }}</span>
          </div>
          <div v-if="game.summary" class="game-summary mt-3">
            <p>{{ game.summary }}</p>
          </div>

          <div v-if="isLoggedIn" class="social-actions mt-3">
            <button
              class="btn-social"
              :class="{ active: isFavorite }"
              @click="emit('toggle-favorite', game.gameId)"
            >
              {{ isFavorite ? '❤️ Favorito' : '🤍 Añadir a favoritos' }}
            </button>
            <button
              class="btn-social ms-2"
              :class="{ active: isPurchased }"
              @click="emit('toggle-purchase', game.gameId)"
            >
              {{ isPurchased ? '✅ Comprado' : '🛒 Marcar como comprado' }}
            </button>
            <div class="social-group-stats mt-2">
              <span v-if="favCount > 0" class="stat-badge fav">
                ❤️ {{ favCount }} del grupo lo quieren
              </span>
              <span v-if="purchasedBy.length > 0" class="stat-badge purchased">
                ✅ {{ purchasedBy.join(', ') }} ya lo tienen
              </span>
            </div>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { useAuth } from '../stores/auth.js'

defineProps({
  game: { type: Object, required: true },
  isFavorite: { type: Boolean, default: false },
  isPurchased: { type: Boolean, default: false },
  favCount: { type: Number, default: 0 },
  purchasedBy: { type: Array, default: () => [] }
})
const emit = defineEmits(['close', 'toggle-favorite', 'toggle-purchase'])

const { isLoggedIn } = useAuth()

function formatDate(dateStr) {
  const d = new Date(dateStr + 'T00:00:00')
  return d.toLocaleDateString('es-ES', { month: 'long', day: 'numeric', year: 'numeric' })
}
</script>

<style scoped>
.social-actions {
  display: flex;
  flex-wrap: wrap;
  align-items: center;
  gap: 0.5rem;
}
.btn-social {
  background: #2a2a3e;
  border: 1px solid #444;
  border-radius: 8px;
  color: #ccc;
  cursor: pointer;
  font-size: 0.875rem;
  padding: 0.4rem 0.75rem;
  transition: background 0.15s, color 0.15s;
}
.btn-social:hover {
  background: #3a3a52;
  color: #fff;
}
.btn-social.active {
  background: #4a4a70;
  border-color: #7878c8;
  color: #fff;
}
.purchased-by {
  width: 100%;
}
.social-group-stats {
  display: flex;
  flex-wrap: wrap;
  gap: 0.5rem;
  width: 100%;
}
.stat-badge {
  font-size: 0.8rem;
  padding: 0.2rem 0.6rem;
  border-radius: 10px;
  background: rgba(255,255,255,0.07);
  color: #bbb;
}
.stat-badge.fav { color: #f97f7f; }
.stat-badge.purchased { color: #7fd97f; }
</style>

