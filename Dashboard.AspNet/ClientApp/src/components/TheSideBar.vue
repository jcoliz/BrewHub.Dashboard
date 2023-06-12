<script setup lang="ts">
import { ref } from 'vue';
import NavItemRouterLink from './NavItemRouterLink.vue';
import NavItemGroup from './NavItemGroup.vue';
import NavItemHeader from './NavItemHeader.vue';
import FeatherIcon from './FeatherIcon.vue';
import { Collapse } from 'bootstrap'

//
// [User Can] Add a new report
//

const maxReports: number = 6;
const numReports = ref<number>(0);

function isOkToAddReport(): boolean {
    return numReports.value < maxReports;
}

function addReport() {
    if (isOkToAddReport())
        numReports.value++;
}

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
              title="Dashboard" 
              link="/" 
              icon="home"
              @click="unshow"
            />
            <NavItemRouterLink 
              title="Orders" 
              link="/album/Orders" 
              icon="file" 
              @click="unshow"
            />
            <NavItemRouterLink 
              title="Products" 
              link="/empty/Products" 
              icon="shopping-cart" 
              @click="unshow"
            />
            <NavItemRouterLink 
              title="Customers" 
              link="/empty/Customers" 
              icon="users" 
              @click="unshow"
            />
            <NavItemRouterLink 
              title="Reports" 
              link="/empty/Reports" 
              icon="bar-chart-2" 
              @click="unshow"
            />
            <NavItemRouterLink 
              title="Integrations" 
              link="/empty/Integrations" 
              icon="layers" 
              @click="unshow"
            />
            <NavItemRouterLink               
              title="Search" 
              class="d-md-none"
              link="/empty/Integrations" 
              icon="search" 
              @click="unshow"
            />
            <NavItemRouterLink 
              title="Login" 
              class="d-md-none"
              link="/empty/Integrations" 
              icon="log-in" 
              @click="unshow"
            />
          </NavItemGroup>
          <NavItemHeader 
            title="Saved reports"
            class="d-none d-md-block"
          >
            <a               
              v-if="isOkToAddReport()"
              data-test-id="AddReport"
              @click="addReport()"
              class="link-secondary link-clickable"
              aria-label="Add a new report"
            >
              <FeatherIcon icon="plus-circle"/>
            </a>
          </NavItemHeader>
          <NavItemGroup 
            data-test-id="SavedReports"
            class="d-none d-md-block"
          >
            <NavItemRouterLink 
              title="Current month" 
              link="/empty/Reports" 
              icon="file-text"
              tid="ReportCM"
              @click="unshow"
            />
            <NavItemRouterLink 
              title="Last quarter" 
              link="/empty/Reports" 
              icon="file-text"
              tid="ReportLQ"
              @click="unshow"
            />
            <NavItemRouterLink 
              title="Social engagement" 
              link="/empty/Reports" 
              icon="file-text"
              tid="ReportSE"
              @click="unshow"
            />
            <NavItemRouterLink 
              title="Year-end sale" 
              link="/empty/Reports" 
              icon="file-text"
              tid="ReportYE"
              @click="unshow"
            />
            <NavItemRouterLink
              v-for="i in numReports"
              data-test-id="NewReport"
              :key="i"
              :title="`New Report ${i}`"
              link="/empty/Reports" 
              icon="file-text"
              :tid="`ReportN${i}`"
            />
          </NavItemGroup>
        </div>
    </nav>    
</template>

