<script setup lang="ts">
import DisplaySlabItem from './DisplaySlabItem.vue';
import { IDisplayMetricGroup } from '../apiclients/apiclient.ts';
import { RouterLink } from 'vue-router';

defineProps<{ slab?: IDisplayMetricGroup, href?: string }>();

</script>

<template>
    <div
        data-test-id="DisplaySlab" 
        class="col"
    >
        <div class="card mb-4 rounded-3 shadow-sm border-primary">
            <div class="card-header py-3 text-bg-primary border-primary">
                <h4 class="my-0 fw-normal">{{ slab?.title }}</h4>
            </div>
            <div class="card-body text-start">
                <ul class="list-group mb-3">
                    <DisplaySlabItem
                        v-for="metric in slab?.telemetry" :metric="metric"
                    />
                    <DisplaySlabItem
                        v-for="metric in slab?.readOnlyProperties" :metric="metric"
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