<script setup lang="ts">
import { ref } from 'vue'
import ChartViewer from '../components/ChartViewer.vue';
import FeatherIcon from '../components/FeatherIcon.vue';
import DisplaySlab from '../components/DisplaySlab.vue';
import { RouterLink } from 'vue-router';
import { DevicesClient, IDisplayMetricGroup, ChartsClient, IChartConfig } from '../apiclients/apiclient.ts';

/*
 * Route inputs
 */

defineProps<{ deviceid?: string }>();

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
        <nav style="--bs-breadcrumb-divider: '>';" aria-label="breadcrumb">
          <ol class="breadcrumb">
            <li 
              v-if="$props.deviceid"
              class="breadcrumb-item" 
            >
              <RouterLink to="/">Home</RouterLink>
            </li>
            <li 
              v-else
              class="breadcrumb-item active" 
              aria-current="page"
            >
              Home
            </li>
            <li 
              v-if="$props.deviceid"
              class="breadcrumb-item active" 
              aria-current="page">
              {{ deviceid }}
            </li>
          </ol>
        </nav>    
      </div>
      <div class="btn-toolbar mb-2 mb-md-0">
        <div 
          class="btn-group me-2" 
          role="group" 
          aria-label="Chart style button group"
        >
          <input 
            id="vbtn-radio1" 
            name="vbtn-radio" 
            type="radio" 
            class="btn-check" 
            autocomplete="off" 
            :value="true" 
          >
          <label 
            data-test-id="ShowBarChart"
            for="vbtn-radio1"
            class="btn btn-sm btn-outline-secondary" 
          >
            Bar
          </label>
          <input 
            id="vbtn-radio2" 
            name="vbtn-radio" 
            type="radio" 
            class="btn-check" 
            autocomplete="off" 
            :value="false" 
          > 
          <label 
            data-test-id="ShowLineChart"
            for="vbtn-radio2"
            class="btn btn-sm btn-outline-secondary" 
          >
            Line
          </label>
        </div>

        <div 
          class="btn-group dropdown" 
          role="group">
          <button 
            id="btnDropTimescale" 
            data-test-id="TimescaleDropDown"
            type="button" 
            class="btn btn-sm btn-outline-secondary dropdown-toggle" 
            data-bs-toggle="dropdown"
          >
            <FeatherIcon icon="calendar"/>
          </button>
          <ul 
            class="dropdown-menu" 
            aria-labelledby="btnDropTimescale"
          >
            <li 
              v-for="(option,index) in [ 'A', 'B' ]" 
              :key="index"
            >
              <a 
                class="dropdown-item" 
                href="javascript:void(0)" 
              >
                {{ option }}
              </a>
            </li>
          </ul>
        </div>
      </div>
    </div>

    <div 
      v-if="chartconfig?.data"
      class="my-5 w-100"
    >
      <ChartViewer :bar="true" :cdata="chartconfig?.data!" :coptions="chartconfig?.options" />
    </div>

    <div class="row row-cols-1 row-cols-md-2 row-cols-lg-3 row-cols-xxl-4 mb-3 text-center">
      <DisplaySlab 
        v-for="slab in slabs"
        :key="slab.id"
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
