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

---

## 3. V1.0 – Local Core

V1.0 soll eine robuste, intern nutzbare lokale Workbench bilden.

### Muss

- Collections mit Hierarchie
- Entry kann mehreren Collections angehören
- Entry Relations
- Activity Log Light
- Suche und Filter
- Markdown-Projektexport
- Full Backup
- validierter Restore
- WinForms-Integration der Kernfunktionen
- Migrationstests
- dokumentierter lokaler Datenpfad

### Relation Types

Bereits früh allgemein halten:

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

### Generische Entry Types

Zusätzlich zu bisherigen Typen:

```text
research_question
research_source
observation
hypothesis
finding
conclusion
```

Diese Typen benötigen noch keine eigenen Tabellen.

---

## 4. V1.1 – Research Comfort Layer

V1.1 soll kleine, risikoarme Funktionen ergänzen, die mehrere Profile sofort nutzen können.

### Geplant

- Research Question Template
- Research Source Template
- Observation Template
- Hypothesis Template
- External Links / References
- bessere Relation-UI
- Backlink-Anzeige
- Attachment-Kommentare
- Attachment Templates
- Checklists
- bessere Suche / Filter UX
- erste Quellen-Metadaten ohne vollständige Literaturverwaltung

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

Nach Abschluss und Verifikation des aktuellen V1-Core-Branches:

1. V1-Core CI vollständig grün bekommen.
2. Collections, Suche, Relations, Export und Backup in WinForms integrieren.
3. Activity Log in UI sichtbar machen.
4. `research_question`, `research_source`, `observation`, `hypothesis` als neutrale Entry Types/Templates ergänzen.
5. Relation Types zentral definieren und validieren.
6. Developer Guide und Test Strategy ergänzen.
7. V1 internen Nutzertest durchführen.
8. Erst danach Timeline/Resource/Structured-Data-Design für V2 implementieren.

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
