<script setup lang="ts">
import FeatherIcon from './FeatherIcon.vue';

defineProps<{ title: string, link: string, icon?: string }>();
defineEmits([ 'click' ]);

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
