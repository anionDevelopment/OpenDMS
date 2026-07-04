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

- [x] Show and download existing documents.
- [x] Allow moving documents to another folder.
- [ ] Allow defining external folder where documents will be pulled from.
- [ ] Allow actions (web requests for example) to be done when a specific event occurs (e. g. when a document will be moved to a specific folder).
- [] Document-summary-generation by AI.

### GOBD conformity

- [ ] Ensure immutability of archived documents (WORM principle: once captured, content can no longer be overwritten).
- [ ] Maintain a version history (create a new version on every change and keep old versions immutably). `Version` exists in the model, but the list of old versions is still a `TODO`.
- [ ] Log all changes completely (who, when, what - including rename, move, update and metadata changes). Currently only partial (only add and hard-delete are logged, many operations are not).
- [ ] Provide a tamper-proof audit log (the log itself must be immutable and protected against subsequent modification).
- [x] Record the immutable capture timestamp of every document (`ImportDate`).
- [x] Store documents unchanged in their original format (`Content`, `OriginalFilename` and `MIMEType`).
- [ ] Ensure completeness of capture (every incoming document is guaranteed to be recorded uniquely; no capture without registration).
- [ ] Provide a unique, sequential and traceable assignment for every document. A sequential `ReadableId` exists, but a guaranteed gap-free and immutable numbering is not yet ensured.
- [ ] Set and manage retention periods. `DeleteIsNotAllowedBefore` and `MustBeHardDeletedAfter` exist in the model, but there is no API to set them.
- [ ] Technically enforce a deletion lock during the retention period (prevent hard-delete before the retention period ends).
- [ ] Perform regulated deletion after the retention period expires (deletion concept/housekeeping). `Housekeeping()` currently throws `NotImplementedException`.
- [x] Support soft-delete (marking instead of physical removal).
- [x] Log traceable deletion with a stated reason (hard-delete with `reason`).
- [ ] Fully enforce an authorization and access-protection concept. Many operations still contain `//TODO check permission`.
- [x] Authenticate users (login and registration with access token).
- [ ] Provide tagging and indexing with metadata (document type, contact/sender, retention date and arbitrary metadata).
- [ ] Provide full-text and metadata search for machine evaluability. A search endpoint exists but is not yet complete.
- [ ] Provide OCR for machine readability and evaluability of scanned documents. A client exists but is not fully implemented yet.
- [ ] Link related documents (for example the relationship between a voucher and its posting).
- [ ] Provide a GoBD-compliant data export for the tax authority (Z1/Z2/Z3 access or export in an evaluable format including index and description standard).
- [ ] Ensure readability and reproduction throughout the entire retention period (reproduce in the original format).
- [ ] Ensure migration and format safety (keep the data evaluable across format and system changes).
- [ ] Protect against data loss (backup and recovery concept ensuring documents cannot be lost).
- [ ] Ensure integrity protection of stored content (for example a checksum/hash per document to prove integrity).
- [ ] Provide procedure documentation (Verfahrensdokumentation describing capture, processing, retention, access and deletion of the system).
- [ ] Ensure timely capture (proof of the prompt recording of supporting documents).

### Non functional requirements

- [ ] Allow login with OpenID.

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
