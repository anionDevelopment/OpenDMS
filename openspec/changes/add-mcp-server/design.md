# Design

## Context

See `proposal.md` for the motivation and `specs/mcp-server/spec.md` for the required behavior. The following facts of the
current code shape the approach (verified in the source-code):

- The backend is hosted by `GRYLibrary` (`Tools.RunAPIServer`, version `[2.1.9]`). Services are registered in
  `Program.SetFunctionalInformationAction`, the web-application is configured in `Program.ConfigureWebApplication`.
- Authentication is done by `AuthSMiddleware`: it reads the credential-header (`X-AccessToken`, `HeaderTools.HeaderName`)
  through `ICredentialsProvider` (`HeaderService`) and accepts local access-tokens and OIDC-access-tokens. Authorization is
  done by `AutSRMiddleware` with `IRoleBasedAuthorizationService` against the groups of `[Authorize(...)]`.
- Both middlewares decide whether a route is protected only through `AbstractMiddleware.TryGetAttributeFromContext`, which
  reads `[Authenticate]`/`[Authorize]` exclusively from the `ControllerActionDescriptor` of the endpoint. A route which is
  not a controller-action (for example one mapped with `MapMcp`) is therefore treated as unprotected. The middleware does not
  fail closed (this is already stated in the readme, section A10).
- The REST-endpoints `OpenDMSBackendController.Search(searchTerm)` and `OpenDMSBackendController.GetDocument(id)` are thin:
  they resolve the user with `Tools.GetUser(this.User, ...)`, call `IBusinessLogicService` with the user-id (which performs
  the permission-checks) and map the result to `DocumentPreviewDTO[]` respectively `DocumentDTO`.
- `DocumentDTO` contains the content as base64 and the ai-summaries, but not the ocr-text. `DocumentPreviewDTO` contains a
  preview-image as base64 and no tags.
- The business-logic signals refusals with `NotAuthorizedException` and `BadRequestException` of `GRYLibrary`.
- `DRequestLoggingMiddleware` logs request- and response-bodies up to 500 characters (see finding SEC-34).

## Goals / Non-Goals

**Goals:**

- One delegation-mechanism which every tool uses, so that a further tool is one small class.
- The permission-model of the REST-api applies unchanged to the MCP-interface, enforced by the same code.
- Only standard MCP-mechanisms, so that every common agent works with the same configuration.

**Non-Goals:**

- OAuth-based MCP-authorization (RFC 9728 protected-resource-metadata, `WWW-Authenticate` with `resource_metadata`) and an
  admin-setting to switch authentication-variants at runtime. This is recorded as finding `OTH-2` and is a later change.
- Long-lived or revocable personal access-tokens for agents (see SEC-1).
- MCP-resources, MCP-prompts, sampling, elicitation and server-to-client notifications. Only tools are provided.
- Exposing the ocr-text: the REST-endpoint `GetDocument` does not return it, and a tool returns nothing which its endpoint
  does not return. If it is wanted, the REST-endpoint is extended first and the tool shows it without further change.

## Decisions

### D1: Official C#-SDK instead of an own protocol-implementation

Use `ModelContextProtocol.AspNetCore` pinned to `[2.3.1]` (`AddMcpServer().WithHttpTransport(...)` and `MapMcp(route)`).
The SDK implements the protocol-versions, the json-rpc-framing, the input-schema-generation from method-signatures and the
Streamable-HTTP-transport which every listed agent supports.

Alternative: implement json-rpc and the transport by hand in a controller. Rejected: large surface, every protocol-revision
would have to be followed manually, and that is exactly the kind of code which produces agent-specific incompatibilities.

### D2: Stateless Streamable-HTTP-transport

`HttpServerTransportOptions.Stateless = true`. Every request carries its credentials and is processed independently.

Rationale: no session-affinity behind a reverse-proxy, no in-memory session-store (compare SEC-22), and the `HttpContext` of
the tool-call is the `HttpContext` of the authenticated request, so the user of the call is unambiguous. Server-to-client
messages are not needed for the two tools.

Alternative: stateful sessions. Rejected: they would bind the user of a tool-call to the request which opened the session
instead of the request which carries the call.

