<script lang="ts">
  type Creative = {
    id: number
    name: string
    htmlContent: string
    status: string
    createdAtUtc: string
  }

  type CreativeForm = {
    name: string
    htmlContent: string
    status: string
  }

  export let showCreateForm = false
  export let selectedCreative: Creative | null = null
  export let createForm: CreativeForm
  export let editForm: CreativeForm
  export let statusOptions: string[] = []
  export let saving = false
  export let onPreview: (title: string, htmlContent: string) => void = () => {}
  export let onCreateCreative: () => void = () => {}
  export let onCancelCreate: () => void = () => {}
  export let onSaveCreative: () => void = () => {}
  export let onLaunchCreative: () => void = () => {}
  export let formatDate: (value: string) => string = (value) => value
  export let statusTone: (status: string) => string = (status) => status.toLowerCase()
</script>

{#if showCreateForm}
  <section class="editor-panel">
    <div class="panel-title-row panel-heading">
      <div>
        <h2>Create Creative</h2>
        <p class="muted panel-copy">Add a new creative to the demo workspace.</p>
      </div>
      <span class="info-chip">New draft</span>
    </div>

    <label>
      Name
      <input bind:value={createForm.name} />
    </label>

    <label>
      HTML Content
      <textarea rows="10" bind:value={createForm.htmlContent}></textarea>
      <span class="field-hint">Raw HTML is stored as-is in this demo app.</span>
    </label>

    <label>
      Status
      <select bind:value={createForm.status}>
        {#each statusOptions as status}
          <option value={status}>{status}</option>
        {/each}
      </select>
    </label>

    <div class="form-actions">
      <button
        class="ghost-button"
        onclick={() => onPreview(createForm.name || 'New Creative', createForm.htmlContent)}
        disabled={saving}
      >
        Preview
      </button>
      <button class="primary-button" onclick={onCreateCreative} disabled={saving}>Create</button>
      <button class="ghost-button" onclick={onCancelCreate} disabled={saving}>Cancel</button>
    </div>
  </section>
{:else if selectedCreative !== null}
  <section class="editor-panel">
    <div class="panel-title-row panel-heading">
      <div>
        <h2>Edit Creative</h2>
        <p class="muted panel-copy">Update content and trigger the placeholder launch request.</p>
      </div>
      <div class="editor-meta">
        <span class={`status-pill ${statusTone(selectedCreative.status)}`}>{selectedCreative.status}</span>
        <span class="created-pill">Created {formatDate(selectedCreative.createdAtUtc)}</span>
      </div>
    </div>

    <label>
      Name
      <input bind:value={editForm.name} />
    </label>

    <label>
      HTML Content
      <textarea rows="10" bind:value={editForm.htmlContent}></textarea>
      <span class="field-hint">Candidates can later discuss sanitization or preview rendering here.</span>
    </label>

    <label>
      Status
      <select bind:value={editForm.status}>
        {#each statusOptions as status}
          <option value={status}>{status}</option>
        {/each}
      </select>
    </label>

    <div class="form-actions">
      <button
        class="ghost-button"
        onclick={() => onPreview(selectedCreative.name, selectedCreative.htmlContent)}
        disabled={saving}
      >
        Preview
      </button>
      <button class="primary-button" onclick={onSaveCreative} disabled={saving}>Save</button>
      <button class="ghost-button" onclick={onLaunchCreative} disabled={saving}>Launch</button>
    </div>
  </section>
{:else}
  <section class="editor-panel empty-state">
    <h2>No creative selected</h2>
    <p class="muted">Select an existing creative or create a new one.</p>
  </section>
{/if}