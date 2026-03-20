<template>
  <select
    id="filter-platform"
    class="filter-select"
    :class="{ 'filter-select--active': isActive }"
    :value="selectedPlatformId ?? ''"
    aria-label="Filtrar por plataforma"
    @change="emit('platform-changed', $event.target.value === '' ? null : +$event.target.value)"
  >
    <option value="">Todas las plataformas</option>
    <option v-for="p in platforms" :key="p.id" :value="p.id">{{ p.abbreviation || p.name }}</option>
  </select>
</template>

<script setup>
defineProps({
  platforms: { type: Array, required: true },
  selectedPlatformId: { type: Number, default: null },
  isActive: { type: Boolean, default: false }
})
const emit = defineEmits(['platform-changed'])
</script>
