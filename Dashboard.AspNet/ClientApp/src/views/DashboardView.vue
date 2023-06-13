<script setup lang="ts">
import { ref, onMounted, onUnmounted, computed } from 'vue'
import { onBeforeRouteUpdate } from 'vue-router'

import ChartViewer from '../components/ChartViewer.vue';
import ChartButtonToolbar from '../components/ChartButtonToolbar.vue';
import DisplaySlab from '../components/DisplaySlab.vue';
import BreadCrumbs from '../components/BreadCrumbs.vue';
import { DevicesClient, IDisplayMetricGroup, ChartsClient, IChartConfig } from '../apiclients/apiclient.ts';

/*
 * Route inputs
 */

const props = defineProps<{ deviceid?: string, componentid?: string }>();

interface IBreadcrumbLink {
  title: string,
  href: string
};

const breadcrumbs = computed(():IBreadcrumbLink[] => {
  if (props.componentid) {
    return [{ title: 'Home', href: '/devices' }, { title: props.deviceid!, href: `/devices/${props.deviceid}` }];
  }
  else if (props.deviceid) {
    return [{ title: 'Home', href: '/devices' }];
  }
  else {
    return [];
  }
})

const currentpage = computed((): string => {
  if (props.componentid) {
    return props.componentid;
  }
  else if (props.deviceid) {
    return props.deviceid;
  }
  else {
    return 'Home';
  }
});

onBeforeRouteUpdate(async (to, _) => {
  const deviceid = to.params["deviceid"] as string;
  const componentid = to.params["componentid"] as string;
  update(deviceid,componentid);
});

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

async function getData(deviceid?: string, componentid?: string) {
  console.log(`getData: ${deviceid ?? "empty"} ${componentid ?? "empty"} `)
  if (componentid)
    slabs.value = await devicesClient.component(deviceid!, componentid);
  else if (deviceid)
    slabs.value = await devicesClient.device(deviceid);
  else
    slabs.value = await devicesClient.slabs();
}

async function getChart(deviceid?: string, componentid?: string) {
  if (componentid)
    chartconfig.value = await chartsClient.componentChart(deviceid!,componentid,0);
  else if (deviceid)
    chartconfig.value = await chartsClient.deviceChart(deviceid,0);
  else
    chartconfig.value = await chartsClient.telemetry();
}

function update(deviceid?: string, componentid?: string) {
  getChart(deviceid, componentid);
  getData(deviceid, componentid);
}

/*
 * Manage interval timers so as to not leak them
 */

const usetimer:boolean = false;
// Note that this shouldn't be faster than the minimum time slice on the smallest chart
const interval = ref<NodeJS.Timer | undefined>(undefined);
onMounted(() => {
  if (usetimer)
  {
    interval.value = setInterval(update, 20000);
    console.log(`Set interval ${interval.value}`);
  }
  update(props.deviceid, props.componentid);
});
onUnmounted(() => {
  console.log(`Clearing interval ${interval.value}`);
  if (interval.value)
  {
    clearInterval(interval.value);
    console.log("Cleared");
  }
});

/*
 * Helpers, to reduce code in HTML
 */

function slabhref (slab: IDisplayMetricGroup): string | undefined
{
  switch (slab.kind) {
    case 1: // device
      return `/devices/${slab.id}`;
    case 2: // component
      return `/devices/${props.deviceid}/${slab.id ?? "device"}`;
    default: // empty=0, grouping=3, or erroneous value
      return undefined;
  }
}

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
        <BreadCrumbs :links="breadcrumbs" :page="currentpage"/>
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
        :href="slabhref(slab)"
      />

    </div>

  </main>
</template>

<style scoped>
.dropdown-menu li:hover {
    cursor: pointer;
}
</style>
