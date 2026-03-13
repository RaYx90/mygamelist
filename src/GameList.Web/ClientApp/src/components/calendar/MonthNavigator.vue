<template>
  <select class="filter-select" :value="selectedMonth" @change="emit('month-changed', +$event.target.value)">
    <option v-for="(name, i) in MONTHS" :key="i + 1" :value="i + 1">{{ name }}</option>
  </select>
</template>

<script setup>
const MONTHS = ['Enero', 'Febrero', 'Marzo', 'Abril', 'Mayo', 'Junio', 'Julio', 'Agosto', 'Septiembre', 'Octubre', 'Noviembre', 'Diciembre']

defineProps({
  selectedMonth: { type: Number, required: true },
  currentYear: { type: Number, required: true }
})
const emit = defineEmits(['month-changed'])
</script>
