import { createApp } from 'vue'
import { createPinia } from 'pinia'
import App from './App.vue'
import router from './router';

// Bootstrap + Sketchy theme CSS
import 'bootswatch/dist/sketchy/bootstrap.min.css'

// BootstrapVueNext plugin
import { createBootstrap } from 'bootstrap-vue-next'
import 'bootstrap-vue-next/dist/bootstrap-vue-next.css'

const app = createApp(App)

app.use(createPinia())
app.use(createBootstrap())
app.use(router)

app.mount('#app')
