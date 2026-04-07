import { expect, test } from '@playwright/test';

test('creative list scrolls after the list gets long', async ({ page }) => {
  await page.setViewportSize({ width: 1496, height: 616 });

  const prefix = `Layout Debug ${Date.now()}`;

  await page.goto('/');
  await page.getByRole('button', { name: 'Log In', exact: true }).click();

  const token = await page.evaluate(() =>
    localStorage.getItem('creative-demo-token'),
  );

  for (let index = 0; index < 14; index += 1) {
    await page.evaluate(
      async ({ authToken, index, prefix }) => {
        await fetch('/api/creatives', {
          method: 'POST',
          headers: {
            'Content-Type': 'application/json',
            'X-Demo-Token': authToken,
          },
          body: JSON.stringify({
            name: `${prefix} ${index}`,
            htmlContent: '<div><h1>Debug</h1><p>Long list seed.</p></div>',
            status: index % 2 === 0 ? 'Draft' : 'Ready',
          }),
        });
      },
      { authToken: token, index, prefix },
    );
  }

  await page.reload();
  await expect(page.locator('.creative-list .list-item').first()).toBeVisible();

  const metrics = await page.evaluate(() => {
    const creativeList = document.querySelector('.creative-list');

    return {
      viewportHeight: window.innerHeight,
      documentHeight: document.documentElement.scrollHeight,
      creativeListClientHeight: creativeList?.clientHeight ?? null,
      creativeListScrollHeight: creativeList?.scrollHeight ?? null,
    };
  });

  expect(metrics.creativeListScrollHeight).toBeGreaterThan(
    metrics.creativeListClientHeight,
  );
  expect(metrics.creativeListClientHeight).toBeLessThan(metrics.viewportHeight);

  await page.screenshot({
    path: 'test-results/layout-debug-1496x616.png',
    fullPage: false,
  });
});
