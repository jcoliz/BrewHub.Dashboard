<script setup lang="ts">
import ThePageTitle from '../components/ThePageTitle.vue';
</script>

<template>
<main 
    data-test-id="Settings" 
    class="col-md-9 ms-sm-auto col-lg-10 px-md-4"
  >
  
<div class="d-flex justify-content-between flex-wrap flex-md-nowrap align-items-center pt-3 pb-2 mb-3 border-bottom">
    <ThePageTitle title="Settings"/>
</div>

<div class="col-md-7 col-lg-8">
    <form>
        <h4 class="mb-3">Global Solution Settings</h4>
        <div class="row g-3">
            <div class="col-sm-6">
                <label for="telemetryinterval" class="form-label">Telemetry Interval</label>
                <input class="form-control" id="telemetryinterval" placeholder="" value="PT1S" required="false" autocomplete="off">
            </div>
            <div class="col-sm-6">
                <label for="warmPathInterval" class="form-label">Warm Path Interval</label>
                <input class="form-control" id="warmPathInterval" placeholder="" value="PT5M" required="false" autocomplete="off">
            </div>
        </div>
        <h4 class="my-3">Dashboard Settings</h4>
        <div class="row g-3">
            <div class="col-sm-6">
                <label for="deviceThreshold" class="form-label">Include Devices Connected Since</label>
                <input class="form-control" id="deviceThreshold" placeholder="" value="PT10M" required="false" autocomplete="off">
            </div>
            <div class="col-sm-6">
                <label for="propertyThreshold" class="form-label">Include Properties Updated Since</label>
                <input class="form-control" id="propertyThreshold" placeholder="" value="PT24H" required="false" autocomplete="off">
            </div>
        </div>
        <hr class="my-4">
        <button class="w-100 btn btn-outline-primary btn-lg" type="submit">Update</button>
    </form>
</div>

</main>
</template>