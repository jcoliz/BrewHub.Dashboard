<script setup lang="ts">

/**
 * Dashboard Page: Primary features of dashboard web app are contained herein
 */

import { ref, onMounted, onUnmounted, computed } from 'vue'
import { onBeforeRouteUpdate } from 'vue-router'
import { DevicesClient, IDisplayMetricGroup, IDisplayMetric, ChartsClient, IChartConfig, TimeframeEnum, ProblemDetails, ApiException } from '../apiclients/apiclient.ts';

import ChartViewer from '../components/ChartViewer.vue';
import ChartButtonToolbar from '../components/ChartButtonToolbar.vue';
import DisplaySlab from '../components/DisplaySlab.vue';
import BreadCrumbs from '../components/BreadCrumbs.vue';

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

/*
 * Routing
 */

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
const showproblem = ref<ProblemDetails | null>(null);
/*
 * Timescale of display
 */

const timescale = ref(TimeframeEnum.Minutes);

/*
 * Handling posting data back to server
 */

function postCommand(slabid: string, metric: IDisplayMetric, payload: string)
{
  console.log(`postCommand: device ${props.deviceid} component ${props.componentid} slab ${slabid} metric ${metric.name} payload ${payload}`);
}

function postUpdate(slabid: string, metric: IDisplayMetric, payload: string)
{
  console.log(`postUpdate: device ${props.deviceid} component ${props.componentid} slab ${slabid} metric ${metric.name} payload ${payload}`);
}

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
    chartconfig.value = await chartsClient.componentChart(deviceid!,componentid,timescale.value);
  else if (deviceid)
    chartconfig.value = await chartsClient.deviceChart(deviceid,timescale.value);
  else
    chartconfig.value = await chartsClient.telemetry();
}

function update(deviceid?: string, componentid?: string) {
  showproblem.value = null;
    getChart(deviceid, componentid)
      .catch(reason => {
        console.log(`ERROR loading chart: ${reason}`);    
       })
    getData(deviceid, componentid)
      .catch(reason => {
        console.log(`ERROR loading data: ${typeof reason} ${reason}`);
        if (reason instanceof ApiException)
        {
          var apiex = reason as ApiException;
          showproblem.value = new ProblemDetails({ status: apiex.status, title: apiex.message });
          console.log(`API EXCEPTION loading data: ${apiex.status} ${apiex.message}`);
        }
        if (reason instanceof ProblemDetails)
        {
          var problem = reason as ProblemDetails;
          console.log(`PROBLEM loading data: ${problem.status} ${problem.title} detail:${problem.detail} instance:${problem.instance}`);
          showproblem.value = problem;
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
        <div
          v-if="showproblem"
        >
        <svg xmlns="http://www.w3.org/2000/svg" style="display: none;">
          <symbol id="exclamation-triangle-fill" viewBox="0 0 16 16">
            <path d="M8.982 1.566a1.13 1.13 0 0 0-1.96 0L.165 13.233c-.457.778.091 1.767.98 1.767h13.713c.889 0 1.438-.99.98-1.767L8.982 1.566zM8 5c.535 0 .954.462.9.995l-.35 3.507a.552.552 0 0 1-1.1 0L7.1 5.995A.905.905 0 0 1 8 5zm.002 6a1 1 0 1 1 0 2 1 1 0 0 1 0-2z"/>
          </symbol>
        </svg>

        <div class="alert alert-danger d-flex align-items-center" role="alert">
          <svg class="bi flex-shrink-0 me-2" role="img" aria-label="Danger:"><use xlink:href="#exclamation-triangle-fill"/></svg>
          <div class="ms-2">
            <p class="my-0">
              <strong>{{ showproblem.status }} {{ showproblem.title }}</strong>
            </p>
            <p v-if="showproblem.instance" class="my-0">{{ showproblem.instance }}</p>
            <p v-if="showproblem.detail" class="my-0">{{ showproblem.detail }}</p>
          </div>
        </div>
        </div>
      </div>
      <ChartButtonToolbar 
        :showtimeframe="!!deviceid" 
        v-model:timeframe="timescale"
        @refresh="update($props.deviceid,$props.componentid)"
      />
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
.bi {
    fill: currentcolor;
    width: 3em;
    height: 3em;
}
</style>
