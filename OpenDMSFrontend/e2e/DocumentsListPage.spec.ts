import { test } from '@playwright/test';
import { expectPageToLookLikeBaseline } from './support/VisualRegression';
import { simulateLoggedInUser } from './support/AuthenticatedSession';
import { simulateDocuments } from './support/SimulatedDocuments';

test.describe('Documents-list-page', () => {
    test('looks like the baseline-screenshot', async ({ page }) => {
        await simulateLoggedInUser(page);
        await simulateDocuments(page);
        await expectPageToLookLikeBaseline(page, '/user/documents', 'documents-list-page');
    });
});
