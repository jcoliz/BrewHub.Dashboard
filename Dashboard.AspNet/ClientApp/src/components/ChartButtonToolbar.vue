<script setup lang="ts">

/**
 * Toolbar shown above the chart on pages which have a chart
 */

import { computed } from 'vue'
import FeatherIcon from '../components/FeatherIcon.vue';
import { TimeframeEnum } from '../apiclients/apiclient.ts';

defineProps<{
    /**
     * Whether to include the timeframe picker at all
     */
    showtimeframe: boolean,

    /**
     * The timeframe to show in the timeframe picker
     */
    timeframe: TimeframeEnum
}>();

defineEmits<{
    /**
     * User has chosen a new value in the timeframe picker
     */
    (e: 'update:timeframe', value: number): void,

    /**
     * User wishes to refresh the data display
     */
    (e: 'refresh'): void,
}>();

const timeframes = computed(() => {
  return Object.keys(TimeframeEnum).filter(x=>isNaN(Number(x)))
})

</script>

<template>
    <div class="btn-toolbar mb-2 mb-md-0">
        <div class="btn-group" role="group" aria-label="Button group with nested dropdown">
            <div 
                v-if="showtimeframe"
                class="btn-group" 
                role="group"
            >
                <button id="btnGroupDrop-timeframe" type="button" class="btn btn-sm btn-outline-secondary dropdown-toggle" data-bs-toggle="dropdown" aria-expanded="false">
                    <FeatherIcon icon="calendar"/>
                    {{ timeframes[$props.timeframe] }}
                </button>
                <ul class="dropdown-menu" aria-labelledby="btnGroupDrop-timeframe">
                    <li 
                        v-for="text in timeframes"
                        class="dropdown-item"
                        @click="$emit('update:timeframe',TimeframeEnum[text]); $emit('refresh');"
                    >
                        {{ text }}
                    </li>
                </ul>
            </div>
            <div class="btn-group" role="group">
                <button 
                    type="button" 
                    class="btn btn-sm btn-outline-secondary" 
                    @click="$emit('refresh')"
                >
                    <FeatherIcon icon="refresh-cw"/>
                </button>
            </div>
        </div>

        <div class="btn-group ms-2">
            <button type="button" class="btn btn-sm btn-outline-secondary">Share</button>
            <button type="button" class="btn btn-sm btn-outline-secondary">Export</button>
        </div>
    </div>
</template>