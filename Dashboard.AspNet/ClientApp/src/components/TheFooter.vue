<script setup lang="ts">
import GithubButton from 'vue-github-button'
import versionText from '../assets/version.txt?raw';
</script>

<template>
    <footer class="row bg-light py-4 mt-3">
        <div class="col text-end">
            <GithubButton href="https://github.com/jcoliz/VueJS.NET.StartKit/">View on GitHub</GithubButton>
            <p class="mb-0">
            <small>Version: {{ versionText }}</small>
            </p>
        </div>
    </footer>
</template>