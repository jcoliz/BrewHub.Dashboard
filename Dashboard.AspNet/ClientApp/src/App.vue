<script setup lang="ts">
import TheSideBar from './components/TheSideBar.vue';
import TheHeader from './components/TheHeader.vue';
import TheFooter from './components/TheFooter.vue';
import { RouterView } from 'vue-router';

</script>

<template>
  <div class="body">
    <TheHeader title="BrewHub.Net" />
    <div class="container-fluid">
      <div class="row">
        <TheSideBar/>
        <RouterView/>
        <TheFooter/>
      </div>
    </div>
  </div>
</template>

<style>

body {
  font-size: .875rem;
}

/* Can't seem to get this as feathericon scoped */

</style>
