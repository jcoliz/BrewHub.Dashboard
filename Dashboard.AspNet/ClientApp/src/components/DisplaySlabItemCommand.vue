<script setup lang="ts">

/**
 * Displays a single command metric on a DisplaySlab
 */

import { ref } from 'vue';
import { IDisplayMetric } from '../apiclients/apiclient.ts';

defineProps<{
    /**
     * Which metric are we displaying
     */
     metric?: IDisplayMetric
}>();

defineEmits<{
    /**
     * User desires to send a command back to the device on this metric
     */
    (e: 'go', text: string): void
}>();

const text = ref<string>("");

</script>

<template>
    <div 
        class="col-12 mt-3"
        v-if="metric"
    >
        <label :for="`command-${metric.id}`" class="h6 form-label">{{ metric.name }}</label>
        <div class="input-group">
            <input 
                type="text" 
                :id="`command-${metric.id}`" 
                class="form-control" 
                :placeholder="metric.value" 
                v-model="text"
            >
                <span 
                    v-if="metric.units"
                    class="input-group-text"
                >
                    {{ metric.units }}
                </span>
            <button 
                class="btn btn-secondary" 
                @click="$emit('go',text)"
            >
                Go
            </button>
        </div>
    </div>
</template>