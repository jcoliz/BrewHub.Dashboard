<script setup lang="ts">
/**
 * Apply consistent formatting to page titles
 */

defineProps<{
  /**
   * Title of the page
   */
  title: string
}>();

</script>
<template>
    <h1
        data-test-id="ThePageTitle" 
        class="h2"
    >
        {{ title }}
    </h1>
</template>