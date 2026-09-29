# CivicConnect Technology Stack

## Current Baseline

| Area | Technology / Version | Evidence |
|---|---|---|
| Language | C# | Current application source files |
| UI | Windows Forms | Presentation/Presentation.csproj |
| Runtime | .NET 8 | BusinessLogic, DataAccess and Models target net8.0; Presentation targets net8.0-windows |
| Data access | Microsoft.Data.SqlClient 7.1.0 | Presentation/DataAccess/DataAccess.csproj |
| Database | SQL Server LocalDB | Presentation/DataAccess/DatabaseConnection.cs |
| Source control | Git / GitHub | Existing repository workflow |
| CI | GitHub Actions | M2 build workflow added in this branch |

## Selection Basis

### Windows Forms

Windows Forms is a .NET desktop UI framework for Windows applications and already matches the existing presentation project. The M1 scope is a desktop application, so replacing the UI framework would add migration effort without a current requirement driving that change.

Source: https://learn.microsoft.com/en-us/dotnet/desktop/winforms/overview/

### .NET 8

The existing solution targets .NET 8. As of September 2026, Microsoft lists .NET 8 as an LTS release in maintenance, with support ending on 10 November 2026. M2 therefore retains .NET 8 to avoid an unnecessary migration during the current implementation increment, while the move to a currently supported LTS release is recorded as a deployment consideration.

Source: https://dotnet.microsoft.com/en-us/platform/support/policy

### Microsoft.Data.SqlClient

The current DataAccess project references Microsoft.Data.SqlClient version 7.1.0. M2 records the version already used by the solution rather than changing dependencies without a requirement.

### SQL Server LocalDB

The current connection uses SQL Server LocalDB and database CityMakerspaceDB. Microsoft describes LocalDB as a SQL Server Express feature targeted at developers and accessed through a local connection.

Source: https://learn.microsoft.com/sql/database-engine/configure-windows/sql-server-express-localdb

## Alternatives Considered

| Decision area | Current selection | Alternative considered | Main consideration |
|---|---|---|---|
| UI | Windows Forms | WPF / web UI | Existing implementation and desktop scope |
| Runtime | .NET 8 | .NET 10 | Avoid mid-M2 migration; record support deadline |
| Database access | Microsoft.Data.SqlClient | ORM | Current repository code directly uses SQL and has no ORM requirement |
| Database | SQL Server LocalDB | Full SQL Server service | Appropriate for local development; production hosting remains separate |

## Compatibility and Deployment Assumptions

- Development requires Windows because the UI project targets Windows Forms.
- The current LocalDB configuration is a development arrangement.
- No production hosting decision is claimed by this document.
- Dependency versions should be reviewed before release.

## Decision

Retain the existing technology stack for M2 and record upgrade or hosting changes as controlled future decisions rather than making unnecessary migration changes during the current baseline.
