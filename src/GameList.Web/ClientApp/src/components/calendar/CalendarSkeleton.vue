<template>
  <div class="calendar-grid" aria-busy="true" aria-label="Cargando calendario...">
    <div class="week-header-cell" v-for="wd in WEEK_DAYS" :key="wd">{{ wd }}</div>
    <div v-for="n in CELL_COUNT" :key="n" class="skeleton-cell" />
  </div>
</template>

<script setup>
const WEEK_DAYS = ['Lun', 'Mar', 'Mié', 'Jue', 'Vie', 'Sáb', 'Dom']
const CELL_COUNT = 35
</script>
