# Site Health Monitor (SHM)

## Summary

SHM is a head-office monitoring system for a fictional multi-site retail chain.
It tracks the health of in-store devices, receives daily sales data from sites,
pushes commands (price changes, new products) down to devices, and raises alerts
when something goes wrong.

The domain is deliberately generic and is not modelled on any real vendor's product.

## Problem

A retail chain with many sites needs to know, from head office, when a store device
or the service running on it stops responding, and when a command sent to a device
fails to arrive. Without this, problems surface only when store staff report them.

## Core features

- **Device health monitoring**: scheduled checks against each device. A device that
  is genuinely unreachable fails the check; status is never faked with a database flag.
- **Sales upload**: sites push daily sales snapshots to head office.
- **Command dispatch**: head office pushes commands (price change, new product)
  to devices.
- **Alerting**: one alert model fed by two detection paths:
  1. a failed scheduled health check
  2. a failed command push to an unreachable device
- **Device control UI**: toggle simulated devices on and off to trigger alerts on demand.

## Health check approach

Checks sit behind `IDeviceHealthChecker`, so the mechanism can change without
touching alerting logic.

1. **HTTP check first**: each simulated device exposes `GET /health`. This works
   against local processes on different ports and detects "service down" failures.
2. **ICMP check later**: added as a second implementation once devices run in
   Docker Compose, to detect "host unreachable". See ADR-002 (to be written).

## Scope

**Version 1:** a failed check or failed command raises an alert.
**Stretch goals:** queued retry for failed commands; distinct alert reasons for
"host unreachable" vs "service down".

## Domain model

- `Site` has many `Device`s
- `Device` (abstract) with `TillPC` and `BackOfficePC` subtypes
- `Device` has many `HeartbeatLog`s and `DeviceCommand`s
- `Site` has many `SalesSnapshot`s
- `DeviceCommand`: `CommandType`, `Payload`, `CreatedAt`
- `Alert`: `DeviceId`, `Reason`, `RaisedAt`

## Tech stack

| Area              | Choice                                                       |
| ----------------- | ------------------------------------------------------------ |
| Backend           | C# / ASP.NET Core Web API, dependency injection              |
| Data              | SQL Server (Docker locally, Azure SQL later), EF Core        |
| Testing           | xUnit, ASP.NET Core integration tests, Playwright (frontend) |
| Simulated devices | Minimal .NET web apps, local processes then Docker Compose   |
| Frontend          | React + TypeScript                                           |
| CI/CD             | GitHub Actions                                               |
| Hosting           | Azure (free tier)                                            |

## Repository structure

    /docs            Project documentation and ADRs
    /src
      SHM.Domain     Entities and core types, no dependencies
      SHM.Services   Application logic and interfaces
      SHM.Data       EF Core persistence
      SHM.Web        ASP.NET Core Web API
      SHM.DeviceSim  Simulated device app
      frontend       React app
    /tests
      SHM.Tests      Unit and integration tests

## Roadmap

### Sprint 0: Setup ✅

- Solution scaffolded, builds cleanly, pushed to GitHub
- Docs and ADR-001 via first PR
- Branch protection on `main`
  **Skills:** Git, GitHub, .NET CLI, PR workflow
  **Done when:** first PR merged and `main` is protected

### Sprint 1: Domain model and tests

- `Site`, abstract `Device`, `TillPC`, `BackOfficePC`, `Alert`, `DeviceCommand`
- Interfaces: `IDeviceHealthChecker`, `ICommandDispatcher`, `IAlertService`
- First unit tests for domain behaviour
  **Skills:** C# OOP (abstract classes, inheritance, interfaces), xUnit, test naming
  **Done when:** domain builds, at least 5 meaningful passing tests

### Sprint 2: Database and first endpoint

- SQL Server running in Docker locally
- `SHM.Data` with EF Core `DbContext`, first migration, seed data
- `GET /api/sites/{siteId}/devices` returning real data
  **Skills:** EF Core, migrations, inheritance mapping (TPH vs TPT), SQL Server,
  Docker basics, ASP.NET Core controllers or minimal APIs, DI registration
  **Done when:** endpoint returns seeded devices; you can explain your TPH/TPT choice

### Sprint 3: CI pipeline

- GitHub Actions workflow: restore, build, test on every push and PR
- PRs blocked from merging unless the build passes
- Integration test for the devices endpoint using `WebApplicationFactory`
  **Skills:** YAML pipelines, CI, integration testing
  **Done when:** a failing test blocks a PR merge

### Sprint 4: Health checks and alerting

- `SHM.DeviceSim`: a minimal app exposing `GET /health`, run several on different ports
- `HttpDeviceHealthChecker` implementing `IDeviceHealthChecker`
- Background service running scheduled checks, writing `HeartbeatLog`s
- Failed check raises an `Alert`
- Unit tests using a fake health checker
  **Skills:** `BackgroundService`, `HttpClient` / `IHttpClientFactory`, async/await,
  logging with `ILogger`, test doubles
  **Done when:** stopping a simulated device produces an alert within one check cycle

### Sprint 5: Commands and sales

- `POST` endpoint to create a `DeviceCommand`; dispatcher pushes it to the device
- Failed push raises an `Alert` (same alert model as health checks)
- Sites push `SalesSnapshot`s to head office
- Input validation and consistent error responses
  **Skills:** REST design, status codes, validation, error handling
  **Done when:** a command sent to a stopped device raises an alert

### Sprint 6: Docker Compose and ICMP

- Dockerfiles for `SHM.Web` and `SHM.DeviceSim`
- Docker Compose running API, SQL Server and several devices together
- `IcmpDeviceHealthChecker` as a second implementation (ADR-002)
  **Skills:** Dockerfiles, Compose networking, swapping implementations via DI
  **Done when:** `docker compose up` runs the whole system; ICMP check works in-network

### Sprint 7: Frontend

- React + TypeScript app: sites list, device status, alerts, device on/off toggles
- Playwright end-to-end test: toggle a device off, see the alert appear
  **Skills:** React, TypeScript, calling APIs, CORS, end-to-end testing
  **Done when:** the full scenario works from the browser and is covered by a test

### Sprint 8: Azure (optional)

- Azure SQL free tier, deploy API to Azure, deploy from GitHub Actions
  **Skills:** Azure portal, connection strings and secrets, continuous deployment
  **Done when:** the live API is reachable and deploys on merge to `main`

## Ways of working

- Every change goes through a feature branch and PR, including docs
- Conventional commit messages (`feat:`, `fix:`, `docs:`, `test:`, `chore:`)
- Write a short PR description: what changed and why
- Retro note per sprint in `docs/retro-log.md`: what went well, what didn't, what to change
