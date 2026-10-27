# ADR-001: Layered solution architecture

**Status:** Accepted
**Date:** 2026-10-25

## Context

SHM needs a backend that can grow from a single endpoint to health checks,
command dispatch and alerting, while staying testable. Core logic must be
unit-testable without a database or network, and infrastructure choices
(database provider, health-check mechanism) should be swappable without
rewriting business logic.

## Decision

Split the backend into separate projects by responsibility:

- **SHM.Domain**: entities and core types. No infrastructure.
- **SHM.Services**: application logic, plus the interfaces it depends on
  (`IDeviceHealthChecker`, `ICommandDispatcher`, `IAlertService`, and
  data-access interfaces).
- **SHM.Data**: EF Core `DbContext` and implementations of the data-access
  interfaces defined in Services.
- **SHM.Web**: ASP.NET Core Web API and the composition root, where all
  dependency injection wiring happens.
- **SHM.Tests**: unit and integration tests.

### Project dependencies

| Project      | References            | Why                                                                                            |
| ------------ | --------------------- | ---------------------------------------------------------------------------------------------- |
| SHM.Domain   | nothing               | The core model must not depend on anything, so it never changes because infrastructure changed |
| SHM.Services | Domain                | Logic operates on domain entities                                                              |
| SHM.Data     | Domain, Services      | Persists domain entities and implements interfaces Services defines                            |
| SHM.Web      | Services, Data        | Exposes the API and registers implementations with DI                                          |
| SHM.Tests    | Domain, Services, Web | Unit tests for logic; integration tests start the API                                          |

Dependencies point inward, towards Domain. Services defines the interfaces it
needs and does not reference Data, so business logic can be tested with fakes
instead of a real database.

## Consequences

**Positive**

- Domain and service logic can be unit tested without a database or network.
- Infrastructure (database, health-check mechanism) can change behind interfaces.
- Clear boundaries make the codebase easier to navigate and review.

**Negative**

- More projects and references than a small app strictly needs.
- Interfaces add indirection; several will have only one implementation at first.
- Boundaries are enforced only by project references and review, so logic can
  still end up in the wrong layer if not watched.
