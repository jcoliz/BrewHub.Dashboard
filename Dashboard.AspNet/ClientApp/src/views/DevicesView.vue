<script setup lang="ts">
import { ref, onMounted, onUnmounted } from 'vue'
import ChartViewer from '../components/ChartViewer.vue';
import ChartButtonToolbar from '../components/ChartButtonToolbar.vue';
import DisplaySlab from '../components/DisplaySlab.vue';
import BreadCrumbs from '../components/BreadCrumbs.vue';
import { DevicesClient, IDisplayMetricGroup, IDisplayMetric, ChartsClient, IChartConfig } from '../apiclients/apiclient.ts';

/*
 * Route inputs
 */

const props = defineProps<{ deviceid: string }>();

/*
 * Primary data to display
 */
const slabs = ref<IDisplayMetricGroup[]>([]);
const chartconfig = ref<IChartConfig | null>(null);

/*
 * Timescale of display
 */

/*
 * Handling posting data back to server
 */

function postCommand(component: string, metric: IDisplayMetric, payload: string)
{
  console.log(`postCommand: device ${props.deviceid} component ${component} metric ${metric.name} payload ${payload}`);
}

function postUpdate(component: string, metric: IDisplayMetric, payload: string)
{
  console.log(`postUpdate: device ${props.deviceid} component ${component} metric ${metric.name} payload ${payload}`);
}

/*
 * Fetching from server
 */

var devicesClient = new DevicesClient();
var chartsClient = new ChartsClient();

async function getData() {
  slabs.value = await devicesClient.device(props.deviceid);
}

async function getChart() {
  chartconfig.value = await chartsClient.deviceChart(props.deviceid,0);
}

function update() {
  getChart();
  getData();
}

/*
 * Manage interval timers so as to not leak them
 */

// Note that this shouldn't be faster than the minimum time slice on the smallest chart

const interval = ref<NodeJS.Timer|undefined>(undefined);
onMounted(() => {
  interval.value = setInterval(update, 20000);
  console.log(`Set interval ${interval.value}`);
  update();
});
onUnmounted(() => {
  console.log(`Clearing interval ${interval.value}`);
  if (interval.value)
  {
    clearInterval(interval.value);
    console.log("Cleared");
  }
});

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
        <h1 class="h2">Devices!</h1>
        <BreadCrumbs :links="[{ title:'Home', href:'/' }]" :page="deviceid"/>
      </div>
      <ChartButtonToolbar/>
    </div>

    <div 
      v-if="chartconfig?.data"
      class="chart-container my-5 w-100"
      style="position: relative;"
    >
      <ChartViewer :bar="false" :cdata="chartconfig?.data!" :coptions="chartconfig?.options" />
    </div>

    <div class="row row-cols-1 row-cols-md-2 row-cols-lg-3 row-cols-xxl-4 mb-3 text-center">
      <DisplaySlab 
        v-for="slab in slabs"
        :key="`${slab.kind}-${slab.id}`"
        :slab="slab"
        :href="`/components/${deviceid}/${slab.id}`"
        @command="(metric,payload) => postCommand(slab.id!,metric,payload)"
        @property="(metric,payload) => postUpdate(slab.id!,metric,payload)"
      />

    </div>

  </main>
</template>

<style scoped>
.dropdown-menu li:hover {
    cursor: pointer;
}
</style>
