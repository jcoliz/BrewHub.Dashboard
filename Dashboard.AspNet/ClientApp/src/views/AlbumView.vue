<script setup lang="ts">
import { computed, ref } from "vue";
import AlbumCard from '../components/AlbumCard.vue';

defineProps<{ title: string }>();

const minCards:number = 1; 
const maxCards:number = 12; 
const numCards = ref<number>(6);

function isOkToRemoveCard(): boolean {
    return numCards.value > minCards;
}

function isOkToAddCard(): boolean {
    return numCards.value < maxCards;
}

function removeCard() {
    if (isOkToRemoveCard())
        numCards.value--;
}

function addCard() {
    if (isOkToAddCard())
        numCards.value++;
}

const numCardsMessage = computed(() =>
{
    if (numCards.value == 1)
        return "is only one card";
    else if (numCards.value == maxCards)
        return `are the maximum ${numCards.value} cards`;
    return `are ${numCards.value} cards`;
});

const addRemoveMessage = computed(() => {
    if (numCards.value == 1)
        return "add";
    else if (numCards.value == maxCards)
        return "remove";
    return "add or remove";
});

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
                <p class="lead text-muted">This is another placeholder page. It was taken directly from the Bootstrap examples. Currently, there <span data-test-id="CardCount">{{ numCardsMessage }}</span> here. Go ahead and {{ addRemoveMessage }} some cards to see reactivity in action!</p>
                <p>
                    <button 
                        data-test-id="AddButton"
                        class="btn btn-primary my-2 me-2" 
                        @click="addCard()" 
                        :disabled="!isOkToAddCard()"
                    >
                        Add one
                    </button>
                    <button 
                        data-test-id="RemoveButton"
                        class="btn btn-secondary my-2" 
                        @click="removeCard()" 
                        :disabled="!isOkToRemoveCard()"
                    >
                        Remove one
                    </button>
                </p>
            </div>
        </div>
    </section>

    <div class="album bg-light">
        <div class="container">
            <div
            data-test-id="AlbumCards"
            class="row row-cols-1 row-cols-sm-2 row-cols-md-3 g-3"
            >
                <AlbumCard 
                    v-for="i in numCards" 
                    :key="i" 
                />
            </div>
        </div>
    </div>

</main>
</template>