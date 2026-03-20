<template>
  <select
    id="filter-category"
    class="filter-select"
    :class="{ 'filter-select--active': isActive }"
    :value="selectedCategory ?? ''"
    aria-label="Filtrar por tipo de juego"
    @change="emit('category-changed', $event.target.value === '' ? null : +$event.target.value)"
  >
    <option value="">Todos los tipos</option>
    <option v-for="c in CATEGORIES" :key="c.value" :value="c.value">{{ c.label }}</option>
  </select>
</template>

<script setup>
defineProps({
  selectedCategory: { type: Number, default: null },
  isActive: { type: Boolean, default: false }
})
const emit = defineEmits(['category-changed'])

const CATEGORIES = [
  { value: 0,  label: '🎮 Juego base' },
  { value: 8,  label: '🔄 Remake' },
  { value: 9,  label: '✨ Remasterización' },
  { value: 2,  label: '📦 Expansión' },
  { value: 4,  label: '🔓 Expansión independiente' },
  { value: 1,  label: '🧩 DLC' },
  { value: 3,  label: '🎁 Bundle' },
  { value: 10, label: '➕ Juego ampliado' },
  { value: 11, label: '🔀 Port' },
]
</script>
