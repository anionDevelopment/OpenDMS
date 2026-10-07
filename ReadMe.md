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

**OWASP-01 - An access-token can not be revoked** (Medium, confirmed)

- Affected component: `OpenDMSBackend/OpenDMSBackend/Services/PersistentAuthenticationService.cs`, `.../Services/BusinessLogicService.cs`
- Attack-surface: internet-exposed api, after a token was leaked
- Evidence: `LogoutEverywhere`, `RemoveUser` and `UpdateUser` throw a `NotImplementedException`; `Logout` removes exactly the one
  token it is called with; `BusinessLogicService.Housekeeping` (whose only task is to remove expired tokens) throws a
  `NotImplementedException` as well and is never called by anybody.
- Impact: a stolen access-token stays usable for up to 24 hours and neither the user nor an administrator has any way to
  invalidate it; locking or deleting a user does not end their running sessions.
- Recommendation: implement `LogoutEverywhere` (delete every token of the user), invalidate every token of a user when the user
  is locked or deleted, and call a housekeeping-run regularly which removes expired tokens.

**OWASP-02 - The token-check is reachable without authentication** (Low, confirmed)

- Affected component: `OpenDMSBackend/OpenDMSBackend/Controller/UserController.cs` (`TokenIsValid`)
- Attack-surface: internet-exposed api
- Evidence: the route carries no `[Authenticate]`, so any caller can have an arbitrary token-value checked, without any limit on
  the number of attempts.
- Impact: the endpoint is an oracle which answers whether a token-value exists. A token is a version-4-guid, so guessing one is
  not practical; the finding is the missing limit and the unnecessary exposure, not a feasible attack today.
- Recommendation: let a client check its own token through an authenticated route and apply the same rate-limit as for the login.

### A02:2025 Security Misconfiguration

**OWASP-03 - The container-image contains the private key of the development-certificate** (High, confirmed)

- Affected component: `OpenDMS/OpenDMS/Dockerfile`, `OpenDMS/OpenDMS/nginx.conf`, `OpenDMS/Other/Resources/DevelopmentCertificate/`, `OpenDMSBackend/OpenDMSBackend/Constants/GeneralConstants.cs`
- Attack-surface: internet-exposed, whenever the built-in port 443 is used directly
- Evidence: `OpenDMSDevelopmentCertificate.key` and `.pfx` (together with the file which holds its password) are part of the
  repository and are copied into the image; `nginx.conf` uses exactly that certificate and key for its `listen 443 ssl`-server.
  The same pfx and its password are additionally compiled into the backend as the constants
  `DevelopmentCertificatePFXHex` and `DevelopmentCertificatePasswordHex`.
- Impact: the private key is public. Anybody who reaches a deployment which relies on the built-in port 443 can decrypt and
  modify the traffic of that deployment. The changelog of version 4.0.0 already records this as an open point.
- Recommendation: build a productive image which contains no development-certificate, require the operator to mount their own
  certificate (or to let the reverse-proxy terminate tls and not offer port 443 at all), and keep the development-certificate in
  the development-image only. Until that is done, state in the installation-article that port 443 of the container must never be
  published.

**OWASP-04 - The application runs as root in the container** (High, confirmed)

- Affected component: `OpenDMS/OpenDMS/Dockerfile`
- Attack-surface: internet-exposed, as the consequence of any other code-execution-finding
- Evidence: the dockerfile contains no `USER`-instruction, so `EntryPoint.sh`, the backend and nginx all run as root.
- Impact: every vulnerability which reaches code-execution (for example through one of the parsers, through libreoffice or
  through the v8-engine of an adapt-script) immediately owns the whole container instead of an unprivileged account.
- Recommendation: create a dedicated user in the image, give it the write-permission for `/Workspace/Configuration`,
  `/Workspace/Logs` and `/Workspace/Data` only, switch to it with `USER`, and document `read_only`, `cap_drop` and
  `no-new-privileges` for the deployment.

**OWASP-05 - No content-security-policy and no further modern security-headers** (High, confirmed)

- Affected component: `OpenDMS/OpenDMS/nginx.conf`
- Attack-surface: internet-exposed
- Evidence: the configuration sets `X-Content-Type-Options`, `X-Frame-Options`, `Referrer-Policy` and the obsolete
  `X-XSS-Protection`, but no `Content-Security-Policy`, no `Strict-Transport-Security`, no `Cross-Origin-Opener-Policy` and no
  `Permissions-Policy`. The `add_header`-directives also only apply to a successful response, not to an error-response.
- Impact: nothing limits what injected script may do, which is what turns OWASP-09 into a complete account-takeover instead of a
  contained defect. Without `Strict-Transport-Security` a first request stays downgradeable.
- Recommendation: add a strict `Content-Security-Policy` (own origin only, `object-src 'none'`, `base-uri 'none'`,
  `frame-ancestors 'none'`) and `Strict-Transport-Security`, mark the headers as `always`, and drop `X-XSS-Protection`.

**OWASP-06 - Secrets are passed as process-arguments** (Medium, confirmed)

