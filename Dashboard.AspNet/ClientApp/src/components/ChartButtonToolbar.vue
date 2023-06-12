<script setup lang="ts">
import FeatherIcon from '../components/FeatherIcon.vue';

</script>

<template>
<div class="btn-toolbar mb-2 mb-md-0">
    <div 
        class="btn-group me-2" 
        role="group" 
        aria-label="Chart style button group"
    >
        <input 
        id="vbtn-radio1" 
        name="vbtn-radio" 
        type="radio" 
        class="btn-check" 
        autocomplete="off" 
        :value="true" 
        >
        <label 
        data-test-id="ShowBarChart"
        for="vbtn-radio1"
        class="btn btn-sm btn-outline-secondary" 
        >
        Bar
        </label>
        <input 
        id="vbtn-radio2" 
        name="vbtn-radio" 
        type="radio" 
        class="btn-check" 
        autocomplete="off" 
        :value="false" 
        > 
        <label 
        data-test-id="ShowLineChart"
        for="vbtn-radio2"
        class="btn btn-sm btn-outline-secondary" 
        >
        Line
        </label>
    </div>

    <div 
        class="btn-group dropdown" 
        role="group">
        <button 
        id="btnDropTimescale" 
        data-test-id="TimescaleDropDown"
        type="button" 
        class="btn btn-sm btn-outline-secondary dropdown-toggle" 
        data-bs-toggle="dropdown"
        >
        <FeatherIcon icon="calendar"/>
        </button>
        <ul 
        class="dropdown-menu" 
        aria-labelledby="btnDropTimescale"
        >
            <li 
                v-for="(option,index) in [ 'A', 'B' ]" 
                :key="index"
            >
                <a 
                class="dropdown-item" 
                href="javascript:void(0)" 
                >
                {{ option }}
                </a>
            </li>
        </ul>
    </div>
</div>
</template>