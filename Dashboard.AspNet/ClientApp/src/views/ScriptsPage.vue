<script setup lang="ts">
</script>
<template>

<main 
    data-test-id="Devices" 
    class="col-md-9 ms-sm-auto col-lg-10 px-md-4"
  >

<div class="container my-5">
  <div class="p-5 text-center bg-body-tertiary rounded-3">
    <h1 class="text-body-emphasis">Device-side Scripting</h1>
    <p class="lead">
      For devices which can run scripts, this is the place to manage them. Scripts are stored in blob storage.
      On this page, you can create, view, upload, configure, or delete those scripts. To run them on a device, use device properties and commands
      to configure and launch them on the targetted devices.
    </p>
  </div>
</div>

<div class="d-flex justify-content-between flex-wrap flex-md-nowrap align-items-center pt-3 pb-2 mb-3 border-bottom">
    <h1 class="h2">Scripts</h1>
    <div class="btn-toolbar mb-2 mb-md-0">
        <div class="btn-group me-2">
            <button type="button" class="btn btn-sm btn-outline-secondary">
                <span data-feather="plus-circle"></span>
                Add
            </button>
            <button type="button" class="btn btn-sm btn-outline-secondary">Import</button>
            <button type="button" class="btn btn-sm btn-outline-secondary">Export</button>
        </div>
    </div>
</div>

<div class="table-responsive">
      <table class="table text-left">
        <thead>
          <tr>
            <th style="width: 36%;">Name</th>
            <th style="width: 20%;">Environment</th>
            <th style="width: 15%;">Version</th>
            <th style="width: 15%;">Updated</th>
            <th style="width: 14%;"></th>
          </tr>
        </thead>
        <tbody>
          <tr>
            <th scope="row" class="text-start">Cold Mash</th>
            <td>Production</td>
            <td>1.0.10</td>
            <td>1 month ago</td>
          </tr>
          <tr>
            <th scope="row" class="text-start">Cold Mash</th>
            <td>Staging</td>
            <td>1.0.15</td>
            <td>4 hours ago</td>
          </tr>
          <tr>
            <th scope="row" class="text-start">Cold Mash</th>
            <td>Development</td>
            <td>1.0.16</td>
            <td>1 hour ago</td>
          </tr>
          <tr>
            <th scope="row" class="text-start">Hot Mash Fast</th>
            <td>Production</td>
            <td>1.1.1</td>
            <td>3 months ago</td>
          </tr>
          <tr>
            <th scope="row" class="text-start">Hot Mash Slow</th>
            <td>Production</td>
            <td>1.1.1</td>
            <td>3 months ago</td>
          </tr>
          <tr>
            <th scope="row" class="text-start">Hot Mash Slow</th>
            <td>Staging</td>
            <td>1.1.1</td>
            <td>9 hours ago</td>
          </tr>
        </tbody>
      </table>
    </div>
</main>
</template>