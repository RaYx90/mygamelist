<template>
  <div class="day-view">
    <!-- Navegación de día -->
    <div class="day-nav">
      <button class="day-nav-btn" @click="emit('prev-day')" title="Día anterior">&#8249;</button>
      <span class="day-nav-date">{{ formattedDate }}</span>
      <button class="day-nav-btn" @click="emit('next-day')" title="Día siguiente">&#8250;</button>
    </div>

    <div v-if="isLoading" class="empty-state">
      <div class="spinner-border" role="status" />
    </div>
    <div v-else-if="releases.length === 0" class="empty-state">
      <div class="empty-state-icon">📅</div>
      <p class="empty-state-text">No hay lanzamientos este día.</p>
    </div>
    <div v-else class="day-releases-list">
      <ReleaseCard
        v-for="r in releases"
        :key="r.id"
        :release="r"
        v-bind="getGameSocialData(r.gameId)"
        @selected="emit('game-selected', $event)"
      />
    </div>
  </div>
</template>

<script setup>
import { computed, toRef } from 'vue'
import ReleaseCard from '../game/ReleaseCard.vue'
import { useGameSocialData } from '../../composables/useGameSocialData.js'

const props = defineProps({
  date: { type: String, required: true },
  releases: { type: Array, default: () => [] },
  gameStatus: { type: Object, default: null },
  isLoading: { type: Boolean, default: false },
})

const emit = defineEmits(['prev-day', 'next-day', 'game-selected'])

const { getForGame: getGameSocialData } = useGameSocialData(toRef(props, 'gameStatus'))

const formattedDate = computed(() => {
  const d = new Date(props.date + 'T00:00:00')
  return d.toLocaleDateString('es-ES', { weekday: 'long', day: 'numeric', month: 'long' })
})
</script>

<style scoped>
.day-view {
  max-width: 640px;
  margin: 0 auto;
}

.day-nav {
  display: flex;
  align-items: center;
  justify-content: space-between;
  margin-bottom: 1.5rem;
  background: var(--card-bg, #1e1e2e);
  border: 1px solid #333;
  border-radius: 12px;
  padding: 0.75rem 1rem;
}

.day-nav-btn {
  background: none;
  border: none;
  color: #ccc;
  font-size: 1.8rem;
  line-height: 1;
  cursor: pointer;
  padding: 0 0.5rem;
  transition: color 0.15s;
}
.day-nav-btn:hover { color: #fff; }

.day-nav-date {
  font-size: 1rem;
  font-weight: 600;
  color: #fff;
  text-transform: capitalize;
}

.day-releases-list {
  display: flex;
  flex-direction: column;
  gap: 0.75rem;
}

.empty-state {
  text-align: center;
  color: #888;
  padding: 3rem 0;
}
</style>
