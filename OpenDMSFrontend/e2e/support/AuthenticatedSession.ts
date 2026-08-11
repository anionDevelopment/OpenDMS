import { Page } from '@playwright/test';
import { respondWith } from './SimulatedBackend';

/*
 * The key under which the application stores the access-token. It is duplicated here on purpose:
 * the testcases use the application from the outside and therefore must not import its sourcecode.
 * (See StorageService.keyAccessToken.)
 */
const storageKeyOfTheAccessToken: string = 'accessToken';

const accessTokenOfTheSimulatedUser: string = 'access-token-of-the-visual-regression-tests';

/*
 * The user-information which the simulated backend returns. The values are fixed because they are
 * displayed in the user-interface and therefore have to be identical in every testrun.
 */
const informationAboutTheSimulatedUser = {
    id: '00000000-0000-0000-0000-000000000001',
    name: 'Testuser',
    isAdmin: true
};

/*
 * Makes the application behave as if a user is logged in.
 *
 * The visual-regression-tests run without a backend, so the few requests which are required to
 * pass the authentication-check are answered by Playwright itself. Doing it this way instead of
 * logging in against a real backend also keeps the screenshots reproducible, because they do not
 * depend on the content of a database.
 *
 * This function has to be called before the page is opened.
 */
export async function simulateLoggedInUser(page: Page): Promise<void> {
    /* The init-script runs in the browser, so everything it needs has to be passed as an argument. */
    await page.addInitScript((accessToken: { key: string, value: string }) => {
        sessionStorage.setItem(accessToken.key, accessToken.value);
    }, { key: storageKeyOfTheAccessToken, value: accessTokenOfTheSimulatedUser });
    await respondWith(page, '**/API/v3/UserController/TokenIsValid*', true);
    await respondWith(page, '**/API/v3/UserController/GetUserInformation*', informationAboutTheSimulatedUser);
}
