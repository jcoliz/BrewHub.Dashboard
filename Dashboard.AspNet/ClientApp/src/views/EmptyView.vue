<script setup lang="ts">

defineProps<{ title: string }>();

</script>

<template>
    <main 
    :data-test-id="$props.title" 
    class="col-md-9 ms-sm-auto col-lg-10 px-md-4"
    >
        <section class="container px-0">
            <div class="row py-3">
                <div class="col-md-8">
                    <h1 class="h2">{{ $props.title }}</h1>
                    <p class="lead text-muted">This page has not yet been implemented.</p>
                </div>
            </div>
        </section>   
    </main>
</template>

<style scoped>
.bi {
    fill: currentcolor;
    width: 1em;
    height: 1em;
}
</style>