<script setup lang="ts">
import FeatherIcon from './FeatherIcon.vue';

defineProps<{ title: string, link: string, icon?: string }>();

</script>

<template>
    <li class="nav-item">
        <a 
            class="nav-link" 
            aria-current="page" 
            :href="link"
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
