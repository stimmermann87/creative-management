<script lang="ts">
  import LineChart from './LineChart.svelte'

  type ChartSeries = {
    key: string
    label: string
    color: string
    axis?: 'left' | 'right'
    formatValue: (value: number) => string
  }

  type CreativeAnalyticsPoint = {
    date: string
    impressions: number
    price: number
  }

  type CreativeAnalytics = {
    creativeId: string
    fromDate: string
    toDate: string
    dailyPoints: CreativeAnalyticsPoint[]
  }

  export let analytics: CreativeAnalytics | null = null
  export let loading = false
  export let errorMessage = ''

  const impressionFormatter = new Intl.NumberFormat('en-US')
  const currencyFormatter = new Intl.NumberFormat('en-US', {
    style: 'currency',
    currency: 'USD',
    minimumFractionDigits: 2,
  })

  function parseDate(value: string): Date {
    return new Date(`${value}T00:00:00`)
  }

  function formatShortDate(value: string): string {
    return parseDate(value).toLocaleDateString('en-US', {
      month: 'short',
      day: 'numeric',
    })
  }

  function formatLongDate(value: string): string {
    return parseDate(value).toLocaleDateString('en-US', {
      month: 'short',
      day: 'numeric',
      year: 'numeric',
    })
  }

  function formatCount(value: number): string {
    return impressionFormatter.format(Math.round(value))
  }

  function formatCurrency(value: number): string {
    return currencyFormatter.format(value)
  }

  $: chartPoints = analytics?.dailyPoints.map((point) => ({
    label: formatShortDate(point.date),
    title: formatLongDate(point.date),
    impressions: point.impressions,
    price: point.price,
  })) ?? []

  const chartSeries: ChartSeries[] = [
    {
      key: 'impressions',
      label: 'Impressions',
      color: '#3485ff',
      formatValue: formatCount,
    },
    {
      key: 'price',
      label: 'Price',
      color: '#d57a3a',
      axis: 'right',
      formatValue: formatCurrency,
    },
  ]

  $: totalImpressions = analytics?.dailyPoints.reduce((sum, point) => sum + point.impressions, 0) ?? 0
  $: totalPrice = analytics?.dailyPoints.reduce((sum, point) => sum + point.price, 0) ?? 0
  $: firstImpressionPoint = analytics?.dailyPoints.find((point) => point.impressions > 0) ?? null
</script>

<section class="analytics-card">
  <div class="analytics-heading">
    <div>
      <h3>Delivery Analytics</h3>
      <p class="analytics-copy">Raw daily impressions and price for the selected creative.</p>
    </div>

    {#if analytics}
      <span class="range-pill">{formatLongDate(analytics.fromDate)} to {formatLongDate(analytics.toDate)}</span>
    {/if}
  </div>

  {#if loading}
    <div class="analytics-state">Loading analytics…</div>
  {:else if errorMessage}
    <div class="analytics-state analytics-error">{errorMessage}</div>
  {:else if analytics}
    <LineChart points={chartPoints} series={chartSeries} />

    <div class="analytics-summary">
      <article>
        <span>Total impressions</span>
        <strong>{formatCount(totalImpressions)}</strong>
      </article>

      <article>
        <span>Total price</span>
        <strong>{formatCurrency(totalPrice)}</strong>
      </article>

      <article>
        <span>First impression date</span>
        <strong>{firstImpressionPoint ? formatLongDate(firstImpressionPoint.date) : 'No delivery yet'}</strong>
      </article>
    </div>
  {:else}
    <div class="analytics-state">Select a creative to view daily delivery.</div>
  {/if}
</section>

<style>
  .analytics-card {
    display: grid;
    gap: 18px;
    padding: 18px;
    border: 1px solid #e8edf6;
    border-radius: 14px;
    background: linear-gradient(180deg, #fcfdff 0%, #f7f9fd 100%);
  }

  .analytics-heading {
    display: flex;
    justify-content: space-between;
    align-items: flex-start;
    gap: 12px;
  }

  .analytics-heading h3 {
    margin: 0;
    font-size: 1.08rem;
    color: #1f284a;
  }

  .analytics-copy {
    margin-top: 6px;
    color: #7a86a5;
    font-size: 0.95rem;
  }

  .range-pill {
    display: inline-flex;
    align-items: center;
    padding: 10px 12px;
    border: 1px solid #e8edf6;
    border-radius: 12px;
    background: #ffffff;
    color: #62708e;
    font-size: 0.88rem;
    font-weight: 700;
    white-space: nowrap;
  }

  .analytics-summary {
    display: grid;
    grid-template-columns: repeat(3, minmax(0, 1fr));
    gap: 12px;
  }

  .analytics-summary article {
    display: grid;
    gap: 6px;
    padding: 14px;
    border: 1px solid #e6ebf5;
    border-radius: 12px;
    background: #ffffff;
  }

  .analytics-summary span {
    color: #7a86a5;
    font-size: 0.9rem;
    font-weight: 700;
  }

  .analytics-summary strong {
    color: #222c47;
    font-size: 1.08rem;
    line-height: 1.3;
  }

  .analytics-state {
    display: grid;
    place-items: center;
    min-height: 220px;
    border: 1px dashed #dfe5f0;
    border-radius: 14px;
    background: #fbfcfe;
    color: #7a86a5;
    text-align: center;
    padding: 16px;
  }

  .analytics-error {
    color: #cb3d3d;
    border-color: #ffd4d4;
    background: #fff4f4;
  }

  @media (max-width: 900px) {
    .analytics-heading {
      flex-direction: column;
      align-items: stretch;
    }

    .analytics-summary {
      grid-template-columns: 1fr;
    }

    .range-pill {
      white-space: normal;
    }
  }
</style>