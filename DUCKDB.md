# DuckDB — query tracking-events.json with SQL

## Setup

```
winget install DuckDB.cli
```

Or download the single `.exe` from https://duckdb.org/docs/installation

## Usage

Open a shell in the repo root and run `duckdb`, then paste:

```sql
CREATE VIEW events AS
SELECT
e.*
FROM (
SELECT unnest(events) AS e
FROM read_json_auto('CreativesIntegration.Api/tracking-events.json')
);
```

Then query normally, e.g. `SELECT eventType, COUNT(*) FROM events GROUP BY 1;`