- Affected component: `OpenDMS/OpenDMS/EntryPoint.sh`
- Attack-surface: internal, every process inside the container
- Evidence: the entry-point builds one argument-string which contains `--InitialAdminPassword`,
  `--InitialDatabaseConnectionString` and `--InitialOCRDataServiceAPIKey` and passes it to `dotnet`.
- Impact: the database-password, the initial administrator-password and the api-key are readable in `/proc/<pid>/cmdline` and in
  every process-listing, and they regularly end up in crash-dumps and in monitoring-data.
- Recommendation: let the backend read these values from the environment (or from a file which is mounted as a secret) instead of
  from the commandline.

**OWASP-07 - The runtime-image contains build- and administration-tooling** (Medium, confirmed)

- Affected component: `OpenDMS/OpenDMS/Dockerfile`
- Attack-surface: internal, after any code-execution
- Evidence: the image installs `curl`, `git`, `wget`, `nodejs`, `npm` and the libreoffice-packages, and none of the
  apt-packages is installed with a pinned version.
- Impact: every one of these tools is a ready-made building-block for a further step of an attack (fetching a payload, pushing
  data out), and the unpinned versions make the image non-reproducible, which also contradicts the convention of this product
  that every used version has to be readable from the source.
- Recommendation: install only what the runtime really needs (the typescript-compiler is needed, `git` and `wget` are not; `wget`
  is only used during the build and can be removed in the same layer), and pin every apt-package with its version.

**OWASP-08 - The example-deployment publishes the database and a database-administration-UI** (Medium, confirmed)

- Affected component: `OpenDMS/Other/Reference/ReferenceContent/Examples/MinimalDockerComposeFile/docker-compose.yml`
- Attack-surface: internet-exposed as soon as the example is used on a reachable host
- Evidence: the compose-file publishes `5432:5432` for the database and `8080:8080` for adminer, and it contains the
  database-password `pa55w0rd` in plain text.
- Impact: the example is referenced by the readme as the minimal deployment, so it will be copied. Whoever copies it exposes the
  database and an unauthenticated-reachable database-administration-UI with a known password.
- Recommendation: remove both port-publications from the example (the containers reach each other over the compose-network
  anyway), move adminer into an optional profile, and generate the password instead of shipping one.

**OWASP-09 - The health-check-endpoint is unauthenticated and enabled by default** (Low, confirmed)

- Affected component: `OpenDMSBackend/OpenDMSBackend/Misc/HealthCheck.cs`, `.../Program.cs`
- Attack-surface: internet-exposed, unauthenticated
- Evidence: `InitialEnableEndpointHealthCheckValue` and `InitialEnableEndpointAvailabilityCheckValue` default to `true`, and the
  route is listed in `RoutesWhereUnauthenticatedAccessIsAllowed`. The health-result carries the names and the state of the
  internal services. The api-specification is hosted unauthenticated as well
  (`HostAPISpecificationForInNonDevelopmentEnvironment = true`).
- Impact: an unauthenticated caller learns which persistence and which external services are used and whether they are currently
  failing, which is useful reconnaissance.
- Recommendation: let the reverse-proxy keep `/API/Other/Maintenance/*` internal, or answer it with a plain status without the
  detail-messages.

### A03:2025 Software Supply Chain Failures

The positive part first: all nuget-dependencies are pinned to an exact version with the bracket-notation (`[2.1.0]`), all
npm-dependencies are pinned exactly, both code-units keep a lock-file (`RestorePackagesWithLockFile`), `nuget.config` clears the
machine-sources so that the build does not depend on the developer-machine, and the pipeline-images for the generation of a
bill-of-materials (syft) and for a secret-scan (betterleaks) are defined in `.ScriptCollection/OCIImages/ImageDefinition.csv`.

**OWASP-10 - A package is downloaded and installed while building the image without verifying it** (Medium, confirmed)

- Affected component: `OpenDMS/OpenDMS/Dockerfile`
- Attack-surface: build-pipeline
- Evidence: `wget https://packages.microsoft.com/config/debian/13/packages-microsoft-prod.deb` followed by `dpkg -i` without any
  checksum- or signature-check, and `npm install -g typescript@5.9.2` without an integrity-hash.
- Impact: whoever can influence that download (a compromised mirror, a proxy in the build-network) gets code-execution in every
  produced image, and the produced image is not reproducible.
- Recommendation: pin the expected checksum of the `.deb` and verify it before installing, or add the repository through a
  key-file which is part of the repository.

**OWASP-11 - Base-images are pinned by a mutable tag** (Medium, confirmed)

- Affected component: `.ScriptCollection/OCIImages/ImageDefinition.csv`
- Attack-surface: build-pipeline
- Evidence: every image is referenced by a version-tag (for example `debian:13.4-slim`), not by a digest.
- Impact: a tag can be moved, so two builds of the same commit can use different base-images, and a compromised upstream-tag ends
  up in the next build unnoticed.
- Recommendation: additionally record the digest of every image and build against the digest.

**OWASP-12 - A dependency-vulnerability does not break the build** (Low, confirmed)