The legacy SSE-transport is not enabled, because all listed agents support Streamable-HTTP.

### D3: Protection of the MCP-route by the existing middlewares, with a fix in GRYLibrary

The MCP-route is mapped with endpoint-metadata: `MapMcp(route).WithMetadata(new AuthenticateAttribute(), new AuthorizeAttribute(CodeUnitSpecificConstants.RolenameUsers))`.
For that to be effective, `GRYLibrary` has to be fixed: `AbstractMiddleware.TryGetAttributeFromContext` has to read the
attribute from `endpoint.Metadata.GetMetadata<T>()`. ASP.NET adds the attributes of a controller-action to the
endpoint-metadata as well, so this one lookup covers controller-actions and mapped endpoints, and the behavior of the
existing routes is unchanged.

How to fix it in `GRYLibrary` (`GRYLibrary/APIServer/MidT/AbstractMiddleware.cs`): replace the lookup through
`ControllerActionDescriptor.MethodInfo.GetCustomAttributes` by `endPoint.Metadata.GetMetadata<T>()`, add a testcase which
maps a minimal-api-endpoint with `WithMetadata(new AuthenticateAttribute())` and asserts that a request without token gets
`401`, release a new version and update the pinned version in `OpenDMSBackend.csproj`.

Alternative: put the MCP-endpoint behind an own controller-action which forwards to the SDK. Rejected: the SDK owns the
request-handling of its route, forwarding would re-implement the transport, and it would be a workaround in this repository
for a defect of `GRYLibrary`. Alternative: an own authentication-check inside the MCP-pipeline. Rejected: a second
authentication-implementation which can drift from the first one.

As defense in depth, a testcase of this repository asserts that the MCP-route answers `401` without token, so a regression in
`GRYLibrary` is noticed.

### D4: Credentials are the existing access-token in the existing header

The agent sends `X-AccessToken: <token>` as static header. Every listed agent supports static headers for a remote
http-MCP-server. The token is the same token the web-UI uses (local login or OIDC-login).

Alternative: additionally accept `Authorization: Bearer <token>`. Deferred to `OTH-2`, because it belongs to the
OAuth-variant and would require a second `ICredentialsProvider`.

Trade-off: a token is valid for 24 hours and has to be copied by the user (see Risks).

### D5: In-process delegation to the controller-action (endpoint-delegation)

A central service `IRESTEndpointDelegator` (folder `MCP`) executes a controller-action in-process:

1. It takes the controller-type and an expression of the action-call, for example
   `delegator.Invoke<OpenDMSBackendController>(controller => controller.Search(searchTerm))`.
2. It reads `[Authorize]` of the target action and checks the groups of the current user with the registered
   `IRoleBasedAuthorizationService`, exactly like `AutSRMiddleware` does. A missing role results in a `NotAuthorizedException`
   before the action runs. An action without `[Authenticate]`/`[Authorize]` is refused, so that a tool can never expose an
   unauthenticated action through an authenticated route.
3. It creates the controller with `ActivatorUtilities.CreateInstance` from the request-services and sets its
   `ControllerContext` to the current `HttpContext`, so that `this.User` is the authenticated principal.
4. It invokes the action and unwraps the `IActionResult`: an `ObjectResult` with a 2xx-status returns its value, any other
   result becomes an error.

The action-expression keeps the delegation refactoring-safe and type-checked: renaming an endpoint or changing its parameters
breaks the build of the tool instead of a runtime-lookup.

Alternative: call `IBusinessLogicService` directly from the tool. Rejected: the user asked for delegation to the REST-endpoint,
the tool would duplicate the mapping and the role-check of the controller, and it would drift when the endpoint changes.
Alternative: HTTP-loopback to the own REST-route. Rejected by the user: second hop, duplicated request-logging, and the
backend would need to know its own address.

### D6: One tool-class per tool, discovered automatically

