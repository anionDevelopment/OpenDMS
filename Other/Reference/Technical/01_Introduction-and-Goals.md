# 1. Introduction and Goals

## Overview

OpenDMS is an open-source document-management-system which its user hosts on their own infrastructure.
It stores documents unchanged in their original format, organizes them in storage-locations and folders, keeps a version-chain per
document, indexes them with tags and with the custom metadata-fields of their storage-location, offers a search over them and
records every change in an audit-log.

## Quality goals

| Quality-goal | Meaning for the architecture |
| ------------ | ---------------------------- |
| Traceability | Every change-operation is recorded in the audit-log together with its initiator and the affected object, and a document is never overwritten: a new version is stored as an own document and the previous one stays retrievable. This is what makes the product usable under the GoBD (see [the solution-strategy](./04_Solution-Strategy.md)). |
| Confidentiality | Nobody may retrieve or change content without an explicit permission for it, not even an administrator. Every business-operation verifies the permission of its caller itself instead of relying on the user-interface to hide something. |
| Operability on premises | The product is deployable by a single administrator on their own infrastructure as one container, without a cloud-service and without a component which has to be reached over the internet (see [the deployment-view](./07_Deployment-View.md)). |
| Good performance | See [the quality-requirements](./10_Quality-Requirements.md). |
| No vulnerabilities | See [the quality-requirements](./10_Quality-Requirements.md). |
