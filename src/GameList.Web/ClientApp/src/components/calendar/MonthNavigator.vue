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
    <button
      v-if="!isOnToday"
      class="month-nav-today"
      @click="emit('go-today')"
      aria-label="Ir a hoy"
    >Hoy</button>
  </div>
</template>

<script setup>
import { computed } from 'vue'

const MONTHS = ['Enero', 'Febrero', 'Marzo', 'Abril', 'Mayo', 'Junio', 'Julio', 'Agosto', 'Septiembre', 'Octubre', 'Noviembre', 'Diciembre']

const props = defineProps({
  selectedMonth: { type: Number, required: true },
  currentYear: { type: Number, required: true },
  selectedDay: { type: String, default: null }
})
const emit = defineEmits(['month-changed', 'go-today'])

const todayStr = (() => {
  const t = new Date()
  return `${t.getFullYear()}-${String(t.getMonth() + 1).padStart(2, '0')}-${String(t.getDate()).padStart(2, '0')}`
})()

const isOnToday = computed(() => props.selectedDay === todayStr)
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

.month-nav-today {
  background: rgba(99,102,241,0.12);
  border: 1px solid #4f46e5;
  border-radius: 6px;
  color: #a5b4fc;
  font-size: 0.78rem;
  font-weight: 600;
  padding: 0 0.6rem;
  height: 26px;
  cursor: pointer;
  white-space: nowrap;
  transition: background 0.15s, color 0.15s;
}

.month-nav-today:hover {
  background: rgba(99,102,241,0.25);
  color: #e0e7ff;
}
</style>
