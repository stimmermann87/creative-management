# Creatives Integration Interview Exercise

This repo contains a deliberately small full-stack exercise:

- `CreativesIntegration.Api`: ASP.NET Core Web API on .NET 8 with in-memory storage only
- `FakeAPI.MicrosoftCurate`: placeholder class library for a fake Microsoft Curate integration
- `FakeAPI.IndexExchange`: placeholder class library for a fake Index Exchange integration
- `creatives-integration-frontend`: Svelte SPA using plain `fetch`
- `playwright-tests`: separate Playwright project for browser automation and visual iteration

The base app is intentionally simple. It supports login, viewing creatives, editing creatives, creating creatives, and a fake launch flow that updates status and simulates part of an external provider workflow.

## Open And Run In Visual Studio

1. Open `CreativesIntegration.sln` in Visual Studio.
2. Start the `CreativesIntegration.Api` project.
3. Open a terminal in the solution root.
4. Run `cd creatives-integration-frontend`.
5. Run `npm install`.
6. Run `npm run dev`.
7. Open `http://localhost:5173`.

The frontend proxies `/api` calls to `http://localhost:5068`, so the API should be running on that port.

## Local Run Commands

Backend:

```bash
dotnet run --project CreativesIntegration.Api
```

Fake integration libraries:

```bash
dotnet build FakeAPI.MicrosoftCurate
dotnet build FakeAPI.IndexExchange
```

Frontend:

```bash
cd creatives-integration-frontend
npm install
npm run dev
```

Playwright:

```bash
cd playwright-tests
npm install
npx playwright install chromium
npm test
```

The Playwright project can also start the backend and frontend automatically through its config.

Demo credentials:

- Username: `demo`
- Password: `password`

## Manual Test Steps

1. Start the backend.
2. Start the frontend.
3. Open `http://localhost:5173`.
4. Log in with the demo credentials.
5. Confirm the seeded creatives are visible.
6. Open one creative and edit its name or HTML content.
7. Save and confirm the updated values are shown.
8. Create a new creative and confirm it appears in the list.
9. Click `Launch` on a creative and confirm the status becomes `LaunchRequested`.
10. Confirm the UI shows a launch result message.

## Playwright Notes

- The standalone Playwright project lives in `playwright-tests`.
- `npm test` runs the smoke test headlessly.
- `npm run test:headed` runs the same test with a visible browser.
- `npm run test:ui` opens the Playwright UI runner.
- `npm run codegen` opens Playwright codegen against the local app for quick iteration.

## Fake Integration Libraries

Two fake provider projects are included in the solution:

- `FakeAPI.MicrosoftCurate`: class library with fake Microsoft Curate service models and in-memory launch-related behavior.
- `FakeAPI.IndexExchange`: class library with fake Index Exchange service models and in-memory launch-related behavior.