- Affected component: `OpenDMSBackend/OpenDMSBackend/OpenDMSBackend.csproj`
- Attack-surface: build-pipeline
- Evidence: `WarningsAsErrors` contains only `NU1605`, and no `NuGetAudit`-settings are made, so an audit-warning about a known
  vulnerable package is only a warning.
- Impact: a known vulnerable dependency can enter a release without anybody having to acknowledge it.
- Recommendation: set `NuGetAuditMode` to `all` and promote `NU1901` to `NU1904` to errors, and let the frontend-build fail on a
  high npm-audit-finding.

### A04:2025 Cryptographic Failures

**OWASP-13 - Passwords are stored as an unsalted single-round sha-256-hash** (Critical, confirmed)

- Affected component: `OpenDMSBackend/OpenDMSBackend/Services/PersistentAuthenticationService.cs` (`Hash`)
- Attack-surface: every stored password; exploited as soon as the database or a backup is read
- Evidence: `Hash` returns the hex-representation of a single `SHA256`-pass over the password. There is no salt, no pepper and no
  key-stretching, and `Login` compares the result with `!=` against the stored value.
- Impact: sha-256 is built to be fast, so a leaked `Users`-table can be attacked with rainbow-tables and with billions of guesses
  per second on commodity hardware; most passwords of a real installation would fall. Because there is no salt, two users with
  the same password are visibly identical, and one cracked hash reveals every account which shares it.
- Recommendation: store the hash with a memory-hard algorithm with a per-user salt (argon2id, or pbkdf2-hmac-sha256 with a high
  iteration-count as the option which the platform brings with it), keep the algorithm and its parameters next to the hash so
  that they can be raised later, re-hash a password transparently on the next successful login, and compare with a
  constant-time comparison.

**OWASP-14 - The address of the ocr- and the ai-service is not restricted to a secure scheme** (Medium, confirmed)

- Affected component: `OpenDMSBackend/OpenDMSBackend/Services/AISummaryServiceClient.cs`, `.../Services/OCRServiceClient.cs`
- Attack-surface: outgoing connection to a configured third party
- Evidence: the configured address is used as it is; `http://` is accepted. The api-key is then sent as a `Bearer`-token
  respectively as the header `X-ApiKey` over an unencrypted connection, together with the full text of the document.
- Impact: a misconfiguration (or a deliberately chosen plain-text endpoint) sends the api-key and the content of every analysed
  document unencrypted over the network. No timeout is set on the `HttpClient` either, so a slow endpoint holds a thread.
- Recommendation: accept only `https` (with an explicit opt-in for a plain-text address inside a trusted network), set an
  explicit timeout, and state in the installation-article that the full text of every document is transmitted to that address.

**OWASP-15 - Access-tokens are generated with a general-purpose guid** (Low, confirmed)

- Affected component: `OpenDMSBackend/OpenDMSBackend/Services/PersistentAuthenticationService.cs`, `.../Services/OIDCLoginService.cs`
- Attack-surface: internet-exposed api
- Evidence: a token is `Guid.NewGuid().ToString()`.
- Impact: a version-4-guid carries 122 random bits from a cryptographically strong source on the supported platforms, so this is
  not exploitable today. It is a finding because the security-property then depends on an implementation-detail of the platform
  instead of on an explicit decision in the code.
- Recommendation: generate the token from `RandomNumberGenerator` explicitly (for example 256 bits, base64url-encoded) and store
  only a hash of it, so that a read of the token-table does not yield usable tokens.

In addition, document-contents are stored unencrypted in the database. That is a legitimate decision for a document-management-
system which has to search them, but it means the protection of the data rests entirely on the database and on its backups, which
belongs into the procedure-documentation (issue 23).

### A05:2025 Injection

No sql-injection was found. Every statement is an embedded `.sql`-resource which is executed with bound parameters
(`GetParameter(...)`); the one place which assembles sql dynamically (`DatabasePersistence.UpdateRole`, which replaces the marker
`__generated__`) only generates parameter-names and no values. The frontend contains no `innerHTML`, no `eval` and no
`bypassSecurityTrust*`-call, so the output-escaping of angular is in place everywhere.

**OWASP-16 - Stored cross-site-scripting through an uploaded document** (High, confirmed)

- Affected component: `OpenDMSFrontend/src/app/modules/user-area/edit-document-menu/edit-document-menu.component.ts`
  (`viewDocument`), `OpenDMSBackend/OpenDMSBackend/Services/BusinessLogicService.cs` (`CreateAndPersistAnalysedDocument`)
- Attack-surface: internet-exposed, every user who may upload a document into a storage-location which somebody else may view
- Evidence: the mime-type of a document is derived from the file-name which the uploader supplies
  (`GetMIMEType(originalFilename)`), and `IsValid` validates nothing at all. `viewDocument` builds a blob with exactly that
  mime-type and opens it with `window.open()` followed by `tab.location.href = fileURL`. A blob-url inherits the origin of the
  page which created it.
- Impact: a user uploads a file named `invoice.html` (or `.svg`) whose content is script. Every user who opens that document
  executes that script in the origin of the web-application, which gives read-access to the access-token in the
  `sessionStorage` and thereby to every document that victim may see. Nothing mitigates it, because no content-security-policy
  exists (OWASP-05).
