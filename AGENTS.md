# AGENTS.md

This repository contains the shared core for the SASD Workbench product family.

## Read before changing code

For architectural or data-model changes, read:

- `docs/010_Lastenheft.md`
- `docs/020_Pflichtenheft_MVP.md`
- `docs/030_Architektur_Dokument.md`
- `docs/040_Database_Design.md`
- `docs/045_Cross_Cutting_Features.md`
- `docs/050_Development_Roadmap.md`
- `docs/060_Developer_Guide.md`
- `docs/070_Test_Strategy.md`

## Core architectural rule

The shared core must remain profile-neutral. Do not add Bible-, Linux-, laboratory-, recipe-, prompt-, health-, or other profile-specific rules to `SASD.Workbench.Domain` unless the requirement is genuinely common to all Workbench products.

Dependency direction:

```text
WinForms -> Application -> Domain
Infrastructure -> Application + Domain
```

The Domain project must not reference SQLite, Windows Forms, the file system, command-line parsing, or profile-specific hosts.

## Shared host composition

Hosts must use `AddSasdWorkbenchCore(...)` from `SASD.Workbench.Infrastructure.DependencyInjection` as the canonical registration of the common local Core.

A specialized Workbench host may add its own UI, profile and module services after the common Core registration. Do not copy the SQLite repository/service wiring into every future Biblical, Health, Admin, Research or Engineering host. If a new profile-neutral Core service is added, register it centrally and extend the composition smoke-test path.

The host remains responsible for selecting the Workbench data root and running database migrations before the UI starts. Host-specific configuration mechanisms such as WinForms command-line options must remain in the host; pass the resulting `WorkbenchDataPaths` into the shared Core rather than teaching the Core how a particular host was configured.

For acceptance/test startup, malformed data-root configuration must fail closed. Never silently fall back to the normal user data directory after the caller explicitly attempted to select an isolated root.

## Build and verification

Use the pinned SDK from `global.json`.

```text
dotnet restore SASD-Workbench.slnx
dotnet build SASD-Workbench.slnx --configuration Release --no-restore
dotnet run --project tests/SASD.Workbench.Domain.Tests/SASD.Workbench.Domain.Tests.csproj --configuration Release --no-build
dotnet run --project tests/SASD.Workbench.Application.Tests/SASD.Workbench.Application.Tests.csproj --configuration Release --no-build
dotnet run --project tests/SASD.Workbench.Infrastructure.Tests/SASD.Workbench.Infrastructure.Tests.csproj --configuration Release --no-build
dotnet run --project tests/SASD.Workbench.WinForms.Tests/SASD.Workbench.WinForms.Tests.csproj --configuration Release --no-build
dotnet run --project tests/SASD.Workbench.SmokeTests/SASD.Workbench.SmokeTests.csproj --configuration Release --no-build
```

The V1 Core smoke test must remain package-light and verify a real SQLite persistence round-trip. It also resolves the production Core registration so composition-root drift is detected by CI. Focused Domain/Application/Infrastructure/Host tests should catch smaller regressions before the smoke gate.

## Database changes

- Never edit an already released migration to change an installed schema.
- Add a new numbered migration instead.
- Keep `PRAGMA foreign_keys = ON` enabled on every connection.
- Repositories own SQL; UI event handlers must never execute SQL directly.
- Persist GUIDs as canonical text and timestamps as UTC ISO-8601 text unless an accepted ADR changes this.
- Preserve caller-observed optimistic version checks through Application and repeat them atomically in SQLite repositories when updating mutable Project/Entry records.

## UI rules

The WinForms project is a host, not the business-logic layer.

Event handlers may collect input, call Application services, and render results. They must not contain persistence, export, backup, or profile-specific business rules.

As V1 grows, prefer focused controls/dialogs over continuously expanding `MainForm`. Shared use cases belong below the UI layer so later Workbench hosts can reuse them.

Nonvisual host concerns such as startup option parsing may be tested in `SASD.Workbench.WinForms.Tests`; do not confuse those tests with full UI automation.

## Definition of done for a core/host change

A change is not complete until:

1. it compiles with warnings treated as errors;
2. the relevant focused Domain/Application/Infrastructure/Host tests pass;
3. the V1 Core smoke test passes;
4. database changes include migrations where required;
5. public core classes/methods have useful XML documentation;
6. the change does not introduce avoidable profile- or host-specific coupling into the Core;
7. relevant documentation is updated when architecture or requirements change.
