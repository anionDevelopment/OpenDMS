# Proposal

## Why

A user of OpenDMS wants to work with their documents through an AI-agent (for example Claude Code, opencode, GitHub Copilot,
Codex or Mistral Vibe) instead of only through the web-UI. The Model Context Protocol (MCP) is the common standard which all
of these agents support, so OpenDMS needs an MCP-server which exposes its existing functionality as MCP-tools - acting
strictly with the identity and the permissions of the user who uses the agent.

## What Changes

- The backend-codeunit `OpenDMSBackend` hosts an MCP-server (Streamable-HTTP-transport, stateless) on its own route below the
  versioned api-prefix. No new codeunit is created.
- The MCP-endpoint is protected by the existing authentication- and authorization-middlewares: the agent sends the existing
  OpenDMS-access-token of the user (local token or OIDC-token) in the existing credential-header. Every tool-call is executed
  as that user.
- Two MCP-tools are provided as a minimal first set:
  - `search_documents`: delegates to the REST-endpoint `Search`.
  - `get_document`: delegates to the REST-endpoint `GetDocument` and returns the metadata and the ai-summaries as text and the
    original file as embedded resource (bounded by a configurable maximum size).
- A tool never implements domain-logic or permission-checks itself: it delegates in-process to the controller-action of the
  corresponding REST-endpoint, and the role-requirement (`[Authorize]`) of that action is enforced for the tool-call as well.
- Adding a further tool for another REST-endpoint requires only one new tool-class which declares the delegation; no
  change of the hosting, the authentication or the delegation-mechanism.
- The request-logging does not log response-bodies of the MCP-route, because they contain document-contents.
- The MCP-route and the agent-configuration are documented in the reference of the backend-codeunit with one
  agent-neutral configuration (url plus header), without agent-specific code.
- Prerequisite outside this repository: `GRYLibrary` has to evaluate `[Authenticate]`/`[Authorize]` also from the
  endpoint-metadata of a non-controller-endpoint. Without that, the MCP-endpoint would be reachable unauthenticated.
- Out of scope (recorded as finding `OTH-2`): OAuth-based MCP-authorization according to RFC 9728 and an admin-setting which
  switches the two authentication-variants on or off at runtime.

## Capabilities

### New Capabilities

- `mcp-server`: The MCP-interface of the backend: transport, authentication with the identity of the user, the rule that
  every tool delegates to an existing REST-endpoint, the provided tools and their results.

### Modified Capabilities

None. The existing requirement "Domain-logic lives only in the business-logic-service" (`architecture`) is respected
unchanged: a tool is an adapter like a controller.

## Impact

- Code: `OpenDMSBackend/OpenDMSBackend/Program.cs` (registration and mapping of the MCP-server, logging-configuration), new
  folder `OpenDMSBackend/OpenDMSBackend/MCP` (delegation-mechanism and tool-classes),
  `Configuration/CodeUnitSpecificConfiguration.cs` (maximum size of an embedded document), new testcases in
  `OpenDMSBackendTests`.
- Dependencies: new nuget-package `ModelContextProtocol.AspNetCore`, pinned to `[2.3.1]`; new `GRYLibrary`-version which
  contains the fix described above.
- API: new route `/API/v{major}/MCP`. The existing REST-api is unchanged. The generated frontend-client is not affected,
  because the MCP-route is not part of the api-specification.
- Documentation: new reference-article in `OpenDMSBackend/Other/Reference/ReferenceContent/Articles`, feature-list in
  `Other/Reference/Reference.md`.
