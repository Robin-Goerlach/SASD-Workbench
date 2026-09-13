# SASD Workbench

**SASD Workbench** is a local, modular desktop application and shared codebase for structured project, research and engineering documentation.

The current development focuses on a robust offline core: projects, entries, templates, attachments, tags, collections, typed relations, search, export and backups. Later versions add cross-cutting capabilities such as timelines, reusable resources, annotations, structured data and context snapshots. Specialized SASD Workbench applications can then build on the same core for lab work, software engineering, Linux administration, prompt experiments, biblical research, recipes and health-related documentation.

> Status: active early development / reusable V1 core foundation  
> Current technical baseline: C# / .NET 10 / Windows Forms / SQLite

---

## Screenshot

![SASD Workbench early mockup](docs/screenshots/screenshot-sasd-workbench-v1-mockup.png)

*Early UI mockup. The current implementation is further advanced than this mockup and the final application may differ.*

---

## Project Goal

The goal of SASD Workbench is to provide a practical local tool and reusable platform for documenting structured work over time.

Typical use cases include:

- project journals
- experiments and observations
- research questions and hypotheses
- source and literature notes
- software tests and bug analyses
- architecture decisions
- Linux administration notes
- prompt engineering experiments
- research notes
- recipe and food experiments
- long-running topic investigations
- timelines and event-based documentation

The application is intentionally designed as a **core platform** first. Specialized products or workflows should later be implemented through profiles, templates, modules or dedicated hosts instead of separate unrelated codebases.

A central rule is:

> Fachprofile dürfen den Core fordern, aber nicht verformen.

Features discovered in specialized projects such as Health Research or Biblical Research are moved into the shared core only when they can be expressed as a neutral, reusable capability.

---

## Current V1 Core

The current codebase contains the common local V1 backend and desktop integration for:

- local SQLite-based project and entry management
- optimistic concurrency and soft delete/archive foundations
- generic entry types and status fields
- reusable templates, including project-local and profile-wide user templates
- shared `general` templates that remain visible/usable from future specialist profiles
- reusable tags and many-to-many entry/tag assignments
- controlled attachment storage with SHA-256 and editable comments
- hierarchical collections with multiple collection memberships per entry
- typed semantic entry relations
- title/content search plus project/type/status/collection/tag filters in the application layer
- portable Markdown project export with copied attachments
- full validated backup/restore with SQLite snapshots and pre-restore safety backups
- automatic lightweight activity history for persisted Core mutations
- focused WinForms dialogs for templates, tags, attachments, search, collections, relations and activity
- desktop commands for export, backup and restore

For SQLite-backed mutations, the primary data change and its automatic activity record share one database transaction. Idempotent no-op assignments do not create duplicate history. This remains a lightweight chronological history, **not** a tamper-evident regulatory audit trail. Attachment file bytes remain outside the SQLite transaction; their metadata and activity record are transactional while the Application service uses compensating cleanup if a new file cannot be persisted successfully.

The attachment desktop workflow deliberately does not open stored files directly yet. This keeps controlled-path resolution out of WinForms until a reusable and security-reviewed open/reveal capability exists in the common Application/Infrastructure boundary.

The Core also contains stable neutral keys and reusable template definitions for `research_question`, `research_source`, `observation`, `hypothesis`, `finding` and `conclusion`. These are generic building blocks rather than specialist domain models. Canonical Core template definitions are still definitions rather than automatically seeded database rows; user templates can already be created from existing entries through the desktop workflow.

GitHub Actions builds the complete solution with warnings treated as errors and runs an end-to-end Core smoke test against real SQLite persistence, migrations, controlled attachments, template compatibility across profiles, transactional activity history, relations, search, export and backup/restore.

---

## Cross-Cutting Capabilities

The common Workbench roadmap includes reusable capabilities derived from multiple specialist notebooks:

- Research Questions
- Observations & Hypotheses
- typed semantic Entry Relations
- Research Sources / References
- Timeline & Events
- Context Snapshots
- Resource / Media Library with non-destructive annotations
- Structured Data Blocks, Measurements and Tables

Research questions, source entries, observations, hypotheses and typed relations already have neutral V1 building blocks. Timeline, generalized Resources, annotations, Context Snapshots and structured Measurements remain later roadmap work.

The common core does not contain medical, biblical, Linux-specific or other profile-specific interpretation.

---

## Architecture

