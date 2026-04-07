<script>
  export let creativesLength = 0
  export let draftCount = 0
  export let readyCount = 0
  export let launchRequestedCount = 0
</script>

<section class="metrics-grid">
  <article class="metric-card shell-card">
    <div class="metric-icon mint">◎</div>
    <div>
      <p class="metric-label">Total Creatives</p>
      <strong class="metric-value">{creativesLength}</strong>
      <small class="metric-detail">All items in the workspace</small>
    </div>
  </article>

  <article class="metric-card shell-card">
    <div class="metric-icon lilac">◌</div>
    <div>
      <p class="metric-label">Drafts</p>
      <strong class="metric-value">{draftCount}</strong>
      <small class="metric-detail">Ready for editing</small>
    </div>
  </article>

  <article class="metric-card shell-card">
    <div class="metric-icon sky">◍</div>
    <div>
      <p class="metric-label">Ready</p>
      <strong class="metric-value">{readyCount}</strong>
      <small class="metric-detail">Available to launch</small>
    </div>
  </article>

  <article class="metric-card shell-card">
    <div class="metric-icon peach">◉</div>
    <div>
      <p class="metric-label">Launch Requested</p>
      <strong class="metric-value">{launchRequestedCount}</strong>
      <small class="metric-detail">Placeholder integration state</small>
    </div>
  </article>
</section>