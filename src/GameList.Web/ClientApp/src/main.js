import { createApp } from 'vue'
import 'bootstrap/dist/css/bootstrap.min.css'
import './assets/app.css'
import App from './App.vue'
import router from './router/index.js'
import { useAuth } from './stores/auth.js'

const auth = useAuth()
auth.init()

createApp(App).use(router).mount('#app')
