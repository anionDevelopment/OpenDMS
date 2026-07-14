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
