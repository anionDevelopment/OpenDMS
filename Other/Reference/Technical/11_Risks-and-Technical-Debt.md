# 11. Risks and Technical Debt

## Risks

### Unauthenticated endpoints

You probably want to protect certain endpoints which are unauthenticated by default.
The design-concept behind is that you protect this endpoint by your reverse-proxy using basic-auth.

Endpoints you maybe want to protect are:

- `/API/Other/Maintenance/Metrics`: The metrics-endpoint is by design available without authentication. This is unauthenticated because only human user will be authenticated, but the metrics-scraper is typically a technical user.
- `/API/Other/Maintenance/CurrentVersion`: Querying the current version of the application should not be considered as weakness, but if you want to harden your server, you can disable this endpoint anyway.

It is possible to enable/disable the maintenance-endpoints using commandline-switch on initial-configuration-generation or later in the configuration-file.

## Technical debts

The known defects which are not fixed yet because fixing them requires a bigger change than a local correction are listed in the
[hints of the codeunit OpenDMSBackend](https://github.com/anionDev/OpenDMS/blob/main/OpenDMSBackend/Other/Reference/ReferenceContent/Hints.md#known-defects-which-require-a-larger-change),
next to the code they belong to. They concern the containment-hierarchy after a move, the error-reporting of the hard-deletion, the
visibility of soft-deleted documents in a container-view, the re-seeding of the readable document-id and the width of the
readable-id-type.
