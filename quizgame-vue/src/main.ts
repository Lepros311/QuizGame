import { createApp } from 'vue'
import { createPinia } from 'pinia'
import App from './App.vue'
import router from './router';
import { useAuthStore } from './stores/auth';
import { apiClient } from './api/client';

// Bootstrap + Sketchy theme CSS
import 'bootswatch/dist/sketchy/bootstrap.min.css'

// BootstrapVueNext plugin
import { createBootstrap } from 'bootstrap-vue-next'
import 'bootstrap-vue-next/dist/bootstrap-vue-next.css'

const app = createApp(App)

app.use(createPinia())

const auth = useAuthStore() // must run after Pinia is installed on the app
auth.loadTokenFromStorage() // re-apply saved token to axios
if (auth.token) {
  apiClient.defaults.headers.common.Authorization = `Bearer ${auth.token}` // sync axios if token was in localStorage
}

app.use(createBootstrap())
app.use(router)

app.mount('#app')
