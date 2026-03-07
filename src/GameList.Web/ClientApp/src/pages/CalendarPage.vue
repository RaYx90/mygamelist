<template>
  <main class="app-main">
    <div class="calendar-container">
      <div class="calendar-controls">
        <div class="filters-bar">
          <MonthNavigator
            :selected-month="selectedMonth"
            :current-year="currentYear"
            @month-changed="onMonthChanged"
          />
          <PlatformFilter
            :platforms="platforms"
            :selected-platform-id="selectedPlatformId"
            @platform-changed="onPlatformChanged"
          />
          <AudienceFilter
            :selected="selectedIsIndie"
            @audience-changed="onAudienceChanged"
          />
          <div class="search-box">
            <svg class="search-icon" viewBox="0 0 24 24" fill="none" stroke="currentColor"
                 stroke-width="2.5" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true">
              <circle cx="11" cy="11" r="7" />
              <line x1="16.5" y1="16.5" x2="22" y2="22" />
            </svg>
            <input
              class="search-input"
              type="text"
              placeholder="Buscar juegos..."
              :value="searchTerm"
              @input="onSearchChanged($event.target.value)"
            />
            <button
              v-if="searchTerm"
              class="search-clear"
              @click="onSearchChanged('')"
              title="Limpiar búsqueda"
              aria-label="Limpiar búsqueda"
            >&#x2715;</button>
          </div>
        </div>
      </div>

      <div v-if="isLoading" class="loading-spinner">
        <div class="spinner-border" role="status">
          <span class="visually-hidden">Cargando lanzamientos...</span>
        </div>
      </div>

      <div v-else-if="filteredCalendarDays.length === 0" class="empty-state">
        <p v-if="searchTerm">No se encontraron juegos que coincidan con "{{ searchTerm }}".</p>
        <p v-else>No hay lanzamientos para {{ monthName }} {{ currentYear }}.</p>
      </div>

      <div v-else class="calendar-grid">
        <DayCell
          v-for="day in allDaysInMonth"
          :key="day"
          :date="day"
          :releases="releasesForDay(day)"
          :game-status="gameStatus"
          @game-selected="openGameDetail"
          @show-more="openDayReleases"
        />
      </div>
    </div>
  </main>

  <DayReleasesModal
    v-if="selectedDayReleases"
    :date="selectedDayDate"
    :releases="selectedDayReleases"
    :game-status="gameStatus"
    @close="closeDayReleases"
    @game-selected="openGameDetail"
  />

  <GameDetailModal
    v-if="selectedGame"
    :game="selectedGame"
    :is-favorite="gameStatus?.myFavorites?.includes(selectedGame.gameId)"
    :is-purchased="gameStatus?.myPurchases?.includes(selectedGame.gameId)"
    :fav-count="(gameStatus?.myFavorites?.includes(selectedGame.gameId) ? 1 : 0) + (gameStatus?.favoritedCountInGroup?.[selectedGame.gameId] ?? 0)"
    :purchased-by="gameStatus?.purchasedByInGroup?.[selectedGame.gameId] ?? []"
    @close="closeGameDetail"
    @toggle-favorite="toggleFavorite"
    @toggle-purchase="togglePurchase"
  />
</template>

<script setup>
import { ref, computed, onMounted, watch } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { getReleases, getPlatforms } from '../api/gameApi.js'
import { getStatus, addFavorite, removeFavorite, markPurchased, unmarkPurchased } from '../api/socialApi.js'
import { useAuth } from '../stores/auth.js'
import MonthNavigator from '../components/MonthNavigator.vue'
import PlatformFilter from '../components/PlatformFilter.vue'
import AudienceFilter from '../components/AudienceFilter.vue'
import DayCell from '../components/DayCell.vue'
import DayReleasesModal from '../components/DayReleasesModal.vue'
import GameDetailModal from '../components/GameDetailModal.vue'

const MONTH_NAMES = [
  'Enero', 'Febrero', 'Marzo', 'Abril', 'Mayo', 'Junio',
  'Julio', 'Agosto', 'Septiembre', 'Octubre', 'Noviembre', 'Diciembre'
]

const route = useRoute()
const router = useRouter()
const auth = useAuth()

const currentYear = new Date().getFullYear()
const selectedMonth = ref(parseInt(route.query.month) || new Date().getMonth() + 1)
const selectedPlatformId = ref(null)
const selectedIsIndie = ref(null)
const searchTerm = ref('')

