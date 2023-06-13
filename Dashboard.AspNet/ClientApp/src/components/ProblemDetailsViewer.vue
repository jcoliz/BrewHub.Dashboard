<script setup lang="ts">

/**
 * Displays a `ProblemDetails` server response in a readable way
 */

import { ProblemDetails } from '../apiclients/apiclient.ts';

defineProps<{
    /**
     * Contents to display
     */
    problem: ProblemDetails,
}>();

</script>
<template>
    <div>
        <svg xmlns="http://www.w3.org/2000/svg" style="display: none;">
            <symbol id="exclamation-triangle-fill" viewBox="0 0 16 16">
            <path d="M8.982 1.566a1.13 1.13 0 0 0-1.96 0L.165 13.233c-.457.778.091 1.767.98 1.767h13.713c.889 0 1.438-.99.98-1.767L8.982 1.566zM8 5c.535 0 .954.462.9.995l-.35 3.507a.552.552 0 0 1-1.1 0L7.1 5.995A.905.905 0 0 1 8 5zm.002 6a1 1 0 1 1 0 2 1 1 0 0 1 0-2z"/>
            </symbol>
        </svg>

        <div class="alert alert-danger d-flex align-items-center" role="alert">
            <svg class="bi flex-shrink-0 me-2" role="img" aria-label="Danger:"><use xlink:href="#exclamation-triangle-fill"/></svg>
            <div class="ms-2">
            <p class="my-0">
                <strong>{{ problem.status }} {{ problem.title }}</strong>
            </p>
            <p v-if="problem.instance" class="my-0">{{ problem.instance }}</p>
            <p v-if="problem.detail" class="my-0">{{ problem.detail }}</p>
            </div>
        </div>
    </div>
</template>

<style scoped>
.bi {
    fill: currentcolor;
    width: 3em;
    height: 3em;
}
</style>
