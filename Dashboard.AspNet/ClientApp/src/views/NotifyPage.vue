<script setup lang="ts">
</script>
<template>

<main 
    data-test-id="Devices" 
    class="col-md-9 ms-sm-auto col-lg-10 px-md-4"
  >
<div class="d-flex justify-content-between flex-wrap flex-md-nowrap align-items-center pt-3 pb-2 mb-3 border-bottom">
    <h1 class="h2">Notifications</h1>
    <div class="btn-toolbar mb-2 mb-md-0">
        <div class="btn-group me-2">
            <button type="button" class="btn btn-sm btn-outline-secondary">
                <span data-feather="user-plus"></span>
                Add
            </button>
            <button type="button" class="btn btn-sm btn-outline-secondary">Import</button>
            <button type="button" class="btn btn-sm btn-outline-secondary">Export</button>
        </div>
    </div>
</div>

<div class="table-responsive">
      <table class="table text-center">
        <thead>
          <tr>
            <th style="width: 36%;">Name</th>
            <th style="width: 14%;">Devices</th>
            <th style="width: 15%;">Status</th>
            <th style="width: 15%;">Duration</th>
            <th style="width: 20%;">Recipient</th>
          </tr>
        </thead>
        <tbody>
          <tr>
            <th scope="row" class="text-start">Failures</th>
            <td>*</td>
            <td>&GreaterEqual;800</td>
            <td>PT5M</td>
            <td>*</td>
          </tr>
          <tr>
            <th scope="row" class="text-start">Errors</th>
            <td>*</td>
            <td>&GreaterEqual;200</td>
            <td>PT4H</td>
            <td>Frank White</td>
          </tr>
          <tr>
            <th scope="row" class="text-start">Warnings</th>
            <td>*</td>
            <td>&gt;0</td>
            <td>PT12H</td>
            <td>Frank White<br/>Martha Brown</td>
          </tr>
        </tbody>
      </table>
    </div>

<div class="d-flex justify-content-between flex-wrap flex-md-nowrap align-items-center pt-3 pb-2 mb-3 border-bottom">
    <h1 class="h2">Recipients</h1>
    <div class="btn-toolbar mb-2 mb-md-0">
        <div class="btn-group me-2">
            <button type="button" class="btn btn-sm btn-outline-secondary">
                <span data-feather="user-plus"></span>
                Add
            </button>
            <button type="button" class="btn btn-sm btn-outline-secondary">Import</button>
            <button type="button" class="btn btn-sm btn-outline-secondary">Export</button>
        </div>
    </div>
</div>

<div class="table-responsive">
      <table class="table text-center">
        <thead>
          <tr>
            <th style="width: 36%;">Name</th>
            <th style="width: 32%;">SMS</th>
            <th style="width: 32%;">EMail</th>
          </tr>
        </thead>
        <tbody>
          <tr>
            <th scope="row" class="text-start">Frank White</th>
            <td>+1(555)432-2342</td>
            <td>fjw@@brewhub.net</td>
          </tr>
          <tr>
            <th scope="row" class="text-start">Martha Brown</th>
            <td></td>
            <td>martha@@brewhub.net</td>
          </tr>
          <tr>
            <th scope="row" class="text-start">Jenny Greene</th>
            <td>+1(555)432-2100</td>
            <td></td>
          </tr>
        </tbody>
      </table>
    </div>

</main>
</template>