<script setup lang="ts">
/**
 * Nav item link to an external site, from within a <nav>
 */

import FeatherIcon from './FeatherIcon.vue';

defineProps<{
    /**
     * Displayed title of the link
     */
    title: string,

    /**
     * Http reference to where the navigation shall go
     */

    link: string,

    /**
     * Optional icon to display with the link
     */
    icon?: string
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
        <a 
            class="nav-link" 
            aria-current="page" 
            target="_blank"
            :href="link"
            @click="$emit('click')"
        >
            <FeatherIcon 
                v-if="icon" 
                :icon="icon"
            />
            <slot></slot>
            {{ title }}
        </a>
    </li>
</template>
