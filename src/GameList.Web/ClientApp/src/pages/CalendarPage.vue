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
            :is-active="selectedPlatformId !== null"
            @platform-changed="handlePlatformChanged"
          />
          <CategoryFilter
            :selected-category="selectedCategory"
            :is-active="selectedCategory !== 0"
            @category-changed="handleCategoryChanged"
          />
          <div
            class="search-box"
            :class="{ 'search-box--active': searchTerm }"
          >
            <svg class="search-icon" viewBox="0 0 24 24" fill="none" stroke="currentColor"
                 stroke-width="2.5" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true">
              <circle cx="11" cy="11" r="7" />
              <line x1="16.5" y1="16.5" x2="22" y2="22" />
            </svg>
            <input
              id="search-games"
              class="search-input"
              type="search"
              name="search"
              placeholder="Buscar juegos..."
              :value="searchTerm"
              aria-label="Buscar juegos"
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

          <button
            v-if="hasActiveFilters"
            class="btn-clear-filters"
            @click="handleClearFilters"
            title="Limpiar todos los filtros"
          >&#x2715; Limpiar filtros</button>
        </div>
      </div>

      <!-- Vista Mes (calendario) -->
      <template v-if="selectedView === 'calendar'">
        <CalendarSkeleton v-if="isLoading" />
        <div v-else-if="filteredCalendarDays.length === 0" class="empty-state">
          <div class="empty-state-icon">🔍</div>
          <p class="empty-state-text" v-if="searchTerm">
            No se encontraron juegos que coincidan con "{{ searchTerm }}" en {{ monthName }}.
          </p>
          <p class="empty-state-text" v-else>
            No hay lanzamientos para {{ monthName }} {{ currentYear }}.
          </p>
          <p v-if="hasActiveFilters" class="empty-state-sub">
            <button class="btn-clear-filters" @click="handleClearFilters">Quitar filtros</button>
          </p>
          <!-- Búsqueda cross-month -->
          <div v-if="searchTerm && crossMonthResults.length === 0 && !crossMonthLoading" class="cross-month-hint">
            <button class="btn-search-year" @click="searchAcrossYear">
              🔎 Buscar "{{ searchTerm }}" en todo {{ currentYear }}
            </button>
          </div>
          <div v-if="crossMonthLoading" class="cross-month-hint">
            <span class="cross-month-loading">Buscando en todo {{ currentYear }}...</span>
          </div>
          <div v-if="crossMonthResults.length > 0" class="cross-month-results">
            <p class="cross-month-title">Encontrado en otros meses:</p>
            <div
              v-for="day in crossMonthResults"
              :key="day.date"
              class="cross-month-item"
            >
              <button
                class="cross-month-go"
                @click="goToMonth(day.date)"
                :title="`Ir a ${formatCrossMonthDate(day.date)}`"
              >
                📅 {{ formatCrossMonthDate(day.date) }}
                <span class="cross-month-games">
                  {{ day.releases.map(r => r.gameName).join(', ') }}
                </span>
                <span class="cross-month-arrow">→</span>
              </button>
            </div>
          </div>
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
import { ref, computed, onMounted, watch, nextTick } from 'vue'
import { useRoute } from 'vue-router'
import { useCalendar } from '../composables/useCalendar.js'
import { useGameStatus } from '../composables/useGameStatus.js'
import MonthNavigator from '../components/calendar/MonthNavigator.vue'
import PlatformFilter from '../components/filters/PlatformFilter.vue'
import CategoryFilter from '../components/filters/CategoryFilter.vue'
import CalendarSkeleton from '../components/calendar/CalendarSkeleton.vue'
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
  selectedDay, crossMonthResults, crossMonthLoading,
  releasesForDay, loadReleases, loadPlatforms,
  onMonthChanged, onPlatformChanged, onCategoryChanged, onSearchChanged, searchAcrossYear, clearFilters,
  goToPrevDay, goToNextDay,
} = useCalendar()

const { gameStatus, loadStatus, toggleFavorite, togglePurchase } = useGameStatus()

const selectedView = ref(window.innerWidth < 640 ? 'day' : 'calendar')

// Sincroniza el día seleccionado con el mes visible al cambiar a vista Día
watch(selectedView, (newView) => {
  if (newView === 'day') {
    const [, m] = selectedDay.value.split('-').map(Number)
    if (m !== selectedMonth.value) {
      selectedDay.value = `${currentYear}-${String(selectedMonth.value).padStart(2, '0')}-01`
    }
  }
})
const selectedGame = ref(null)
const selectedDayReleases = ref(null)
const selectedDayDate = ref(null)
const previousDayDate = ref(null)

const hasActiveFilters = computed(() =>
  selectedPlatformId.value !== null || selectedCategory.value !== 0 || searchTerm.value !== ''
)

onMounted(async () => {
  await Promise.all([
    loadPlatforms(),
    loadReleases().then(() => loadStatus(calendarDays.value.flatMap(d => d.releases.map(r => r.gameId))))
  ])
})

watch(() => route.query.month, async (val) => {
  const m = parseInt(val)
  if (m >= 1 && m <= 12 && m !== selectedMonth.value) {
    selectedMonth.value = m
    await loadReleases()
    await loadStatus(calendarDays.value.flatMap(d => d.releases.map(r => r.gameId)))
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

async function handleClearFilters() {
  await clearFilters()
  await loadStatus(calendarDays.value.flatMap(d => d.releases.map(r => r.gameId)))
}

async function handlePrevDay() {
  const monthChanged = await goToPrevDay()
  if (monthChanged) await loadStatus(calendarDays.value.flatMap(d => d.releases.map(r => r.gameId)))
}

async function handleNextDay() {
  const monthChanged = await goToNextDay()
  if (monthChanged) await loadStatus(calendarDays.value.flatMap(d => d.releases.map(r => r.gameId)))
}

function goToMonth(dateStr) {
  const month = parseInt(dateStr.split('-')[1])
  handleMonthChanged(month)
}

function formatCrossMonthDate(dateStr) {
  const d = new Date(dateStr + 'T00:00:00')
  return d.toLocaleDateString('es-ES', { day: 'numeric', month: 'long' })
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
.cross-month-hint {
  margin-top: 1rem;
}
.btn-search-year {
  background: rgba(99,102,241,0.12);
  border: 1px solid #4f46e5;
  border-radius: 8px;
  color: #a5b4fc;
  font-size: 0.85rem;
  padding: 0.45rem 1rem;
  cursor: pointer;
  transition: background 0.15s;
}
.btn-search-year:hover { background: rgba(99,102,241,0.22); }
.cross-month-loading { color: #6366f1; font-size: 0.85rem; }
.cross-month-results {
  margin-top: 1.25rem;
  max-width: 480px;
  width: 100%;
}
.cross-month-title {
  font-size: 0.78rem;
  color: #64748b;
  margin: 0 0 0.5rem;
  text-transform: uppercase;
  letter-spacing: 0.05em;
}
.cross-month-item { margin-bottom: 0.35rem; }
.cross-month-go {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  width: 100%;
  background: #1e1e35;
  border: 1px solid #3d3d5e;
  border-radius: 8px;
  color: #cbd5e1;
  font-size: 0.82rem;
  padding: 0.5rem 0.75rem;
  cursor: pointer;
  text-align: left;
  transition: border-color 0.15s, background 0.15s;
}
.cross-month-go:hover { border-color: #6366f1; background: #23233f; }
.cross-month-games {
  flex: 1;
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
  color: #94a3b8;
}
.cross-month-arrow { color: #6366f1; font-weight: 700; flex-shrink: 0; }

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
