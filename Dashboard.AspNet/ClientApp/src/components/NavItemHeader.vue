<script setup lang="ts">

/**
 * Header to define a section in a <nav>
 */

defineProps<{
  /**
   * Text to show the user on this header
   */
  title: string
}>();

</script>

<template>
    <div>
        <h6 class="sidebar-heading d-flex justify-content-between align-items-center px-3 mt-4 mb-1 text-muted">
          <span>{{ $props.title }}</span>
          <slot></slot>
        </h6>
    </div>
</template>