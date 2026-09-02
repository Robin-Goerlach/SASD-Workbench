# Changelog

All notable changes to SASD Workbench will be documented in this file.

The project follows a pragmatic form of Semantic Versioning while it is below 1.0.

## [Unreleased]

### Added

- Shared .NET 10 project configuration and pinned SDK.
- Neutral `Project` and `Entry` domain entities.
- Application services and repository contracts for projects and entries.
- SQLite connection factory and embedded migration runner.
- Initial V0.1 schema for settings, projects, and entries.
- SQLite project and entry repositories with optimistic version checks.
- Local Workbench data-path abstraction.
- Minimal WinForms desktop host for project and entry editing.
- Package-free end-to-end V0.1 smoke test.
- GitHub Actions CI workflow.
- Agent engineering guardrails in `AGENTS.md`.

### Changed

- Implementation baseline moved from the original .NET 8 planning assumption to .NET 10 LTS for the new shared codebase.

## [0.0.0] - 2026-05-12

### Added

- Initial repository documentation: Lastenheft, Pflichtenheft, architecture document, database design, license, README, and early UI mockup.
