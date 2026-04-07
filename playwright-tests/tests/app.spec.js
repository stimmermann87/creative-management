import { expect, test } from '@playwright/test';

test('user can log in, create a creative, and request launch', async ({
  page,
}) => {
  const creativeName = `Playwright Creative ${Date.now()}`;

  await page.goto('/');

  await expect(
    page.getByRole('heading', { name: 'Creative Management' }),
  ).toBeVisible();
  await page.getByRole('button', { name: 'Log In', exact: true }).click();

  await expect(
    page.getByRole('heading', { name: 'Creatives', exact: true }),
  ).toBeVisible();
  await expect(page.getByText('Spring Sale Banner')).toBeVisible();

  await page
    .locator('.workspace-card')
    .getByRole('button', { name: 'New Creative', exact: true })
    .click();
  await expect(
    page.getByRole('heading', { name: 'Create Creative' }),
  ).toBeVisible();

  await page.getByLabel('Name').fill(creativeName);
  await page
    .getByLabel('HTML Content')
    .fill(
      '<div><h1>Playwright</h1><p>Created from browser automation.</p><script>document.body.setAttribute("data-preview-script", "ran")</script></div>',
    );

  await page.getByRole('button', { name: 'Create', exact: true }).click();

  const editorPanel = page.locator('.editor-panel');
  await expect(editorPanel.getByLabel('Name')).toHaveValue(creativeName);
  await expect(page.locator('.creative-list')).toContainText(creativeName);

  await editorPanel
    .getByRole('button', { name: 'Launch', exact: true })
    .click();

  await expect(page.locator('.banner.success')).toContainText(
    'Placeholder only. No third-party launch integration is implemented in this base exercise.',
  );
  await expect(editorPanel.getByLabel('Status')).toHaveValue('LaunchRequested');
});
