import { createApp } from 'vue'
import { createPinia } from 'pinia'
import App from './App.vue'
import router from './router'
import { attachUnauthorizedRedirect } from './api/attachUnauthorizedRedirect'
import { useAuthStore } from './stores/auth'
import 'bootswatch/dist/sketchy/bootstrap.min.css'
import './styles/app-theme.css'
import { createBootstrap } from 'bootstrap-vue-next'
import 'bootstrap-vue-next/dist/bootstrap-vue-next.css'

async function bootstrap() {
  const app = createApp(App)

  const pinia = createPinia()
  app.use(pinia)

  const auth = useAuthStore(pinia)
  attachUnauthorizedRedirect(pinia)
  await auth.hydrateFromStorage()

  app.use(createBootstrap())
  app.use(router)

  app.mount('#app')
}

void bootstrap()
