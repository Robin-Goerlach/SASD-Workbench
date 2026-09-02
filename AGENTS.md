# AGENTS.md

This repository contains the shared core for the SASD Workbench product family.

## Read before changing code

For architectural or data-model changes, read:

- `docs/010_Lastenheft.md`
- `docs/020_Pflichtenheft_MVP.md`
- `docs/030_Architektur_Dokument.md`
- `docs/040_Database_Design.md`

## Core architectural rule

The shared core must remain profile-neutral. Do not add Bible-, Linux-, laboratory-, recipe-, prompt-, health-, or other profile-specific rules to `SASD.Workbench.Domain` unless the requirement is genuinely common to all Workbench products.

Dependency direction:

```text
WinForms -> Application -> Domain
Infrastructure -> Application + Domain
```

The Domain project must not reference SQLite, Windows Forms, the file system, or profile-specific hosts.

## Build and verification

Use the pinned SDK from `global.json`.

```text
dotnet restore SASD-Workbench.slnx
dotnet build SASD-Workbench.slnx --configuration Release --no-restore
dotnet run --project tests/SASD.Workbench.SmokeTests/SASD.Workbench.SmokeTests.csproj --configuration Release --no-build
```

The smoke test must remain package-light and verify a real SQLite persistence round-trip.

## Database changes

- Never edit an already released migration to change an installed schema.
- Add a new numbered migration instead.
- Keep `PRAGMA foreign_keys = ON` enabled on every connection.
- Repositories own SQL; UI event handlers must never execute SQL directly.
- Persist GUIDs as canonical text and timestamps as UTC ISO-8601 text unless an accepted ADR changes this.
- Preserve optimistic version checks when updating mutable core records.

## UI rules

The WinForms project is a host, not the business-logic layer.

Event handlers may collect input, call Application services, and render results. They must not contain persistence, export, backup, or profile-specific business rules.

## Definition of done for a core change

A change is not complete until:

1. it compiles with warnings treated as errors;
2. the V0.1 smoke test passes;
3. database changes include migrations where required;
4. public core classes/methods have useful XML documentation;
5. the change does not introduce avoidable profile-specific coupling;
6. relevant documentation is updated when architecture or requirements change.
