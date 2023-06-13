<script setup lang="ts">
import archPng from '../assets/arch.png';
</script>
<template>

<main 
    data-test-id="Settings" 
    class="col-md-9 ms-sm-auto col-lg-10 px-md-4"
  >
  <div class="mb-4 bg-body-tertiary rounded-3">
      <div class="container-fluid py-5">
        <h1 class="text-body-emphasis">About BrewHub.Net</h1>
        <p class="col-md-8 lead">BrewHub.Net is a reference implementation of my IoT Reference Architecture for Azure IoT. It is based on a real-life
      design I did for a medium-size local distillery. I recast it for brewing instead of distilling to ensure I didn't
      include any of their intellectual property.
    </p>
        <button class="btn btn-primary btn-lg" type="button">Learn more</button>

      <img :src="archPng" class="img-fluid pt-5" alt="Architecture diagram"/>
      </div>

</div>


</main>
</template>