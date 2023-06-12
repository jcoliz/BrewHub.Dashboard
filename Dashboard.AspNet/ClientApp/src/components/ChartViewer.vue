<script setup lang="ts">
import { computed } from 'vue'
import { Bar, Line } from 'vue-chartjs'
import { Chart as ChartJS, Title, Tooltip, Legend, LineElement, BarElement, PointElement, CategoryScale, LinearScale } from 'chart.js'

ChartJS.register(Title, Tooltip, Legend, LineElement, BarElement, PointElement, CategoryScale, LinearScale)

const props = defineProps<{ labels: string[], values: number[], bar: boolean }>();

const chartdata = computed(() => {
  return {
    labels: props.labels,
    datasets: [{
      data: props.values,
      backgroundColor: '#9684A1',
      borderColor: '#985F99',
      borderWidth: props.bar ? 0 : 4,
      pointBackgroundColor: '#ABA8B2'
    }]
  }
})

const chartoptions = {
  plugins: {
    legend: {
      display: false
    },
    tooltip: {
      boxPadding: 3
    }
  }
};
</script>

<template>
    <Bar 
      v-if="bar" 
      data-test-id="BarChart"
      :data="chartdata" 
      :options="chartoptions" 
    />
    <Line 
      v-if="!bar" 
      data-test-id="LineChart"
      :data="chartdata" 
      :options="chartoptions" 
    />
</template>
