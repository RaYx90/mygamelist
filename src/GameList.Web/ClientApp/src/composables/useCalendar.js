/**
 * Composable que gestiona todo el estado y la lógica del calendario de lanzamientos.
 * Extraído de CalendarPage.vue para reducirlo a solo template + coordinación.
 */
import { ref, computed } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { getReleases, getPlatforms } from '../api/gameApi.js'

const MONTH_NAMES = [
  'Enero', 'Febrero', 'Marzo', 'Abril', 'Mayo', 'Junio',
  'Julio', 'Agosto', 'Septiembre', 'Octubre', 'Noviembre', 'Diciembre'
]

export function useCalendar() {
  const route = useRoute()
  const router = useRouter()
  const currentYear = new Date().getFullYear()

  const selectedMonth = ref(parseInt(route.query.month) || new Date().getMonth() + 1)
  const selectedPlatformId = ref(null)
  const searchTerm = ref('')
  const isLoading = ref(true)
  const calendarDays = ref([])
  const platforms = ref([])

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
      calendarDays.value = await getReleases(currentYear, selectedMonth.value, selectedPlatformId.value)
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
    await loadReleases()
  }

  async function onPlatformChanged(id) {
    selectedPlatformId.value = id
    await loadReleases()
  }

  function onSearchChanged(term) {
    searchTerm.value = term
  }

  return {
    currentYear,
    selectedMonth,
    selectedPlatformId,
    searchTerm,
    isLoading,
    calendarDays,
    platforms,
    monthName,
    firstDayColumnStart,
    allDaysInMonth,
    filteredCalendarDays,
    releasesForDay,
    loadReleases,
    loadPlatforms,
    onMonthChanged,
    onPlatformChanged,
    onSearchChanged
  }
}
