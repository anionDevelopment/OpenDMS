# backend-permission-logic Specification

## Purpose

Defines where the permission-related business-logic of the backend must be located.

## Requirements

### Requirement: Location of the Permission-Business-Logic

The business-logic for permissions SHALL be implemented in `OpenDMSBackend/OpenDMSBackend/Services/BusinessLogicService.cs`.

#### Scenario: Adding or changing permission-logic

- **WHEN** permission-logic is added or changed
- **THEN** the implementation is done in `OpenDMSBackend/OpenDMSBackend/Services/BusinessLogicService.cs`
