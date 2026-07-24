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
