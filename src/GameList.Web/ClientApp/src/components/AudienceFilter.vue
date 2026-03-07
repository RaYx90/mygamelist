<template>
  <select
    class="filter-select"
    :value="selected === null ? '' : String(selected)"
    @change="emit('audience-changed', $event.target.value === '' ? null : $event.target.value === 'false')"
  >
    <option value="">Todos</option>
    <option value="true">Indie</option>
    <option value="false">No Indie</option>
  </select>
</template>

<script setup>
defineProps({
  selected: { default: null }
})
const emit = defineEmits(['audience-changed'])
</script>
