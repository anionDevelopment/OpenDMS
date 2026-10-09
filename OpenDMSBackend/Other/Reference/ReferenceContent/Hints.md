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

Start the project using the usual debug-button in Visual Studio.
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

## Indexing a document: tags and custom metadata-fields

A document is indexed (see issue #14) by two mechanisms which are deliberately kept apart, because they answer two different questions.

### Tags

A tag is a named and colored label which exists on its own and is usable in every storage-location, so it answers "which documents belong together".
A tag is either a global tag, which every user can see and use and which only an administrator may create, or it belongs to a single user (`Tag.OwnerUserId`), who is then the only one who can see and use it.
Every authenticated user may therefore create a tag of their own, while creating a global one requires the administrator-role.
The name of a tag must not be empty and must not be used by another tag which is visible for the same user yet (compared case-insensitively), because two tags with the same name can not be distinguished in the user-interface.
A tag can be renamed, be given another color and be deleted; deleting it removes it from every document it is assigned to.
Changing or deleting a tag is allowed for an administrator in case of a global tag and for the owner in case of a tag which belongs to a user.
Assigning a tag to a document and removing the assignment again requires the permission to change that document.
Assigning a tag which is already assigned, and removing one which is not assigned, are both rejected instead of being silently ignored, so that a caller notices when it works on an outdated state.

### Custom metadata-fields

A custom metadata-field is defined per storage-location, so it answers "which values does this kind of document carry".
Only a moderator of a storage-location may define a field (name + type `String`, `Boolean`, `Double` or `Timestamp`), and every document contained in that storage-location (directly or in one of its folders) can optionally hold a value for each field.
This covers the fields which issue #14 names as examples, for example the document-type and the contact/sender.
A value is validated against the type of its field and is stored in a normalized representation (for example a boolean-value in its lower-case form `true`/`false`), so that every reader gets the same format back regardless of the culture of the writer.
Holding no value is a different state than holding an empty text: clearing a value removes the association instead of storing an empty string.
The retention-date is deliberately not a metadata-field: it is held by `Document.DeleteIsNotAllowedBefore` and `Document.MustBeHardDeletedAfter` (see issue #9), so that there is exactly one place which decides when a document may and must be deleted.

### Common properties of both

The assignment of a tag and the value of a metadata-field are direct per-document-row associations and are not versioned on their own; a metadata-only new version (for example a title-change) copies both forward.
Every change of a tag-assignment and of a metadata-value writes an audit-log-entry, as every other change-operation does.
Both are part of the search: `Search` finds a document by the name of a tag which is assigned to it and by the values it holds for the metadata-fields of its storage-location, besides its title, its filenames and its ocr-content. What is still missing is a search which is restricted to a single field and a machine-evaluable export of the result, which belongs to issue #15.

## Known defects which require a larger change

The defects of this codeunit which were found by a code-review and which require a bigger change than a local correction are listed as `Backend-01` to `Backend-05` in the [findings of the product reference](https://github.com/anionDev/OpenDMS/blob/main/Other/Reference/Reference.md#findings), together with their current state.
