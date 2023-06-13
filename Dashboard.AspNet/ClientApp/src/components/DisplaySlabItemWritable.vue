<script setup lang="ts">

/**
 * Displays a single writable property metric on a DisplaySlab
 */

import { ref, onMounted } from 'vue';
import { IDisplayMetric } from '../apiclients/apiclient.ts';
const props = defineProps<{ metric?: IDisplayMetric}> ();
defineEmits<{ (e: 'update', text: string): void }>();

const text = ref<string|undefined>(undefined);

onMounted(() => {
    text.value = props.metric?.value
});

</script>

<template>
    <div 
        class="col-12 mt-3"
        v-if="metric"
    >
        <label :for="`writable-${metric.id}`" class="h6 form-label">{{ metric.name }}</label>
        <div class="input-group">
            <input 
                type="text" 
                :id="`writable-${metric.id}`" 
                class="form-control" 
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
                @click="$emit('update',text)"
            >
                Update
            </button>
        </div>
    </div>
</template>