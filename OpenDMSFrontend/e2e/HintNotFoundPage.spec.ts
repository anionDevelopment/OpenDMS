import { test } from '@playwright/test';
import { expectPageToLookLikeBaseline } from './support/VisualRegression';

test.describe('Hint-not-found-page', () => {
    /*
     * Checked in both color-schemes (see the note in "HomePage.spec.ts"). The page is not behind the login, so the
     * scheme is forced through the operating-system-setting the browser reports (emulateMedia).
     */
    test('looks like the baseline-screenshot in the light color-scheme', async ({ page }) => {
        await page.emulateMedia({ colorScheme: 'light' });
        await expectPageToLookLikeBaseline(page, '/this-route-does-not-exist', 'light_hint-not-found-page');
    });

    test('looks like the baseline-screenshot in the dark color-scheme', async ({ page }) => {
        await page.emulateMedia({ colorScheme: 'dark' });
        await expectPageToLookLikeBaseline(page, '/this-route-does-not-exist', 'dark_hint-not-found-page');
    });
});
