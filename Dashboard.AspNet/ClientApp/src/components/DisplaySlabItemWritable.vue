<script setup lang="ts">
import { IDisplayMetric } from '../apiclients/apiclient.ts';
defineProps<{ metric?: IDisplayMetric}> ();
</script>

<template>
    <div 
        class="col-12 mt-3"
        v-if="metric"
    >
        <label :for="`writable-${metric.id}`" class="h6 form-label">{{ metric.name }}</label>
        <div class="input-group">
            <input type="text" :id="`writable-${metric.id}`" class="form-control" :value="metric.value">
                <span 
                    v-if="metric.units"
                    class="input-group-text"
                >
                    {{ metric.units }}
                </span>
            <button type="submit" class="btn btn-secondary">Update</button>
        </div>
    </div>
</template>