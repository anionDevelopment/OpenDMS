# Hints

## Requirements

The following tools from the [tools-list](https://github.com/anionDev/ScriptCollection/blob/main/ScriptCollection/Other/Reference/ReferenceContent/Articles/RequirementsForCommonProjectStructure.md#Tools) are required to build this code-unit:

- `cyclonedx-npm`
- `docfx`
- `git`
- `gitversion`
- `ng`
- `npm`
- `openapi-generator-cli`
- `python`
- `reportgenerator`
- `scriptcollection`

## IDE

The recommended IDE for this codeunit is [Visual Studio Code](https://code.visualstudio.com/).

## Generated API-client

The folder `src/app/generated/open-dms-backend` is an auto-generated `typescript-angular`-client for the OpenDMSBackend-API and must not be edited manually (changes are overwritten on the next regeneration).
It is generated from the backend's OpenAPI-specification: run `Build.py` in the backend-codeunit (this produces the spec) and then `CommonTasks.py` in this frontend-codeunit (this regenerates the client via `openapi-generator-cli`).

Caveat when regenerating locally: the backend's spec-generation uses `swagger tofile` against the built backend-assembly.
This step relies on the build-image (`SCBuilder`) and does not run against the GRYLibrary-APIServer-host in a plain local `dotnet build` in the `Development`-environment (the host validates its full service-graph on build, and Swashbuckle's host-resolver falls back to looking for a `Startup`-type).
So an up-to-date client should be produced by the regular build (or in the build-image); after a backend-API-change the client has to be regenerated rather than the endpoint-methods being written by hand.

## Known defects which require a larger change

The following defects were found by a pure code-review (independent of the business-logic). They are not fixed yet because fixing them requires a bigger change than a local correction.

- The permission-dependent parts of the UI are permanently enabled: `EditContainerMenuComponent.userIsAllowedToAddDocument`, `EditContainerMenuComponent.userIsAllowedToAddFolder` and `MetadataFieldsComponent.userIsModerator` are hard-coded to `true` (each with a `TODO`). Every user therefore sees actions they may not be allowed to perform; the backend rejects them, but only after the user triggered them. Showing them correctly requires an endpoint which reports the caller's permissions for a content-object and wiring it through the container- and storage-location-views.
- `ContentViewComponent` loads the containee-ids it gets from `StorageLocationDTO`/`FolderDTO` with one request per document and per folder. For a container with many entries this produces a request-storm; a batch-endpoint (previews of all documents of a container) would be needed instead.
- `ContentViewComponent.addDocument` is typed as taking a `DocumentDTO` but is also called with a `DocumentPreviewDTO` (from `loadDocument`), and the component's list is a `DocumentPreviewDTO[]`. This only compiles because every property of the generated DTOs is optional. Cleaning this up means separating "a document was added" (which yields an id) from "a preview was loaded" throughout the container-view.
