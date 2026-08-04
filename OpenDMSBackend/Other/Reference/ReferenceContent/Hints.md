# Hints

## Requirements

The following tools from the [tools-list](https://github.com/anionDev/ScriptCollection/blob/main/ScriptCollection/Other/Reference/ReferenceContent/Articles/RequirementsForCommonProjectStructure.md#Tools) are required to build this code-unit:

- `docfx`
- `docker`
- `dotnet-coverage`
- `git`
- `gitversion`
- `java`
- `python`
- `reportgenerator`
- `scriptcollection`
- `swagger`

## IDE

The recommended IDE for this codeunit is [Visual Studio](https://visualstudio.com/).

Start the project using the usual usual debug-button in Visual Studio.
When you start the backend the first time then a configuration-file will be generated which will be used.
This configuration-file is located in `<repository-root>\OpenDMSBackend\Other\Workspace\Configuration\Configuration.xml`.
This configuration-file will always be used if available when running the backend locally.
If a debugger is attached (which is typically the case when developing and starting the backend in the IDE) when the configuration-file is generated then an internal transient database-storage will be used instead of a persistent database.
So to develop/debug the backend you do not have to run a local database, but you can if you want.
If you want to use a real database when debugging then you have to configure it in `Configuration.xml`. by adjusting the `DatabasePersistenceConfiguration`-section accordingly:
In this `DatabasePersistenceConfiguration`-section you can edit the `DatabaseType`-value and the `DatabaseConnectionString`-value.
For running transient set `DatabaseType` to `Transient`.
(In this case `DatabasePersistenceConfiguration` is not used obviously.)
Apart from the transient-mode the supported values for `DatabaseType` are:

- `PostgreSQL`
- `MariaDB`

To reset all your local backend-configuration-values etc. to a plain state you can simply remove the entire `<repository-root>\OpenDMSBackend\Other\Workspace`-folder.

## Custom metadata-fields

Custom metadata-fields (see issue #2) are defined per storage-location: only a moderator of a storage-location may define a field (name + type `String` or `Boolean`), and every document contained in that storage-location (directly or in one of its folders) can optionally hold a value for each field.
The value a document holds is a direct per-document-row association (analogous to tags) and is not versioned on its own; a metadata-only new version (for example a title-change) copies the values forward.
A boolean-value is validated and stored in its normalized lower-case form (`true`/`false`).
The types `double` and `timestamp` mentioned in issue #2 are not supported yet.

## Known defects which require a larger change

The following defects were found by a pure code-review (independent of the business-logic). They are not fixed yet because fixing them requires a bigger change than a local correction.

- `BusinessLogicService.Move` does not detach the moved containee from its previous parent (there is a `TODO` for it). `IPersistence.SetParentOfContainee` only inserts a row into `Container_Containee`, so after a move the containee is contained in both the old and the new container. Fixing this properly means giving `SetParentOfContainee` a "replace the existing parent"-semantic (or letting `Move` call `RemoveChild` first) and reworking the corresponding SQL-statement, which affects every caller (add-document, add-folder, new-version, metadata-version).
- `BusinessLogicService.HardDelete` wraps its whole body in a `try`/`catch` which only logs the exception. As a result a failed hard-deletion is reported to the caller as success. Reporting the failure requires deciding which errors are expected (for example hard-deleting a container, which `DatabasePersistence.HardDelete` rejects with `NotImplementedException`) and how the scheduled housekeeping-run should continue after a single failure.
- Soft-deleted documents are still contained in the containee-lists of `StorageLocationDTO`/`FolderDTO`, so they keep appearing in the container-view of the web-UI although they are excluded from the search and from the latest-documents-list. A consistent behaviour requires the soft-delete-filter to be applied when the content of a container is loaded (`GetScriptGetContentOfContainer` and `TransientPersistence`), plus a decision on how a deleted document can still be reached (recycle-bin).
- `IPersistence.GetLatestReadableId` is documented as "the highest readable id that has been issued so far", but both implementations return the *amount* of documents. This only coincides with the highest issued id as long as every `Documents`-row was created with exactly one generated id and no row is ever removed. It is used to re-seed the `IdGenerator` on startup, so a divergence produces duplicate readable ids. A correct implementation needs a `max("ReadableId")`-query (and a corresponding statement per database-type).
- `DatabasePersistence` reads the `ReadableId`-column (`BIGINT`) via `reader.GetInt32(...)` and the model uses `ulong`, while `IPersistence.GetIdFromReadableId` and the corresponding API-endpoint use `uint`. Making the readable id consistently 64-bit affects the persistence, the business-logic-interface, the controller and therefore the generated frontend-client.
