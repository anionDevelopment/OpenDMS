# 11. Risks and Technical Debt

## Risks

### Unauthenticated endpoints

You probably want to protect certain entdpoints which are unauthenticated by default.
The design-contept behind is that you protect this endpoint by your reverse-proxy using basic-auth.

Endpoints you maybe want to protect are:

- `/API/Other/Maintenance/Metrics`: The metrics-endpoint is by design available without authentication. This is unauthentcated because only human user will be authenticated, but the metrics-scraper is typically a technical user.
- `/API/Other/Maintenance/CurrentVersion`: Querying the current version of the application should not be considered as weakness, but if you want to harden your server, you can disable this endpoint anyway.

It is possible to enable/disable the maintenance-endpoints using commandline-switch on initial-configuration-generation or later in the configuration-file.

## Technical debts

Currently there are no technical depts.
