<script setup lang="ts">

defineProps<{ links: { title:string, href:string }[], page:string }>();

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