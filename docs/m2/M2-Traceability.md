# M2 Traceability

## End-to-End Example

| Stage | Evidence |
|---|---|
| Requirement | FR-011 requires a controlled request-status transition model. NFR-009 requires attributable lifecycle history. |
| Quality driver | NFR-009 requires actor, timestamp and request identifier for accepted lifecycle actions. |
| Architecture responsibility | BusinessLogic owns lifecycle rules; DataAccess owns persistence; domain events handle lifecycle side effects. |
| Data decision | Lifecycle history is represented as a separate status-history concept, with a row-version value planned for OCC. |
| Design decision | ADR-004 selects in-process domain events for status-change side effects. |
| Technology decision | The existing C#/.NET 8 stack and Microsoft.Data.SqlClient 7.1.0 are retained; see ADR-002. |
| Application evidence | ServiceRequestService publishes ServiceRequestStatusChangedEvent after a successful repository status update. StatusChangeAuditHandler records request ID, previous status, new status, actor and timestamp. |
| Integration evidence | ADR-005 and the adapter classes establish a vendor-neutral contractor dispatch boundary without introducing a live external service. |
| Verification | GitHub Actions workflow M2 Build restores and builds the solution on pull requests to main. |

## M2 Scope Boundary

The data-history relationships and row-version column are a design baseline and are not represented as active database schema changes in this branch. External contractor integration is also not claimed as a production integration because M1 kept those integrations outside the committed baseline.

## Related Artefacts

- docs/m2/architecture/CivicConnect-Architecture.md
- docs/m2/architecture/ADR-001-Architecture.md
- docs/m2/technology/Technology-Stack.md
- docs/m2/technology/ADR-002-Technology-Stack.md
- docs/m2/data/CivicConnect-Data-Persistence.md
- docs/m2/data/ADR-003-Persistence-Concurrency.md
- docs/m2/design/ADR-004-Domain-Event-Handling.md
- docs/m2/design/ADR-005-External-Service-Integration.md
- Presentation/BusinessLogic/ServiceRequestService.cs
- Presentation/BusinessLogic/Infrastructure/DomainEvents/
- Presentation/BusinessLogic/Integrations/
- .github/workflows/m2-build.yml
