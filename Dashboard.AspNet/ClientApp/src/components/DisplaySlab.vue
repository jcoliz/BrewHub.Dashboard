<script setup lang="ts">
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
                    <li 
                        v-for="metric in slab?.telemetry" :key="slab?.id"
                        class="list-group-item d-flex justify-content-between lh-sm"
                    >
                        <div>
                            <h6 class="my-0">{{ metric.name }}</h6>
                            <small class="text-body-secondary">{{ metric.value }}</small>
                        </div>
                    </li>
                    <li 
                        v-for="metric in slab?.readOnlyProperties" :key="slab?.id"
                        class="list-group-item d-flex justify-content-between lh-sm"
                    >
                        <div>
                            <h6 class="my-0">{{ metric.name }}</h6>
                            <small class="text-body-secondary">{{ metric.value }}</small>
                        </div>
                    </li>
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