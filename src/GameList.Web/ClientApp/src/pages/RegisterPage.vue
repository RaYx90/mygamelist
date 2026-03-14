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
          <div class="input-eye-wrapper">
            <input v-model="password" :type="showPassword ? 'text' : 'password'" class="form-control" required minlength="6" autocomplete="new-password" />
            <button type="button" class="btn-eye" @click="showPassword = !showPassword" :aria-label="showPassword ? 'Ocultar contraseña' : 'Mostrar contraseña'">
              <svg v-if="!showPassword" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M1 12s4-8 11-8 11 8 11 8-4 8-11 8-11-8-11-8z"/><circle cx="12" cy="12" r="3"/></svg>
              <svg v-else viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M17.94 17.94A10.07 10.07 0 0 1 12 20c-7 0-11-8-11-8a18.45 18.45 0 0 1 5.06-5.94"/><path d="M9.9 4.24A9.12 9.12 0 0 1 12 4c7 0 11 8 11 8a18.5 18.5 0 0 1-2.16 3.19"/><line x1="1" y1="1" x2="23" y2="23"/></svg>
            </button>
          </div>
        </div>
        <div class="mb-3">
          <label class="form-label">Código de acceso</label>
          <div class="input-eye-wrapper">
            <input v-model="inviteCode" :type="showCode ? 'text' : 'password'" class="form-control" required autocomplete="off" placeholder="Necesitas el código para registrarte" />
            <button type="button" class="btn-eye" @click="showCode = !showCode" :aria-label="showCode ? 'Ocultar código' : 'Mostrar código'">
              <svg v-if="!showCode" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M1 12s4-8 11-8 11 8 11 8-4 8-11 8-11-8-11-8z"/><circle cx="12" cy="12" r="3"/></svg>
              <svg v-else viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M17.94 17.94A10.07 10.07 0 0 1 12 20c-7 0-11-8-11-8a18.45 18.45 0 0 1 5.06-5.94"/><path d="M9.9 4.24A9.12 9.12 0 0 1 12 4c7 0 11 8 11 8a18.5 18.5 0 0 1-2.16 3.19"/><line x1="1" y1="1" x2="23" y2="23"/></svg>
            </button>
          </div>
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
const showPassword = ref(false)
const showCode = ref(false)
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
.input-eye-wrapper {
  position: relative;
  display: flex;
  align-items: center;
}
.input-eye-wrapper .form-control {
  padding-right: 2.8rem;
}
.btn-eye {
  position: absolute;
  right: 0.6rem;
  background: none;
  border: none;
  color: #888;
  cursor: pointer;
  padding: 0.25rem;
  display: flex;
  align-items: center;
  transition: color 0.15s;
}
.btn-eye:hover { color: #ccc; }
.btn-eye svg { width: 1.2rem; height: 1.2rem; }
</style>
