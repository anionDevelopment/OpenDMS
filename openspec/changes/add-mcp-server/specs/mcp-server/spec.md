# Spec Delta

## Purpose

Lets a user work with OpenDMS through any AI-agent which supports the Model Context Protocol, with exactly the permissions
the user has in OpenDMS itself.

## ADDED Requirements

### Requirement: The backend offers an MCP-endpoint

The backend SHALL offer an MCP-server at the route `/API/v{major-version}/MCP` which implements the Streamable-HTTP-transport
of the Model Context Protocol in stateless mode. It SHALL be part of the backend and SHALL NOT require any additional
service or container.

#### Scenario: An agent initializes a connection

- **WHEN** an authenticated MCP-client sends an `initialize`-request to the MCP-route
- **THEN** the server answers with its name, its version and the capability `tools`

#### Scenario: An agent lists the tools

- **WHEN** an authenticated MCP-client sends a `tools/list`-request
- **THEN** the server answers with every provided tool, each with a name, a description and an input-schema

#### Scenario: Requests are not bound to a server-side session

- **WHEN** two consecutive requests of the same client are processed by two different instances of the backend
- **THEN** both requests succeed, because no server-side session-state is required

### Requirement: The MCP-endpoint requires the authentication of the user

Every request to the MCP-route SHALL be authenticated with the same credential-header and the same token-types (local
access-token and OIDC-access-token) as the REST-api. A request without a valid token SHALL be refused before any MCP-message
is processed.

#### Scenario: A request without token

- **WHEN** a request without the credential-header is sent to the MCP-route
- **THEN** the backend answers with status `401` and no MCP-message is processed

#### Scenario: A request with an invalid or expired token

- **WHEN** a request with an unknown or expired access-token is sent to the MCP-route
- **THEN** the backend answers with status `401`

#### Scenario: A request with a valid token

- **WHEN** a request with a valid access-token of a user with the role of all users is sent to the MCP-route
- **THEN** the request is processed as that user

### Requirement: A tool-call acts with the identity and the permissions of the user

A tool-call SHALL be executed as the user whose token authenticated the request, and it SHALL succeed only if the same user
would succeed with the corresponding REST-request. A tool SHALL NOT grant any permission which the REST-endpoint does not
grant, and it SHALL NOT bypass the role-requirement of that endpoint.

#### Scenario: A user searches

- **WHEN** a user calls a search-tool
- **THEN** the result contains exactly the documents which the REST-endpoint `Search` returns for that user and that term

#### Scenario: A user requests a document they may not see

- **WHEN** a user calls `get_document` with the id of a document they have no permission for
- **THEN** the tool-call returns an error-result and no content, metadata or existence-information beyond what the
  REST-endpoint reveals in the same case

#### Scenario: A tool delegates to an endpoint which requires a role the user does not have

- **WHEN** a tool delegates to a REST-endpoint which requires a role and the user does not have that role
- **THEN** the tool-call returns an error-result and the endpoint-logic is not executed

### Requirement: Every tool delegates to an existing REST-endpoint

Every MCP-tool SHALL delegate to exactly one existing REST-endpoint of the backend and SHALL return the result of that
endpoint, adapted only in its representation. A tool SHALL NOT contain domain-logic or permission-logic.

#### Scenario: A tool is added for a further REST-endpoint

- **WHEN** a developer adds a tool for a further REST-endpoint
- **THEN** the new tool only declares its name, its description, its parameters, the REST-endpoint it delegates to and the
  representation of the result, and neither the hosting, the authentication nor the delegation has to be changed

#### Scenario: The behavior of a REST-endpoint changes

- **WHEN** the behavior of a REST-endpoint which a tool delegates to changes
- **THEN** the tool shows the changed behavior without being changed itself

### Requirement: Tool search_documents

The server SHALL provide the tool `search_documents` with the required string-parameter `searchTerm`. It SHALL delegate to
the REST-endpoint `Search` and SHALL return the found documents as structured content and as text-content containing the
same data as JSON.

#### Scenario: A search with hits

- **WHEN** a user calls `search_documents` with a term which matches documents visible to them
- **THEN** the result contains one entry per found document, in the order of the REST-endpoint, with at least its id, its
  readable id, its title, its filename, its mime-type, its import-date and its short ai-summary

#### Scenario: Search-results contain no preview-image

- **WHEN** a user calls `search_documents`
- **THEN** no entry of the result contains the preview-image of the document, because binary data in a list of results
  only consumes the context of the agent

#### Scenario: A search without hits

- **WHEN** a user calls `search_documents` with a term which matches no document visible to them
- **THEN** the result is a successful result with an empty list

#### Scenario: A search without term

- **WHEN** a client calls `search_documents` without `searchTerm`
- **THEN** the tool-call returns an error-result which names the missing parameter

### Requirement: Tool get_document

The server SHALL provide the tool `get_document` with the required string-parameter `documentId`. It SHALL delegate to the
REST-endpoint `GetDocument` and SHALL return the metadata and the ai-summaries of the document as text-content and the
original file as embedded resource with its mime-type.

#### Scenario: A document within the size-limit

- **WHEN** a user calls `get_document` for a document they may see and whose content is not larger than the configured
  maximum size for embedded documents
- **THEN** the result contains a text-content with the metadata and the ai-summaries and an embedded resource with the
  binary content of the document and its mime-type

#### Scenario: A document above the size-limit

- **WHEN** a user calls `get_document` for a document whose content is larger than the configured maximum size
- **THEN** the result contains the text-content with the metadata and the ai-summaries, no embedded resource, and a text
  which states that the file was omitted because of its size and names the size and the limit

#### Scenario: An unknown document

- **WHEN** a user calls `get_document` with an id which does not exist
- **THEN** the tool-call returns an error-result

### Requirement: Errors are returned as tool-errors

An error of a delegated REST-endpoint SHALL be returned as a tool-result marked as error with a message which the agent can
show to the user. The message SHALL NOT contain a stack-trace or internal information beyond what the REST-endpoint itself
returns for the same case.

#### Scenario: The REST-endpoint refuses the request

- **WHEN** the delegated REST-endpoint refuses the request because of a missing permission or an invalid argument
- **THEN** the tool-result is marked as error and contains a short description of the reason

#### Scenario: An unexpected error happens

- **WHEN** an unexpected exception happens while a tool-call is processed
- **THEN** the tool-result is marked as error with a generic message, and the exception is logged in the backend

### Requirement: The MCP-interface is agent-neutral

The MCP-server SHALL only use mechanisms of the MCP-specification and of HTTP which every common MCP-client supports
(Streamable-HTTP-transport, a static request-header for the credentials). It SHALL NOT contain any behavior which depends on
the client-name or the client-version.

#### Scenario: Different agents connect

- **WHEN** two different MCP-clients connect with the same url and the same credential-header
- **THEN** both get the same tools and the same results for the same calls

### Requirement: Document-contents of MCP-responses are not written to the request-log

The request-logging SHALL NOT write the response-body of the MCP-route, because it contains document-contents.

#### Scenario: A document is retrieved through MCP

- **WHEN** a user calls `get_document` through the MCP-route
- **THEN** the request-log contains the request but not the content of the response
