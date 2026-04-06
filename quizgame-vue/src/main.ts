import { createApp } from 'vue'
import { createPinia } from 'pinia'
import App from './App.vue'

// Bootstrap + Sketchy theme CSS
import 'bootswatch/dist/sketchy/bootstrap.min.css'

// BootstrapVueNext plugin
import { createBootstrap } from 'bootstrap-vue-next'
import 'bootstrap-vue-next/dist/bootstrap-vue-next.css'

const app = createApp(App)

app.use(createPinia())
app.use(createBootstrap())

app.mount('#app')
