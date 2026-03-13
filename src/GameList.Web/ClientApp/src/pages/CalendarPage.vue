<template>
  <main class="app-main">
    <div class="calendar-container">
      <div class="calendar-controls">
        <div class="filters-bar">
          <MonthNavigator
            :selected-month="selectedMonth"
            :current-year="currentYear"
            @month-changed="handleMonthChanged"
          />
          <PlatformFilter
            :platforms="platforms"
            :selected-platform-id="selectedPlatformId"
            @platform-changed="handlePlatformChanged"
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
        <div class="week-header-cell" v-for="wd in WEEK_DAYS" :key="wd">{{ wd }}</div>
        <DayCell
          v-for="(day, index) in allDaysInMonth"
          :key="day"
          :date="day"
          :releases="releasesForDay(day)"
          :game-status="gameStatus"
          :style="index === 0 ? { gridColumnStart: firstDayColumnStart } : undefined"
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
import { ref, onMounted, watch } from 'vue'
import { useRoute } from 'vue-router'
import { useCalendar } from '../composables/useCalendar.js'
import { useGameStatus } from '../composables/useGameStatus.js'
import MonthNavigator from '../components/calendar/MonthNavigator.vue'
import PlatformFilter from '../components/filters/PlatformFilter.vue'
import DayCell from '../components/calendar/DayCell.vue'
import DayReleasesModal from '../components/calendar/DayReleasesModal.vue'
import GameDetailModal from '../components/game/GameDetailModal.vue'

const WEEK_DAYS = ['Lun', 'Mar', 'Mié', 'Jue', 'Vie', 'Sáb', 'Dom']

const route = useRoute()
const {
  currentYear, selectedMonth, selectedPlatformId, searchTerm,
  isLoading, platforms, monthName, firstDayColumnStart,
  allDaysInMonth, filteredCalendarDays, calendarDays,
  releasesForDay, loadReleases, loadPlatforms,
  onMonthChanged, onPlatformChanged, onSearchChanged
} = useCalendar()

const { gameStatus, loadStatus, toggleFavorite, togglePurchase } = useGameStatus()

const selectedGame = ref(null)
const selectedDayReleases = ref(null)
const selectedDayDate = ref(null)

onMounted(async () => {
  await Promise.all([
    loadPlatforms(),
    loadReleases().then(() => loadStatus(calendarDays.value.flatMap(d => d.releases.map(r => r.gameId))))
  ])
})

watch(() => route.query.month, (val) => {
  const m = parseInt(val)
  if (m >= 1 && m <= 12 && m !== selectedMonth.value) {
    selectedMonth.value = m
    loadReleases()
  }
})

async function handleMonthChanged(month) {
  await onMonthChanged(month)
  await loadStatus(calendarDays.value.flatMap(d => d.releases.map(r => r.gameId)))
}

async function handlePlatformChanged(id) {
  await onPlatformChanged(id)
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
</script>