- Recommendation: do not hand the stored mime-type to the browser for rendering. Deliver a document as a download
  (`Content-Disposition: attachment` respectively a blob with `application/octet-stream`, which the download-path already does
  correctly), render only a type which is known to be safe inline, and keep an allowlist of accepted mime-types on upload. A
  sandboxed `iframe` with `Content-Security-Policy: sandbox` is the alternative if an inline-view of arbitrary types is wanted.

**OWASP-17 - Argument-injection in the entry-point of the container** (Low, confirmed)

- Affected component: `OpenDMS/OpenDMS/EntryPoint.sh`
- Attack-surface: whoever sets the environment-variables of the container
- Evidence: every value is appended to one string without quoting (`argument+=" --InitialDomain $InitialDomain"`) and that string
  is then passed unquoted to `dotnet`, so the shell splits it into words and expands globs.
- Impact: a value which contains a space becomes two arguments, so a value can inject a further commandline-option (for example
  its own `--InitialAdminPassword`), and a value which contains `*` is expanded against the file-system. A password which simply
  contains a space breaks the start.
- Recommendation: collect the arguments in a bash-array and pass it as `"${arguments[@]}"`, or - better, see OWASP-06 - read the
  values from the environment in the application.

**OWASP-18 - The typescript-compiler is started through a shell** (Low, confirmed)

- Affected component: `OpenDMSBackend/OpenDMSBackend/BackgroundServices/ManagementService.cs` (`RunTSC`)
- Attack-surface: internal, the adapt-script of an import-definition
- Evidence: the compiler is called as `/bin/bash -c "tsc <args>"` respectively `cmd.exe /c`. The argument is a constant today, so
  there is nothing to inject at the moment.
- Impact: none today. It is one refactoring away from being a command-injection, because the pattern invites passing a dynamic
  value into the string.
- Recommendation: start `tsc` directly with its argument-list instead of through a shell.

The generation of the adapt-script itself was checked as well: `ToTSStringLiteral` escapes backslash, double-quote and both
line-breaks before a value (among them the attacker-controllable ocr-content) is written into the generated typescript, which is
complete for a double-quoted literal of the targeted language-version. No script-injection was found there.

### A06:2025 Insecure Design

**OWASP-19 - An upload is not restricted in any way** (High, confirmed)

- Affected component: `OpenDMSBackend/OpenDMSBackend/Services/BusinessLogicService.cs` (`IsValid`), `OpenDMS/OpenDMS/nginx.conf`
- Attack-surface: internet-exposed, every authenticated user
- Evidence: `IsValid` returns `true` for every document and contains only a todo. There is no allowlist of types, no size-limit
  and no check that the content matches the claimed type. nginx allows a body of 800 megabytes, and the content travels through
  the api as a base64-string inside json, so the whole document exists several times in memory.
- Impact: one user can exhaust the memory and the disk of the installation with a few requests, and the missing type-check is the
  precondition of OWASP-16.
- Recommendation: define a maximum document-size and enforce it in the backend as well, validate the type of the content against
  its claimed mime-type instead of trusting the file-name, and keep an allowlist of accepted types.

**OWASP-20 - A password can not be changed** (High, confirmed)

- Affected component: `OpenDMSBackend/OpenDMSBackend/Controller/UserController.cs`, `.../Services/BusinessLogicService.cs`
- Attack-surface: internet-exposed api
- Evidence: the repository contains no operation to change or to reset a password, and no password-rule of any kind (no minimum
  length, no check against a known-breached list).
- Impact: the initial administrator-password can never be changed through the product, although the readme asks the user to
  change it after the first login; a user whose password was leaked has no way to react; and nothing stops a one-character
  password.
- Recommendation: add an authenticated change-password-operation (which requires the current password and ends every other
  session of that user), a reset which an administrator can trigger, and a minimum-strength-rule.

**OWASP-21 - Search and the latest-documents-list check the permission per result** (Medium, confirmed)

- Affected component: `OpenDMSBackend/OpenDMSBackend/Services/BusinessLogicService.cs` (`Search`, `GetLatestDocuments`)
- Attack-surface: internet-exposed, every authenticated user
- Evidence: `GetLatestDocuments` reads every document-id of the installation and then calls `UserIsAllowedToViewContent` for each
  of them, which walks the containment-hierarchy with one database-query per level; `Search` does the same for every hit. The
  search-statement itself has no `limit` and matches with `ilike '%term%'` over the ocr-content of every document.
- Impact: a single cheap request causes a load which grows with the whole document-count, so a handful of authenticated requests
  can keep the database busy. A search-term of `%` matches everything.
- Recommendation: resolve the permission in the query (or cache the permission-relevant hierarchy per request), page the result,
  and escape the wildcard-characters of the search-term.

**OWASP-22 - The pending oidc-logins grow without a bound** (Medium, confirmed)

- Affected component: `OpenDMSBackend/OpenDMSBackend/Services/OIDCLoginService.cs`
- Attack-surface: internet-exposed, unauthenticated (`OIDCController/InitiateOIDCLogin`)
- Evidence: every call adds an entry to the in-memory dictionary `_PendingLogins`; the cleanup removes only entries which are
  older than ten minutes and runs only when a further login is initiated.
