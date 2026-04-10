<script>
  import { onMount } from 'svelte'
  import CreativeEditorPanel from './components/CreativeEditorPanel.svelte'
  import CreativeListPanel from './components/CreativeListPanel.svelte'
  import DashboardSidebar from './components/DashboardSidebar.svelte'
  import LoginView from './components/LoginView.svelte'
  import MetricsGrid from './components/MetricsGrid.svelte'
  import PreviewModal from './components/PreviewModal.svelte'

  const tokenStorageKey = 'creative-demo-token'
  const statusOptions = ['Draft', 'Ready', 'LaunchRequested']

  let token = ''
  let creatives = []
  let selectedId = null
  let showCreateForm = false
  let loading = false
  let saving = false
  let showPreview = false
  let previewTitle = 'Creative Preview'
  let previewDocument = buildPreviewDocument('')
  let analytics = null
  let analyticsLoading = false
  let analyticsError = ''
  let pendingAnalyticsId = null
  let errorMessage = ''
  let launchMessage = ''
  let searchTerm = ''
  let statusFilter = 'All'

  let loginForm = {
    username: 'demo',
    password: 'password',
  }

  let createForm = emptyCreativeForm()
  let editForm = emptyCreativeForm()

  onMount(async () => {
    token = localStorage.getItem(tokenStorageKey) ?? ''

    if (token) {
      await loadCreatives()
    }
  })

  function emptyCreativeForm() {
    return {
      name: '',
      htmlContent: '<div><h1>New Creative</h1><p>Edit me.</p></div>',
      status: 'Draft',
    }
  }

  function fillEditForm(creative) {
    editForm = {
      name: creative.name,
      htmlContent: creative.htmlContent,
      status: creative.status,
    }
  }

  async function login() {
    errorMessage = ''
    launchMessage = ''

    try {
      const response = await fetch('/api/login', {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify(loginForm),
      })

      const body = await response.json()

      if (!response.ok) {
        throw new Error(body.message ?? 'Login failed.')
      }

      token = body.token
      localStorage.setItem(tokenStorageKey, token)
      await loadCreatives()
    } catch (error) {
      errorMessage = error.message
    }
  }

  async function api(path, options = {}) {
    const response = await fetch(path, {
      ...options,
      headers: {
        'Content-Type': 'application/json',
        'X-Demo-Token': token,
        ...(options.headers ?? {}),
      },
    })

    const hasJsonBody = response.status !== 204
    const body = hasJsonBody ? await response.json() : null

    if (!response.ok) {
      if (response.status === 401) {
        logout()
      }

      const validationErrors = body?.errors
        ? Object.values(body.errors).flat().join(' ')
        : null

      throw new Error(body?.message ?? body?.title ?? validationErrors ?? 'Request failed.')
    }

    return body
  }

  async function loadCreatives() {
    loading = true
    errorMessage = ''

    try {
      creatives = await api('/api/creatives')

      if (selectedId) {
        const selectedCreative = creatives.find((creative) => creative.id === selectedId)

        if (selectedCreative) {
          fillEditForm(selectedCreative)

          if (!analytics || analytics.creativeId !== selectedCreative.id) {
            await loadAnalytics(selectedCreative.id)
          }
        } else {
          selectedId = null
          clearAnalytics()
        }
      }

      if (!selectedId && creatives.length > 0 && !showCreateForm) {
        openCreative(creatives[0])
      }
    } catch (error) {
      errorMessage = error.message
    } finally {
      loading = false
    }
  }

  function openCreative(creative) {
    selectedId = creative.id
    showCreateForm = false
    launchMessage = ''
    fillEditForm(creative)
    void loadAnalytics(creative.id)
  }

  function startCreate() {
    selectedId = null
    showCreateForm = true
    launchMessage = ''
    errorMessage = ''
    createForm = emptyCreativeForm()
    clearAnalytics()
  }

  async function createCreative() {
    saving = true
    errorMessage = ''

    try {
      const created = await api('/api/creatives', {
        method: 'POST',
        body: JSON.stringify(createForm),
      })

      showCreateForm = false
      await loadCreatives()
      openCreative(created)
    } catch (error) {
      errorMessage = error.message
    } finally {
      saving = false
    }
  }

  async function saveCreative() {
    saving = true
    errorMessage = ''

    try {
      await api(`/api/creatives/${selectedId}`, {
        method: 'PUT',
        body: JSON.stringify(editForm),
      })

      await loadCreatives()
      launchMessage = ''
    } catch (error) {
      errorMessage = error.message
    } finally {
      saving = false
    }
  }

  async function launchCreative() {
    saving = true
    errorMessage = ''

    try {
      const result = await api(`/api/creatives/${selectedId}/launch`, {
        method: 'POST',
      })

      launchMessage = result.message
      await loadCreatives()
    } catch (error) {
      errorMessage = error.message
    } finally {
      saving = false
    }
  }

  function logout() {
    token = ''
    creatives = []
    selectedId = null
    showCreateForm = false
    showPreview = false
    clearAnalytics()
    errorMessage = ''
    launchMessage = ''
    localStorage.removeItem(tokenStorageKey)
  }

  async function loadAnalytics(creativeId) {
    analyticsLoading = true
    analyticsError = ''
    pendingAnalyticsId = creativeId

    try {
      const result = await api(`/api/creatives/${creativeId}/analytics?days=30`)

      if (pendingAnalyticsId !== creativeId) {
        return
      }

      analytics = result
    } catch (error) {
      if (pendingAnalyticsId !== creativeId) {
        return
      }

      analytics = null
      analyticsError = error.message
    } finally {
      if (pendingAnalyticsId === creativeId) {
        analyticsLoading = false
      }
    }
  }

  function clearAnalytics() {
    analytics = null
    analyticsLoading = false
    analyticsError = ''
    pendingAnalyticsId = null
  }

  function openPreview(title, htmlContent) {
    previewTitle = title || 'Creative Preview'
    previewDocument = buildPreviewDocument(htmlContent ?? '')
    showPreview = true
  }

  function closePreview() {
    showPreview = false
  }

  function buildPreviewDocument(htmlContent) {
    const trimmedContent = htmlContent.trim()

    if (!trimmedContent) {
      return `<!doctype html>
<html lang="en">
  <head>
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1" />
    <title>Creative Preview</title>
    <style>
      body {
        font-family: Aptos, "Segoe UI Variable", "Segoe UI", sans-serif;
        color: #272b44;
        margin: 0;
        padding: 24px;
        background: #ffffff;
      }
    </style>
  </head>
  <body>
    <p>No HTML content to preview.</p>
  </body>
</html>`
    }

    if (/<html[\s>]/i.test(trimmedContent)) {
      return trimmedContent
    }

    return `<!doctype html>
<html lang="en">
  <head>
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1" />
    <title>Creative Preview</title>
    <style>
      body {
        font-family: Aptos, "Segoe UI Variable", "Segoe UI", sans-serif;
        color: #272b44;
        margin: 0;
        padding: 24px;
        background: #ffffff;
      }
    </style>
  </head>
  <body>${trimmedContent}</body>
</html>`
  }

  function formatDate(value) {
    return new Date(value).toLocaleString()
  }

  function statusTone(status) {
    return status.toLowerCase()
  }

  function matchesSearch(creative) {
    const normalizedSearch = searchTerm.trim().toLowerCase()

    if (!normalizedSearch) {
      return true
    }

    return [creative.name, creative.htmlContent, creative.status]
      .join(' ')
      .toLowerCase()
      .includes(normalizedSearch)
  }

  function matchesStatus(creative) {
    return statusFilter === 'All' || creative.status === statusFilter
  }

  $: selectedCreative = creatives.find((creative) => creative.id === selectedId) ?? null
  $: filteredCreatives = creatives.filter((creative) => matchesSearch(creative) && matchesStatus(creative))
  $: readyCount = creatives.filter((creative) => creative.status === 'Ready').length
  $: draftCount = creatives.filter((creative) => creative.status === 'Draft').length
  $: launchRequestedCount = creatives.filter((creative) => creative.status === 'LaunchRequested').length
