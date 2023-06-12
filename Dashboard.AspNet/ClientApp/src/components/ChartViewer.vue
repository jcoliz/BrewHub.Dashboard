<script setup lang="ts">
import { Bar, Line } from 'vue-chartjs'
import { Chart as ChartJS, Title, Tooltip, Legend, LineElement, BarElement, PointElement, CategoryScale, LinearScale } from 'chart.js'

ChartJS.register(Title, Tooltip, Legend, LineElement, BarElement, PointElement, CategoryScale, LinearScale)

defineProps<{ bar: boolean, cdata: any, coptions: any }>();

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
