<script setup lang="ts">
/**
 * Navigation breadcrumbs, so you can find your way home
 */

/**
 * Information we need about any one link in the upper breadcrumb history
 */
export interface IBreadCrumbsLink {
  title: string,
  href: string
};

defineProps<{
    /**
     * List of links back to previous pages
     */
    links: IBreadCrumbsLink[],

    /**
     * Display name of the page we're on
     */
    page: string
}>();

</script>

<template>
    <nav style="--bs-breadcrumb-divider: '>';" aria-label="breadcrumb">
        <ol class="breadcrumb">        
        <li 
            v-for="link in links"
            class="breadcrumb-item" 
        >
            <RouterLink :to="link.href">{{ link.title }}</RouterLink>
        </li>
        <li 
            class="breadcrumb-item active" 
            aria-current="page"
        >
            {{ page }}
        </li>
        </ol>
    </nav>
</template>