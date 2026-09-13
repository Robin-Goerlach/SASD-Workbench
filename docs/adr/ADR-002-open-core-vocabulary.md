# ADR-002 – Offenes Core-Vokabular für Entry- und Relation-Typen

> **Status:** Accepted  
> **Datum:** 2026-09-13  
> **Entscheidungsebene:** gemeinsame SASD Workbench Codebasis

## Kontext

Mehrere geplante Workbench-Produkte benötigen dieselben allgemeinen Konzepte, beispielsweise Research Questions, Sources, Observations, Hypotheses und semantische Beziehungen zwischen Entries. Gleichzeitig dürfen spätere Fachprofile wie Biblical Research, Health, Linux/Admin oder Engineering nicht gezwungen sein, jede neue fachliche Bezeichnung in den gemeinsamen Core einzubauen.

Zwei extreme Lösungen wären problematisch:

1. vollständig freie Strings ohne gemeinsame Konvention – dadurch entstehen Schreibvarianten und inkompatible Bedeutungen;
2. geschlossene C#-Enums – dadurch müsste der Core für jede spätere profilerweiterung geändert und neu veröffentlicht werden.

## Entscheidung

Der Core verwendet ein **offenes, aber konventionsgebundenes Vokabular**.

### Entry Types

`CoreEntryTypes` stellt stabile, profile-neutrale Schlüssel bereit:

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

Diese Liste ist ausdrücklich **nicht geschlossen**. Fachprofile dürfen zusätzliche Entry-Type-Schlüssel definieren.

### Relation Types

`EntryRelationTypes` stellt die gemeinsam verwendeten Relation-Schlüssel bereit:

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

Auch diese Liste bleibt offen. Neue Relation-Schlüssel werden jedoch als stabile Machine Keys normalisiert und validiert:

- lowercase,
- snake_case,
- erstes Zeichen ASCII-Buchstabe,
- danach ASCII-Buchstaben, Ziffern und `_`,
- maximal 100 Zeichen.

Dadurch können Profile eigene Beziehungen ergänzen, ohne beliebige Schreibvarianten in der Datenbank zu erzeugen.

### Core Templates

`CoreTemplateCatalog` beschreibt kleine, profile-neutrale V1-Template-Skelette für die oben genannten gemeinsamen Entry Types.

Die Definitionen werden zunächst **nicht automatisch in SQLite geseedet**. Ein automatisches Seed-Verfahren würde eine stabile fachliche Template-Identität, Update-/Override-Regeln und ein Konfliktmodell für vom Benutzer veränderte System-Templates benötigen. Diese Komplexität wird nicht vorgezogen.

Hosts oder spätere Initialisierungsdienste können die kanonischen Definitionen verwenden, ohne Markdown-Skelette mehrfach zu implementieren.

## Alternativen

### A – Geschlossene Enums

Verworfen. Sie wären typsicher, würden aber jedes neue Fachprofil unnötig an Core-Releases koppeln.

### B – Beliebige freie Strings

Verworfen. Sie erlauben zu leicht inkompatible Varianten wie `Related To`, `related-to` und `related_to`.

### C – Separate Relationstabellen pro Fachprofil

Verworfen. Die Semantik einer Beziehung ist fachübergreifend genug, um das gemeinsame Entry-Relation-Modell zu verwenden.

## Auswirkungen

### Positiv

- gemeinsame, stabile Begriffe für wiederverwendbare Funktionen;
- Profile bleiben erweiterbar, ohne den Core zu verändern;
- Relation-Daten werden früh normalisiert;
- UI, Suche, Export und spätere Knowledge-Graph-Ansichten können gemeinsame Keys verwenden;
- neutrale Template-Skelette müssen nicht in jedem Produkt neu geschrieben werden.

### Trade-offs

- Strings besitzen weniger Compiler-Sicherheit als geschlossene Enums;
- Profile müssen ihre zusätzlichen Keys selbst konsistent definieren;
- automatische System-Template-Installation bleibt eine spätere, separat zu entscheidende Funktion.

## Folgeentscheidungen

- UI-Komponenten sollen für Built-in-Keys sinnvolle Auswahlwerte anbieten, aber zusätzliche gültige Werte nicht grundsätzlich verhindern.
- Ein späteres Profil-/Modulsystem kann eigene Vokabulare registrieren, ohne die Core-Tabellen zu verändern.
- Strukturierte Spezialdaten erhalten erst dann eigene Domainobjekte/Tabellen, wenn ein konkreter fachlicher Bedarf dies rechtfertigt.
