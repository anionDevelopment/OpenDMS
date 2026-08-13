import { Page } from '@playwright/test';
import { respondWith } from './SimulatedBackend';

/*
 * A one-pixel png in a fixed color. It is used as preview-image of the simulated documents.
 * The image is scaled up by the user-interface, but because it consists of exactly one pixel the
 * result is a plain area of that color and therefore contains no scaling-artefacts which could
 * differ between the browsers.
 */
const previewImageAsBase64: string = 'iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAIAAACQd1PeAAAADElEQVR42mOYt+IIAAPyAgtr2afYAAAAAElFTkSuQmCC';

/*
 * The readable id of the document whose page is checked by the visual-regression-tests.
 */
export const readableIdOfTheDocumentWithAnOwnPage: number = 1002;

/*
 * The documents which the simulated backend returns. All displayed values are fixed because they
 * appear in the screenshots and therefore have to be identical in every testrun. The two documents
 * differ on purpose: only one of them has an ai-summary and only one of them has more than one
 * version, so both variants of these parts of the user-interface are covered.
 */
const previewOfTheFirstDocument = {
    id: '00000000-0000-0000-0000-000000000101',
    title: 'Invoice of the example-company',
    filename: 'Invoice.pdf',
    originalFilename: 'Invoice-original.pdf',
    importDate: '2024-01-15T09:30:00+00:00',
    readableId: 1001,
    mimeType: 'application/pdf',
    previewAsBase64: previewImageAsBase64,
    isSoftDeleted: false,
    groupOfBusinessOwner: 'Accounting',
    addedByUserId: '00000000-0000-0000-0000-000000000001',
    aiSummaryShort: null,
    versionNumber: 1,
    versionTimestamp: '2024-01-15T09:30:00+00:00'
};

const previewOfTheSecondDocument = {
    id: '00000000-0000-0000-0000-000000000102',
    title: 'Contract with the example-supplier',
    filename: 'Contract.pdf',
    originalFilename: 'Contract-original.pdf',
    importDate: '2024-02-03T11:45:00+00:00',
    readableId: readableIdOfTheDocumentWithAnOwnPage,
    mimeType: 'application/pdf',
    previewAsBase64: previewImageAsBase64,
    isSoftDeleted: false,
    groupOfBusinessOwner: 'Purchasing',
    addedByUserId: '00000000-0000-0000-0000-000000000001',
    aiSummaryShort: 'A contract about the delivery of example-goods.',
    versionNumber: 3,
    versionTimestamp: '2024-03-08T16:20:00+00:00'
};

/*
 * The full document which belongs to the second preview. The document-page shows more values than
 * the document-list, so this object contains the additional ones.
 */
const theDocumentWithAnOwnPage = {
    ...previewOfTheSecondDocument,
    tags: [],
    documentContentAsBase64: null,
    documentPreviewAsBase64: previewImageAsBase64,
    assignedLanguages: [],
    aiSummaryLong: 'The example-supplier delivers example-goods to the example-company. '
        + 'The contract is valid for one year and is extended automatically if none of the parties terminates it.',
    metadataValues: {}
};

/*
 * Makes the simulated backend answer the requests which the document-list and the document-page
 * send while they are rendered.
 *
 * This function has to be called before the page is opened.
 */
export async function simulateDocuments(page: Page): Promise<void> {
    await respondWith(page, '**/API/v3/OpenDMSBackend/GetLatestDocuments*', [previewOfTheFirstDocument, previewOfTheSecondDocument]);
    /*
     * The document-list contains a tab with the storage-locations. That tab is not the selected one,
     * but its content is rendered nevertheless and therefore its request has to be answered too.
     */
    await respondWith(page, '**/API/v3/OpenDMSBackend/GetAllViewableStorageLocations*', []);
    await respondWith(page, '**/API/v3/OpenDMSBackend/GetDocumentFromReadableId*', theDocumentWithAnOwnPage);
    await respondWith(page, '**/API/v3/OpenDMSBackend/GetDocumentPreview*', previewOfTheSecondDocument);
}
