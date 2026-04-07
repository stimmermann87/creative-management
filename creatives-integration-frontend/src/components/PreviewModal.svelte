<script lang="ts">
  type KeyboardEventLike = {
    key: string
  }

  type MouseEventLike = {
    stopPropagation: () => void
  }

  type KeyboardPropagationEventLike = {
    stopPropagation: () => void
  }

  export let open = false
  export let title = 'Creative Preview'
  export let document = ''
  export let onClose: () => void = () => {}

  function handleKeydown(event: KeyboardEventLike) {
    if (open && event.key === 'Escape') {
      onClose()
    }
  }

  function handleModalClick(event: MouseEventLike) {
    event.stopPropagation()
  }

  function handleModalKeydown(event: KeyboardPropagationEventLike) {
    event.stopPropagation()
  }
</script>

<svelte:window on:keydown={handleKeydown} />

{#if open}
  <div class="modal-backdrop" role="presentation" onclick={onClose}>
    <div
      class="preview-modal"
      role="dialog"
      tabindex="-1"
      aria-modal="true"
      aria-label="Creative Preview"
      onclick={handleModalClick}
      onkeydown={handleModalKeydown}
    >
      <div class="preview-modal-header">
        <div>
          <p class="eyebrow preview-eyebrow">Preview</p>
          <h2>{title}</h2>
        </div>
        <button class="ghost-button preview-close" onclick={onClose}>Close</button>
      </div>

      <div class="preview-frame-shell">
        <iframe
          class="preview-frame"
          title="Creative preview"
          sandbox="allow-same-origin allow-scripts allow-forms allow-modals allow-popups"
          srcdoc={document}
        ></iframe>
      </div>
    </div>
  </div>
{/if}