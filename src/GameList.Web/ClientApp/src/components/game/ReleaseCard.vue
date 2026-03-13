<template>
  <div
    class="release-card"
    :class="release.releaseType === 1 ? 'exclusive' : 'multiplatform'"
    role="button"
    :title="`${release.gameName} — ${release.platformName}`"
    @click="emit('selected', release)"
  >
    <img
      v-if="release.coverImageUrl"
      :src="release.coverImageUrl"
      :alt="`${release.gameName} cover`"
      class="release-cover"
      loading="lazy"
    />
    <div v-else class="release-cover-placeholder">🎮</div>

    <div class="release-info">
      <span class="release-name">{{ release.gameName }}</span>
      <span
        class="release-platform badge"
        :class="release.releaseType === 1 ? 'bg-warning text-dark' : 'bg-info text-dark'"
      >{{ release.allPlatformLabels.join(', ') }}</span>
      <div v-if="favCount > 0 || purchasedCount > 0" class="release-social-badges">
        <span v-if="favCount > 0" class="social-pill fav" :class="{ mine: isFavorite }">
          ❤️ {{ favCount }}
        </span>
        <span v-if="purchasedCount > 0" class="social-pill purchased" :class="{ mine: isPurchased }">
          ✅ {{ purchasedCount }}
        </span>
      </div>
    </div>
  </div>
</template>

<script setup>
defineProps({
  release: { type: Object, required: true },
  favCount: { type: Number, default: 0 },
  purchasedCount: { type: Number, default: 0 },
  isFavorite: { type: Boolean, default: false },
  isPurchased: { type: Boolean, default: false }
})
const emit = defineEmits(['selected'])
</script>

<style scoped>
.release-social-badges {
  display: flex;
  gap: 0.25rem;
  margin-top: 0.2rem;
  flex-wrap: wrap;
}
.social-pill {
  font-size: 0.65rem;
  padding: 0.1rem 0.35rem;
  border-radius: 10px;
  background: rgba(255,255,255,0.08);
  color: #aaa;
  line-height: 1.4;
  white-space: nowrap;
}
.social-pill.mine {
  background: rgba(255,255,255,0.15);
  color: #fff;
}
</style>
