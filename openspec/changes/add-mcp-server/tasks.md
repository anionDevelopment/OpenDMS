# Tasks

## 1. Prerequisite in GRYLibrary (outside this repository)

- [ ] 1.1 Ask the maintainer of `GRYLibrary` to fix `AbstractMiddleware.TryGetAttributeFromContext` as described in design
  D3 (read the attribute from `endpoint.Metadata.GetMetadata<T>()`, with a testcase for a mapped minimal-api-endpoint) and to
  release a new version; verify by the released version-number and its changelog-entry
- [ ] 1.2 Update the pinned `GRYLibrary`-version in `OpenDMSBackend/OpenDMSBackend/OpenDMSBackend.csproj` and the lock-file
  and verify that the existing backend-testcases are green with `RunTestcases.py` of the backend-codeunit

## 2. Hosting and protection of the MCP-route

- [ ] 2.1 Add `ModelContextProtocol.AspNetCore` pinned to `[2.3.1]` to `OpenDMSBackend.csproj` and update
  `packages.lock.json`; verify the backend builds with `Build.py` of the backend-codeunit
- [ ] 2.2 Add the route-constant of the MCP-route (`/API/v{major}/MCP`) to `CodeUnitSpecificConstants` and register
  `AddMcpServer().WithHttpTransport(stateless).WithToolsFromAssembly()` in `Program.SetFunctionalInformationAction` and
  `MapMcp(route).WithMetadata(new AuthenticateAttribute(), new AuthorizeAttribute(RolenameUsers)).ExcludeFromDescription()` in
  `Program.ConfigureWebApplication` (design D1, D2, D3, D10); verify that the generated api-specification does not contain
  the route
- [ ] 2.3 Add `MCPMaximalEmbeddedDocumentSizeInBytes` (seeded with `10485760`) to `CodeUnitSpecificConfiguration`, check how
  an existing configuration-file without the property is loaded and handle it explicitly; verify with a testcase which loads
  a configuration without the property
- [ ] 2.4 Add the MCP-route to `RoutesWhereResponseBodyIsNotLogged` (design D9); verify with a testcase that a
  `tools/call`-response is not contained in the request-log
- [ ] 2.5 Add testcases for the protection: no token gives `401`, invalid token gives `401`, valid token allows
  `initialize` and `tools/list`; verify they are green with `RunTestcases.py`

## 3. Delegation-mechanism

- [ ] 3.1 Implement `IRESTEndpointDelegator`/`RESTEndpointDelegator` in `OpenDMSBackend/OpenDMSBackend/MCP` as described in
  design D5 (role-check from `[Authorize]` with `IRoleBasedAuthorizationService`, refusal of actions without
  `[Authenticate]`/`[Authorize]`, controller-creation with the current `HttpContext`, unwrapping of the `IActionResult`) and
  register it; verify with unit-testcases for each of the four steps
- [ ] 3.2 Implement the error-translation of design D8 in the delegator; verify with testcases that a
  `NotAuthorizedException` and a `BadRequestException` become error-results with their message and that an unexpected
  exception becomes an error-result with a generic message and a log-entry
- [ ] 3.3 Add doc-comments to the delegator which explain how a new tool is added; verify the build has no new warnings

## 4. Tool search_documents

- [ ] 4.1 Implement the result-type and the tool-class `SearchDocumentsTool` in `MCP/Tools` (design D6, D7), delegating to
  `OpenDMSBackendController.Search`; verify with `tools/list` that name, description, input-schema, output-schema and
  annotations are announced
- [ ] 4.2 Add testcases: the tool returns the same documents in the same order as the REST-endpoint for the same user, an
  empty list without hits, an error without `searchTerm`, no preview-image in the result, and a second user does not see the
  documents of the first one; verify they are green

## 5. Tool get_document

- [ ] 5.1 Implement the tool-class `GetDocumentTool` in `MCP/Tools` (design D6, D7), delegating to
  `OpenDMSBackendController.GetDocument` and returning text-content plus embedded resource bounded by
  `MCPMaximalEmbeddedDocumentSizeInBytes`; verify with `tools/list`
- [ ] 5.2 Add testcases: a permitted document returns text and embedded resource with correct mime-type and content, a
  document above the limit returns text without resource and with the size-hint, a document without permission and an
  unknown id return error-results; verify they are green

## 6. Documentation

- [ ] 6.1 Write the reference-article `OpenDMSBackend/Other/Reference/ReferenceContent/Articles/MCPServer.md`: url, header,
  how the user gets the token, one agent-neutral configuration-example (url plus header, token from an environment-variable),
  the list of tools, the size-limit, the 24-hour-validity of the token and the prompt-injection-hint; link it in `index.md` of
  the reference; verify the documented configuration works with at least two different agents against a locally started
  backend
- [ ] 6.2 Add the MCP-server to the feature-list in `Other/Reference/Reference.md` and add the MCP-route to the
  attack-surface-table of the readme; verify both links resolve

## 7. Integration

- [ ] 7.1 Run `scbuildcodeunits` and verify the pipeline is green (build, testcases, linting, security-checks)
- [ ] 7.2 Run `openspec validate add-mcp-server --strict` and verify there is no error and no warning

## Workflow follow-up

- Archive the change after the review.
- Implement finding `OTH-2` (OAuth-based MCP-authorization according to RFC 9728 and admin-setting for the
  authentication-variants) in a separate change.