- Impact: an unauthenticated caller can fill the memory of the process with pending logins. The state is held in the process as
  well, so a second instance of the backend can not complete a login which the first one started.
- Recommendation: limit the number of pending logins (and reject further ones), and hold the state where every instance can read
  it if the product is supposed to run more than once.

**OWASP-23 - A server-side script from the configuration runs without a limit** (Medium, confirmed)

- Affected component: `OpenDMSBackend/OpenDMSBackend/BackgroundServices/ManagementService.cs` (`RunAdaptScript`)
- Attack-surface: internal, whoever may write the configuration-file
- Evidence: the adapt-script-body of an import-definition is compiled and executed in a `V8ScriptEngine` without a runtime-limit
  and without a heap-limit. No host-object and no host-type is added to the engine, so the script can not reach the .net-side -
  that part is sound.
- Impact: a script with an endless loop stops the whole management-background-service, and with it the regulated deletion. The
  script is also compiled by starting an external process for every single imported document.
- Recommendation: set `MaxRuntimeHeapSize` and a runtime-timeout on the engine, and compile a script once per import-definition
  instead of once per document.

**OWASP-24 - The last administrator can remove their own administrator-role** (Low, confirmed)

- Affected component: `OpenDMSBackend/OpenDMSBackend/Services/BusinessLogicService.cs` (`SetRolesOfUser`)
- Attack-surface: internet-exposed, administrator
- Evidence: the operation only checks that the caller is an administrator and then sets the role-set of the target-user, which may
  be the caller and may be empty.
- Impact: an installation can be left without any administrator, and there is no way back because no password-reset and no
  recovery-path exists (see OWASP-20).
- Recommendation: refuse an operation which would leave the installation without an administrator, analogously to the rule which
  already guarantees that a folder always keeps a moderator.

### A07:2025 Authentication Failures

**OWASP-25 - The initial administrator-password is "admin"** (Critical, confirmed)

- Affected component: `OpenDMSBackend/OpenDMSBackend/Services/InitializationService.cs`
- Attack-surface: internet-exposed, unauthenticated
- Evidence: when `InitialAdminPassword` is not passed, the administrator `admin` is created with the password `admin`. Nothing
  forces a change, nothing warns at runtime, and - see OWASP-20 - the product offers no way to change it at all.
- Impact: every installation which was started without that parameter can be taken over by anybody who knows the product. The
  administrator-role manages the global roles, so the takeover extends to granting oneself further permissions.
- Recommendation: do not create a usable default-password. Either require `InitialAdminPassword`, or generate a random one, write
  it to the log once and force a change at the first login.

**OWASP-26 - The login has no limit on the number of attempts** (High, confirmed)

- Affected component: `OpenDMSBackend/OpenDMSBackend/Controller/UserController.cs` (`Login`), `.../Program.cs`
- Attack-surface: internet-exposed, unauthenticated
- Evidence: no rate-limiting-middleware is registered in `Program.cs`, no counter of failed attempts exists on the user, and
  `UserIsLocked` is only ever set manually. nginx defines no `limit_req` either. A second factor is not offered.
- Impact: passwords can be guessed at the speed of the network, which together with the missing password-rule (OWASP-20) and the
  fast hash (OWASP-13) makes a credential-stuffing-run cheap.
- Recommendation: rate-limit per account and per source-address, lock an account temporarily after repeated failures, and offer a
  second factor at least for the administrator-role.

**OWASP-27 - The access-token is readable by script in the browser** (Medium, confirmed)

- Affected component: `OpenDMSFrontend/src/app/services/storage.service.ts`
- Attack-surface: internet-exposed, in combination with any cross-site-scripting
- Evidence: the token is kept in the `sessionStorage` and is sent as a header. That it is a header and not a cookie is a good
  decision, because it removes cross-site-request-forgery structurally; the price is that script can read it.
- Impact: every cross-site-scripting (see OWASP-16) directly yields a valid token which can not be revoked (see OWASP-01).
- Recommendation: either hold the token in a cookie with `HttpOnly`, `Secure` and `SameSite=Strict` and add an explicit
  csrf-protection, or keep the header-approach and accept that it stands and falls with the content-security-policy of OWASP-05;
  in either case bind the token to a short idle-timeout.

**OWASP-28 - The login reveals whether a user-name exists** (Low, confirmed)

- Affected component: `OpenDMSBackend/OpenDMSBackend/Services/PersistentAuthenticationService.cs` (`Login`)
- Attack-surface: internet-exposed, unauthenticated
- Evidence: the answer-text is the same for both cases, which is right, but for an unknown user-name the hash is never computed,
  so the response is measurably faster.
- Impact: user-names can be enumerated, which makes a targeted attack easier.
- Recommendation: always compute a hash, also for an unknown user-name.

A token is valid for exactly 24 hours, is not rotated and has no idle-timeout; that is a design-decision rather than a defect,
but it enlarges the window of OWASP-01 and should be reconsidered together with it.

### A08:2025 Software or Data Integrity Failures

