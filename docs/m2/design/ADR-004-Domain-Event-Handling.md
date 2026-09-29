# ADR-004: Domain Event Handling

## Status

Proposed

## Context

CivicConnect status changes can have side effects such as audit logging and future notifications.

Assignment 2 compared direct procedural calls with an Observer pattern implemented through an in-memory domain event publisher.

## Decision

Use an in-process domain event publisher for service-request status changes.

The status-change operation publishes ServiceRequestStatusChangedEvent. Independent handlers can subscribe to the event without adding direct dependencies from the lifecycle method to every downstream consumer.

## Expected Benefits

- Reduced coupling between lifecycle changes and side effects
- New handlers can be added without changing the status transition rules
- Audit handling can be tested independently
- The current desktop application does not require a network boundary for these internal events

## Trade-offs

- Event flow is less direct than a single method call
- Handler failure must be isolated
- In-memory events are not durable across application restarts

## Affected Components

- ServiceRequestService
- IDomainEvent
- IDomainEventPublisher
- InMemoryDomainEventPublisher
- ServiceRequestStatusChangedEvent
- StatusChangeAuditHandler

## Application Evidence

ServiceRequestService.AdvanceRequestStatus publishes the event only after a successful repository update.

## Related Requirements

- FR-011 controlled status transitions
- NFR-009 auditability
- NFR-010 maintainability

## Research Basis

Assignment 2 Task 1 compared Observer/Domain Event handling with direct procedural calls and identified lifecycle decoupling as the key concern.
