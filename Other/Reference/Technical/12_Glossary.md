# 12. Glossary

## Terms

The terms of this table are the ones the source-code uses. They are listed here because their meaning in OpenDMS is
narrower than the everyday meaning of the word.

| Term | Meaning |
| ---- | ------- |
| Content | Anything which OpenDMS stores and protects by permissions: a storage-location, a folder or a document. |
| Storage-location | The root of a containment-hierarchy. A storage-location contains folders and documents, defines the custom metadata-fields which its documents can hold and is the level at which permissions are granted. |
| Folder | A container inside a storage-location which contains further folders and documents. |
| Containee | A content-object which is contained in a container, which is a folder or a document. |
| Container | A content-object which contains containees, which is a storage-location or a folder. |
| Document | A stored file together with the data which OpenDMS holds about it (title, filename, original filename, MIME-type, import-date, OCR-content, preview, tags, metadata-values and retention-dates). |
| Version | A document which replaces an earlier document. The earlier one stays stored unchanged, is hidden from the normal listings and stays retrievable through the version-history. Only the newest version of a chain is the latest version. |
| Readable id | A sequential number per document which is shown to the user, in contrast to the technical id, which is a GUID. |
| Tag | A named and colored label which can be assigned to arbitrarily many documents. A tag is either global or belongs to a single user. |
| Metadata-field | A field which a moderator defines for a storage-location (with a name and one of the types `String`, `Boolean`, `Double` and `Timestamp`) and for which every document of that storage-location can optionally hold one value. |
| Moderator | A user which has all permissions on a content-object and on its contents and which manages the permissions of it. Called "owner" in parts of the persistence. |
| Administrator | A user with the global administrator-role. An administrator manages users and their roles but is not allowed to see or change content without a permission for it. |
| Soft-delete | Marking a document as deleted without removing anything. It can be undone. |
| Hard-delete | Removing the content of a document irreversibly. The audit-log keeps the trace of it. |
| Lock-up period | The period until `DeleteIsNotAllowedBefore`, during which a document must not be hard-deleted. |
| Delete-deadline | The date `MustBeHardDeletedAfter`, after which OpenDMS hard-deletes the document automatically. |
| Import-definition | A configured folder of the file-system from which documents are pulled into a storage-location, optionally with an adapt-script which sets properties of the imported document. |
| Audit-log | The log which records every change-operation together with its initiator and the affected object. |
| Transient persistence | An in-memory persistence which is used for developing, debugging and the quick test instead of a database. Its content is lost when the application ends. |

## Abbreviations

| Abbreviation | Meaning |
| ------------ | ------- |
| DMS | document-management-system |
| GoBD | "Grundsätze zur ordnungsmäßigen Führung und Aufbewahrung von Büchern, Aufzeichnungen und Unterlagen in elektronischer Form sowie zum Datenzugriff", the German regulation this product aims to conform to |
| OCR | optical character recognition |
| OIDC | OpenID Connect |
