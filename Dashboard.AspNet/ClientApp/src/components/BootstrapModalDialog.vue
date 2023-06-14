<script setup lang="ts">
import { ref, onMounted, watch } from 'vue';
import { Modal } from "bootstrap";

export interface IBootstrapModalDialogInterface {
    title?: string
    message?: string
    showCloseIcon?: boolean
    showCancelButton?: boolean
}

const props = defineProps<{
  /**
   * Overall description of what's happening here
   */
  data: IBootstrapModalDialogInterface,

  /**
   * Whether the dialog should currently be shown to the user
   */
  show: Boolean
}>();

const emit = defineEmits<{
    /**
     * User has closed the dialog
     */
    (e: 'update:show', value:boolean): void
}>();

const rootelement = ref<HTMLElement | undefined>(undefined);
const modal = ref<Modal | undefined>(undefined);

onMounted(() => {
  if (rootelement.value) {
    rootelement.value.addEventListener("hide.bs.modal", () => {
      emit('update:show',false);
    });
    const instance = Modal.getInstance(rootelement.value) ?? undefined;
    if (instance != undefined)
      modal.value = instance;
    else
      modal.value = new Modal(rootelement.value);
    if (props.show && modal.value) {
      modal.value.show();
    }
  }
});
watch(
  () => props.show,
  (newValue, _) => {
    console.log(`watching show: ${newValue}`);
    if (newValue && modal.value) {
        modal.value.show();
      }
  });
</script>

<template>
  <div data-test-id="DialogModal" class="modal fade" ref="rootelement">
    <div class="modal-dialog modal-dialog-centered">
      <div class="modal-content">
        <div class="modal-header">
          <slot name="header">
            <h5 class="modal-title">{{ $props.data.title }}</h5>
            <button
              v-if="data.showCloseIcon"
              type="button"
              class="btn-close"
              data-bs-dismiss="modal"
              aria-label="Close"
            ></button>
          </slot>
        </div>
        <div class="modal-body">
          <slot>
            {{ $props.data.message }}
          </slot>
        </div>
        <div class="modal-footer">
          <slot name="footer">
            <button
              v-if="data.showCancelButton"
              type="button"
              class="btn btn-outline-secondary"
              data-bs-dismiss="modal"
            >
              Cancel
            </button>
            <button
              type="button"
              class="btn btn-primary"
              data-bs-dismiss="modal"
            >
              OK
            </button>
          </slot>
        </div>
      </div>
    </div>
  </div>
</template>

<style></style>