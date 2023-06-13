<script setup lang="ts">

/**
 * Standard site header
 */

import FeatherIcon from './FeatherIcon.vue';

defineProps<{
  /**
   * Brand wording to use in upper-left corner of site
   */
  title: string
}>();
</script>

<template>
    <header class="navbar navbar-dark sticky-top bg-dark flex-md-nowrap p-0 shadow">
      <a 
        class="navbar-brand col-md-3 col-lg-2 me-0 px-3" 
        href="#"
      >
        {{ title }}
      </a>
      <button 
        class="navbar-toggler position-absolute d-md-none collapsed" 
        type="button" 
        data-bs-toggle="collapse" 
        data-bs-target="#sidebarMenu" 
        aria-controls="sidebarMenu" 
        aria-expanded="false" 
        aria-label="Toggle navigation"
      >
        <span class="navbar-toggler-icon"></span>
      </button>
      <input 
        class="form-control form-control-dark w-100 d-md-block d-none" 
        type="text" 
        placeholder="Search" 
        aria-label="Search"
      >
      <div class="navbar-nav d-md-block d-none">
        <div class="nav-item text-nowrap">
          <a 
            class="nav-link px-3" 
            href="#"
          >
            <FeatherIcon icon="log-in"/>
            Sign in
          </a>
        </div>
      </div>
    </header>    
</template>