# ADR-003 – Transactional Lightweight Activity History

**Status:** Accepted  
**Date:** 2026-09-13  
**Decision scope:** SASD Workbench Core V1

## Context

The V1 Core already contains an `activity_log` table and an `ActivityLogService`, but ordinary Core mutations originally did not create activity records automatically. A desktop host could have solved this by calling the activity service after each button action. That approach would be simple for one UI, but it would create exactly the duplication the shared Workbench codebase is intended to avoid:

- every future host would have to remember the same logging calls,
- direct Application-service use could bypass the history,
- a successful data mutation followed by a failed activity insert would leave misleading partial state,
- UI code would become responsible for a cross-cutting persistence concern.

The V1 activity log is explicitly a lightweight chronological history. It is **not** a tamper-evident audit trail and does not satisfy regulatory audit requirements.

## Decision

Automatic Core activity recording is implemented at the SQLite repository persistence boundary.

For SQLite-backed mutations:

1. the repository opens one SQLite connection,
2. it starts one transaction,
3. it performs the primary mutation,
4. it writes the automatic activity record through `SqliteActivityWriter`,
5. it commits only after both writes succeed.

If the activity insert fails, disposing the uncommitted transaction rolls the primary mutation back as well.

Stable Core activity keys live in `CoreActivityTypes`. Hosts and profiles may add genuinely profile-specific action keys, but they should reuse Core keys for Core operations.

## Why the repository boundary

The repository is the lowest shared layer that still knows whether a mutation was actually persisted. This matters for idempotent operations such as `INSERT OR IGNORE` tag or collection assignments: only a real database change should create a history item.

Recording in WinForms would duplicate behavior across hosts. Recording only in Application services would require a transaction abstraction spanning Application and Infrastructure or would risk a successful primary write followed by a failed history write. V1 does not need that additional unit-of-work abstraction because the current Core is intentionally local and SQLite-based.

## Atomicity guarantees

### Guaranteed

For mutations contained entirely in the Workbench SQLite database, the primary row change and its automatic activity record share the same SQLite transaction.

Examples include:

- project create/update/archive/delete,
- entry create/update/archive/delete,
- template metadata changes,
- tag metadata and entry-tag membership,
- collection metadata and entry-collection membership,
- entry relation create/delete,
- attachment **metadata** create/update/delete.

### Not guaranteed across SQLite and the file system

Attachment bytes are stored in controlled file storage, not inside SQLite. A database transaction cannot atomically include ordinary file-system writes.

The existing `AttachmentService` compensates a failed metadata insert by deleting the newly copied file. Attachment deletion remains a soft-delete of metadata; physical cleanup is deliberately separate. This is a practical local-first reliability model, not a distributed transaction.

## Activity contents

Automatic V1 activities deliberately remain compact. They record:

- stable action type,
- human-readable description,
- project and/or entry context where available,
- selected old/new identifiers or status values,
- timestamp from the shared `IClock`.

They do **not** store complete before/after object snapshots. Full history/versioning remains a later concern.

## Explicit activities

`ActivityLogService.RecordAsync(...)` remains available for meaningful explicit events that are not implied by a repository mutation, for example a workflow milestone or user-authored chronological note.

Automatic repository activities and explicit activities therefore complement each other rather than replace each other.

## Backup and restore

The activity table is part of the SQLite database snapshot. Restore therefore restores history to the same point in time as the rest of the Workbench state. Activities created after a backup disappear when that older backup is restored, matching the mutations they described.

The end-to-end smoke test verifies this behavior.

## Consequences

### Positive

- future Workbench hosts receive Core activity recording automatically,
- UI code does not own persistence history,
- SQLite mutations and history cannot diverge because of a second failed insert,
- idempotent no-op writes do not create false duplicate activities,
- deterministic tests continue to use the shared `IClock`,
- backup/restore naturally includes the corresponding history.

### Trade-offs

- SQLite repositories now contain a small cross-cutting dependency on `SqliteActivityWriter`,
- this is not a backend-neutral transaction abstraction,
- complete old/new snapshots are not available,
- file-system attachment operations cannot be part of the SQLite transaction,
- V1 activity history must not be marketed or documented as a regulatory audit trail.

## Revisit when

Revisit this decision if the Workbench gains:

- a second persistence backend,
- multi-user/server transactions,
- event sourcing,
- tamper-evident audit requirements,
- a formal Unit of Work across multiple repositories,
- cryptographic signing or regulated electronic records.

At that point the activity mechanism may evolve into a broader domain-event/outbox/audit architecture. V1 should not introduce that complexity prematurely.
