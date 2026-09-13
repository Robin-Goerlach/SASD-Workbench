# SASD Workbench – Development Roadmap

> **Dokumentstatus:** Working Roadmap  
> **Projekt:** SASD Workbench  
> **Stand:** 2026-09-13  
> **Geltungsbereich:** gemeinsamer Workbench Core und darauf aufbauende Fachprofile

---

## 1. Ziel

Diese Roadmap übersetzt Lastenheft, Pflichtenheft, Architektur, Database Design und die Querschnittsergänzung `045_Cross_Cutting_Features.md` in eine belastbare Entwicklungsreihenfolge.

Die Priorität lautet:

1. Core stabilisieren,
2. Wiederverwendbarkeit sichern,
3. Datenhoheit und Recovery gewährleisten,
4. erst danach Komfort- und Fachfunktionen ausbauen.

---

## 2. Bereits erreicht

### V0.1 – Foundation

- Solution und Projektstruktur
- Domain / Application / Infrastructure / WinForms
- SQLite-Migrationssystem
- Projects
- Entries
- optimistic concurrency
- Soft Delete / Archive
- UTC-Zeitstempel
- minimale Desktop-UI
- CI Build
- End-to-End Smoke Test

### V0.5 – Core Content

- Templates
- Tags
- Entry-Tags
- Attachments
- kontrollierter Dateispeicher
- SHA-256 für Attachments
- Template-basierte Entry-Erzeugung
- erweiterter Smoke Test

### V1.0 – Core Backend / Recovery Foundation

Backend und technische Querschnittsfunktionen sind inzwischen vorhanden und über den End-to-End-Smoke-Test abgesichert:

- hierarchische Collections
- Mehrfachzuordnung von Entries zu Collections
- typisierte Entry Relations
- Activity Log Light
- Suche und Filter
- Markdown-Projektexport
- Full Backup mit SQLite-Snapshot
- validierter Restore mit Safety Backup
- Schutz gegen Restore-Path-Traversal
- gemeinsamer `AddSasdWorkbenchCore(...)` Composition Root für spätere Workbench-Hosts
- zentrale profile-neutrale Entry-Type-Schlüssel
- offenes, validiertes Relation-Vokabular
- wiederverwendbare profile-neutrale Core-Template-Definitionen

Noch nicht abgeschlossen ist insbesondere die vollständige Desktop-Integration der V1-Funktionen.

---

## 3. V1.0 – Local Core

V1.0 soll eine robuste, intern nutzbare lokale Workbench bilden.

### Muss

- [x] Collections mit Hierarchie
- [x] Entry kann mehreren Collections angehören
- [x] Entry Relations
- [x] Activity Log Light
- [x] Suche und Filter
- [x] Markdown-Projektexport
- [x] Full Backup
- [x] validierter Restore
- [ ] WinForms-Integration der Kernfunktionen
- [x] Migrationen im realen End-to-End-Smoke-Test wiederholt/idempotent ausführen
- [x] dokumentierter lokaler Datenpfad / zentrale Pfadabstraktion

### Relation Types

Zentral im Core definiert und als offenes Machine-Key-Vokabular validiert:

```text
related_to
references
based_on
supports
contradicts
confirms
refutes
possibly_related_to
part_of
precedes
follows
variant_of
replaces
uses
compares_with
```

Fachprofile dürfen weitere valide Relation Keys ergänzen; der Core verwendet bewusst kein geschlossenes Enum. Siehe `docs/adr/ADR-002-open-core-vocabulary.md`.

### Generische Entry Types

Im gemeinsamen Core als stabile Schlüssel vorhanden:

```text
note
research_note
research_question
research_source
observation
hypothesis
finding
conclusion
```

Diese Typen benötigen keine eigenen Tabellen. Die Liste ist offen für weitere profilspezifische Typen außerhalb des Core.

---

## 4. V1.1 – Research Comfort Layer

V1.1 soll kleine, risikoarme Funktionen ergänzen, die mehrere Profile sofort nutzen können.

### Geplant

- [x] profile-neutrale Definition für Research Question Template
- [x] profile-neutrale Definition für Research Source Template
- [x] profile-neutrale Definition für Observation Template
- [x] profile-neutrale Definition für Hypothesis Template
- [ ] kontrollierte Installation/Aktualisierung kanonischer System-Templates in persistenten Workbench-Daten
- [ ] External Links / References
- [ ] bessere Relation-UI
- [ ] Backlink-Anzeige
- [ ] Attachment-Kommentare
- [ ] Attachment Templates
- [ ] Checklists
- [ ] bessere Suche / Filter UX
- [ ] erste Quellen-Metadaten ohne vollständige Literaturverwaltung

Die Core-Template-Skelette liegen zunächst als kanonischer Katalog im Application Layer vor. Eine automatische SQLite-Seed-Logik wird bewusst erst eingeführt, wenn Identität, Benutzeranpassungen und Update-/Override-Regeln für System-Templates geklärt sind.

### Nicht Teil von V1.1

- Graphdatenbank
- komplexer Blockeditor
- automatische medizinische oder fachliche Interpretation
- Cloud Sync

---

## 5. V2.0 – Knowledge, Timeline & Resources

