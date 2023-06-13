<script setup lang="ts">
import { IDisplayMetric } from '../apiclients/apiclient.ts';
defineProps<{ metric?: IDisplayMetric}> ();
</script>

<template>
    <li 
        v-if="metric"
        class="list-group-item d-flex justify-content-between lh-sm"
    >
        <div>
            <h6 class="my-0">{{ metric.name }}</h6>
            <small class="text-body-secondary">{{ metric.value }}</small>
        </div>
    </li>
</template>