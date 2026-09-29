# ADR-003: Unit of Work and Optimistic Concurrency

## Status

Proposed

## Context

Assignment 2 identified municipal issue submission as an operation that can require several related writes. A partial failure could leave incomplete lifecycle or assignment information.

The alternatives considered were application-managed transactions with optimistic concurrency and database stored procedures with pessimistic locking.

## Decision

Use an application-managed Unit of Work transaction boundary for multi-table request operations, with optimistic concurrency based on a row-version value.

## Rationale

The transaction boundary remains visible in application code, service behaviour can be tested without embedding business logic in SQL procedures, and the database does not need to hold long-lived locks while unrelated application work is performed.

The database remains responsible for relational constraints and final integrity checks.

## Consequences

Positive:
- Atomic multi-record operations
- Explicit conflict detection
- Business logic remains in C#
- Transaction responsibility is visible in the application layer

Trade-offs:
- Transaction coordination adds code
- Concurrency conflicts need an application response
- The current schema must be extended with a row-version value before OCC is active

## Implementation Boundary

The current repository continues to perform single-operation SQL writes. Unit of Work and OCC are established as the M2 design baseline for the first multi-record operation that requires them.

## Related Requirements

- NFR-007 Reliability
- NFR-009 Auditability

## Research Basis

Assignment 2 Task 2 compared application-managed ACID transactions with OCC against stored procedures with pessimistic locking and recommended the application-managed approach for CivicConnect.
