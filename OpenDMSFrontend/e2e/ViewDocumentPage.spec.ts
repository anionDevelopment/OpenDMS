import { test } from '@playwright/test';
import { expectPageToLookLikeBaseline } from './support/VisualRegression';
import { simulateLoggedInUser } from './support/AuthenticatedSession';
import { readableIdOfTheDocumentWithAnOwnPage, simulateDocuments } from './support/SimulatedDocuments';

test.describe('View-document-page', () => {
    /*
     * Checked in both color-schemes (see the note in "UserSettingsPage.spec.ts"). The page is behind the login, so
     * the scheme is the one the logged-in user chose (answered to the theme-request after the login).
     */
    test('looks like the baseline-screenshot in the light color-scheme', async ({ page }) => {
        await simulateLoggedInUser(page, 'light');
        await simulateDocuments(page);
        await expectPageToLookLikeBaseline(page, `/user/document/${readableIdOfTheDocumentWithAnOwnPage}`, 'light-view-document-page');
    });

    test('looks like the baseline-screenshot in the dark color-scheme', async ({ page }) => {
        await simulateLoggedInUser(page, 'dark');
        await simulateDocuments(page);
        await expectPageToLookLikeBaseline(page, `/user/document/${readableIdOfTheDocumentWithAnOwnPage}`, 'dark-view-document-page');
    });
});
