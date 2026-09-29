# ADR-005: External Service Integration

## Status

Proposed

## Context

M1 excludes external CRM, WhatsApp, email and emergency-dispatch integrations from the committed baseline. Assignment 2 nevertheless identifies heterogeneous municipal contractor APIs as a genuine design problem for future CivicConnect integration.

## Decision

Use an Adapter boundary for contractor dispatch integrations.

The core application depends on IContractorDispatchAdapter. Each vendor-specific implementation maps the common CivicConnect request into the external contract expected by that provider.

The M2 implementation uses MunicipalContractorAdapter with an in-memory client so the mapping and dependency boundary can be exercised without a live external service.

## Benefits

- Core request logic does not depend on vendor payload types
- New vendor integrations can be introduced behind the same interface
- Mapping logic is isolated
- A development client can be injected without network calls

## Trade-offs

- Adds an interface and mapping layer
- Adapter classes must be maintained when vendor contracts change
- No live external network integration is claimed by this M2 implementation

## Affected Components

- IContractorDispatchAdapter
- ContractorDispatchRequest
- ContractorDispatchResult
- MunicipalDispatchPayload
- IMunicipalContractorClient
- MunicipalContractorAdapter
- InMemoryMunicipalContractorClient

## Research Basis

Assignment 2 Task 1 compared the Adapter pattern with Abstract Factory for heterogeneous contractor APIs and identified schema isolation, maintainability and testability as the main concerns.

## Scope

The M1 baseline keeps external integrations out of the committed product scope, so this implementation establishes the design boundary without claiming a production integration.
