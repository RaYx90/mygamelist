<template>
  <div class="day-cell" :class="releases.length > 0 ? 'has-releases' : 'empty-day'">
    <div class="day-header">
      <span class="day-number">{{ dayNumber }}</span>
      <span class="day-weekday">{{ weekdayName }}</span>
    </div>
    <div v-if="releases.length > 0" class="day-releases">
      <ReleaseCard
        v-for="r in releases.slice(0, 3)"
        :key="r.id"
        :release="r"
        v-bind="getGameSocialData(r.gameId)"
        @selected="emit('game-selected', $event)"
      />
      <div
        v-if="releases.length > 3"
        class="more-releases"
        role="button"
        @click="emit('show-more', date)"
      >+{{ releases.length - 3 }} más</div>
    </div>
  </div>
</template>

<script setup>
import { computed } from 'vue'
import ReleaseCard from './ReleaseCard.vue'

const props = defineProps({
  date: { type: String, required: true },
  releases: { type: Array, default: () => [] },
  gameStatus: { type: Object, default: null }
})
const emit = defineEmits(['game-selected', 'show-more'])

const dayNumber = computed(() => parseInt(props.date.split('-')[2]))

const WEEKDAYS = ['Dom', 'Lun', 'Mar', 'Mié', 'Jue', 'Vie', 'Sáb']
const weekdayName = computed(() => {
  const [y, m, d] = props.date.split('-').map(Number)
  return WEEKDAYS[new Date(y, m - 1, d).getDay()]
})

function getGameSocialData(gameId) {
  if (!props.gameStatus) return { favCount: 0, purchasedCount: 0, isFavorite: false, isPurchased: false }
  const isFavorite = props.gameStatus.myFavorites?.includes(gameId) ?? false
  const isPurchased = props.gameStatus.myPurchases?.includes(gameId) ?? false
  const otherFavs = props.gameStatus.favoritedCountInGroup?.[gameId] ?? 0
  const otherPurchases = props.gameStatus.purchasedByInGroup?.[gameId]?.length ?? 0
  return {
    favCount: (isFavorite ? 1 : 0) + otherFavs,
    purchasedCount: (isPurchased ? 1 : 0) + otherPurchases,
    isFavorite,
    isPurchased
  }
}
</script>