Each tool is a class in `MCP/Tools` marked with `[McpServerToolType]` and one method with `[McpServerTool(Name = ...)]` and
`[Description(...)]`. The method gets `IRESTEndpointDelegator` and its parameters injected and only declares the delegation
and the mapping of the result. Registration is `WithToolsFromAssembly()`, so adding a tool needs no change of `Program.cs`.
Tool-names are `snake_case` verbs (`search_documents`, `get_document`), which is the common convention of MCP-tools and is
valid for every agent. Every tool declares its MCP-annotations (`ReadOnly = true`, `Destructive = false`, `Idempotent = true`
for the two read-only tools) so that agents can ask the user for confirmation consistently once write-tools are added.

### D7: Representation of the results

- `search_documents`: returns a list of a dedicated result-type (id, readable id, title, filename, original filename,
  mime-type, import-date, short ai-summary, version-number) mapped from `DocumentPreviewDTO`, without `PreviewAsBase64`.
  The SDK returns it as `structuredContent` with an output-schema and additionally as json-text-content, so agents without
  support for structured content work as well.
- `get_document`: returns `CallToolResult` with a `TextContentBlock` (metadata and both ai-summaries as readable text) and,
  if the content is not larger than `MCPMaximalEmbeddedDocumentSizeInBytes`, an `EmbeddedResourceBlock` with a
  `BlobResourceContents` (`uri` `opendms://documents/{id}`, `mimeType` of the document, `blob` = the base64-content of the
  `DocumentDTO`). Above the limit, a text states the size and the limit instead.
- `MCPMaximalEmbeddedDocumentSizeInBytes` is a new property of `CodeUnitSpecificConfiguration`, seeded with `10485760` (10
  MiB). Rationale: base64 increases the size by a third, and the context of an agent is the scarce resource.

### D8: Error-handling

The delegator translates `NotAuthorizedException` and `BadRequestException` of `GRYLibrary` into an `McpException` with the
message of the exception, which the SDK returns as tool-result with `isError = true`. Every other exception is logged with the
server-log and becomes an error-result with a generic message. No exception-message of an unexpected exception reaches the
agent.

### D9: Logging

The MCP-route is added to `RoutesWhereResponseBodyIsNotLogged` of `DRequestLoggingConfiguration` in `Program.cs`.

### D10: Not part of the api-specification

The MCP-route is excluded from the generated api-specification (`ExcludeFromDescription()`), so the generation of the
frontend-client in `OpenDMSFrontend/src/app/generated` is not affected.

## Risks / Trade-offs

- [Missing protection if the `GRYLibrary`-fix is not available] → The task-list makes the fix a blocking first step, and a
  testcase in this repository asserts `401` without token. The change must not be merged before that testcase is green.
- [Token expires after 24 hours and must be copied manually into the agent-configuration] → Documented; the clean solution
  is the OAuth-variant of `OTH-2`.
- [A stolen agent-configuration contains a valid token] → Same risk as a stolen browser-session (SEC-1, SEC-27); the
  documentation recommends an environment-variable instead of a literal in the configuration-file, which all listed agents
  support.
- [Instantiating controllers outside of MVC bypasses MVC-filters and model-validation] → No filters are used by the
  controllers today; the role-check is replicated explicitly (D5 step 2); parameters are validated by the input-schema of the
  tool and by the business-logic. A testcase per tool asserts that the tool and the REST-endpoint return the same data.
- [Large documents consume the context of the agent] → Size-limit of D7.
- [Prompt-injection through document-contents: a document can contain instructions which the agent follows] → Cannot be
  prevented by the server; it is limited by the fact that the agent can only do what the user can do, and the first tools are
  read-only. To be documented in the reference-article.
- [`ModelContextProtocol.AspNetCore` is a young package with frequent releases] → Pinned exactly; the delegation-mechanism
  and the tools depend only on its attributes and result-types.

## Migration Plan

Additive change. Existing clients and the REST-api are unaffected. Rollback means removing the `MapMcp`-call; no data is
migrated. The new configuration-property is seeded for new configuration-files; for an existing configuration-file the
missing property gets the seeded default by the configuration-loading of `GRYLibrary` (to be verified in task 2.3, otherwise
the property is added explicitly by the migration of the configuration).

## Open Questions

- Whether the server shall additionally announce `instructions` (a short text in the `initialize`-result which explains the
  domain to the agent). It does not change the specs or the tasks; it can be added when the tool-set grows.
