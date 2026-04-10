<script lang="ts">
  type ChartPoint = {
    label: string
    title?: string
    [key: string]: string | number | undefined
  }

  type ChartAxis = 'left' | 'right'

  type ChartSeries = {
    key: string
    label: string
    color: string
    axis?: ChartAxis
    formatValue?: (value: number) => string
  }

  export let points: ChartPoint[] = []
  export let series: ChartSeries[] = []

  const chartWidth = 720
  const chartHeight = 260
  const padding = { top: 16, right: 62, bottom: 34, left: 62 }

  function axisFor(item: ChartSeries): ChartAxis {
    return item.axis ?? 'left'
  }

  function pointValue(point: ChartPoint, key: string): number {
    const value = point[key]
    return typeof value === 'number' ? value : 0
  }

  function seriesMax(items: ChartSeries[]): number {
    const maxValue = Math.max(
      0,
      ...items.flatMap((item) => points.map((point) => pointValue(point, item.key))),
    )

    return maxValue > 0 ? maxValue : 1
  }

  function xFor(index: number): number {
    const plotWidth = chartWidth - padding.left - padding.right

    if (points.length <= 1) {
      return padding.left + plotWidth / 2
    }

    return padding.left + (index / (points.length - 1)) * plotWidth
  }

  function yFor(value: number, maxValue: number): number {
    const plotHeight = chartHeight - padding.top - padding.bottom
    const ratio = maxValue === 0 ? 0 : value / maxValue
    return padding.top + plotHeight - ratio * plotHeight
  }

  function pathFor(item: ChartSeries, maxValue: number): string {
    return points
      .map((point, index) => {
        const command = index === 0 ? 'M' : 'L'
        return `${command} ${xFor(index)} ${yFor(pointValue(point, item.key), maxValue)}`
      })
      .join(' ')
  }

  function ticks(maxValue: number): number[] {
    return [0, maxValue / 3, (maxValue * 2) / 3, maxValue]
  }

  function formatTick(item: ChartSeries | undefined, value: number): string {
    if (!item) {
      return String(value)
    }

    return item.formatValue ? item.formatValue(value) : Math.round(value).toLocaleString()
  }

  $: leftSeries = series.filter((item) => axisFor(item) === 'left')
  $: rightSeries = series.filter((item) => axisFor(item) === 'right')
  $: leftMax = seriesMax(leftSeries)
  $: rightMax = rightSeries.length > 0 ? seriesMax(rightSeries) : leftMax
  $: leftTicks = ticks(leftMax)
  $: rightTicks = ticks(rightMax)
  $: xAxisLabels = [0, Math.floor((points.length - 1) / 2), points.length - 1]
    .filter((index, position, values) => index >= 0 && values.indexOf(index) === position)
    .map((index) => ({
      index,
      text: points[index]?.label ?? '',
    }))
</script>

{#if points.length === 0}
  <div class="chart-empty">No chart data available.</div>
{:else}
  <div class="chart-shell">
    <div class="chart-legend" aria-hidden="true">
      {#each series as item}
        <span class="legend-item">
          <span class="legend-swatch" style={`background:${item.color}`}></span>
          {item.label}
        </span>
      {/each}
    </div>

    <svg viewBox={`0 0 ${chartWidth} ${chartHeight}`} role="img" aria-label="Line chart">
      {#each leftTicks as tick}
        <line
          x1={padding.left}
          y1={yFor(tick, leftMax)}
          x2={chartWidth - padding.right}
          y2={yFor(tick, leftMax)}
          class="grid-line"
        />
        <text x={padding.left - 12} y={yFor(tick, leftMax) + 4} class="axis-label axis-label-left">
          {formatTick(leftSeries[0], tick)}
        </text>
      {/each}

      {#if rightSeries.length > 0}
        {#each rightTicks as tick}
          <text x={chartWidth - padding.right + 12} y={yFor(tick, rightMax) + 4} class="axis-label axis-label-right">
            {formatTick(rightSeries[0], tick)}
          </text>
        {/each}
      {/if}

      {#each leftSeries as item}
        <path d={pathFor(item, leftMax)} stroke={item.color} class="series-path" />

        {#each points as point, index}
          <circle
            cx={xFor(index)}
            cy={yFor(pointValue(point, item.key), leftMax)}
            r="3.5"
            fill={item.color}
          >
            <title>{`${point.title ?? point.label}: ${item.label} ${formatTick(item, pointValue(point, item.key))}`}</title>
          </circle>
        {/each}
      {/each}

      {#each rightSeries as item}
        <path d={pathFor(item, rightMax)} stroke={item.color} class="series-path" />

        {#each points as point, index}
          <circle
            cx={xFor(index)}
            cy={yFor(pointValue(point, item.key), rightMax)}
            r="3.5"
            fill={item.color}
          >
            <title>{`${point.title ?? point.label}: ${item.label} ${formatTick(item, pointValue(point, item.key))}`}</title>
          </circle>
        {/each}
      {/each}

      {#each xAxisLabels as label}
        <text x={xFor(label.index)} y={chartHeight - 6} text-anchor="middle" class="axis-label axis-label-bottom">
          {label.text}
        </text>
      {/each}
    </svg>
  </div>
{/if}

<style>
  .chart-shell {
    display: grid;
    gap: 14px;
  }

  .chart-legend {
    display: flex;
    flex-wrap: wrap;
    gap: 14px;
    color: #62708e;
    font-size: 0.92rem;
    font-weight: 700;
  }

  .legend-item {
    display: inline-flex;
    align-items: center;
    gap: 8px;
  }

  .legend-swatch {
    width: 12px;
    height: 12px;
    border-radius: 999px;
  }

  svg {
    width: 100%;
    height: auto;
    overflow: visible;
  }

  .grid-line {
    stroke: #e8edf6;
    stroke-width: 1;
    stroke-dasharray: 4 6;
  }

  .series-path {
    fill: none;
    stroke-width: 2.5;
    stroke-linecap: round;
    stroke-linejoin: round;
  }

  .axis-label {
    fill: #7a86a5;
    font-size: 12px;
    font-weight: 600;
  }

  .axis-label-left {
    text-anchor: end;
  }

  .axis-label-right {
    text-anchor: start;
  }

  .chart-empty {
    display: grid;
    place-items: center;
    min-height: 220px;
    border: 1px dashed #dfe5f0;
    border-radius: 14px;
    color: #7a86a5;
    background: #fbfcfe;
  }
</style>