import { createApp } from 'vue'
import { createPinia } from 'pinia'
import App from './App.vue'
import router from './router'
import { useAuthStore } from './stores/auth'
import 'bootswatch/dist/sketchy/bootstrap.min.css'
import { createBootstrap } from 'bootstrap-vue-next'
import 'bootstrap-vue-next/dist/bootstrap-vue-next.css'

const app = createApp(App)

const pinia = createPinia()
app.use(pinia)

const auth = useAuthStore(pinia)
void auth.hydrateFromStorage()

app.use(createBootstrap())
app.use(router)

app.mount('#app')
