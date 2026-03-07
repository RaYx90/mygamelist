import { createRouter, createWebHistory } from 'vue-router'
import CalendarPage from '../pages/CalendarPage.vue'
import LoginPage from '../pages/LoginPage.vue'
import RegisterPage from '../pages/RegisterPage.vue'
import GroupPage from '../pages/GroupPage.vue'

const router = createRouter({
  history: createWebHistory(),
  routes: [
    { path: '/', component: CalendarPage, meta: { requiresAuth: true } },
    { path: '/login', component: LoginPage },
    { path: '/register', component: RegisterPage },
    { path: '/grupo', component: GroupPage, meta: { requiresAuth: true } }
  ]
})

router.beforeEach((to, _from, next) => {
  if (to.meta.requiresAuth) {
    const raw = localStorage.getItem('gl_user')
    if (!raw) return next(`/login?redirect=${encodeURIComponent(to.fullPath)}`)
  }
  next()
})

export default router
