<template>
  <div class="day-cell" :class="releases.length > 0 ? 'has-releases' : 'empty-day'">
    <div class="day-header">
      <span class="day-number">{{ dayNumber }}</span>
      <span class="day-weekday">{{ weekdayName }}</span>
    </div>
    <div v-if="releases.length > 0" class="day-releases">
      <ReleaseCard
        v-for="r in releases.slice(0, visibleCount)"
        :key="r.id"
        :release="r"
        v-bind="getGameSocialData(r.gameId)"
        @selected="emit('game-selected', $event)"
      />
      <div
        v-if="releases.length > visibleCount"
        class="more-releases"
        role="button"
        @click="emit('show-more', date)"
      >+{{ releases.length - visibleCount }} más</div>
    </div>
  </div>
</template>

<script setup>
import { computed, ref, onMounted, onBeforeUnmount, toRef } from 'vue'
import ReleaseCard from '../game/ReleaseCard.vue'
import { useGameSocialData } from '../../composables/useGameSocialData.js'

const props = defineProps({
  date: { type: String, required: true },
  releases: { type: Array, default: () => [] },
  gameStatus: { type: Object, default: null }
})
const emit = defineEmits(['game-selected', 'show-more'])

const isMobile = ref(window.innerWidth <= 600)
function onResize() { isMobile.value = window.innerWidth <= 600 }
onMounted(() => window.addEventListener('resize', onResize))
onBeforeUnmount(() => window.removeEventListener('resize', onResize))

const visibleCount = computed(() => isMobile.value ? 5 : 3)
const dayNumber = computed(() => parseInt(props.date.split('-')[2]))

const WEEKDAYS = ['Dom', 'Lun', 'Mar', 'Mié', 'Jue', 'Vie', 'Sáb']
const weekdayName = computed(() => {
  const [y, m, d] = props.date.split('-').map(Number)
  return WEEKDAYS[new Date(y, m - 1, d).getDay()]
})

const { getForGame: getGameSocialData } = useGameSocialData(toRef(props, 'gameStatus'))
</script>
