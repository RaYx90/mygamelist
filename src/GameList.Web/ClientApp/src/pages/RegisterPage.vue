<template>
  <div class="auth-container">
    <div class="auth-card">
      <h2 class="auth-title">Crear cuenta</h2>
      <form @submit.prevent="handleRegister">
        <div class="mb-3">
          <label class="form-label">Nombre de usuario</label>
          <input v-model="username" type="text" class="form-control" required minlength="3" autocomplete="username" />
        </div>
        <div class="mb-3">
          <label class="form-label">Email</label>
          <input v-model="email" type="email" class="form-control" required autocomplete="email" />
        </div>
        <div class="mb-3">
          <label class="form-label">Contraseña</label>
          <input v-model="password" type="password" class="form-control" required minlength="6" autocomplete="new-password" />
        </div>
        <div class="mb-3">
          <label class="form-label">Código de acceso</label>
          <input v-model="inviteCode" type="password" class="form-control" required autocomplete="off" placeholder="Necesitas el código para registrarte" />
        </div>
        <div v-if="error" class="alert alert-danger py-2">{{ error }}</div>
        <button type="submit" class="btn btn-primary w-100" :disabled="loading">
          <span v-if="loading" class="spinner-border spinner-border-sm me-2" role="status" />
          Registrarse
        </button>
      </form>
      <p class="mt-3 text-center">
        ¿Ya tienes cuenta? <RouterLink to="/login">Inicia sesión</RouterLink>
      </p>
    </div>
  </div>
</template>

<script setup>
import { ref } from 'vue'
import { useRouter } from 'vue-router'
import { register as apiRegister } from '../api/authApi.js'
import { useAuth } from '../composables/useAuth.js'

const router = useRouter()
const auth = useAuth()

const username = ref('')
const email = ref('')
const password = ref('')
const inviteCode = ref('')
const error = ref('')
const loading = ref(false)

async function handleRegister() {
  error.value = ''
  loading.value = true
  try {
    const data = await apiRegister(username.value, email.value, password.value, inviteCode.value)
    auth.login(data)
    router.push('/')
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
