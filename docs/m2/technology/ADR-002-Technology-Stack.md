# ADR-002: Technology Stack

## Status

Proposed

## Context

M1 deferred the technology stack and required M2 to justify the selected technologies against requirements, team capability, schedule, cost, security, maintainability and deployment compatibility.

## Decision

Retain the existing CivicConnect stack:

- C#
- Windows Forms
- .NET 8
- Microsoft.Data.SqlClient 7.1.0
- SQL Server LocalDB
- GitHub Actions for initial build verification

## Rationale

The stack already supports the current solution structure, matches the desktop scope and avoids an unnecessary framework migration during M2.

.NET 8 is approaching its support end date, so upgrading to a currently supported LTS release must be handled before production use.

LocalDB is suitable for development but is not being treated as the final production database-hosting decision.

## Consequences

Positive:
- Low migration effort
- Consistent development environment
- Existing source code remains compatible
- Small dependency footprint

Trade-offs:
- Windows Forms limits the UI to Windows
- LocalDB is a development-oriented database arrangement
- .NET 8 requires a planned upgrade before support ends

## References

https://learn.microsoft.com/en-us/dotnet/desktop/winforms/overview/

https://dotnet.microsoft.com/en-us/platform/support/policy

https://learn.microsoft.com/sql/database-engine/configure-windows/sql-server-express-localdb
