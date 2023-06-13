<script setup lang="ts">
import { ref } from 'vue'
import ChartViewer from '../components/ChartViewer.vue';
import ChartButtonToolbar from '../components/ChartButtonToolbar.vue';
import DisplaySlab from '../components/DisplaySlab.vue';
import BreadCrumbs from '../components/BreadCrumbs.vue';
import { DevicesClient, IDisplayMetricGroup, ChartsClient, IChartConfig } from '../apiclients/apiclient.ts';

/*
 * Primary data to display
 */
const slabs = ref<IDisplayMetricGroup[]>([]);
const chartconfig = ref<IChartConfig | null>(null);

/*
 * Timescale of display
 */

/*
 * Fetching from server
 */

var devicesClient = new DevicesClient();
var chartsClient = new ChartsClient();

async function getData() {
  slabs.value = await devicesClient.slabs();
}

async function getChart() {
  chartconfig.value = await chartsClient.telemetry();
}

function update()
{
  getChart();
  getData();
}

update();
setInterval(update, 2000);

</script>

<template>
  <main 
    data-test-id="Devices" 
    class="col-md-9 ms-sm-auto col-lg-10 px-md-4"
  >
    <div class="chartjs-size-monitor">
      <div class="chartjs-size-monitor-expand">
        <div class=""></div>
      </div>
      <div class="chartjs-size-monitor-shrink">
        <div class=""></div>
      </div>    
    </div>

    <div class="d-flex justify-content-between flex-wrap flex-md-nowrap align-items-center pt-3 pb-2 mb-3 border-bottom">
      <div>
        <h1 class="h2">Devices</h1>
        <BreadCrumbs :links="[]" page="Home"/>
      </div>
      <ChartButtonToolbar/>
    </div>

    <div 
      v-if="chartconfig?.data"
      class="chart-container w-100 my-5"
      style="position: relative;"
    >
      <ChartViewer :bar="true" :cdata="chartconfig?.data!" :coptions="chartconfig?.options" />
    </div>

    <div class="row row-cols-1 row-cols-md-2 row-cols-lg-3 row-cols-xxl-4 mb-3 text-center">
      <DisplaySlab 
        v-for="slab in slabs"
        :key="`${slab.kind}-${slab.id}`"
        :slab="slab"
        :href="`/devices/${slab.id}`"
      />

    </div>

  </main>
</template>

<style scoped>
.dropdown-menu li:hover {
    cursor: pointer;
}
</style>
