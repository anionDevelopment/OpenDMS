# OpenDMS

OpenDMS is an open-source document-management-system which is hosted by its user on their own infrastructure.
It stores documents unchanged in their original format, organizes them in storage-locations and folders, keeps a version-chain
per document, indexes them with tags and with the custom metadata-fields of their storage-location, offers a search over them and
records every change in an audit-log. It aims to fulfil the requirements which the German GoBD puts on such a system, for example
the traceability of every change, the retention-periods and the regulated deletion after they expired.

The product is delivered as one OCI-image which contains the web-frontend and the backend, and it is operated behind a
reverse-proxy which terminates TLS. The documents are stored in a PostgreSQL- or a MariaDB-database.

## Technical documentation

- [1. Introduction and Goals](./Technical/01_Introduction-and-Goals.md)
- [2. Constraints](./Technical/02_Constraints.md)
- [3. Context and Scope](./Technical/03_Context-and-Scope.md)
- [4. Solution Strategy](./Technical/04_Solution-Strategy.md)
- [5. Building Block View](./Technical/05_Building-Block-View.md)
- [6. Runtime View](./Technical/06_Runtime-View.md)
- [7. Deployment View](./Technical/07_Deployment-View.md)
- [8. Crosscutting Concepts](./Technical/08_Crosscutting-Concepts.md)
- [9. Architectural Decisions](./Technical/09_Architectural-Decisions.md)
- [10. Quality Requirements](./Technical/10_Quality-Requirements.md)
- [11. Risks and Technical Debt](./Technical/11_Risks-and-Technical-Debt.md)
- [12. Glossary](./Technical/12_Glossary.md)

## Responsibilities

| Responsibility  | Name and contact-information |
| --------------- | ---------------------------- |
| Product-owner   | Marius Göcke                         |
| Product-manager | Marius Göcke                 |
| Support         | Bugs and feature-requests: Use GitHub-issues<br>Support for issues which are not in the responsibility of the developer: Support-contracts on demand |

## License & Pricing

See [License.txt](https://github.com/anionDev/OpenDMS/blob/main/License.txt).
(For pricing of support-contracts please contact the product-owner.)

## External resources

- [Repository](https://github.com/anionDev/OpenDMS)
