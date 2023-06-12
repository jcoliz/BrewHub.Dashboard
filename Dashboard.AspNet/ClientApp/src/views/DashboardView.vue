<script setup lang="ts">
import { ref } from 'vue'
import { IWeatherForecast, WeatherForecastClient, TimeScale } from '../apiclients/apiclient';
import ChartViewer from '../components/ChartViewer.vue';
import FeatherIcon from '../components/FeatherIcon.vue';

/*
 * Primary data to display
 */

const isbargraph = ref(true); // else it's a line graph
const forecasts = ref<IWeatherForecast[]>([]);
const chartlabels = ref<string[]>([]);
const chartvalues = ref<number[]>([]);

/*
 * Timescale of display
 */

const timescale = ref<TimeScale>(1);

export interface timescaleenum {
  name: string,
  value: TimeScale
}

const timescale_options: timescaleenum[] = [
  {
    name: 'Two Days',
    value: 0
  },
  {
    name: 'This Week',
    value: 1
  },
  {
    name: 'Next Week',
    value: 2
  },
  {
    name: 'This Month',
    value: 3
  },
  {
    name: 'Next Month',
    value: 4
  }
];

function updateTimescale(value:TimeScale)
{
  if (value != timescale.value)
  {
    timescale.value = value;
    getData();
  }
}

/*
 * Fetching from server
 */

const client = new WeatherForecastClient();

async function getData() {
  forecasts.value = await client.get(timescale.value);
  chartlabels.value = forecasts.value.map(f => f.date!.toLocaleDateString("en-us", { dateStyle: "short" }));
  chartvalues.value = forecasts.value.map(f => f.temperatureF!);
}

getData()

</script>

<template>
  <main 
    data-test-id="Dashboard" 
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
      <h1 class="h2">Dashboard</h1>
      <div class="btn-toolbar mb-2 mb-md-0">
        <div 
          class="btn-group me-2" 
          role="group" 
          aria-label="Chart style button group"
        >
          <input 
            id="vbtn-radio1" 
            name="vbtn-radio" 
            v-model="isbargraph"
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
            v-model="isbargraph"
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
            {{  timescale_options[ timescale ].name }}
          </button>
          <ul 
            class="dropdown-menu" 
            aria-labelledby="btnDropTimescale"
          >
            <li 
              v-for="(option,index) in timescale_options" 
              :key="index"
            >
              <a 
                class="dropdown-item" 
                @click="updateTimescale(option.value)" 
                href="javascript:void(0)" 
              >
                {{ option.name }}
              </a>
            </li>
          </ul>
        </div>
      </div>
    </div>

    <ChartViewer 
      :values="chartvalues" 
      :labels="chartlabels" 
      :bar="isbargraph"
    />

    <h2 class="h3">
      Forecasts
    </h2>

    <div 
      v-if="forecasts" 
      class="table-responsive"
    >
      <table 
        data-test-id="Forecasts"
        class="table table-striped table-sm">
        <thead>
          <tr>
            <th scope="col">#</th>
            <th scope="col">Date</th>
            <th scope="col">Temp C</th>
            <th scope="col">Temp F</th>
            <th scope="col">Summary</th>
          </tr>
        </thead>
        <tbody>
          <tr 
            v-for="item in forecasts" 
            :key="item.id"
          >
            <td>{{ item.id }}</td>
            <td>{{ item.date?.toLocaleDateString("en-us", { dateStyle: "short" }) }}</td>
            <td>{{ item.temperatureC }}</td>
            <td>{{ item.temperatureF }}</td>
            <td>{{ item.summary }}</td>
          </tr>
        </tbody>
      </table>
    </div>
  </main>
</template>

<style scoped>
.dropdown-menu li:hover {
    cursor: pointer;
}
</style>
