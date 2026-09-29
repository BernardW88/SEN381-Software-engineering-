# ADR-001: Layered Architecture

## Status

Proposed

## Context

M1 deliberately deferred architecture selection. The current solution already separates Presentation, BusinessLogic, DataAccess and Models projects.

The M2 architecture must support access control, auditability and maintainability while remaining proportionate to a two-person development team and the current desktop application scope.

## Alternatives

1. Monolithic project structure
2. Layered application architecture
3. Distributed services

## Decision

Use a layered application architecture with Presentation, Business Logic, Data Access, Models and SQL Server LocalDB persistence.

Use in-process domain events for lifecycle side effects. External contractor integrations remain behind adapter interfaces.

## Consequences

Positive:
- Clear responsibility boundaries
- Existing projects remain usable
- Business rules are separated from persistence
- Domain-event handlers can be added without changing lifecycle logic for every consumer
- External vendor schemas can be isolated behind adapters

Trade-offs:
- More interfaces and classes than a single-project implementation
- In-process events are not a replacement for a durable message broker
- LocalDB remains a development persistence choice rather than a production hosting plan

## Evidence

- Presentation/Presentation.slnx
- Presentation/Presentation/Presentation.csproj
- Presentation/BusinessLogic/BusinessLogic.csproj
- Presentation/DataAccess/DataAccess.csproj
- Presentation/Models/Models.csproj
