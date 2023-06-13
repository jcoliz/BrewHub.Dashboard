<script setup lang="ts">

/**
 * Application sidebar for navigation
 */

import { ref } from 'vue';
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
const sidebar = ref<HTMLElement|null>(null);
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
              link="/empty/About" 
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
            title="Edge Components"
          />
          <NavItemGroup>
            <NavItemLink 
              title="Grafana" 
              link="http://localhost:3000/"
              icon="bar-chart"
              @click="unshow"
            />
            <NavItemLink 
              title="InfluxDB" 
              link="http://localhost:8086" 
              icon="database"
              @click="unshow"
            />
            <NavItemLink 
              title="VerneMQ" 
              link="http://localhost:8888/status" 
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

