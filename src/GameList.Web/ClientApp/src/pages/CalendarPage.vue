<template>
  <main class="app-main">
    <div class="calendar-container">
      <div class="calendar-controls">
        <div class="filters-bar">
          <!-- Toggle de vista -->
          <div class="view-toggle">
            <button class="view-toggle-btn" :class="{ active: selectedView === 'calendar' }" @click="selectedView = 'calendar'">Mes</button>
            <button class="view-toggle-btn" :class="{ active: selectedView === 'day' }" @click="selectedView = 'day'">Día</button>
          </div>

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
          <CategoryFilter
            :selected-category="selectedCategory"
            @category-changed="handleCategoryChanged"
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

      <!-- Vista Mes (calendario) -->
      <template v-else-if="selectedView === 'calendar'">
        <div v-if="filteredCalendarDays.length === 0" class="empty-state">
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
      </template>

      <!-- Vista Día -->
      <DayView
        v-else
        :date="selectedDay"
        :releases="releasesForDay(selectedDay)"
        :game-status="gameStatus"
        :is-loading="isLoading"
        @prev-day="handlePrevDay"
        @next-day="handleNextDay"
        @game-selected="openGameDetail"
      />
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
    :can-go-back="!!previousDayDate"
    @close="closeGameDetail"
    @go-back="goBackToDayReleases"
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
import CategoryFilter from '../components/filters/CategoryFilter.vue'
import DayCell from '../components/calendar/DayCell.vue'
import DayView from '../components/calendar/DayView.vue'
import DayReleasesModal from '../components/calendar/DayReleasesModal.vue'
import GameDetailModal from '../components/game/GameDetailModal.vue'

const WEEK_DAYS = ['Lun', 'Mar', 'Mié', 'Jue', 'Vie', 'Sáb', 'Dom']

const route = useRoute()
const {
  currentYear, selectedMonth, selectedPlatformId, selectedCategory, searchTerm,
  isLoading, platforms, monthName, firstDayColumnStart,
  allDaysInMonth, filteredCalendarDays, calendarDays,
  selectedDay, releasesForDay, loadReleases, loadPlatforms,
  onMonthChanged, onPlatformChanged, onCategoryChanged, onSearchChanged,
  goToPrevDay, goToNextDay,
} = useCalendar()

const { gameStatus, loadStatus, toggleFavorite, togglePurchase } = useGameStatus()

const selectedView = ref(window.innerWidth < 640 ? 'day' : 'calendar')
const selectedGame = ref(null)
const selectedDayReleases = ref(null)
const selectedDayDate = ref(null)
const previousDayDate = ref(null)

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

async function handleCategoryChanged(category) {
  await onCategoryChanged(category)
}

async function handlePrevDay() {
  const monthChanged = await goToPrevDay()
  if (monthChanged) await loadStatus(calendarDays.value.flatMap(d => d.releases.map(r => r.gameId)))
}

async function handleNextDay() {
  const monthChanged = await goToNextDay()
  if (monthChanged) await loadStatus(calendarDays.value.flatMap(d => d.releases.map(r => r.gameId)))
}

function openGameDetail(release) {
  previousDayDate.value = selectedDayDate.value
  closeDayReleases()
  selectedGame.value = release
}

function closeGameDetail() {
  selectedGame.value = null
  previousDayDate.value = null
}

function goBackToDayReleases() {
  selectedGame.value = null
  if (previousDayDate.value) {
    openDayReleases(previousDayDate.value)
    previousDayDate.value = null
  }
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

<style scoped>
.view-toggle {
  display: flex;
  gap: 2px;
  background: #16161e;
  border: 1px solid #333;
  border-radius: 8px;
  padding: 2px;
}

.view-toggle-btn {
  padding: 0.3rem 0.85rem;
  border: none;
  border-radius: 6px;
  background: transparent;
  color: #888;
  font-size: 0.85rem;
  font-weight: 500;
  cursor: pointer;
  transition: background 0.15s, color 0.15s;
}

.view-toggle-btn.active {
  background: #4a4a80;
  color: #fff;
}
</style>