const isLoading = ref(true)
const calendarDays = ref([])
const platforms = ref([])
const gameStatus = ref(null)

const selectedGame = ref(null)
const selectedDayReleases = ref(null)
const selectedDayDate = ref(null)

const monthName = computed(() => MONTH_NAMES[selectedMonth.value - 1])

const allDaysInMonth = computed(() => {
  const count = new Date(currentYear, selectedMonth.value, 0).getDate()
  return Array.from({ length: count }, (_, i) => {
    const d = i + 1
    return `${currentYear}-${String(selectedMonth.value).padStart(2, '0')}-${String(d).padStart(2, '0')}`
  })
})

const filteredCalendarDays = computed(() => {
  if (!searchTerm.value) return calendarDays.value
  const term = searchTerm.value.toLowerCase()
  return calendarDays.value
    .map(day => ({ ...day, releases: day.releases.filter(r => r.gameName.toLowerCase().includes(term)) }))
    .filter(day => day.releases.length > 0)
})

function releasesForDay(dateStr) {
  return filteredCalendarDays.value.find(d => d.date === dateStr)?.releases ?? []
}

async function loadReleases() {
  isLoading.value = true
  try {
    calendarDays.value = await getReleases(currentYear, selectedMonth.value, selectedPlatformId.value, selectedIsIndie.value, auth.authHeader)
  } finally {
    isLoading.value = false
  }
}

async function loadStatus() {
  if (!auth.isLoggedIn) { gameStatus.value = null; return }
  try {
    const gameIds = calendarDays.value.flatMap(d => d.releases.map(r => r.gameId))
    gameStatus.value = await getStatus(auth.authHeader, gameIds)
  } catch {
    gameStatus.value = null
  }
}

onMounted(async () => {
  await Promise.all([
    getPlatforms(auth.authHeader).then(p => { platforms.value = p }),
    loadReleases().then(() => loadStatus())
  ])
})

watch(() => route.query.month, (val) => {
  const m = parseInt(val)
  if (m >= 1 && m <= 12 && m !== selectedMonth.value) {
    selectedMonth.value = m
    loadReleases()
  }
})

async function onMonthChanged(month) {
  selectedMonth.value = month
  router.replace({ query: { ...route.query, month } })
  await loadReleases()
  await loadStatus()
}

async function onPlatformChanged(id) {
  selectedPlatformId.value = id
  await loadReleases()
}

async function onAudienceChanged(isIndie) {
  selectedIsIndie.value = isIndie
  await loadReleases()
}

function onSearchChanged(term) {
  searchTerm.value = term
}

function openGameDetail(release) {
  closeDayReleases()
  selectedGame.value = release
}

function closeGameDetail() {
  selectedGame.value = null
}

function openDayReleases(dateStr) {
  selectedDayDate.value = dateStr
  selectedDayReleases.value = filteredCalendarDays.value.find(d => d.date === dateStr)?.releases ?? null
}

function closeDayReleases() {
  selectedDayReleases.value = null
  selectedDayDate.value = null
}

async function toggleFavorite(gameId) {
  if (!auth.isLoggedIn) return
  const isFav = gameStatus.value?.myFavorites?.includes(gameId)
  try {
    if (isFav) {
      await removeFavorite(gameId, auth.authHeader)
      gameStatus.value = { ...gameStatus.value, myFavorites: gameStatus.value.myFavorites.filter(id => id !== gameId) }
    } else {
      await addFavorite(gameId, auth.authHeader)
      gameStatus.value = { ...gameStatus.value, myFavorites: [...(gameStatus.value?.myFavorites ?? []), gameId] }
    }
  } catch (e) {
    console.error('Error toggling favorite', e)
  }
}

async function togglePurchase(gameId) {
  if (!auth.isLoggedIn) return
  const isPurchased = gameStatus.value?.myPurchases?.includes(gameId)
  try {
    if (isPurchased) {
      await unmarkPurchased(gameId, auth.authHeader)
      gameStatus.value = { ...gameStatus.value, myPurchases: gameStatus.value.myPurchases.filter(id => id !== gameId) }
    } else {
      await markPurchased(gameId, auth.authHeader)
      gameStatus.value = { ...gameStatus.value, myPurchases: [...(gameStatus.value?.myPurchases ?? []), gameId] }
    }
  } catch (e) {
    console.error('Error toggling purchase', e)
  }
}
</script>
