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
- Package-light end-to-end Core smoke test.
- GitHub Actions CI workflow.
- Agent engineering guardrails in `AGENTS.md`.
- V0.5 Core Content support for templates, tags, entry-tag assignments and controlled attachments.
- SHA-256 hashing and controlled local storage for attachment files.
- Template-based entry creation.
- V1 hierarchical collections with many-to-many entry membership.
- V1 typed entry relations and lightweight activity logging.
- V1 entry search and filters for project, type, status, collection and tag.
- Portable Markdown project export including attachment copies and hashes.
- Full local backup/restore with SQLite snapshots, archive validation, path-traversal protection and pre-restore safety backups.
- Shared `AddSasdWorkbenchCore(...)` dependency-injection registration for reusable Workbench hosts.
- Profile-neutral V1 entry type catalog for notes, research questions, sources, observations, hypotheses, findings and conclusions.
- Profile-neutral Core template catalog for the shared V1 research/knowledge workflow.
- Central open relation vocabulary with stable built-in keys and validation for profile-defined relation keys.
- Focused WinForms dialogs for project-scoped search, collection membership, typed relations and activity history.
- Desktop commands for portable Markdown project export and validated full backup/restore.
- Stable `CoreActivityTypes` keys for automatic Core history records.
- Transactional automatic activity recording for SQLite-backed project, entry, template, tag, collection, relation and attachment-metadata mutations.
- Developer Guide describing shared-Core extension rules, migration/storage boundaries and host composition.
- Test Strategy covering unit/integration growth, the real SQLite smoke path, recovery gates and failure-path expectations.
- V1 Internal Acceptance Test checklist for repeatable desktop, recovery and UX validation.
- ADR-003 documenting lightweight activity-history guarantees and explicit non-goals.

### Changed

- Implementation baseline moved from the original .NET 8 planning assumption to .NET 10 LTS for the new shared codebase.
- WinForms startup now consumes the same canonical Core registration that future specialist Workbench hosts can reuse.
- The V1 Core smoke test now validates the production composition root in addition to real persistence, migration, search, export and backup/restore round-trips.
- Entry relations now normalize stable machine keys to lowercase snake case while still allowing future profile-specific relation types.
- The desktop shell now delegates V1 Core tools to focused dialogs instead of adding persistence or feature logic directly to `MainForm`.
- SQLite repositories now write automatic activity records in the same database transaction as the primary mutation; idempotent no-op assignments do not create duplicate history.
- The V1 smoke test now forces an activity-write failure to verify rollback and verifies that backup/restore returns activity history to the same point in time as the restored data.

### Fixed

- Corrected SQL clause composition in SQLite entry search that could produce `FROM entries eWHERE ...`.
- Disabled connection pooling for transient backup snapshot/validation databases so Windows file handles do not block archive creation or restore file moves.

## [0.0.0] - 2026-05-12

### Added

- Initial repository documentation: Lastenheft, Pflichtenheft, architecture document, database design, license, README, and early UI mockup.
