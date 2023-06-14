<script setup lang="ts">

/**
 * Dashboard Page: Primary features of dashboard web app are contained herein
 */

import { ref, onMounted, onUnmounted, computed } from 'vue'
import { onBeforeRouteUpdate } from 'vue-router'
import * as api from '../apiclients/apiclient.ts';

import ChartViewer from '../components/ChartViewer.vue';
import ChartButtonToolbar from '../components/ChartButtonToolbar.vue';
import DisplaySlab from '../components/DisplaySlab.vue';
import BreadCrumbs from '../components/BreadCrumbs.vue';
import ProblemDetailsViewer from '../components/ProblemDetailsViewer.vue';
import ThePageTitle from '../components/ThePageTitle.vue';
import BootstrapModalDialog from '../components/BootstrapModalDialog.vue';
import { IBootstrapModalDialogInterface } from '../components/BootstrapModalDialog.vue'

const props = defineProps<{
  /**
   * ID for the device we're viewing, or undefined for show summary of all devices
   * 
   * NOTE: Use the `hasValue` function to determine whether this actually contains
   * a value. Technically there are valid names which would resolve to falsy.
   */
  deviceid?: string,

  /**
   * ID for the component we're viewing, or undefined for show summary of all
   * components on this device.
   * 
   * NOTE: If a componentid is set, then a deviceid MUST be set.
   * 
   * NOTE: Use the `hasValue` function to determine whether this actually contains
   * a value. Technically there are valid names which would resolve to falsy.
   */
  componentid?: string
}>();

function hasValue(value: string | undefined): boolean {
  return (value != undefined) && value.length > 0; 
}

//
// Routing
//

interface IBreadcrumbLink {
  title: string,
  href: string
};

