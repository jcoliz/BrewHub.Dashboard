<script setup lang="ts">
import { computed } from 'vue'
import { icons, FeatherIconNames } from 'feather-icons'

// TODO: Figure out how to validate that this string IS one of 
// feather.FeatherIconNames
const props = defineProps<{ icon:string }>();

const iconSvg = computed(() => {
  return icons[ props.icon as FeatherIconNames ].toSvg()
})
</script>

<template>
    <span v-html="iconSvg"></span>
</template>