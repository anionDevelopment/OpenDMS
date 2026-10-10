import { test } from '@playwright/test';
import { expectPageToLookLikeBaseline } from './support/VisualRegression';

test.describe('Home-page', () => {
    /*
     * The page is checked in both color-schemes, because the dark one is not a variation of a few colors of the
     * light one: every color-token of the theme has its own value there. A regression which only shows up in the
     * dark scheme - an element which keeps a bright background and becomes unreadable, for example - would stay
     * unnoticed if only one of the two was checked. The page is not behind the login, so the scheme is forced
     * through the operating-system-setting the browser reports (emulateMedia) instead of through a logged-in
     * user's chosen scheme.
     */
    test('looks like the baseline-screenshot in the light color-scheme', async ({ page }) => {
        await page.emulateMedia({ colorScheme: 'light' });
        await expectPageToLookLikeBaseline(page, '/', 'light_home-page');
    });

    test('looks like the baseline-screenshot in the dark color-scheme', async ({ page }) => {
        await page.emulateMedia({ colorScheme: 'dark' });
        await expectPageToLookLikeBaseline(page, '/', 'dark_home-page');
    });
});