const breadcrumbs = computed(():IBreadcrumbLink[] => {
  if (hasValue(props.componentid)) {
    return [{
      title: 'Home',
      href: '/devices'
    },
    {
      title: props.deviceid!,
      href: `/devices/${props.deviceid}`
    }];
  }
  else if (hasValue(props.deviceid)) {
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

onBeforeRouteUpdate(async (to, { }) => {

  // If deviceid or componentid are missing, they will be "" here
  const deviceid = to.params["deviceid"] as string;
  const componentid = to.params["componentid"] as string;

  update(deviceid,componentid);
});

//
// Primary data to display
//

const slabs = ref<api.IDisplayMetricGroup[]>([]);
const chartconfig = ref<api.IChartConfig | undefined>(undefined);
const showproblem = ref<api.ProblemDetails | undefined>(undefined);

//
// Timescale of display
//

const timescale = ref(api.TimeframeEnum.Minutes);

//
// Communication back to server
//

const devicesClient = new api.DevicesClient();
const chartsClient = new api.ChartsClient();

const postCommandDialogShowing = ref(false);
const postCommandDialogConfig = ref<IBootstrapModalDialogInterface>({});

function postCommand(slabid: string, metric: api.IDisplayMetric, payload: string)
{
  console.log(`postCommand: device ${props.deviceid} component ${props.componentid} slab ${slabid} metric ${metric.name} payload ${payload}`);

  const componentToSend = hasValue(props.componentid) ? props.componentid! : slabid;

  devicesClient
    .executeCommand(props.deviceid!, componentToSend , metric.id!, payload)
    .then(() => {
      postCommandDialogConfig.value = {
        title: "Post Command",
        message: `Successfully sent "${metric.name}" command to "${props.deviceid}/${componentToSend}".`
      };
      postCommandDialogShowing.value = true;
    })
    .catch(reason => {
      const problem = getProblemDetails(reason);
      const stringify = JSON.stringify(problem);
      console.log(`PROBLEM: ${JSON.stringify(stringify)}`);

      postCommandDialogConfig.value = {
        title: "ERROR: Post Command Failed",
        message: stringify
      };
      postCommandDialogShowing.value = true;
    });
} 

function postUpdate(slabid: string, metric: api.IDisplayMetric, payload: string)
{
  console.log(`postUpdate: device ${props.deviceid} component ${props.componentid} slab ${slabid} metric ${metric.name} payload ${payload}`);

  const componentToSend = hasValue(props.componentid) ? props.componentid! : slabid;

  devicesClient
    .setProperty(props.deviceid!, componentToSend, metric.id!, payload)
    .then(() => {
      postCommandDialogConfig.value = {
        title: "Update Property",
        message: `Successfully sent "${metric.name}" property update to "${props.deviceid}/${componentToSend}".`
      };
      postCommandDialogShowing.value = true;
    })
    .catch(reason => {
      const problem = getProblemDetails(reason);
      const stringify = JSON.stringify(problem);
      console.log(`PROBLEM: ${JSON.stringify(stringify)}`);

      postCommandDialogConfig.value = {
        title: "ERROR: Update Property Failed",
        message: stringify
      };
      postCommandDialogShowing.value = true;
    });
}

//
// Fetching from server
//

async function getData(deviceid?: string, componentid?: string) {
  if (hasValue(componentid))
    slabs.value = await devicesClient.component(deviceid!, componentid!);
  else if (hasValue(deviceid))
    slabs.value = await devicesClient.device(deviceid!);
  else
    slabs.value = await devicesClient.slabs();
}

async function getChart(deviceid?: string, componentid?: string) {
  if (hasValue(componentid))
    chartconfig.value = await chartsClient.componentChart(deviceid!,componentid!,timescale.value);
  else if (hasValue(deviceid))
    chartconfig.value = await chartsClient.deviceChart(deviceid!,timescale.value);
  else
    chartconfig.value = await chartsClient.telemetry();
}

function getProblemDetails(reason: any): api.ProblemDetails
{
  if (reason instanceof api.ProblemDetails)
  {
    return reason;
  }
  if (reason instanceof api.ApiException)
  {
    return new api.ProblemDetails({ status: reason.status, title: reason.message });
  }
  if (typeof reason === "string")
  {
    return new api.ProblemDetails({ title: reason });
  }

  var detail = JSON.stringify(reason);
  return new api.ProblemDetails({ title: "Unrecognized Error", detail });
}

function update(deviceid?: string, componentid?: string) {
  showproblem.value = undefined;
  getChart(deviceid, componentid)
    .catch(reason => {
      // Note that we don't bother really with getchart failures, on the idea that if
      // chart fails to load, probably also the device data will fail to load, so that's
      // where we'll deal with it
      console.log(`ERROR loading chart: ${JSON.stringify(reason)}`);    
      })
  getData(deviceid, componentid)
    .catch(reason =>
    { 
      showproblem.value = getProblemDetails(reason);
    });
}

//
// Manage interval timers so as to not leak them
//

const usetimer = false;
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
  if (interval.value)
  {
    console.log(`Clearing interval ${interval.value}`);
    clearInterval(interval.value);
    console.log("Cleared");
  }
  else
  {
    console.log("No interval to clear");
  }
});

//
// Helpers, to reduce code in HTML
//

function slabhref (slab: api.IDisplayMetricGroup): string | undefined
{
  switch (slab.kind) {
    case api.DisplayMetricGroupKind.Device:
      return `/devices/${slab.id}`;
    case api.DisplayMetricGroupKind.Component:
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
    <div class="d-flex justify-content-between flex-wrap flex-md-nowrap align-items-center pt-3 pb-2 mb-3 border-bottom">
      <div>
        <ThePageTitle title="Devices"/>
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

    <ChartViewer 
      v-if="chartconfig?.data?.datasets?.length"
      :bar="chartconfig?.type === 'bar'" 
      :cdata="chartconfig?.data!" 
      :coptions="chartconfig?.options" 
      />

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

    <BootstrapModalDialog 
      :data="postCommandDialogConfig"       
      v-model:show="postCommandDialogShowing"
    />
  </main>
</template>

<style scoped>
.dropdown-menu li:hover {
    cursor: pointer;
}
</style>
