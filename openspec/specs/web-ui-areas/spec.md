# web-ui-areas Specification

## Purpose

Defines that the web-ui is split into a separate admin-area and user-area.

## Requirements

### Requirement: Separated Admin-Area and User-Area

The web-ui SHALL provide an admin-area and a user-area as separate areas.

#### Scenario: Administrative functions

- **WHEN** a function is administrative
- **THEN** it is located in the admin-area

#### Scenario: Regular user-functions

- **WHEN** a function is a regular user-function
- **THEN** it is located in the user-area
