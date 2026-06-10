// End-to-end: play every level in the web course simulator and verify each
// one is actually winnable (clean qualifying run under standard course time).
import { test, expect } from '@playwright/test';
import { readFileSync, mkdirSync } from 'node:fs';
import { join, dirname } from 'node:path';
import { fileURLToPath } from 'node:url';

const here = dirname(fileURLToPath(import.meta.url));
const data = JSON.parse(readFileSync(join(here, '..', 'data', 'gamedata.json'), 'utf8'));
const screenshotDir = join(here, 'screenshots');
mkdirSync(screenshotDir, { recursive: true });

const courses = data.courses.map(c => ({
  file: c._file.replace('.asset', ''),
  name: c.courseName,
  sct: c.standardTime,
  obstacles: c.resolvedSequence.length,
}));

test.describe('every level is winnable', () => {
  for (const course of courses) {
    test(`${course.name} — qualifies with default breed`, async ({ page }) => {
      await page.goto(`/websim/index.html?course=${course.file}&breed=BorderCollie&speed=50&autorun=1`);

      const banner = page.getByTestId('result');
      await expect(banner).toHaveAttribute('data-state', 'qualified', { timeout: 45000 });
      await expect(banner).toContainText('QUALIFIED');

      // All obstacles completed
      await expect(page.getByTestId('progress')).toHaveText(`${course.obstacles}/${course.obstacles}`);

      // Finish time is under the standard course time
      const time = parseFloat(await page.getByTestId('timer').textContent());
      expect(time).toBeLessThanOrEqual(course.sct);

      await page.screenshot({ path: join(screenshotDir, `${course.file}.png`) });
    });
  }
});

test('slowest breed (Pug) still completes the hardest course under max time', async ({ page }) => {
  await page.goto('/websim/index.html?course=Westminster&breed=Pug&speed=50&autorun=1');
  const banner = page.getByTestId('result');
  // Must finish (qualified or time faults) — never non-qualifying on time.
  await expect(banner).not.toHaveAttribute('data-state', 'running', { timeout: 45000 });
  await expect(banner).not.toHaveAttribute('data-state', 'nq');
});

test('level select offers all 11 courses and 19 breeds', async ({ page }) => {
  await page.goto('/websim/index.html');
  await expect(page.getByTestId('course-select').locator('option')).toHaveCount(11);
  await expect(page.getByTestId('breed-select').locator('option')).toHaveCount(19);
});
