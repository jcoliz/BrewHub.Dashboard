<script setup lang="ts">

/**
 * Dashboard Page: Primary features of dashboard web app are contained herein
 */

import { ref, onMounted, onUnmounted, computed } from 'vue'
import { onBeforeRouteUpdate } from 'vue-router'
import { DevicesClient, IDisplayMetricGroup, IDisplayMetric, DisplayMetricGroupKind, ChartsClient, IChartConfig, TimeframeEnum, ProblemDetails, ApiException } from '../apiclients/apiclient.ts';

import ChartViewer from '../components/ChartViewer.vue';
import ChartButtonToolbar from '../components/ChartButtonToolbar.vue';
import DisplaySlab from '../components/DisplaySlab.vue';
import BreadCrumbs from '../components/BreadCrumbs.vue';
import ProblemDetailsViewer from '../components/ProblemDetailsViewer.vue';

const props = defineProps<{
  /**
   * ID for the device we're viewing, or undefined for show summary of all devices
   */
  deviceid?: string,

  /**
   * ID for the component we're viewing, or undefined for show summary of all
   * components on this device.
   * 
   * NOTE: If a componentid is set, then a deviceid MUST be set.
   */
  componentid?: string
}>();

//
// Routing
//

interface IBreadcrumbLink {
  title: string,
  href: string
};

const breadcrumbs = computed(():IBreadcrumbLink[] => {
  if (props.componentid) {
    return [{
      title: 'Home',
      href: '/devices'
    },
    {
      title: props.deviceid!,
      href: `/devices/${props.deviceid}`
    }];
  }
  else if (props.deviceid) {
    return [{
      title: 'Home',
      href: '/devices'
    }];
  }
  else {
    return [];
  }
})

const currentpage = computed((): string => {
  if (props.componentid != undefined) {
    return props.componentid;
  }
  else if (props.deviceid != undefined) {
    return props.deviceid;
  }
  else {
    return 'Home';
  }
});

onBeforeRouteUpdate(async (to, {}) => {
  const deviceid = to.params["deviceid"] as string;
  const componentid = to.params["componentid"] as string;
  update(deviceid,componentid);
});

//
// Primary data to display
//

const slabs = ref<IDisplayMetricGroup[]>([]);
const chartconfig = ref<IChartConfig | undefined>(undefined);
const showproblem = ref<ProblemDetails | undefined>(undefined);

//
// Timescale of display
//

const timescale = ref(TimeframeEnum.Minutes);

//
// Communication back to server
//

function postCommand(slabid: string, metric: IDisplayMetric, payload: string)
{
  console.log(`postCommand: device ${props.deviceid} component ${props.componentid} slab ${slabid} metric ${metric.name} payload ${payload}`);
}

function postUpdate(slabid: string, metric: IDisplayMetric, payload: string)
{
  console.log(`postUpdate: device ${props.deviceid} component ${props.componentid} slab ${slabid} metric ${metric.name} payload ${payload}`);
}

//
// Fetching from server
//

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
    chartconfig.value = await chartsClient.componentChart(deviceid!,componentid,timescale.value);
  else if (deviceid)
    chartconfig.value = await chartsClient.deviceChart(deviceid,timescale.value);
  else
    chartconfig.value = await chartsClient.telemetry();
}

function update(deviceid?: string, componentid?: string) {
  showproblem.value = undefined;
    getChart(deviceid, componentid)
      .catch(reason => {
        // Note that we don't bother really with getchart failures, on the idea that if
        // chart fails to load, probably also the device data will fail to load, so that's
        // where we'll deal with it
        console.log(`ERROR loading chart: ${reason}`);    
       })
    getData(deviceid, componentid)
      .catch(reason => {
        if (reason instanceof ApiException)
        {
          showproblem.value = new ProblemDetails({ status: reason.status, title: reason.message });
          console.log(`API EXCEPTION loading data: ${reason.status} ${reason.message}`);
        }
        else if (reason instanceof ProblemDetails)
        {
          console.log(`PROBLEM loading data: ${reason.status} ${reason.title} detail:${reason.detail} instance:${reason.instance}`);
          showproblem.value = reason;
        }
        else if (typeof reason === "string")
        {
          console.log(`ERROR loading data: ${reason}`);
          showproblem.value = new ProblemDetails({ title: reason });
        }
        else
        {
          var detail = JSON.stringify(reason);
          console.log(`ERROR loading data: ${detail}`);
          showproblem.value = new ProblemDetails({ title: "Unrecognized Error", detail });
        }
       })
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
    case DisplayMetricGroupKind.Device:
      return `/devices/${slab.id}`;
    case DisplayMetricGroupKind.Component:
      return `/devices/${props.deviceid}/${slab.id ?? "device"}`;
    default: // Empty, or erroneous value
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
        <BreadCrumbs 
          :links="breadcrumbs" 
          :page="currentpage"
        />
        <ProblemDetailsViewer 
          v-if="showproblem"
          :problem="showproblem"  
        />
      </div>
      <ChartButtonToolbar 
        :showtimeframe="!!deviceid" 
        v-model:timeframe="timescale"
        @refresh="update($props.deviceid,$props.componentid)"
      />
    </div>

    <div 
      v-if="chartconfig?.data?.datasets?.length"
      class="chart-container w-100 my-5"
      style="position: relative;"
    >
      <ChartViewer 
        :bar="true" 
        :cdata="chartconfig?.data!" 
        :coptions="chartconfig?.options" 
      />
    </div>

    <div class="row row-cols-1 row-cols-md-2 row-cols-lg-3 row-cols-xxl-4 mb-3 text-center">
      <DisplaySlab 
        v-for="slab in slabs"
        :key="`${slab.kind}-${slab.id}`"
        :slab="slab"
        :href="slabhref(slab)"
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
