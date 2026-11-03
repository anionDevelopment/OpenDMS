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

See the [feature-list in the product reference](./Other/Reference/Reference.md#features).

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

The findings of this category (SEC-1 and SEC-2) and their current state are listed in the [findings of the product reference](./Other/Reference/Reference.md#findings).

### A02:2025 Security Misconfiguration

The findings of this category (SEC-3, SEC-4, SEC-5, SEC-6, SEC-7, SEC-8 and SEC-9) and their current state are listed in the [findings of the product reference](./Other/Reference/Reference.md#findings).

### A03:2025 Software Supply Chain Failures

The positive part first: all nuget-dependencies are pinned to an exact version with the bracket-notation (`[2.1.0]`), all
npm-dependencies are pinned exactly, both code-units keep a lock-file (`RestorePackagesWithLockFile`), `nuget.config` clears the
machine-sources so that the build does not depend on the developer-machine, and the pipeline-images for the generation of a
bill-of-materials (syft) and for a secret-scan (betterleaks) are defined in `.ScriptCollection/OCIImages/ImageDefinition.csv`.

The findings of this category (SEC-10, SEC-11 and SEC-12) and their current state are listed in the [findings of the product reference](./Other/Reference/Reference.md#findings).

### A04:2025 Cryptographic Failures

The findings of this category (SEC-13, SEC-14 and SEC-15) and their current state are listed in the [findings of the product reference](./Other/Reference/Reference.md#findings).

In addition, document-contents are stored unencrypted in the database. That is a legitimate decision for a document-management-
system which has to search them, but it means the protection of the data rests entirely on the database and on its backups, which
belongs into the procedure-documentation (issue 23).

### A05:2025 Injection

No sql-injection was found. Every statement is an embedded `.sql`-resource which is executed with bound parameters
(`GetParameter(...)`); the one place which assembles sql dynamically (`DatabasePersistence.UpdateRole`, which replaces the marker
`__generated__`) only generates parameter-names and no values. The frontend contains no `innerHTML`, no `eval` and no
`bypassSecurityTrust*`-call, so the output-escaping of angular is in place everywhere.

The findings of this category (SEC-16, SEC-17 and SEC-18) and their current state are listed in the [findings of the product reference](./Other/Reference/Reference.md#findings).

The generation of the adapt-script itself was checked as well: `ToTSStringLiteral` escapes backslash, double-quote and both
line-breaks before a value (among them the attacker-controllable ocr-content) is written into the generated typescript, which is
complete for a double-quoted literal of the targeted language-version. No script-injection was found there.

### A06:2025 Insecure Design

The findings of this category (SEC-19, SEC-20, SEC-21, SEC-22, SEC-23 and SEC-24) and their current state are listed in the [findings of the product reference](./Other/Reference/Reference.md#findings).

### A07:2025 Authentication Failures

The findings of this category (SEC-25, SEC-26, SEC-27 and SEC-28) and their current state are listed in the [findings of the product reference](./Other/Reference/Reference.md#findings).

A token is valid for exactly 24 hours, is not rotated and has no idle-timeout; that is a design-decision rather than a defect,
but it enlarges the window of SEC-1 and should be reconsidered together with it.

### A08:2025 Software or Data Integrity Failures

This is the category in which the product itself already names most of the gaps: the feature-list names the tamper-proof
audit-log (issue 6), the immutability of an archived document (issue 4), the integrity-protection of the stored content
(issue 22) and the gap-free numbering (issue 8) as not implemented. They are repeated here because they are security-findings and not only
gobd-findings.

The findings of this category (SEC-29, SEC-30 and SEC-31) and their current state are listed in the [findings of the product reference](./Other/Reference/Reference.md#findings).

### A09:2025 Security Logging and Alerting Failures

The product has a real audit-log for its business-operations, which covers the change-side well: adding, changing, moving,
renaming, sharing, tag- and metadata-changes and both kinds of deletion each write an entry with the acting user. What is missing
is the security-side of logging.

The findings of this category (SEC-32, SEC-33 and SEC-34) and their current state are listed in the [findings of the product reference](./Other/Reference/Reference.md#findings).

### A10:2025 Mishandling of Exceptional Conditions

The findings of this category (SEC-35, BUG-1, BUG-2 and BUG-3) and their current state are listed in the [findings of the product reference](./Other/Reference/Reference.md#findings).

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

1. Replace the password-hashing with a salted memory-hard algorithm and re-hash on the next login (SEC-13), and remove the
   default-password `admin` (SEC-25). Both are reachable without any precondition and compromise the whole installation.
2. Close the stored-cross-site-scripting: deliver a document as a download instead of as its stored mime-type, and add an
   allowlist of accepted types (SEC-16, SEC-19). Add the content-security-policy in the same step (SEC-5).
3. Build a productive image without the development-certificate and let it run as a non-root user (SEC-3, SEC-4).
4. Add a rate-limit and a lockout to the login and log every failed attempt (SEC-26, SEC-32).
5. Add the missing password-management (change and administrative reset) and make a token revocable (SEC-20, SEC-1).
6. Replace every swallowed exception of the background-service by a logged one, and exclude the authentication-routes from the
   request-logging so that no access-token is written to a log-file (SEC-33, SEC-34).
7. Harden the deployment-surface: secrets through the environment instead of the commandline, a slimmer runtime-image, and an
   example-deployment which publishes nothing unnecessary (SEC-6, SEC-7, SEC-8, SEC-17).
8. Make the data trustworthy: hash-chain the audit-log and store a checksum per document (SEC-29, SEC-30), which are the
   issues 6 and 22 of the feature-list.
9. Work off the robustness-findings (SEC-21, SEC-22, SEC-23, SEC-35, BUG-1, BUG-2) and the remaining
   supply-chain- and crypto-findings (SEC-10, SEC-11, SEC-12, SEC-14, SEC-15, SEC-31).
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
