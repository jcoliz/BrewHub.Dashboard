<script setup lang="ts">

/**
 * Displays a group of metrics in a nice-looking card
 */

import MetricsItem from './MetricsItem.vue';
import MetricsItemWritable from './MetricsItemWritable.vue';
import MetricsItemCommand from './MetricsItemCommand.vue';
import { IDisplayMetricGroup, IDisplayMetric } from '../apiclients/apiclient.ts';
import { RouterLink } from 'vue-router';

defineProps<{
    /**
     * Contents to display
     */
    slab?: IDisplayMetricGroup,

    /**
     * Where to link to for more details
     */
    href?: string
}>();

defineEmits<{
    /**
     * User desires to send a command back to the device
     */
    (e: 'command', metric: IDisplayMetric, payload: string): void,

    /**
     * User desires to update a writable property on the device
     */
     (e: 'property', metric: IDisplayMetric, payload: string): void,
}>();

</script>

<template>
    <div
        data-test-id="MetricsCard" 
        class="col"
    >
        <div class="card mb-4 rounded-3 shadow-sm border-primary">
            <div class="card-header py-3 text-bg-primary border-primary">
                <h4 class="my-0 fw-normal">{{ slab?.title }}</h4>
            </div>
            <div class="card-body text-start">
                <ul class="list-group mb-3">
                    <MetricsItem
                        v-for="metric in slab?.telemetry" :metric="metric"
                    />
                    <MetricsItem
                        v-for="metric in slab?.readOnlyProperties" :metric="metric"
                    />
                    <MetricsItemWritable
                        v-for="metric in slab?.writableProperties" :metric="metric"
                        @update="(text) => $emit('property',metric,text)"
                    />
                    <MetricsItemCommand
                        v-for="metric in slab?.commands" :metric="metric"
                        @go="(text) => $emit('command',metric,text)"
                    />
                </ul>
                <RouterLink 
                    v-if="href"
                    :to="href"
                >
                    <button class="w-100 btn btn-lg btn-outline-primary">
                        Details
                    </button>
                </RouterLink>
            </div>
        </div>
    </div>
</template>