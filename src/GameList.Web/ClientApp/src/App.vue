<template>
  <header class="app-header">
    <RouterLink to="/" class="app-logo">🎮 <strong>GameList</strong></RouterLink>
    <div class="app-title">
      Lanzamientos <span class="year-badge">{{ currentYear }}</span>
    </div>
    <nav class="app-nav">
      <RouterLink v-if="auth.isLoggedIn" to="/grupo" class="nav-link">Grupo</RouterLink>
      <RouterLink v-if="!auth.isLoggedIn" to="/login" class="nav-link">Entrar</RouterLink>
      <RouterLink v-if="!auth.isLoggedIn" to="/register" class="nav-link">Registrarse</RouterLink>
      <button v-if="auth.isLoggedIn" class="nav-link btn-link" @click="handleLogout">
        {{ auth.state.username }} · Salir
      </button>
    </nav>
  </header>

  <RouterView />
</template>

<script setup>
import { useRouter } from 'vue-router'
import { useAuth } from './stores/auth.js'

const router = useRouter()
const auth = useAuth()
const currentYear = new Date().getFullYear()

function handleLogout() {
  auth.logout()
  router.push('/login')
}
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
.btn-link {
  background: none;
  border: none;
  cursor: pointer;
  padding: 0;
  font-size: 0.9rem;
}
.app-title {
  font-size: 1.1rem;
  font-weight: 700;
  color: #f1f5f9;
  letter-spacing: 0.01em;
}
</style>

