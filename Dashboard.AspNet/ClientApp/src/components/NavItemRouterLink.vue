<script setup lang="ts">

/**
 * Nav item link to a location known to vue-router, from within a <nav>
 */

import FeatherIcon from './FeatherIcon.vue';
import { RouterLink } from 'vue-router';

defineProps<{
    /**
     * Displayed title of the link
     */
    title: string,

    /**
     * Vue-router reference to where the navigation shall go
     */

    link: string,

    /**
     * Optional icon to display with the link
     */
    icon?: string

    /**
     * Optional data-test-id value for this link
     *  Else will use the title
     */
     tid?: string    
}>();

defineEmits<{
    /**
     * User has clicked on this link
     */
    (e: 'click'): void
}>();

</script>

<template>
    <li class="nav-item">
        <RouterLink 
            :data-test-id="`Nav-${tid??title}`"
            class="nav-link" 
            aria-current="page" 
            :to="link"
            @click="$emit('click')"
        >
            <FeatherIcon 
                v-if="icon" 
                :icon="icon"
            />
            {{ title }}
        </RouterLink>
    </li>
</template>
