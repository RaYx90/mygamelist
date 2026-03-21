<template>
  <div
    class="day-cell"
    :class="[
      releases.length > 0 ? 'has-releases' : 'empty-day',
      isToday ? 'today' : ''
    ]"
  >
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
        tabindex="0"
        @click="emit('show-more', date)"
        @keydown.enter="emit('show-more', date)"
      >+{{ releases.length - visibleCount }} más</div>
    </div>
  </div>
</template>

<script setup>
import { computed, toRef } from 'vue'
import ReleaseCard from '../game/ReleaseCard.vue'
import { useGameSocialData } from '../../composables/useGameSocialData.js'
import { useIsMobile } from '../../composables/useIsMobile.js'

const props = defineProps({
  date: { type: String, required: true },
  releases: { type: Array, default: () => [] },
  gameStatus: { type: Object, default: null }
})
const emit = defineEmits(['game-selected', 'show-more'])

const isMobile = useIsMobile()

const visibleCount = computed(() => isMobile.value ? 5 : 3)
const dayNumber = computed(() => parseInt(props.date.split('-')[2]))

const WEEKDAYS = ['Dom', 'Lun', 'Mar', 'Mié', 'Jue', 'Vie', 'Sáb']
const weekdayName = computed(() => {
  const [y, m, d] = props.date.split('-').map(Number)
  return WEEKDAYS[new Date(y, m - 1, d).getDay()]
})

const todayStr = (() => {
  const t = new Date()
  return `${t.getFullYear()}-${String(t.getMonth() + 1).padStart(2, '0')}-${String(t.getDate()).padStart(2, '0')}`
})()
const isToday = computed(() => props.date === todayStr)

const { getForGame: getGameSocialData } = useGameSocialData(toRef(props, 'gameStatus'))
</script>
