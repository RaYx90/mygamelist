import { createApp } from 'vue'
import 'bootstrap/dist/css/bootstrap.min.css'
import './assets/app.css'
import App from './App.vue'
import router from './router/index.js'
import { useAuth } from './composables/useAuth.js'

// Verifica la sesión activa (cookie HttpOnly) antes de montar la app,
// para que el router guard tenga el estado de auth ya poblado.
;(async () => {
  await useAuth().init()
  createApp(App).use(router).mount('#app')
})()
