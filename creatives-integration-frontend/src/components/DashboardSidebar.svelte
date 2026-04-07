<script>
  export let onStartCreate = () => {}
</script>

<aside class="sidebar shell-card">
  <div class="sidebar-brand">
    <div class="brand-icon">◎</div>
    <div>
      <strong>Dashboard</strong>
      <small>v0.1</small>
    </div>
  </div>

  <nav class="sidebar-nav" aria-label="Primary">
    <button class="nav-link" type="button">Overview</button>
    <button class="nav-link" type="button">Workflow</button>
    <button class="nav-link active" type="button">Creatives</button>
    <button class="nav-link" type="button">Launches</button>
    <button class="nav-link" type="button">Support</button>
  </nav>

  <div class="sidebar-promo">
    <p class="promo-title">Need a new draft?</p>
    <p class="promo-copy">Start a fresh creative and keep the workflow moving.</p>
    <button class="sidebar-cta" onclick={onStartCreate}>New Creative</button>
  </div>

  <div class="sidebar-user">
    <div class="user-avatar">D</div>
    <div>
      <strong>Demo User</strong>
      <small>Project Manager</small>
    </div>
  </div>
</aside>