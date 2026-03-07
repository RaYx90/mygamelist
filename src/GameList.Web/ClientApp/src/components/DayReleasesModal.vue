<template>
  <div class="modal-overlay" @click="emit('close')">
    <div class="modal-content day-releases-modal" @click.stop>
      <button class="modal-close" @click="emit('close')">✕</button>
      <h3 class="day-releases-title">{{ formattedDate }} — {{ releases.length }} lanzamientos</h3>
      <div class="day-releases-list">
        <ReleaseCard
          v-for="r in releases"
          :key="r.id"
          :release="r"
          v-bind="getGameSocialData(r.gameId)"
          @selected="emit('game-selected', $event)"
        />
      </div>
    </div>
  </div>
</template>

<script setup>
import { computed } from 'vue'
import ReleaseCard from './ReleaseCard.vue'

const props = defineProps({
  date: { type: String, required: true },
  releases: { type: Array, required: true },
  gameStatus: { type: Object, default: null }
})
const emit = defineEmits(['close', 'game-selected'])

const formattedDate = computed(() => {
  const d = new Date(props.date + 'T00:00:00')
  return d.toLocaleDateString('es-ES', { month: 'long', day: 'numeric' })
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
