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
import { computed, toRef, onMounted, onBeforeUnmount } from 'vue'
import ReleaseCard from '../game/ReleaseCard.vue'
import { useGameSocialData } from '../../composables/useGameSocialData.js'

const props = defineProps({
  date: { type: String, required: true },
  releases: { type: Array, required: true },
  gameStatus: { type: Object, default: null }
})
const emit = defineEmits(['close', 'game-selected'])

function onEscape(e) { if (e.key === 'Escape') emit('close') }
onMounted(() => document.addEventListener('keydown', onEscape))
onBeforeUnmount(() => document.removeEventListener('keydown', onEscape))

const formattedDate = computed(() => {
  const d = new Date(props.date + 'T00:00:00')
  return d.toLocaleDateString('es-ES', { month: 'long', day: 'numeric' })
})

const { getForGame: getGameSocialData } = useGameSocialData(toRef(props, 'gameStatus'))
</script>
