# 11. Risks and Technical Debt

## Risks

### Metrics-endpoint

The metrics-endpoint (`/API/Other/Maintenance/Metrics`) is by design available without authentication.
This is because only human user will be authenticated, but the metrics-scraper is typically a technical user.
The design-contept behind is that you protect this endpoint by your reverse-proxy using basic-auth.

## Technical debts

Currently there are no technical depts.