This is the category in which the product itself already names most of the gaps: the readme lists the tamper-proof audit-log
(issue 6), the immutability of an archived document (issue 4), the integrity-protection of the stored content (issue 22) and the
gap-free numbering (issue 8) as not implemented. They are repeated here because they are security-findings and not only
gobd-findings.

**OWASP-29 - The audit-log can be modified afterwards** (High, confirmed)

- Affected component: `OpenDMSBackend/OpenDMSBackend/Program.cs` (`AuditLogConfiguration`, `./Audit.log`)
- Attack-surface: internal, whoever reaches the file-system of the container or the mounted log-volume
- Evidence: the audit-log is an ordinary writable log-file in the log-folder. There is no append-only-mode, no signature, no
  hash-chain and no transfer to a system outside the container.
- Impact: the log records who changed which object, and that record is exactly what an attacker who got onto the machine would
  remove. Its evidential value is therefore limited to the case in which the host itself stayed intact.
- Recommendation: chain the entries with a hash (every entry carries the hash of its predecessor), additionally write them to a
  write-once-destination or to an external log-sink, and separate the permission to write the log from the permission to change
  it.

**OWASP-30 - A document has no integrity-value and is not immutable** (High, confirmed)

- Affected component: `OpenDMSBackend/OpenDMSBackend/Model/BusinessTypes/Document.cs`, `.../Services/DatabasePersistence.cs`
- Attack-surface: internal, whoever reaches the database
- Evidence: a document carries no checksum, so a change of the `Content`-column can neither be detected nor proven; the version-
  mechanism marks the old row as not-latest but does not prevent it from being updated.
- Impact: the central promise of an archive - that what comes out is what went in - can not be verified.
- Recommendation: store a hash of the content on capture, verify it on every read, and refuse an update of the content-column of
  an existing version at the database-level.

**OWASP-31 - The produced assemblies carry no valid signature** (Low, confirmed)

- Affected component: `OpenDMSBackend/OpenDMSBackend/OpenDMSBackend.csproj`
- Attack-surface: build-pipeline and distribution
- Evidence: `SignAssembly` is `true` but `DelaySign` is `true` as well and only a public key is present; the warning `CS8002`
  about references without a strong name is suppressed.
- Impact: the strong name does not verify, so it provides no integrity-guarantee for a delivered assembly. No signature and no
  provenance-attestation of the container-image exists either.
- Recommendation: either complete the signing in the release-pipeline or stop claiming it, and sign the released container-image
  (for example with cosign) and publish the bill-of-materials which the pipeline already generates next to it.

### A09:2025 Security Logging and Alerting Failures

The product has a real audit-log for its business-operations, which covers the change-side well: adding, changing, moving,
renaming, sharing, tag- and metadata-changes and both kinds of deletion each write an entry with the acting user. What is missing
is the security-side of logging.

**OWASP-32 - A failed login is not logged** (High, confirmed)

- Affected component: `OpenDMSBackend/OpenDMSBackend/Services/PersistentAuthenticationService.cs` (`Login`)
- Attack-surface: internet-exposed
- Evidence: `ThrowInvalidCredentialsException` throws without writing anything, and no other place records a failed attempt. A
  successful login is not written to the audit-log either, nor is a refused authorization.
- Impact: a password-guessing-run against the installation leaves no trace at all, so neither an alert nor a later investigation
  is possible. Together with OWASP-26 this means brute-force is both unlimited and invisible.
- Recommendation: log every failed and every successful login with the user-name, the source-address and the timestamp into the
  audit-log, log a refused authorization as well, and alert on a threshold of failures.

**OWASP-33 - Exceptions of the background-service are swallowed silently** (High, confirmed)

- Affected component: `OpenDMSBackend/OpenDMSBackend/BackgroundServices/ManagementService.cs`
- Attack-surface: internal
- Evidence: `RunTask` catches every exception with an empty block marked `//TODO log exception`, and the import-loop contains
  three further empty catch-blocks, among them the one around the adapt-script.
- Impact: the regulated deletion and the import can fail permanently without anybody noticing, which for a retention-driven
  system is a compliance-failure and not only a bug. A failing adapt-script is invisible as well.
- Recommendation: log every caught exception with its context into the management-service-log, and expose a metric or a
  health-signal for a task which keeps failing.

**OWASP-34 - The access-token of every login is written to the request-log** (High, confirmed)

- Affected component: `OpenDMSBackend/OpenDMSBackend/Program.cs` (`ConfigurationForDLoggingMiddleware`), `OpenDMSBackend/OpenDMSBackend/Controller/UserController.cs` (`Login`)
- Attack-surface: internal, the log-files and everything they are forwarded to
- Evidence: the request-logging-middleware is configured to log up to 500 characters of the request- and response-body, and the
  routes which are excluded from logging are only the api-specification, the metrics and the health-check. The behaviour of the
  middleware of `GRYLibrary` was verified: request-headers are logged only when they are listed explicitly in
  `LoggedHTTPRequeustHeader`, which stays empty here, so the `password`- and the `accessToken`-header are not logged. The bodies
  however are always written in full to the log-file (`ShouldLogEntireRequestContentInLogFile` returns `true` in every case) and
  are not redacted, and the response of `Login` is the `AccessToken`-object, which is far shorter than the limit of 500
  characters.
