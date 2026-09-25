# OpenDMS

[![CodeFactor](https://www.codefactor.io/repository/github/aniondev/OpenDMS/badge/main)](https://www.codefactor.io/repository/github/aniondev/OpenDMS/overview/main)
![Coverage](./OpenDMS/Other/Resources/TestCoverageBadges/badge_shieldsio_linecoverage_blue.svg)
![Lines of code](https://img.shields.io/tokei/lines/github/anionDev/OpenDMS)

## Purpose

`OpenDMS` is an open source document-management-system.

## Quick-start

See [the reference of the OpenDMS-codeunit](./OpenDMS/Other/Reference/ReferenceContent/index.md).

## Technical product reference

See the general [product reference](./Other/Reference/Reference.md).

## Features

### Usability

- ✅ Show and download existing documents.
- ✅ Allow moving documents to another folder.
- ✅ Allow defining external folder where documents will be pulled from. (Reads file from file-system)
- ❌ [Allow actions (web requests for example) to be done when a specific event occurs (e. g. when a document will be moved to a specific folder).](https://github.com/anionDevelopment/OpenDMS/issues/1)
- ✅ Document-summary-generation by AI. (the admin must provide an openai-api-compatible-api-endpoint and its credentials)
- ✅ Allow creating custom document-attributes which every document optionally has. Supported attribute-types: bool (for example "tax-relevant" yes/no), double, timestamp (for example a deadline) and string (free-text). A moderator of a storage-location defines custom metadata-fields for that storage-location and every contained document can optionally hold a value for each of them. The storage-location-view of the web-UI shows these fields and lets a moderator add, rename and remove them, and the page of a document lets the value of every field be set and cleared. The type of a field can not be changed after it was defined, because the values which the documents already hold for it were validated against it.
- ❌ [Allow assigning a document to a person in the sense of personal data of the GDPR.](https://github.com/anionDevelopment/OpenDMS/issues/3)

### GOBD conformity

- ❌ [Ensure immutability of archived documents (WORM principle: once captured, content can no longer be overwritten).](https://github.com/anionDevelopment/OpenDMS/issues/4)
- ✅ Maintain a version history (upload a new version of a document; the new version is stored as a regular, immutable document and linked to the old one, which is hidden from the normal listings but remains retrievable via the version-history).
- ✅ Log all changes completely (who, when, what): every change-operation (add, update, title- and metadata-changes, tag changes, version-upload, rename, move, storage-location sharing, soft- and hard-delete) writes an audit-log-entry recording the acting user (or that it was an automatic system-operation) and the affected object.
- ❌ [Provide a tamper-proof audit log (the log itself must be immutable and protected against subsequent modification).](https://github.com/anionDevelopment/OpenDMS/issues/6)
- ✅ Record the immutable capture timestamp of every document (`ImportDate`).
- ✅ Store documents unchanged in their original format (`Content`, `OriginalFilename` and `MIMEType`).
- ❌ [Ensure completeness of capture (every incoming document is guaranteed to be recorded uniquely; no capture without registration).](https://github.com/anionDevelopment/OpenDMS/issues/7)
- ❌ [Provide a unique, sequential and traceable assignment for every document. A sequential `ReadableId` exists, but a guaranteed gap-free and immutable numbering is not yet ensured.](https://github.com/anionDevelopment/OpenDMS/issues/8)
- ✅ Set and manage retention periods: `DeleteIsNotAllowedBefore` and `MustBeHardDeletedAfter` can be set for every document via the API and on the page of the document in the web-UI, and every change is audit-logged.
- ✅ Technically enforce a deletion lock during the retention period: a hard-deletion is refused as long as the `DeleteIsNotAllowedBefore` of the affected document (or of any document contained in the affected folder or storage-location) is in the future. The rule applies to every caller, including the automatic housekeeping.
- ✅ Perform regulated deletion after the retention period expires: a scheduled housekeeping-run (`DoScheduledHardDeletions` in the management-background-service) hard-deletes every document whose retention-deadline (`MustBeHardDeletedAfter`) has been reached, in a traceable way (audit-logged with a reason).
- ✅ Support soft-delete (marking instead of physical removal).
- ✅ Log traceable deletion with a stated reason (hard-delete with `reason`).
- ✅ Fully enforce an authorization and access-protection concept: every business-operation verifies the caller's permission. Read-operations require view-permission (administrator, owner of the containing storage-location, or a user it was shared with) and change-operations require edit-permission (administrator or owner of the containing storage-location). Automatic system-operations (imports, scheduled deletion) are exempt.
- ✅ Provide role- and permission-management. Administrators only have administrative access; they are not automatically allowed to change (or retrieve) content. Every content-object (storage-location, folder and document) can have arbitrarily many moderators ("owners") and arbitrarily many users with an explicit view- or edit-grant; permissions are inherited down the containment-hierarchy (a moderator or grant on a folder applies to its contents). By default nobody (not even an administrator) may retrieve or change the contents except a moderator or a user that a moderator has explicitly granted view- or edit-permission. Every folder and storage-location must always keep at least one moderator. Administrators additionally manage global roles (a user can have multiple roles) via an admin user-overview in the UI.
- ✅ Authenticate users (login and registration with access token).
- ✅ Provide tagging and indexing with metadata: a document can be indexed with tags (named and colored labels which can be assigned to arbitrarily many documents; a tag is either global or belongs to a single user) and with the custom metadata-fields of its storage-location (for example document-type or contact/sender). Both can be changed in the web-UI on the page of a document, tags are managed on the settings-page, the search finds a document by its tags and by its metadata-values, and every change is audit-logged. The retention-date is not a metadata-field but is held by `DeleteIsNotAllowedBefore` and `MustBeHardDeletedAfter`. The details of this feature are described in the section [Feature 14: tagging and indexing with metadata](#feature-14-tagging-and-indexing-with-metadata).
- ❌ [Provide full-text and metadata search for machine evaluability. The search finds a document by its title, its filenames, its OCR-content, its tags and its metadata-values, but a search which is restricted to a single field and a machine-evaluable export of the result are still missing.](https://github.com/anionDevelopment/OpenDMS/issues/15)
- ✅ Provide OCR for machine readability and evaluability of scanned documents: the OCR-service-client fully calls the SimpleOCR-service (extract OCR-text, convert a document to a picture, query the supported languages and wait until the service is available). When no OCR-service is configured a mock is used instead.
- ❌ [Link related documents (for example the relationship between a voucher and its posting).](https://github.com/anionDevelopment/OpenDMS/issues/17)
- ❌ [Provide a GoBD-compliant data export for the tax authority (Z1/Z2/Z3 access or export in an evaluable format including index and description standard).](https://github.com/anionDevelopment/OpenDMS/issues/18)
- ❌ [Ensure readability and reproduction throughout the entire retention period (reproduce in the original format).](https://github.com/anionDevelopment/OpenDMS/issues/19)
- ❌ [Ensure migration and format safety (keep the data evaluable across format and system changes).](https://github.com/anionDevelopment/OpenDMS/issues/20)
- ❌ [Protect against data loss (backup and recovery concept ensuring documents cannot be lost).](https://github.com/anionDevelopment/OpenDMS/issues/21)
- ❌ [Ensure integrity protection of stored content (for example a checksum/hash per document to prove integrity).](https://github.com/anionDevelopment/OpenDMS/issues/22)
- ❌ [Provide procedure documentation (Verfahrensdokumentation describing capture, processing, retention, access and deletion of the system).](https://github.com/anionDevelopment/OpenDMS/issues/23)
- ❌ [Ensure timely capture (proof of the prompt recording of supporting documents).](https://github.com/anionDevelopment/OpenDMS/issues/24)

### Non functional requirements

- ✅ Allow login with OpenID. (via [OpenID Connect](https://github.com/anionDevelopment/OpenDMS/issues/25): an admin configures one or more OIDC providers, a user logs in through the provider's authorization flow, and an OpenDMS account is created automatically on the user's first login through that provider.)
- ❌ [Export and import of all data (technical requirement for migrating everything from one server to another; really all data is exported and imported).](https://github.com/anionDevelopment/OpenDMS/issues/26)

### Feature 14: tagging and indexing with metadata

The feature [tagging and indexing with metadata](https://github.com/anionDevelopment/OpenDMS/issues/14) is implemented: a document can be indexed with tags and with the custom metadata-fields of its storage-location. A moderator of a storage-location defines its fields (name and type: `string`, `bool`, `double` or `timestamp`), and every document contained in that storage-location can optionally hold one value per field. The document-type and the contact/sender, which the issue names as examples, are modelled as such custom fields and not as fixed standard-fields, so that every installation can name and structure them as it needs them. The retention-date is deliberately not a metadata-field: it is held by the document-properties `DeleteIsNotAllowedBefore` and `MustBeHardDeletedAfter`.

The topic covers the following points, which are all implemented:

- The retention-dates (`DeleteIsNotAllowedBefore` and `MustBeHardDeletedAfter`) of a document can be set via the API and on the page of the document, and the deletion-lock is enforced technically: a hard-deletion is refused as long as the retention-period of an affected document has not ended (see the issues [9](https://github.com/anionDevelopment/OpenDMS/issues/9) and [10](https://github.com/anionDevelopment/OpenDMS/issues/10)).
- A metadata-field is of the type `string`, `bool`, `double` or `timestamp` (see [issue 2](https://github.com/anionDevelopment/OpenDMS/issues/2)). A value is validated against the type of its field and is stored in a normalized representation, so that every reader gets the same format back regardless of the culture of the writer.
- The search finds a document by its tags and by its metadata-values (the remaining points of the search itself are listed in [issue 15](https://github.com/anionDevelopment/OpenDMS/issues/15)).
- A tag can be renamed, be given another color and be deleted; deleting it removes it from every document it is assigned to. A tag is either a global tag, which every user can use and which only an administrator can create, change and delete, or it belongs to a single user, who is the only one who can see, use, change and delete it. Tags are managed on the settings-page of the web-UI.
- A metadata-field can be renamed. Its type can deliberately not be changed, because the values which the documents already hold for it were validated against it; a field of another type is defined as a new field.
- Moving a document into another storage-location transfers the values it holds to the fields of the new storage-location which have the same name and the same type. A value which has no such field is removed, so that no value stays stored which is neither shown nor changeable. Both cases are audit-logged.

## Getting Started

### Usage

TODO

## Build

This product requires to use `scbuildcodeunits` implemented/provided by [ScriptCollection](https://github.com/anionDev/ScriptCollection) to build the project.

## Changelog

See the [Changelog-folder](./Other/Resources/Changelog).

## Contribute

Contributions are always welcome.

See [Contributing.md](./Contributing.md) for information about that.

## Repository-structure

This product uses the [CommonProjectStructure](https://projects.aniondev.de/PublicProjects/Common/ProjectTemplates/-/blob/main/Conventions/RepositoryStructure/CommonProjectStructure/CommonProjectStructure.md) as repository-structure.

## Branching-system

This product follows the [GitFlowSimplified](https://projects.aniondev.de/PublicProjects/Common/ProjectTemplates/-/blob/main/Conventions/BranchingSystem/GitFlowSimplified/GitFlowSimplified.md)-branching-system.

## Versioning

This product follows the [SemVerPractise](https://projects.aniondev.de/PublicProjects/Common/ProjectTemplates/-/blob/main/Conventions/Versioning/SemVerPractise/SemVerPractise.md)-versioning-system.

## License

See [License.txt](./License.txt).
