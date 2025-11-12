# API Reference

<script setup>
import { ApiReference } from '@scalar/api-reference'
import spec from '../openapi.json'
</script>

<ApiReference :configuration="{
  spec,
  hideModels: false,
  hideDownloadButton: false,
}" />

<style>
.api-reference {
  margin-top: 2rem;
}
</style>
