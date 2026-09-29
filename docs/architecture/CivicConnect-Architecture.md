# CivicConnect Architecture

## Architecture Overview

CivicConnect uses a layered architecture consisting of the Presentation,
Business Logic, Data Access and Database layers.

## Architecture Structure

Presentation Layer
↓
Business Logic Layer
↓
Data Access Layer
↓
SQL Server LocalDB

## Presentation Layer

The Presentation layer provides the Windows Forms user interface.

Responsibilities:
- User interaction
- Form input
- Displaying service requests
- Dashboard presentation

## Business Logic Layer

The Business Logic layer contains the main application rules.

Responsibilities:
- Service request creation
- Input validation
- Status transition rules
- Dashboard metrics
- User services
- Notifications
- Escalation
- Error handling and logging

## Data Access Layer

The Data Access layer handles communication with the database.

Current components:
- IServiceRequestRepository
- ServiceRequestRepository
- IUserRepository
- UserRepository
- DatabaseConnection

Repositories perform database operations using SQL queries.

## Database Layer

CivicConnect currently uses SQL Server LocalDB.

Database:
CityMakerspaceDB

The database stores application information such as service requests and users.

## Main Request Flow

User Interface
→ ServiceRequestService
→ IServiceRequestRepository
→ ServiceRequestRepository
→ SQL Server LocalDB

## Architecture Drivers

The main quality drivers are:

- Security and access control
- Auditability
- Performance
- Maintainability
- Reliability and recoverability

## Architecture Alternatives

### Monolithic Architecture

All application responsibilities would exist together.

Advantage:
- Simple initial structure

Disadvantages:
- Poorer separation of concerns
- More difficult maintenance as the application grows

### Layered Architecture

Responsibilities are separated into Presentation, Business Logic and Data Access layers.

Advantages:
- Clear separation of responsibilities
- Easier maintenance and testing
- Matches the current project structure

Disadvantage:
- More structure and interfaces are required

### Distributed Microservices

The application could be divided into separately deployed services.

Advantages:
- Independent deployment
- Strong service isolation

Disadvantages:
- Higher complexity
- Additional communication and deployment requirements
- Not currently justified by the project scope

## M2 Architecture Decision

CivicConnect will continue using a layered architecture.

The architecture is proportionate to the current project scope and existing implementation. It separates user interaction, business rules and database operations while allowing the system to be extended later.
