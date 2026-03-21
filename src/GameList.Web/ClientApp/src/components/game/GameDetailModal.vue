<template>
  <div class="modal-overlay" @click="emit('close')">
    <div class="modal-content" @click.stop>
      <div class="modal-actions">
        <button v-if="canGoBack" class="modal-action-btn" @click="emit('go-back')" title="Volver al listado del día">←</button>
        <button class="modal-action-btn" @click="emit('close')">✕</button>
      </div>
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
          <div class="badges-row mb-2">
            <span class="game-badge badge-category">{{ categoryLabel }}</span>
            <span
              class="game-badge"
              :class="game.releaseType === 1 ? 'badge-exclusive' : 'badge-multi'"
            >
              {{ game.releaseType === 1
                ? 'Exclusivo de plataforma'
                : `Multiplataforma (${game.allPlatformLabels.join(', ')})` }}
            </span>
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
          <div v-if="game.summaryEs || game.summary" class="game-summary mt-3">
            <p>{{ game.summaryEs || game.summary }}</p>
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
              <span v-if="favoritedBy.length > 0" class="stat-badge fav expandable" role="button" tabindex="0" @click="showFavNames = !showFavNames" @keydown.enter="showFavNames = !showFavNames">
                ❤️ {{ favCount }} del grupo {{ favCount === 1 ? 'lo quiere' : 'lo quieren' }} <span class="expand-arrow">{{ showFavNames ? '▲' : '▼' }}</span>
              </span>
              <div v-if="showFavNames && favoritedBy.length > 0" class="stat-names">{{ favoritedBy.join(', ') }}</div>
              <span v-if="purchasedBy.length > 0" class="stat-badge purchased expandable" role="button" tabindex="0" @click="showPurNames = !showPurNames" @keydown.enter="showPurNames = !showPurNames">
                ✅ {{ purchasedBy.length }} ya {{ purchasedBy.length === 1 ? 'lo tiene' : 'lo tienen' }} <span class="expand-arrow">{{ showPurNames ? '▲' : '▼' }}</span>
              </span>
              <div v-if="showPurNames && purchasedBy.length > 0" class="stat-names">{{ purchasedBy.join(', ') }}</div>
            </div>
          </div>

          <div v-if="storeLinks.length > 0" class="store-links">
            <a
              v-for="link in storeLinks"
              :key="link.label"
              :href="link.url"
              class="store-link"
              target="_blank"
              rel="noopener noreferrer"
            >{{ link.icon }} {{ link.label }}</a>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, computed, onMounted, onBeforeUnmount } from 'vue'
import { useAuth } from '../../composables/useAuth.js'
import { useFormatDate } from '../../composables/useFormatDate.js'

const CATEGORY_LABELS = {
  0: '🎮 Juego base',
  1: '🧩 DLC',
  2: '📦 Expansión',
  3: '🎁 Bundle',
  4: '🔓 Expansión independiente',
  5: '🔧 Mod',
  6: '📺 Episodio',
  7: '📅 Temporada',
  8: '🔄 Remake',
  9: '✨ Remasterización',
  10: '➕ Juego ampliado',
  11: '🔀 Port',
  12: '🍴 Fork',
  13: '📦 Pack',
  14: '🔃 Actualización',
  99: '❓ Desconocido'
}

const props = defineProps({
  game: { type: Object, required: true },
  isFavorite: { type: Boolean, default: false },
  isPurchased: { type: Boolean, default: false },
  favCount: { type: Number, default: 0 },
  favoritedBy: { type: Array, default: () => [] },
  purchasedBy: { type: Array, default: () => [] },
  canGoBack: { type: Boolean, default: false }
})
const emit = defineEmits(['close', 'toggle-favorite', 'toggle-purchase', 'go-back'])

function onEscape(e) { if (e.key === 'Escape') emit('close') }
onMounted(() => document.addEventListener('keydown', onEscape))
onBeforeUnmount(() => document.removeEventListener('keydown', onEscape))

const categoryLabel = computed(() => CATEGORY_LABELS[props.game.gameCategory] ?? CATEGORY_LABELS[99])

const { isLoggedIn } = useAuth()
const { formatDate } = useFormatDate()

const showFavNames = ref(false)
const showPurNames = ref(false)

const storeLinks = computed(() => {
  const links = []
  const platforms = props.game.allPlatformLabels ?? []
  const name = encodeURIComponent(props.game.gameName)

  if (platforms.some(p => /\bpc\b|windows/i.test(p))) {
    links.push({ label: 'Steam', url: `https://store.steampowered.com/search/?term=${name}`, icon: '🖥️' })
  }
  if (platforms.some(p => /playstation|ps[45]/i.test(p))) {
    links.push({ label: 'PS Store', url: `https://store.playstation.com/es-es/search/${name}`, icon: '🎮' })
  }
  if (platforms.some(p => /switch/i.test(p))) {
    links.push({ label: 'Nintendo', url: `https://www.nintendo.com/search/#q=${name}&p=1&cat=gme&sort=df`, icon: '🔴' })
  }
  if (platforms.some(p => /xbox|series x|series s/i.test(p))) {
    links.push({ label: 'Xbox', url: `https://www.xbox.com/es-ES/Search/Results?q=${name}`, icon: '🟢' })
  }
  if (props.game.gameSlug) {
    links.push({ label: 'IGDB', url: `https://www.igdb.com/games/${props.game.gameSlug}`, icon: '📋' })
  }
  return links
})
</script>

<style scoped>
.modal-actions {
  position: absolute;
  top: 1rem;
  right: 1rem;
  display: flex;
  gap: 0.4rem;
  z-index: 1;
}
.modal-action-btn {
  background: #2d2d4e;
  border: none;
  color: #94a3b8;
  border-radius: 6px;
  padding: 0.3rem 0.6rem;
  cursor: pointer;
  font-size: 0.9rem;
  transition: background 0.15s, color 0.15s;
}
.modal-action-btn:hover {
  background: #3d3d5e;
  color: #e2e8f0;
}
.badges-row {
  display: flex;
  flex-wrap: wrap;
  gap: 0.5rem;
  align-items: stretch;
}
.game-badge {
  display: inline-flex;
  align-items: center;
  font-size: 0.75rem;
  font-weight: 600;
  padding: 0.25rem 0.65rem;
  border-radius: 20px;
  line-height: 1.2;
}
.badge-category {
  background: #6c3483;
  color: #fff;
}
.badge-exclusive {
  background: #f59e0b;
  color: #1a1a2e;
}
.badge-multi {
  background: #06b6d4;
  color: #1a1a2e;
}
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
.stat-badge.expandable { cursor: pointer; transition: background 0.15s; }
.stat-badge.expandable:hover { background: rgba(255,255,255,0.12); }
.expand-arrow { font-size: 0.6rem; opacity: 0.6; margin-left: 0.2rem; }
.stat-names {
  width: 100%;
  font-size: 0.75rem;
  color: #94a3b8;
  padding: 0.3rem 0.6rem;
  background: rgba(255,255,255,0.04);
  border-radius: 8px;
}
</style>