- Impact: the access-token of every login is written to `Requests.log` in plain text, so read-access to the logs is equivalent to
  taking over the accounts which logged in, for the whole lifetime of those tokens (24 hours, and they can not be revoked, see
  OWASP-01). The first 500 characters of every uploaded document and of every retrieved document land there as well.
- Recommendation: exclude the authentication-routes from the request-logging (`NotLoggedRoutes`) until `GRYLibrary` offers a
  redaction for bodies, and do not transport credentials in headers of a `PUT` but in the body. The underlying defect belongs
  into `GRYLibrary`, where it is recorded as `GRY-30`.

### A10:2025 Mishandling of Exceptional Conditions

**OWASP-35 - The permission-check can loop forever** (Medium, confirmed)

- Affected component: `OpenDMSBackend/OpenDMSBackend/Services/BusinessLogicService.cs` (`UserHasPermissionInHierarchy`)
- Attack-surface: internet-exposed, every authenticated request which touches content
- Evidence: the method walks the containment-hierarchy in a `while (true)`-loop and leaves it only when it reaches a
  storage-location. There is no depth-limit and no detection of a cycle, and nothing in the data-model prevents a cycle (`Move`
  checks the permission on both ends, but no check that the target is not a descendant of the moved object was found).
- Impact: a containment-relation which contains a cycle makes every request which touches the affected object hang forever and
  occupy a thread and a database-connection, until the whole api stops answering.
- Recommendation: limit the depth, detect a repeated id and fail with a clear error, and refuse a `Move` which would move a
  container into one of its own descendants.

**OWASP-36 - Reachable operations are not implemented and answer with an internal error** (Medium, confirmed)

- Affected component: `OpenDMSBackend/OpenDMSBackend/Services/PersistentAuthenticationService.cs` (ten places),
  `.../Services/DatabasePersistence.cs` (six places), `.../Services/BusinessLogicService.cs` (`Housekeeping`)
- Attack-surface: internet-exposed, depending on the operation
- Evidence: seventeen members of implemented interfaces throw a `NotImplementedException`, among them `Logout(ClaimsPrincipal)`,
  `GetPrincipal`, `AccessTokenExists`, `UpdateUser` and `GetAllUser`.
- Impact: an interface promises an operation which fails at runtime. Whether a given one is reachable from outside depends on
  which path of `GRYLibrary` calls it, so the product relies on a caller never taking that path - which an upgrade of the
  library can change silently.
- Recommendation: implement the members which belong to the used paths, and let the ones which are deliberately unsupported throw
  a `NotSupportedException` with a reason, so that the distinction between "missing" and "not applicable" is visible.

**OWASP-37 - A failed initialization does not stop the server** (Medium, confirmed)

- Affected component: `OpenDMSBackend/OpenDMSBackend/Services/InitializationService.cs`
- Attack-surface: internet-exposed, at every start
- Evidence: `Initialize` catches every exception, logs it and sets the state to `InitializationFailed`; the web-application keeps
  running and keeps accepting requests. The database-migration and the creation of the administrator are part of exactly that
  method.
- Impact: the server answers requests in a state in which the migration may have run only halfway. Whether the following
  requests fail cleanly or work on an inconsistent schema depends on where the initialization broke off.
- Recommendation: let a failed initialization end the process (or make every business-route answer with a clear
  service-unavailable as long as the state is not `Initialized`), so that an orchestrator can restart the container instead of
  keeping a half-initialized one alive.

**OWASP-38 - An unknown oidc-provider produces an internal error** (Low, confirmed)

- Affected component: `OpenDMSBackend/OpenDMSBackend/Services/OIDCLoginService.cs` (`GetProviderConfig`)
- Attack-surface: internet-exposed, unauthenticated
- Evidence: the method throws a `KeyNotFoundException`, while `OIDCController.InitiateOIDCLogin` documents a
  `404 Not Found`. `ParseNullableDateTime` likewise lets a `FormatException` escape when a script returns an unparseable date.
- Impact: a wrong input is answered as an internal error instead of as a client-error, which hides real errors in the monitoring
  and can reveal internal detail depending on the exception-middleware.
- Recommendation: throw the exception-type which maps to the documented status-code, and validate the value before parsing it.

Two further points of this category concern the behaviour of `GRYLibrary` and were verified there afterwards (version 2.1.3; the
result is recorded in the readme of that repository). The first one is harmless: `DefaultExceptionHandlerMiddleware` answers with
an empty body in every environment, so no stack-trace and no exception-message reaches the client. The second one is not:
authentication is required only for a route whose action carries `[Authenticate]` or `[Authorize]`, so the authentication-
middleware does not fail closed for a route which carries neither - every endpoint of this product has one of the two attributes,
so nothing is open today, but the protection rests on an annotation which nothing enforces. A third point was found while
verifying those two: a security-decision of the library is signalled as an exception and is only turned into a status-code by the
exception-middleware, so the 401 and the 403 of this product depend on that middleware staying configured.

