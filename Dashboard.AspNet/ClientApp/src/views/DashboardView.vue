<script setup lang="ts">
import ChartViewer from '../components/ChartViewer.vue';
import FeatherIcon from '../components/FeatherIcon.vue';

/*
 * Primary data to display
 */


/*
 * Timescale of display
 */


/*
 * Fetching from server
 */

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

    <ChartViewer :bar="true" :labels="[]" :values="[]"/>

    <h2 class="h3">
      Forecasts
    </h2>

    <div 
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
