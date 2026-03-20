<template>
  <div class="month-navigator">
    <button
      class="month-nav-btn"
      :disabled="selectedMonth === 1"
      @click="emit('month-changed', selectedMonth - 1)"
      aria-label="Mes anterior"
    >&#8249;</button>
    <select
      id="filter-month"
      class="filter-select"
      :value="selectedMonth"
      aria-label="Mes"
      @change="emit('month-changed', +$event.target.value)"
    >
      <option v-for="(name, i) in MONTHS" :key="i + 1" :value="i + 1">{{ name }}</option>
    </select>
    <button
      class="month-nav-btn"
      :disabled="selectedMonth === 12"
      @click="emit('month-changed', selectedMonth + 1)"
      aria-label="Mes siguiente"
    >&#8250;</button>
  </div>
</template>

<script setup>
const MONTHS = ['Enero', 'Febrero', 'Marzo', 'Abril', 'Mayo', 'Junio', 'Julio', 'Agosto', 'Septiembre', 'Octubre', 'Noviembre', 'Diciembre']

defineProps({
  selectedMonth: { type: Number, required: true },
  currentYear: { type: Number, required: true }
})
const emit = defineEmits(['month-changed'])
</script>

<style scoped>
.month-navigator {
  display: flex;
  align-items: center;
  gap: 2px;
}

.month-nav-btn {
  background: #12122a;
  border: 1px solid #3d3d5e;
  color: #94a3b8;
  border-radius: 6px;
  width: 26px;
  height: 26px;
  display: flex;
  align-items: center;
  justify-content: center;
  cursor: pointer;
  font-size: 1.1rem;
  line-height: 1;
  padding: 0;
  flex-shrink: 0;
  transition: background 0.15s, color 0.15s, border-color 0.15s;
}

.month-nav-btn:hover:not(:disabled) {
  background: #1e1e3e;
  color: #e2e8f0;
  border-color: #6366f1;
}

.month-nav-btn:disabled {
  opacity: 0.3;
  cursor: not-allowed;
}
</style>
