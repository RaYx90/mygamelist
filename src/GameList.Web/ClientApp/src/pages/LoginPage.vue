<template>
  <div class="auth-container">
    <div class="auth-card">
      <h2 class="auth-title">Iniciar sesión</h2>
      <form @submit.prevent="handleLogin">
        <div class="mb-3">
          <label class="form-label">Email</label>
          <input v-model="email" type="email" class="form-control" required autocomplete="email" />
        </div>
        <div class="mb-3">
          <label class="form-label">Contraseña</label>
          <input v-model="password" type="password" class="form-control" required autocomplete="current-password" />
        </div>
        <div v-if="error" class="alert alert-danger py-2">{{ error }}</div>
        <button type="submit" class="btn btn-primary w-100" :disabled="loading">
          <span v-if="loading" class="spinner-border spinner-border-sm me-2" role="status" />
          Entrar
        </button>
      </form>
      <p class="mt-3 text-center">
        ¿No tienes cuenta? <RouterLink to="/register">Regístrate</RouterLink>
      </p>
    </div>
  </div>
</template>

<script setup>
import { ref } from 'vue'
import { useRouter, useRoute } from 'vue-router'
import { login as apiLogin } from '../api/authApi.js'
import { useAuth } from '../composables/useAuth.js'

const router = useRouter()
const route = useRoute()
const auth = useAuth()

const email = ref('')
const password = ref('')
const error = ref('')
const loading = ref(false)

async function handleLogin() {
  error.value = ''
  loading.value = true
  try {
    const data = await apiLogin(email.value, password.value)
    auth.login(data)
    router.push(route.query.redirect || '/')
  } catch (e) {
    error.value = e.message
  } finally {
    loading.value = false
  }
}
</script>

<style scoped>
.auth-container {
  display: flex;
  justify-content: center;
  align-items: center;
  min-height: 70vh;
  padding: 1rem;
}
.auth-card {
  background: var(--card-bg, #1e1e2e);
  border: 1px solid var(--border-color, #333);
  border-radius: 12px;
  padding: 2rem;
  width: 100%;
  max-width: 420px;
}
.auth-title {
  font-size: 1.5rem;
  font-weight: 700;
  margin-bottom: 1.5rem;
  text-align: center;
}
</style>
