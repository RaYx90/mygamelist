<template>
  <header class="app-header">
    <RouterLink to="/" class="app-logo">🎮 <strong>GameList</strong></RouterLink>
    <div class="app-title">
      Lanzamientos <span class="year-badge">{{ currentYear }}</span>
    </div>
    <nav class="app-nav">
      <template v-if="isLoggedIn">
        <UserMenuDropdown />
      </template>
      <template v-else>
        <RouterLink to="/login" class="nav-link">Entrar</RouterLink>
        <RouterLink to="/register" class="nav-link">Registrarse</RouterLink>
      </template>
    </nav>
  </header>

  <RouterView />
</template>

<script setup>
import { useAuth } from './composables/useAuth.js'
import UserMenuDropdown from './components/user/UserMenuDropdown.vue'

const { isLoggedIn } = useAuth()
const currentYear = new Date().getFullYear()
</script>

<style scoped>
.app-nav {
  display: flex;
  align-items: center;
  gap: 1rem;
}
.nav-link {
  color: inherit;
  text-decoration: none;
  font-size: 0.9rem;
  opacity: 0.85;
  transition: opacity 0.15s;
}
.nav-link:hover {
  opacity: 1;
}
.app-title {
  font-size: 1.1rem;
  font-weight: 700;
  color: #f1f5f9;
  letter-spacing: 0.01em;
}
</style>
