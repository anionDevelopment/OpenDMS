# OpenDMS

[![CodeFactor](https://www.codefactor.io/repository/github/aniondev/OpenDMS/badge/main)](https://www.codefactor.io/repository/github/aniondev/OpenDMS/overview/main)
![Line-coverage of the backend](./OpenDMSBackend/Other/Resources/TestCoverageBadges/badge_shieldsio_linecoverage_blue.svg)
![Line-coverage of the frontend](./OpenDMSFrontend/Other/Resources/TestCoverageBadges/badge_shieldsio_linecoverage_blue.svg)
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

OpenDMS is delivered as one OCI-image which contains the web-frontend and the backend. To run it you need a container-runtime, a
PostgreSQL- or MariaDB-database and a reverse-proxy which terminates TLS in front of it.

- [Usage of the container](./OpenDMS/Other/Reference/ReferenceContent/Articles/Usage.md): which ports it offers and which of them
  the reverse-proxy is supposed to forward to.
- [Example-deployment](./OpenDMS/Other/Reference/ReferenceContent/Examples/MinimalDockerComposeFile/ReadMe.md): a minimal
  docker-compose-file with a database, which can be started with `task BaseExampleStart`.
- [Commandline-parameter](./OpenDMSBackend/Other/Reference/ReferenceContent/Articles/CommandlineParameter.md): the values the
  configuration-file is seeded with on the first start, which the container takes as environment-variables of the same name.
- [Supported databases](./OpenDMSBackend/Other/Reference/ReferenceContent/Articles/SupportedDatabases.md).

The first user is the administrator `admin`, whose initial password is `admin` unless `InitialAdminPassword` was passed. Change it
after the first login.

## OWASP-Top-10-analysis

This section records the result of a security-analysis of this repository against the
[OWASP Top 10:2025](https://owasp.org/Top10/2025/), which is the current official release of that list. Every one of the ten
categories is assessed explicitly; a category without a finding states that. The analysis is a review of the source-code, the
configuration and the container-definition of this repository. It is not a penetration-test, so a finding which could not be
decided from the code is marked as such instead of being claimed.

### Scope and attack-surface

| Component | Exposure | Entry-points |
| --- | --- | --- |
| `OpenDMSFrontend` (angular-web-application, served by nginx) | internet-exposed | every page of the web-UI; it holds the access-token of the user |
| `OpenDMSBackend` HTTP-API (`/API/v3/...`), proxied by nginx | internet-exposed | document-upload and -download, search, tag- and metadata-operations, permission-management, login (`UserController/Login`), token-check (`UserController/TokenIsValid`), the whole OIDC-login-flow (`OIDCController/*`) |
| Maintenance-routes (`/API/Other/Maintenance/HealthCheck`, `.../Metrics`) and the api-specification | internet-exposed, unauthenticated | health-check and api-specification are reachable without authentication; metrics are disabled by default |
| nginx (`OpenDMS/OpenDMS/nginx.conf`) | internet-exposed | terminates the built-in port 443 and port 8080, proxies `/API/` to the backend on loopback |
| Import-directory of an import-definition (`ManagementService.ImportNewDocuments`) | internal-only | every file in the configured source-folder is imported, and the optional adapt-script of that import-definition is executed server-side in a V8-engine |
| OCR-service and ai-summary-service (`OCRServiceClient`, `AISummaryServiceClient`) | outgoing to a configured third party | the content and the extracted text of every document leave the system towards the configured address |
| Database (PostgreSQL or MariaDB) | internal-only | reached by the backend only |
| Build- and release-pipeline (`scbuildcodeunits`, `Dockerfile`) | local-only / build-infrastructure | base-images, apt-packages, nuget- and npm-packages, the `.deb` which is downloaded while building the image |

### Assets and trust-boundaries

Sensitive assets: the stored documents and their ocr-content (regularly personal and financial data), the password-hashes of the
users, the access-tokens, the audit-log, the database-connection-string, the api-keys of the ocr- and the ai-service, and the
private key of the shipped tls-certificate.

The trust-boundaries which untrusted input crosses are: browser to nginx, nginx to the backend-api, the import-folder to the
background-service, the backend to the external ocr- and ai-service, and the ci-pipeline to the produced container-image. The
strongest boundary of the product is the permission-check in `BusinessLogicService`: the api-layer only authenticates and maps,
and every business-operation decides for itself whether the acting user may perform it.

### A01:2025 Broken Access Control

The access-control-concept itself is implemented consistently and is the strongest part of the application: every endpoint of
`OpenDMSBackendController` and `UserController` carries `[Authenticate]` together with `[Authorize]`, and every business-operation
in `BusinessLogicService` additionally resolves the permission itself through `UserHasPermissionInHierarchy`, which follows a
default-deny-model with inheritance along the containment-hierarchy and does not grant an administrator access to content. No
operation which takes an object-id was found which trusts that id without checking it, so no insecure direct object reference was
found. Sixteen testcases assert that an operation is refused for a user without the permission.

The findings of this category (OWASP-01 and OWASP-02) and their current state are listed in the [findings of the product reference](./Other/Reference/Reference.md#findings).

### A02:2025 Security Misconfiguration

The findings of this category (OWASP-03, OWASP-04, OWASP-05, OWASP-06, OWASP-07, OWASP-08 and OWASP-09) and their current state are listed in the [findings of the product reference](./Other/Reference/Reference.md#findings).

### A03:2025 Software Supply Chain Failures

The positive part first: all nuget-dependencies are pinned to an exact version with the bracket-notation (`[2.1.0]`), all
npm-dependencies are pinned exactly, both code-units keep a lock-file (`RestorePackagesWithLockFile`), `nuget.config` clears the
machine-sources so that the build does not depend on the developer-machine, and the pipeline-images for the generation of a
bill-of-materials (syft) and for a secret-scan (betterleaks) are defined in `.ScriptCollection/OCIImages/ImageDefinition.csv`.

The findings of this category (OWASP-10, OWASP-11 and OWASP-12) and their current state are listed in the [findings of the product reference](./Other/Reference/Reference.md#findings).

### A04:2025 Cryptographic Failures

The findings of this category (OWASP-13, OWASP-14 and OWASP-15) and their current state are listed in the [findings of the product reference](./Other/Reference/Reference.md#findings).

In addition, document-contents are stored unencrypted in the database. That is a legitimate decision for a document-management-
system which has to search them, but it means the protection of the data rests entirely on the database and on its backups, which
belongs into the procedure-documentation (issue 23).

### A05:2025 Injection

No sql-injection was found. Every statement is an embedded `.sql`-resource which is executed with bound parameters
(`GetParameter(...)`); the one place which assembles sql dynamically (`DatabasePersistence.UpdateRole`, which replaces the marker
`__generated__`) only generates parameter-names and no values. The frontend contains no `innerHTML`, no `eval` and no
`bypassSecurityTrust*`-call, so the output-escaping of angular is in place everywhere.

The findings of this category (OWASP-16, OWASP-17 and OWASP-18) and their current state are listed in the [findings of the product reference](./Other/Reference/Reference.md#findings).

The generation of the adapt-script itself was checked as well: `ToTSStringLiteral` escapes backslash, double-quote and both
line-breaks before a value (among them the attacker-controllable ocr-content) is written into the generated typescript, which is
complete for a double-quoted literal of the targeted language-version. No script-injection was found there.

### A06:2025 Insecure Design

The findings of this category (OWASP-19, OWASP-20, OWASP-21, OWASP-22, OWASP-23 and OWASP-24) and their current state are listed in the [findings of the product reference](./Other/Reference/Reference.md#findings).

### A07:2025 Authentication Failures

The findings of this category (OWASP-25, OWASP-26, OWASP-27 and OWASP-28) and their current state are listed in the [findings of the product reference](./Other/Reference/Reference.md#findings).

A token is valid for exactly 24 hours, is not rotated and has no idle-timeout; that is a design-decision rather than a defect,
but it enlarges the window of OWASP-01 and should be reconsidered together with it.

### A08:2025 Software or Data Integrity Failures

This is the category in which the product itself already names most of the gaps: the readme lists the tamper-proof audit-log
(issue 6), the immutability of an archived document (issue 4), the integrity-protection of the stored content (issue 22) and the
gap-free numbering (issue 8) as not implemented. They are repeated here because they are security-findings and not only
gobd-findings.

The findings of this category (OWASP-29, OWASP-30 and OWASP-31) and their current state are listed in the [findings of the product reference](./Other/Reference/Reference.md#findings).

### A09:2025 Security Logging and Alerting Failures

The product has a real audit-log for its business-operations, which covers the change-side well: adding, changing, moving,
renaming, sharing, tag- and metadata-changes and both kinds of deletion each write an entry with the acting user. What is missing
is the security-side of logging.

The findings of this category (OWASP-32, OWASP-33 and OWASP-34) and their current state are listed in the [findings of the product reference](./Other/Reference/Reference.md#findings).

### A10:2025 Mishandling of Exceptional Conditions

The findings of this category (OWASP-35, OWASP-36, OWASP-37 and OWASP-38) and their current state are listed in the [findings of the product reference](./Other/Reference/Reference.md#findings).

Two further points of this category concern the behaviour of `GRYLibrary` and were verified there afterwards (version 2.1.3; the
result is recorded in the readme of that repository). The first one is harmless: `DefaultExceptionHandlerMiddleware` answers with
an empty body in every environment, so no stack-trace and no exception-message reaches the client. The second one is not:
authentication is required only for a route whose action carries `[Authenticate]` or `[Authorize]`, so the authentication-
middleware does not fail closed for a route which carries neither - every endpoint of this product has one of the two attributes,
so nothing is open today, but the protection rests on an annotation which nothing enforces. A third point was found while
verifying those two: a security-decision of the library is signalled as an exception and is only turned into a status-code by the
exception-middleware, so the 401 and the 403 of this product depend on that middleware staying configured.

### Validation and test-coverage

The backend has 175 testcases, and sixteen of them assert that an operation is refused for a user who lacks the permission - the
access-control is therefore the one security-property which is really tested. No test exists for any of the following, all of
which are the high-risk paths of this analysis: the login (`PersistentAuthenticationService.Login`), the password-hashing, the
validity and the expiry of an access-token, the behaviour of an upload with an unexpected type or an excessive size, the
rendering-path of a document in the frontend, the sandbox of an adapt-script, and the permission-check against a cyclic
containment-hierarchy. Adding a test for each of them is the cheapest way to keep the findings of this section from coming back.

Useful secure defaults which are already in place and which should be kept: the default-deny-permission-model with inheritance,
the api-key- and token-transport in headers instead of in cookies (which removes cross-site-request-forgery structurally), the
exact pinning of every nuget- and npm-dependency together with lock-files and a cleared package-source-list, the parameterized
sql-statements, the fonts which are delivered with the application instead of being fetched from a third party, and the
pipeline-images for the secret-scan and for the bill-of-materials.

### Prioritized remediation plan

1. Replace the password-hashing with a salted memory-hard algorithm and re-hash on the next login (OWASP-13), and remove the
   default-password `admin` (OWASP-25). Both are reachable without any precondition and compromise the whole installation.
2. Close the stored-cross-site-scripting: deliver a document as a download instead of as its stored mime-type, and add an
   allowlist of accepted types (OWASP-16, OWASP-19). Add the content-security-policy in the same step (OWASP-05).
3. Build a productive image without the development-certificate and let it run as a non-root user (OWASP-03, OWASP-04).
4. Add a rate-limit and a lockout to the login and log every failed attempt (OWASP-26, OWASP-32).
5. Add the missing password-management (change and administrative reset) and make a token revocable (OWASP-20, OWASP-01).
6. Replace every swallowed exception of the background-service by a logged one, and exclude the authentication-routes from the
   request-logging so that no access-token is written to a log-file (OWASP-33, OWASP-34).
7. Harden the deployment-surface: secrets through the environment instead of the commandline, a slimmer runtime-image, and an
   example-deployment which publishes nothing unnecessary (OWASP-06, OWASP-07, OWASP-08, OWASP-17).
8. Make the data trustworthy: hash-chain the audit-log and store a checksum per document (OWASP-29, OWASP-30), which are the
   issues 6 and 22 of the feature-list.
9. Work off the robustness-findings (OWASP-21, OWASP-22, OWASP-23, OWASP-35, OWASP-36, OWASP-37) and the remaining
   supply-chain- and crypto-findings (OWASP-10, OWASP-11, OWASP-12, OWASP-14, OWASP-15, OWASP-31).
10. Add the missing security-tests listed above, and verify the two open assumptions against `GRYLibrary`.

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
