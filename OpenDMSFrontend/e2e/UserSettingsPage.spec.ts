import { test } from '@playwright/test';
import { expectPageToLookLikeBaseline } from './support/VisualRegression';
import { simulateLoggedInUser } from './support/AuthenticatedSession';

test.describe('User-settings-page', () => {
    test('looks like the baseline-screenshot', async ({ page }) => {
        await simulateLoggedInUser(page);
        await expectPageToLookLikeBaseline(page, '/user/settings', 'user-settings-page');
    });
});
