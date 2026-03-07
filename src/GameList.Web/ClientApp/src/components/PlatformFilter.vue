<template>
  <select
    class="filter-select"
    :value="selectedPlatformId ?? ''"
    @change="emit('platform-changed', $event.target.value === '' ? null : +$event.target.value)"
  >
    <option value="">Todas las plataformas</option>
    <option v-for="p in platforms" :key="p.id" :value="p.id">{{ p.abbreviation || p.name }}</option>
  </select>
</template>

<script setup>
defineProps({
  platforms: { type: Array, required: true },
  selectedPlatformId: { type: Number, default: null }
})
const emit = defineEmits(['platform-changed'])
</script>
