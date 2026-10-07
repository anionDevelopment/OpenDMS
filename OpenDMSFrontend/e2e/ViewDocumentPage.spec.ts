import { test } from '@playwright/test';
import { expectPageToLookLikeBaseline } from './support/VisualRegression';
import { simulateLoggedInUser } from './support/AuthenticatedSession';
import { readableIdOfTheDocumentWithAnOwnPage, simulateDocuments } from './support/SimulatedDocuments';

test.describe('View-document-page', () => {
    test('looks like the baseline-screenshot', async ({ page }) => {
        await simulateLoggedInUser(page);
        await simulateDocuments(page);
        await expectPageToLookLikeBaseline(page, `/user/document/${readableIdOfTheDocumentWithAnOwnPage}`, 'view-document-page');
    });
});
