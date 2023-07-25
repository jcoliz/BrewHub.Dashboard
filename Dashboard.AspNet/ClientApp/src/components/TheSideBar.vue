<script setup lang="ts">

/**
 * Application sidebar for navigation
 */

import { ref, computed } from 'vue';
import NavItemRouterLink from './NavItemRouterLink.vue';
import NavItemLink from './NavItemLink.vue';
import NavItemGroup from './NavItemGroup.vue';
import NavItemHeader from './NavItemHeader.vue';
import { Collapse } from 'bootstrap'

//
// Hide sidebar if it's shown from a collapsed state (small screens)
//
// Bug 1562: Navigation menu stays open after selecting item
// Note: See discussion this bug for alternatives considered
//
const sidebar = ref<HTMLElement|undefined>(undefined);
function unshow()
{
  if (sidebar.value && sidebar.value.classList.contains("show"))
  {
    const bsCollapse = Collapse.getInstance(sidebar.value);
    if (bsCollapse)
    {
      bsCollapse.hide();
    }
  }
}

//
// User Story 1641: Dashboard local links should link to machine where dashboard is running
//

const hostname = computed(() => {
  return location.hostname;
})

</script>

<template>
    <nav 
      data-test-id="TheSideBar"
      id="sidebarMenu"
      ref="sidebar"
      class="col-md-3 col-lg-2 d-md-block bg-light sidebar collapse"
    >
      <div class="position-sticky pt-3">
        <NavItemGroup>
            <NavItemRouterLink 
              title="Devices" 
              link="/devices" 
              icon="activity"
              @click="unshow"
            />
            <NavItemRouterLink 
              title="Scripts" 
              link="/scripts" 
              icon="cpu" 
              @click="unshow"
            />
            <NavItemRouterLink 
              title="Notify" 
              link="/notify" 
              icon="alert-triangle" 
              @click="unshow"
            />
            <NavItemRouterLink 
              title="Settings" 
              link="/settings" 
              icon="sliders" 
              @click="unshow"
            />
            <NavItemRouterLink 
              title="About" 
              link="/about" 
              icon="help-circle" 
              @click="unshow"
            />
            <NavItemRouterLink               
              title="Search" 
              class="d-md-none"
              link="/empty/Search" 
              icon="search" 
              @click="unshow"
            />
            <NavItemRouterLink 
              title="Login" 
              class="d-md-none"
              link="/empty/Login" 
              icon="log-in" 
              @click="unshow"
            />
          </NavItemGroup>
          <NavItemHeader 
            :title="`${hostname} Services`"
          />
          <NavItemGroup>
            <NavItemLink 
              title="Grafana" 
              :link="`http://${hostname}:3000/`"
              icon="bar-chart"
              @click="unshow"
            />
            <NavItemLink 
              title="InfluxDB" 
              :link="`http://${hostname}:8086/`"
              icon="database"
              @click="unshow"
            />
            <NavItemLink 
              title="VerneMQ" 
              :link="`http://${hostname}:8888/status`"
              icon="radio"
              @click="unshow"
            />
          </NavItemGroup>
          <NavItemHeader 
            title="Azure Services"
          />
          <NavItemGroup>
            <NavItemLink 
              title="Digital Twins" 
              link="https://explorer.digitaltwins.azure.net/"
              icon="compass"
              @click="unshow"
            />
            <NavItemLink 
              title="3D Scenes Studio" 
              link="https://explorer.digitaltwins.azure.net/3dscenes"
              icon="box"
              @click="unshow"
            />
            <NavItemLink 
              title="Power BI" 
              link="https://app.powerbi.com/"
              icon="bar-chart"
              @click="unshow"
            />
          </NavItemGroup>
        </div>
    </nav>    
</template>

<style scoped>
.sidebar {
  position: fixed;
  top: 0;
  /* rtl:raw:
  right: 0;
  */
  bottom: 0;
  /* rtl:remove */
  left: 0;
  z-index: 100; /* Behind the navbar */
  padding: 48px 0 0; /* Height of navbar */
  box-shadow: inset -1px 0 0 rgba(0, 0, 0, .1);
}

.sidebar :deep(.nav-link) {
  font-weight: 500;
  color: #333;
}

.sidebar :deep(.nav-link .feather) {
  margin-right: 4px;
  color: #727272;
}

.sidebar :deep(.nav-link.router-link-active),
.sidebar :deep(.nav-link.active) {
  color: #2470dc;
}

.sidebar :deep(.nav-link:hover .feather),
.sidebar :deep(.nav-link.active .feather) {
  color: inherit;
}

</style>