### Validation and test-coverage

The backend has 172 testcases, and sixteen of them assert that an operation is refused for a user who lacks the permission - the
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

### Summary of all findings

| Id | OWASP-category | Affected component | Criticality | Confidence | Status |
| --- | --- | --- | --- | --- | --- |
| OWASP-13 | A04 Cryptographic Failures | `PersistentAuthenticationService.Hash` | Critical | confirmed | open |
| OWASP-25 | A07 Authentication Failures | `InitializationService` | Critical | confirmed | open |
| OWASP-03 | A02 Security Misconfiguration | `Dockerfile`, `nginx.conf`, `GeneralConstants` | High | confirmed | open |
| OWASP-04 | A02 Security Misconfiguration | `Dockerfile` | High | confirmed | open |
| OWASP-05 | A02 Security Misconfiguration | `nginx.conf` | High | confirmed | open |
| OWASP-16 | A05 Injection | `edit-document-menu.component.ts`, `BusinessLogicService` | High | confirmed | open |
| OWASP-19 | A06 Insecure Design | `BusinessLogicService.IsValid`, `nginx.conf` | High | confirmed | open |
| OWASP-20 | A06 Insecure Design | `UserController`, `BusinessLogicService` | High | confirmed | open |
| OWASP-26 | A07 Authentication Failures | `UserController.Login`, `Program` | High | confirmed | open |
| OWASP-29 | A08 Software or Data Integrity Failures | audit-log-configuration in `Program` | High | confirmed | open (issue 6) |
| OWASP-30 | A08 Software or Data Integrity Failures | `Document`, `DatabasePersistence` | High | confirmed | open (issue 22) |
| OWASP-32 | A09 Security Logging and Alerting Failures | `PersistentAuthenticationService.Login` | High | confirmed | open |
| OWASP-33 | A09 Security Logging and Alerting Failures | `ManagementService` | High | confirmed | open |
| OWASP-34 | A09 Security Logging and Alerting Failures | request-logging-configuration in `Program`, `UserController.Login` | High | confirmed | open |
| OWASP-01 | A01 Broken Access Control | `PersistentAuthenticationService`, `BusinessLogicService` | Medium | confirmed | open |
| OWASP-06 | A02 Security Misconfiguration | `EntryPoint.sh` | Medium | confirmed | open |
| OWASP-07 | A02 Security Misconfiguration | `Dockerfile` | Medium | confirmed | open |
| OWASP-08 | A02 Security Misconfiguration | example-`docker-compose.yml` | Medium | confirmed | open |
| OWASP-10 | A03 Software Supply Chain Failures | `Dockerfile` | Medium | confirmed | open |
| OWASP-11 | A03 Software Supply Chain Failures | `ImageDefinition.csv` | Medium | confirmed | open |
| OWASP-14 | A04 Cryptographic Failures | `AISummaryServiceClient`, `OCRServiceClient` | Medium | confirmed | open |
| OWASP-21 | A06 Insecure Design | `BusinessLogicService.Search`, `.GetLatestDocuments` | Medium | confirmed | open |
| OWASP-22 | A06 Insecure Design | `OIDCLoginService` | Medium | confirmed | open |
| OWASP-23 | A06 Insecure Design | `ManagementService.RunAdaptScript` | Medium | confirmed | open |
| OWASP-27 | A07 Authentication Failures | `storage.service.ts` | Medium | confirmed | open |
| OWASP-35 | A10 Mishandling of Exceptional Conditions | `BusinessLogicService.UserHasPermissionInHierarchy` | Medium | confirmed | open |
| OWASP-36 | A10 Mishandling of Exceptional Conditions | `PersistentAuthenticationService`, `DatabasePersistence`, `BusinessLogicService` | Medium | confirmed | open |
| OWASP-37 | A10 Mishandling of Exceptional Conditions | `InitializationService` | Medium | confirmed | open |
| OWASP-02 | A01 Broken Access Control | `UserController.TokenIsValid` | Low | confirmed | open |
| OWASP-09 | A02 Security Misconfiguration | `HealthCheck`, `Program` | Low | confirmed | open |
| OWASP-12 | A03 Software Supply Chain Failures | `OpenDMSBackend.csproj` | Low | confirmed | open |
| OWASP-15 | A04 Cryptographic Failures | `PersistentAuthenticationService`, `OIDCLoginService` | Low | confirmed | open |
| OWASP-17 | A05 Injection | `EntryPoint.sh` | Low | confirmed | open |
| OWASP-18 | A05 Injection | `ManagementService.RunTSC` | Low | confirmed | open |
| OWASP-24 | A06 Insecure Design | `BusinessLogicService.SetRolesOfUser` | Low | confirmed | open |
| OWASP-28 | A07 Authentication Failures | `PersistentAuthenticationService.Login` | Low | confirmed | open |
| OWASP-31 | A08 Software or Data Integrity Failures | `OpenDMSBackend.csproj` | Low | confirmed | open |
| OWASP-38 | A10 Mishandling of Exceptional Conditions | `OIDCLoginService.GetProviderConfig` | Low | confirmed | open |

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