Business and domain logic are separated from the Windows Forms frontend.

Current solution structure:

```text
sasd-workbench/
  docs/
  src/
    SASD.Workbench.Domain/
    SASD.Workbench.Application/
    SASD.Workbench.Infrastructure/
    SASD.Workbench.WinForms/
  tests/
    SASD.Workbench.SmokeTests/
```

Architecture principle:

```text
UI / Host
   ↓
Application
   ↓
Domain

Infrastructure implements Application abstractions for SQLite, file storage,
export and backup/restore and is wired by the host composition root.
```

`AddSasdWorkbenchCore(...)` is the canonical profile-neutral dependency-injection registration for current and future Workbench hosts. A specialist host selects its data root, runs migrations, reuses the common Core registration and adds only its own profile/UI modules.

The Domain and Application layers remain independent of Windows Forms and profile-specific UI decisions. `MainForm` is intentionally a coordinator: substantial Core tools live in focused dialogs rather than accumulating persistence or feature rules in one form.

---

## Planned Profiles / Products

The shared core can later support different profiles or dedicated Workbench hosts, for example:

| Profile / Product | Purpose |
|---|---|
| General | General project and work documentation |
| Lab / Research | Experiments, protocols, measurements, sources |
| Software / Engineering | ADRs, tests, bug analyses, releases |
| Linux Admin | Servers, changes, incidents, maintenance |
| Prompt Research | Prompts, model tests, result comparisons |
| Biblical Research | Topics, sources, persons, events, arguments, open questions |
| Food & Health | Diary, measurements, observations, nutrition and context documentation |

The current Core deliberately starts with generic primitives such as Entry, Template, Tag, Collection, Attachment and Relation. Additional generic primitives such as Timeline Event, Resource and Measurement are introduced only in the later roadmap stages where their concrete cross-profile requirements are understood.

---

## Documentation

Core project documentation:

- [010 – Lastenheft](docs/010_Lastenheft.md)
- [020 – Pflichtenheft MVP](docs/020_Pflichtenheft_MVP.md)
- [030 – Architektur](docs/030_Architektur_Dokument.md)
- [040 – Database Design](docs/040_Database_Design.md)
- [045 – Cross-Cutting Features](docs/045_Cross_Cutting_Features.md)
- [050 – Development Roadmap](docs/050_Development_Roadmap.md)
- [060 – Developer Guide](docs/060_Developer_Guide.md)
- [070 – Test Strategy](docs/070_Test_Strategy.md)
- [080 – V1 Internal Acceptance Test](docs/080_V1_Internal_Acceptance_Test.md)
- [ADR-001 – Shared Core Composition](docs/adr/ADR-001-shared-core-composition.md)
- [ADR-002 – Open Core Vocabulary](docs/adr/ADR-002-open-core-vocabulary.md)
- [ADR-003 – Transactional Lightweight Activity History](docs/adr/ADR-003-transactional-lightweight-activity-history.md)
- [CHANGELOG](CHANGELOG.md)
- [Agent / repository instructions](AGENTS.md)

`045_Cross_Cutting_Features.md` captures reusable capabilities discovered in later specialist-project discussions. ADRs record architecture decisions that should remain stable across future SASD Workbench products.

The V1 internal acceptance checklist is prepared but must still be executed on a real Windows desktop; CI does not pretend to replace human UX/recovery validation.

---

## Non-Goals for Version 1

Version 1 is intentionally local and limited.

Not planned for V1:

- cloud synchronization
- mobile app
- multi-user/team mode
- regulatory electronic signatures
- medical diagnosis or therapy recommendations
- mandatory AI integration
- full plugin system
- graph database
- full literature-management replacement
- complex domain-specific laboratory or health analysis

---

## Development Principle

Before adding a new specialized feature to the shared Core, the project should ask:

1. Do at least two different profiles plausibly benefit from it?
2. Can it be named and modeled without domain-specific terminology?
3. Can the Core store and process it without making domain interpretations?
4. Can export, backup, migration and tests remain complete?

If not, the feature belongs in the specialist profile instead of the common Core.

---

## License

This project is licensed under the MIT License. See [LICENSE](LICENSE).

---

## About SASD-GmbH

SASD-GmbH stands for **Scientific and Software Development**.

The project is intended as part of a broader SASD strategy to build practical, well-documented and maintainable tools for scientific, technical and software-oriented workflows.
