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
- ❌ [Allow creating custom document-attributes which every document optionally has. Supported attribute-types: bool (for example "tax-relevant" yes/no), double, timestamp (for example a deadline) and string (free-text).](https://github.com/anionDevelopment/OpenDMS/issues/2)
- ❌ [Allow assigning a document to a person in the sense of personal data of the GDPR.](https://github.com/anionDevelopment/OpenDMS/issues/3)

### GOBD conformity

- ❌ [Ensure immutability of archived documents (WORM principle: once captured, content can no longer be overwritten).](https://github.com/anionDevelopment/OpenDMS/issues/4)
- ✅ Maintain a version history (upload a new version of a document; the new version is stored as a regular, immutable document and linked to the old one, which is hidden from the normal listings but remains retrievable via the version-history).
- ❌ [Log all changes completely (who, when, what - including rename, move, update and metadata changes). Currently only partial (only add and hard-delete are logged, many operations are not).](https://github.com/anionDevelopment/OpenDMS/issues/5)
- ❌ [Provide a tamper-proof audit log (the log itself must be immutable and protected against subsequent modification).](https://github.com/anionDevelopment/OpenDMS/issues/6)
- ✅ Record the immutable capture timestamp of every document (`ImportDate`).
- ✅ Store documents unchanged in their original format (`Content`, `OriginalFilename` and `MIMEType`).
- ❌ [Ensure completeness of capture (every incoming document is guaranteed to be recorded uniquely; no capture without registration).](https://github.com/anionDevelopment/OpenDMS/issues/7)
- ❌ [Provide a unique, sequential and traceable assignment for every document. A sequential `ReadableId` exists, but a guaranteed gap-free and immutable numbering is not yet ensured.](https://github.com/anionDevelopment/OpenDMS/issues/8)
- ❌ [Set and manage retention periods. `DeleteIsNotAllowedBefore` and `MustBeHardDeletedAfter` exist in the model, but there is no API to set them.](https://github.com/anionDevelopment/OpenDMS/issues/9)
- ❌ [Technically enforce a deletion lock during the retention period (prevent hard-delete before the retention period ends).](https://github.com/anionDevelopment/OpenDMS/issues/10)
- ✅ Perform regulated deletion after the retention period expires: a scheduled housekeeping-run (`DoScheduledHardDeletions` in the management-background-service) hard-deletes every document whose retention-deadline (`MustBeHardDeletedAfter`) has been reached, in a traceable way (audit-logged with a reason).
- ✅ Support soft-delete (marking instead of physical removal).
- ✅ Log traceable deletion with a stated reason (hard-delete with `reason`).
- ❌ [Fully enforce an authorization and access-protection concept. Many operations still contain `//TODO check permission`.](https://github.com/anionDevelopment/OpenDMS/issues/12)
- ❌ [Provide role- and permission-management. Admins only have administrative access; they are not automatically allowed to change everything. Per-folder moderators decide who may retrieve and change the contents of a folder. By default nobody (not even admins) may retrieve or change folder-contents via the API except those a folder-moderator has explicitly allowed.](https://github.com/anionDevelopment/OpenDMS/issues/13)
- ✅ Authenticate users (login and registration with access token).
- ❌ [Provide tagging and indexing with metadata (document type, contact/sender, retention date and arbitrary metadata).](https://github.com/anionDevelopment/OpenDMS/issues/14)
- ❌ [Provide full-text and metadata search for machine evaluability. A search endpoint exists but is not yet complete.](https://github.com/anionDevelopment/OpenDMS/issues/15)
- ❌ [Provide OCR for machine readability and evaluability of scanned documents. A client exists but is not fully implemented yet.](https://github.com/anionDevelopment/OpenDMS/issues/16)
- ❌ [Link related documents (for example the relationship between a voucher and its posting).](https://github.com/anionDevelopment/OpenDMS/issues/17)
- ❌ [Provide a GoBD-compliant data export for the tax authority (Z1/Z2/Z3 access or export in an evaluable format including index and description standard).](https://github.com/anionDevelopment/OpenDMS/issues/18)
- ❌ [Ensure readability and reproduction throughout the entire retention period (reproduce in the original format).](https://github.com/anionDevelopment/OpenDMS/issues/19)
- ❌ [Ensure migration and format safety (keep the data evaluable across format and system changes).](https://github.com/anionDevelopment/OpenDMS/issues/20)
- ❌ [Protect against data loss (backup and recovery concept ensuring documents cannot be lost).](https://github.com/anionDevelopment/OpenDMS/issues/21)
- ❌ [Ensure integrity protection of stored content (for example a checksum/hash per document to prove integrity).](https://github.com/anionDevelopment/OpenDMS/issues/22)
- ❌ [Provide procedure documentation (Verfahrensdokumentation describing capture, processing, retention, access and deletion of the system).](https://github.com/anionDevelopment/OpenDMS/issues/23)
- ❌ [Ensure timely capture (proof of the prompt recording of supporting documents).](https://github.com/anionDevelopment/OpenDMS/issues/24)

### Non functional requirements

- ❌ [Allow login with OpenID.](https://github.com/anionDevelopment/OpenDMS/issues/25)
- ❌ [Export and import of all data (technical requirement for migrating everything from one server to another; really all data is exported and imported).](https://github.com/anionDevelopment/OpenDMS/issues/26)

## Getting Started

### Usage

TODO

## Reference

The OpenDMS-reference can be found [here](./Other/Resources/Reference/Reference.md).

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
