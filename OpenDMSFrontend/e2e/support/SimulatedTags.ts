import { Page } from '@playwright/test';
import { respondWith } from './SimulatedBackend';

/*
 * The tags which the simulated user can use. One of them is a global tag and the other one belongs to the user, so
 * the tag-management shows both kinds of tags. The values are fixed because they are displayed in the
 * user-interface and therefore have to be identical in every testrun.
 */
const globalTag = { id: '00000000-0000-0000-0000-000000000201', name: 'Contract', colorCode: '283593', ownerUserId: null };
const tagOfTheUser = { id: '00000000-0000-0000-0000-000000000203', name: 'Deadline', colorCode: '0277BD', ownerUserId: '00000000-0000-0000-0000-000000000001' };

/*
 * Makes the simulated backend answer the request for the tags which the user can use.
 *
 * This function has to be called before the page is opened.
 */
export async function simulateTags(page: Page): Promise<void> {
    await respondWith(page, '**/API/v3/OpenDMSBackend/GetTags*', [globalTag, tagOfTheUser]);
}
