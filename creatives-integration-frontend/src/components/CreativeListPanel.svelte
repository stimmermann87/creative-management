<script lang="ts">
  type Creative = {
    id: number
    name: string
    htmlContent: string
    status: string
    createdAtUtc: string
  }

  export let filteredCreatives: Creative[] = []
  export let loading = false
  export let selectedId: number | null = null
  export let showCreateForm = false
  export let onOpenCreative: (creative: Creative) => void = () => {}
  export let formatDate: (value: string) => string = (value) => value
  export let statusTone: (status: string) => string = (status) => status.toLowerCase()
</script>

<aside class="list-panel">
  <div class="panel-title-row panel-heading">
    <div>
      <h3 class="section-title">Creative list</h3>
      <p class="muted panel-copy">Select one to edit or request launch.</p>
    </div>
    <span class="info-chip">{loading ? 'Refreshing...' : `${filteredCreatives.length} shown`}</span>
  </div>

  {#if filteredCreatives.length === 0}
    <div class="list-empty">
      <h3>No matching creatives</h3>
      <p class="muted">Try a different search or filter, or create a new one.</p>
    </div>
  {:else}
    <ul class="creative-list">
      {#each filteredCreatives as creative}
        <li>
          <button
            class:selected={creative.id === selectedId && !showCreateForm}
            class="list-item"
            onclick={() => onOpenCreative(creative)}
          >
            <div class="list-item-header">
              <strong>{creative.name}</strong>
              <span class={`status-pill ${statusTone(creative.status)}`}>{creative.status}</span>
            </div>
            <small>{formatDate(creative.createdAtUtc)}</small>
          </button>
        </li>
      {/each}
    </ul>
  {/if}
</aside>