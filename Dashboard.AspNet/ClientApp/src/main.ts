import './style.css'
import 'bootstrap/dist/css/bootstrap.css'
import 'bootstrap-icons/font/bootstrap-icons.css'
import 'feather-icons/dist/feather.min.js'
import 'bootstrap'
import App from './App.vue'
import { createApp } from 'vue'
import { createRouter, createWebHistory } from 'vue-router';

import DashboardView from './views/DashboardView.vue'
import AlbumView from './views/AlbumView.vue'
import EmptyView from './views/EmptyView.vue'
import ScriptsPage from './views/ScriptsPage.vue'
import NotifyPage from './views/NotifyPage.vue'
import SettingsPage from './views/SettingsPage.vue'

/*
 * Set up routing
 */

// Define some routes
// - Each route should map to a component.
const routes = [
  {
    path: '/devices/:deviceid?/:componentid?',
    component: DashboardView,
    props: true
  },
  { path: '/', redirect: '/devices' },
  { path: '/scripts', component: ScriptsPage, props: true },
  { path: '/notify', component: NotifyPage, props: true },
  { path: '/settings', component: SettingsPage, props: true },
  { path: '/album/:title', component: AlbumView, props: true },
  { path: '/empty/:title', component: EmptyView, props: true },
  { path: '/:pathMatch(.*)*', redirect: '/empty/NotFound' }
]

// Create the router instance and pass the `routes` option
const router = createRouter({
  // 4. Provide the history implementation to use. We are using the hash history for simplicity here.
  history: createWebHistory(),
  routes, // short for `routes: routes`
})

// Create the root instance.
const app = createApp(App)

// Make sure to _use_ the router instance to make the
// whole app router-aware.
app.use(router)

// Mount it, and off we go!
app.mount('#app')
