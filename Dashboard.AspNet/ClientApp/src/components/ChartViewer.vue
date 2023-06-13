<script setup lang="ts">

/**
 * Site-specific conventions for displaying a chart
 */

import { Bar, Line } from 'vue-chartjs'
import { Chart as ChartJS, Title, Tooltip, Legend, LineElement, BarElement, PointElement, CategoryScale, LinearScale } from 'chart.js'

ChartJS.register(Title, Tooltip, Legend, LineElement, BarElement, PointElement, CategoryScale, LinearScale)

defineProps<{
  /**
   * Whether to display a bar chart (false for line chart)
   */
  bar: boolean,

  /**
   * Chart.js chart data object
   */
  cdata: any,

  /**
   * Chart.js chart options object
   */
  coptions: any
}>();

</script>

<template>
    <Bar 
      v-if="bar" 
      data-test-id="BarChart"
      :data="$props.cdata" 
      :options="$props.coptions" 
    />
    <Line 
      v-if="!bar" 
      data-test-id="LineChart"
      :data="$props.cdata" 
      :options="$props.coptions" 
    />
</template>
