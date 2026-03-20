/**
 * Composable que gestiona todo el estado y la lógica del calendario de lanzamientos.
 * Extraído de CalendarPage.vue para reducirlo a solo template + coordinación.
 */
import { ref, computed } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { getReleases, getPlatforms, searchReleases } from '../api/gameApi.js'

const MONTH_NAMES = [
  'Enero', 'Febrero', 'Marzo', 'Abril', 'Mayo', 'Junio',
  'Julio', 'Agosto', 'Septiembre', 'Octubre', 'Noviembre', 'Diciembre'
]

function toDateStr(date) {
  return `${date.getFullYear()}-${String(date.getMonth() + 1).padStart(2, '0')}-${String(date.getDate()).padStart(2, '0')}`
}

export function useCalendar() {
  const route = useRoute()
  const router = useRouter()
  const currentYear = new Date().getFullYear()

  const selectedMonth = ref(parseInt(route.query.month) || new Date().getMonth() + 1)
  const selectedPlatformId = ref(null)
  const selectedCategory = ref(0) // 0 = MainGame por defecto; null = sin filtro
  const searchTerm = ref('')
  const isLoading = ref(true)
  const calendarDays = ref([])
  const platforms = ref([])
  const selectedDay = ref(toDateStr(new Date()))
  const crossMonthResults = ref([])
  const crossMonthLoading = ref(false)

  const monthName = computed(() => MONTH_NAMES[selectedMonth.value - 1])

  const firstDayColumnStart = computed(() => {
    const dayOfWeek = new Date(currentYear, selectedMonth.value - 1, 1).getDay()
    return ((dayOfWeek - 1 + 7) % 7) + 1
  })

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
      calendarDays.value = await getReleases(currentYear, selectedMonth.value, selectedPlatformId.value, selectedCategory.value)
    } finally {
      isLoading.value = false
    }
  }

  async function loadPlatforms() {
    platforms.value = await getPlatforms()
  }

  async function onMonthChanged(month) {
    selectedMonth.value = month
    router.replace({ query: { ...route.query, month } })
    selectedDay.value = `${currentYear}-${String(month).padStart(2, '0')}-01`
    await loadReleases()
  }

  async function onPlatformChanged(id) {
    selectedPlatformId.value = id
    await loadReleases()
  }

  async function onCategoryChanged(category) {
    selectedCategory.value = category
    await loadReleases()
  }

  function onSearchChanged(term) {
    searchTerm.value = term
    crossMonthResults.value = []
  }

  async function searchAcrossYear() {
    if (!searchTerm.value || searchTerm.value.length < 2) return
    crossMonthLoading.value = true
    try {
      crossMonthResults.value = await searchReleases(currentYear, searchTerm.value)
    } catch {
      crossMonthResults.value = []
    } finally {
      crossMonthLoading.value = false
    }
  }

  /** Resetea todos los filtros a su valor por defecto y recarga los datos. */
  async function clearFilters() {
    selectedPlatformId.value = null
    selectedCategory.value = 0
    searchTerm.value = ''
    crossMonthResults.value = []
    await loadReleases()
  }

  // Devuelve true si el mes cambió (el llamante puede recargar el status)
  async function goToPrevDay() {
    const [y, m, d] = selectedDay.value.split('-').map(Number)
    const prev = new Date(y, m - 1, d - 1)
    selectedDay.value = toDateStr(prev)
    const newMonth = prev.getMonth() + 1
    if (newMonth !== selectedMonth.value) {
      selectedMonth.value = newMonth
      router.replace({ query: { ...route.query, month: newMonth } })
      await loadReleases()
      return true
    }
    return false
  }

  // Devuelve true si el mes cambió
  async function goToNextDay() {
    const [y, m, d] = selectedDay.value.split('-').map(Number)
    const next = new Date(y, m - 1, d + 1)
    selectedDay.value = toDateStr(next)
    const newMonth = next.getMonth() + 1
    if (newMonth !== selectedMonth.value) {
      selectedMonth.value = newMonth
      router.replace({ query: { ...route.query, month: newMonth } })
      await loadReleases()
      return true
    }
    return false
  }

  return {
    currentYear,
    selectedMonth,
    selectedPlatformId,
    selectedCategory,
    searchTerm,
    isLoading,
    calendarDays,
    platforms,
    monthName,
    firstDayColumnStart,
    allDaysInMonth,
    filteredCalendarDays,
    selectedDay,
    crossMonthResults,
    crossMonthLoading,
    releasesForDay,
    loadReleases,
    loadPlatforms,
    onMonthChanged,
    onPlatformChanged,
    onCategoryChanged,
    onSearchChanged,
    searchAcrossYear,
    clearFilters,
    goToPrevDay,
    goToNextDay,
  }
}
