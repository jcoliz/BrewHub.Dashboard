<script setup lang="ts">
import FeatherIcon from './FeatherIcon.vue';
import { RouterLink } from 'vue-router';

defineProps<{ title: string, link: string, icon?: string, tid?: string }>();
defineEmits([ 'click' ]);

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