V2 erweitert die Workbench von einer strukturierten Journal-App zu einer allgemeinen Wissens- und Forschungsplattform.

### Timeline & Events

- `timeline_events`
- exakte und unscharfe Datierungen
- Zeiträume
- Timeline-Ansicht
- Filter nach Event Type
- Relation zu Entries

### Resource Library

- wiederverwendbare Resources
- ein Resource kann mehreren Entries zugeordnet werden
- Metadaten für Bilder, PDFs, Screenshots und andere Medien
- SHA-256
- kontrollierter Storage
- Source URI / Creator / Capture Date optional

### Annotationen

- nicht destruktive PDF-/Bild-Annotationen
- Annotation als eigenes Datenobjekt
- Text, Seite, Position und Typ

### Quellen / Literatur

- strukturiertere Source Metadata
- BibTeX Import/Export
- RIS Import/Export
- CSL JSON Vorbereitung
- Zotero-Interoperabilität über Austauschformate/API

### Structured Data

Erste generische Datenblöcke:

```text
Markdown
Table
Measurement
Checklist
SourceReference
Quote
Code
```

### Auswertung

- einfache Tabellenansicht
- CSV Import
- Diagramme aus strukturierten Daten
- Saved Searches

---

## 6. V3.0 – Fachprofile & Context

V3 macht aus dem stabilen Core eine Produktfamilie.

### Profile

- General
- Lab / Research
- Software / Engineering
- Linux Admin
- Prompt Research
- Biblical Research
- Recipe
- Food & Health

### Context Snapshots

Generische Momentaufnahmen externer Bedingungen:

```text
ContextSnapshot
  SnapshotType
  CapturedAt
  Source
  DataJson
  Hash?
```

Beispiele:

- Wetter
- Systemzustand
- Softwareversion
- Git Commit
- Geräte-/Umgebungsdaten

### Measurements

- strukturierte Messwerte
- Einheiten
- Zeitpunkt
- optionale Context-Snapshot-Verbindung
- profilabhängige Darstellung

Der Core interpretiert Messwerte nicht fachlich.

---

## 7. V4.0 – Professional Platform

- Benutzer und Rollen
- Review Workflow
- Freigaben / Signaturen
- tamper-evident Audit
- Verschlüsselung
- kontrollierter Sync
- API / CLI
- optional AI-Unterstützung
- Plugin-/Module-System

Funktionen wie Timeline, Relations, Sources, Measurements und Context Snapshots müssen so entworfen sein, dass sie später in Rechte-, Audit- und Sync-Modelle eingebunden werden können.

---

## 8. Reihenfolge der nächsten Entwicklungsschritte

Stand 2026-09-13:

1. [x] V1-Core CI vollständig grün bekommen und Backend-/Recovery-Funktionen nach `main` übernehmen.
2. [x] Gemeinsamen profile-neutralen Composition Root schaffen, damit spätere Workbench-Hosts keine Core-Verdrahtung kopieren.
3. [x] `research_question`, `research_source`, `observation`, `hypothesis`, `finding` und `conclusion` als neutrale Core Entry Types und Template-Definitionen ergänzen.
4. [x] Relation Types zentral definieren und als offenes Vokabular validieren.
5. [ ] Collections, Suche, Relations, Export und Backup/Restore in WinForms integrieren.
6. [ ] Activity Log in UI sichtbar machen.
7. [ ] WinForms-Shell in fokussierte Controls/Dialoge zerlegen, damit neue Core-Funktionen `MainForm` nicht monolithisch machen.
8. [ ] Developer Guide und Test Strategy ergänzen.
9. [ ] V1 internen Nutzertest durchführen.
10. [ ] Erst danach Timeline/Resource/Structured-Data-Design für V2 implementieren.

---

## 9. Architektur-Gates

Vor Implementierung eines neuen Fachfeatures ist zu prüfen:

- Ist es wirklich Core oder Profil?
- Gibt es mindestens zwei plausible Profile, die davon profitieren?
- Kann das Modell fachneutral benannt werden?
- Ist eine neue Tabelle wirklich notwendig?
- Reicht zunächst ein Entry Type, Template oder Relation Type?
- Bleiben Export und Backup vollständig?
- Gibt es einen Test für Migration und Roundtrip?

---

## 10. Definition of Done für Core-Funktionen

Eine Core-Funktion gilt nicht allein deshalb als fertig, weil der Code kompiliert.

Mindestens erforderlich:

- Domain/Application-Grenzen eingehalten
- persistente Speicherung getestet
- Migration vorhanden, falls Schemaänderung nötig
- Fehlerpfade berücksichtigt
- Datenexport/Backup nicht gebrochen
- CI grün
- Smoke-/Integrationstest erweitert
- Dokumentation aktualisiert
- keine profilspezifische Logik in den Core gezogen

---

## 11. Leitlinie

> Fachprofile dürfen den Core fordern, aber nicht verformen.

Neue Anforderungen aus Health, Biblical Research, Linux Admin, Lab oder anderen Workbenches werden zuerst fachlich verstanden und danach auf eine wiederverwendbare Core-Abstraktion geprüft. Nur diese Abstraktion wird in die gemeinsame Codebase übernommen.