</script>

<svelte:head>
  <title>Creative Management Demo</title>
</svelte:head>

{#if !token}
  <LoginView {loginForm} {errorMessage} onLogin={login} />
{:else}
  <main class="dashboard-shell">
    <div class="dashboard-grid">
      <DashboardSidebar onStartCreate={startCreate} />

      <section class="workspace">
        <header class="workspace-topbar">
          <div class="workspace-greeting">
            <p class="eyebrow">Creative Ops Demo</p>
            <h1>Hello Demo Team</h1>
          </div>

          <div class="workspace-topbar-actions">
            <label class="search-shell">
              <span class="search-icon">⌕</span>
              <input aria-label="Search creatives" bind:value={searchTerm} placeholder="Search" />
            </label>
            <button class="ghost-button" onclick={logout}>Log Out</button>
          </div>
        </header>

        <MetricsGrid
          creativesLength={creatives.length}
          {draftCount}
          {readyCount}
          {launchRequestedCount}
        />

        <section class="workspace-card shell-card">
          <div class="workspace-card-header">
            <div>
              <h2>Creatives</h2>
              <p class="muted panel-copy">Internal creative workspace</p>
            </div>

            <div class="workspace-toolbar">
              <label class="search-shell compact-search">
                <span class="search-icon">⌕</span>
                <input aria-label="Search creatives" bind:value={searchTerm} placeholder="Search" />
              </label>

              <label class="filter-shell">
                <span>Filter</span>
                <select aria-label="Filter by status" bind:value={statusFilter}>
                  <option value="All">All statuses</option>
                  {#each statusOptions as status}
                    <option value={status}>{status}</option>
                  {/each}
                </select>
              </label>

              <button class="primary-button" onclick={startCreate}>New Creative</button>
            </div>
          </div>

          {#if errorMessage}
            <p class="banner error">{errorMessage}</p>
          {/if}

          {#if launchMessage}
            <p class="banner success">{launchMessage}</p>
          {/if}

          <section class="layout dashboard-content">
            <CreativeListPanel
              {filteredCreatives}
              {loading}
              {selectedId}
              {showCreateForm}
              onOpenCreative={openCreative}
              formatDate={formatDate}
              statusTone={statusTone}
            />

            <CreativeEditorPanel
              {showCreateForm}
              {selectedCreative}
              {createForm}
              {editForm}
              {analytics}
              analyticsLoading={analyticsLoading}
              analyticsError={analyticsError}
              {statusOptions}
              {saving}
              onPreview={openPreview}
              onCreateCreative={createCreative}
              onCancelCreate={() => (showCreateForm = false)}
              onSaveCreative={saveCreative}
              onLaunchCreative={launchCreative}
              formatDate={formatDate}
              statusTone={statusTone}
            />
          </section>
        </section>
      </section>
    </div>

    <PreviewModal
      open={showPreview}
      title={previewTitle}
      document={previewDocument}
      onClose={closePreview}
    />
  </main>
{/if}
