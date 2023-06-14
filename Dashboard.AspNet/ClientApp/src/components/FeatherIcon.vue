<script setup lang="ts">

/**
 * Displays a single feather icon
 */

import { computed } from 'vue'
import { icons, FeatherIconNames } from 'feather-icons'

const props = defineProps<{
  /**
   * Name of the icon, as defined by feather-icons (feather.FeatherIconNames)
   * 
   * TODO: Need to figure out how to validate this!
   */
  icon: string
}>();

const iconSvg = computed(() => {
  return icons[ props.icon as FeatherIconNames ].toSvg()
})
</script>

<template>
    <span v-html="iconSvg"></span>
</template>

<style>
/* Doesn't apply if I scope it?? */

svg.feather {
  width: 16px;
  height: 16px;
  vertical-align: text-bottom;
}

</style>