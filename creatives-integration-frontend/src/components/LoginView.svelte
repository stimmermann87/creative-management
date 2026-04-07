<script>
  export let loginForm
  export let errorMessage = ''
  export let onLogin = () => {}
</script>

<main class="login-shell">
  <div class="login-layout">
    <section class="login-intro">
      <p class="eyebrow">Creative Ops Demo</p>
      <h1>Creative Management</h1>
      <p class="lead">
        Review, update, and request launch for internal ad creatives in one intentionally small app.
      </p>

      <div class="intro-chips">
        <span class="info-chip">In-memory data</span>
        <span class="info-chip">Demo token auth</span>
        <span class="info-chip">Placeholder launch</span>
      </div>
    </section>

    <section class="card login-card login-panel">
      <div class="panel-copy-block">
        <h2>Sign In</h2>
        <p class="muted">Use the seeded demo account to enter the app.</p>
      </div>

      <div class="demo-credentials">
        <strong>Demo login</strong>
        <div>Username: demo</div>
        <div>Password: password</div>
      </div>

      <label>
        Username
        <input bind:value={loginForm.username} />
      </label>

      <label>
        Password
        <input type="password" bind:value={loginForm.password} />
      </label>

      <button class="primary-button" onclick={onLogin}>Log In</button>

      {#if errorMessage}
        <p class="error-text">{errorMessage}</p>
      {/if}
    </section>
  </div>
</main>