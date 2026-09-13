# SASD Workbench – Querschnittsfunktionen aus Fachprofilen

> **Dokumentstatus:** Accepted Design Supplement  
> **Projekt:** SASD Workbench  
> **Geltungsbereich:** SASD Workbench Core und spätere Fachprofile  
> **Stand:** 2026-09-13  
> **Bezug:** Ergänzung zu Lastenheft, Pflichtenheft, Architektur und Database Design

---

## 1. Zweck

Dieses Dokument überführt Funktionen, die ursprünglich in fachlich getrennten Vorhaben wie Biblical Research Notebook und Health Research Notebook entstanden sind, in allgemeine, wiederverwendbare Workbench-Fähigkeiten.

Ziel ist ausdrücklich **nicht**, Gesundheits-, Bibel-, Labor- oder Admin-Fachlogik in den Core zu ziehen. Der Core soll stattdessen generische Bausteine bereitstellen, aus denen Fachprofile eigene Workflows zusammensetzen können.

Die hier beschriebenen Fähigkeiten gelten als querschnittliche Produkterweiterung der bestehenden Workbench-Dokumentation. Wo dieses Dokument eine Funktion präzisiert, ist die allgemeine Core-Abstraktion maßgeblich; fachliche Details verbleiben im jeweiligen Profil.

---

## 2. Leitprinzip

Fachprofile sind Anforderungsgeber für den gemeinsamen Core.

Eine Funktion gehört dann in den Core oder in ein generisches Core-Modul, wenn sie:

1. in mehreren fachlich unterschiedlichen Profilen sinnvoll einsetzbar ist,
2. ohne Kenntnis fachlicher Begriffe modelliert werden kann,
3. keine fachliche Interpretation erzwingt,
4. einen stabilen wiederverwendbaren Dienst oder Datenbaustein darstellt.

Beispiele:

- Eine Blutdruckmessung ist fachlich und gehört nicht in den Core.
- Ein zeitgestempelter strukturierter Messwert ist allgemein und kann später durch ein generisches Measurement-Modul unterstützt werden.
- Eine Bibelperson ist fachlich und gehört nicht in den Core.
- Ein Ereignis auf einer Timeline und eine typisierte Beziehung zwischen zwei Entries sind allgemein.

---

## 3. Übersicht der neuen Querschnittsfunktionen

| ID | Fähigkeit | Zielversion | V1-Verhalten |
|---|---|---:|---|
| CF-001 | Timeline & Events | V2 | Architektur nicht verbauen |
| CF-002 | Research Questions | V1/V1.1 | als generischer Entry Type möglich |
| CF-003 | Observations & Hypotheses | V1.1/V2 | zunächst über Entry Types und Relations |
| CF-004 | Erweiterte typisierte Relations | V1.1 | vorhandene Entry Links erweitern |
| CF-005 | Research Sources / References | V1.1/V2 | zunächst Entry + externe Referenzdaten |
| CF-006 | Context Snapshots | V3 | Datenmodell vorbereiten, nicht implementieren |
| CF-007 | Resource / Media Library & Annotations | V2 | Attachments bleiben V1-Basis |
| CF-008 | Structured Data Blocks / Measurements / Tables | V2/V3 | V1 bleibt Markdown-first |

---

## 4. CF-001 – Timeline & Events

### 4.1 Ziel

Die Workbench soll zeitliche Zusammenhänge fachneutral darstellen können. Ein Timeline-Ereignis kann sich auf einen Entry beziehen, muss dies aber nicht zwingend tun.

Beispiele:

- Forschung: Experiment, Beobachtung, Publikation, Review.
- Biblical Research: Person, historisches Ereignis, Zeitraum, Schriftstelle.
- Health: Messung, Mahlzeit, Aktivität, Symptom, Arzttermin.
- Linux Admin: Deployment, Wartung, Incident, Konfigurationsänderung.
- Software Engineering: Requirement, Commit, Build, Release, Bug.

### 4.2 Besondere Anforderung: Zeitgenauigkeit

Die Zeitangabe darf nicht auf exakte Zeitstempel beschränkt werden. Unterstützt werden sollen perspektivisch:

- exakter Zeitpunkt,
- Datum ohne Uhrzeit,
- Monat,
- Jahr,
- ungefähres Datum,
- Zeitraum,
- unbekannte oder unscharfe Datierung.

Daher soll eine spätere Event-Struktur eine explizite `DatePrecision` bzw. vergleichbare Semantik besitzen.

### 4.3 Vorläufiges Modell

```text
TimelineEvent
  Id
  ProjectId
  EntryId?
  Title
  Description?
  EventType
  StartDateTime?
  EndDateTime?
  DatePrecision
  Approximate
  SortKey?
  CreatedAt
  UpdatedAt
```

### 4.4 Abgrenzung

Der Core kennt keine Gesundheitssymptome, Bibelpersonen oder Releases als Sonderklassen. Diese Bedeutung entsteht über `EventType`, Entry Types, Templates und Profile.

---

## 5. CF-002 – Research Questions

### 5.1 Ziel

Eine Recherche beginnt häufig mit einer Frage und nicht mit einer fertigen Notiz. Die Workbench soll deshalb Forschungs- und Analysefragen als erstklassigen Arbeitsgegenstand unterstützen.

Beispiele:

- „Was ist die Ursache dieses Fehlers?“
- „Welche Quellen stützen diese These?“
- „Wann wurde ein bestimmtes historisches Ereignis eingeordnet?“
- „Gibt es einen wiederkehrenden Zusammenhang zwischen zwei Beobachtungen?“

### 5.2 V1-Umsetzung

V1 benötigt keine neue Tabelle. `research_question` wird als generischer Entry Type geführt.

Empfohlene Vorlage:

```markdown
# Forschungsfrage

## Ausgangsfrage

## Kontext

## Quellen

## Beobachtungen

## Hypothesen

## Gegenargumente

## Zwischenfazit

## Offene Punkte

## Nächste Schritte
```

### 5.3 Beziehungen

Research Questions sollen mit Quellen, Beobachtungen, Hypothesen, Experimenten, Entscheidungen und Ergebnissen über Entry Links verbunden werden können.

---

## 6. CF-003 – Observations & Hypotheses

### 6.1 Ziel

Die Workbench muss zwischen dokumentierter Beobachtung und Interpretation unterscheiden können.

Eine Beobachtung beschreibt, **was festgestellt wurde**. Eine Hypothese beschreibt, **welcher mögliche Zusammenhang angenommen wird**.

Diese Trennung ist besonders wichtig, um Korrelation, Vermutung und Ursache nicht versehentlich gleichzusetzen.

### 6.2 Generische Entry Types

Für frühe Versionen genügen:

```text
observation
hypothesis
finding
conclusion
```

### 6.3 Reifegrad einer Hypothese

Perspektivisch soll eine Hypothese einen Status oder Bewertungsgrad erhalten können, beispielsweise:

```text
proposed
observed_once
observed_repeatedly
unclear
supported
contradicted
rejected
confirmed
```

Die konkreten Statuswerte können profilabhängig konfigurierbar sein.

### 6.4 Keine automatische Kausalitätsaussage

Der Core darf aus zeitlicher Nähe, Korrelation oder gemeinsamen Tags keine Kausalität ableiten. Beziehungen wie `possibly_related_to` müssen semantisch von `caused_by` unterschieden bleiben.

---

## 7. CF-004 – Erweiterte typisierte Entry Relations

### 7.1 Ziel

Die vorhandenen `entry_links` sollen sich langfristig zu einem leichten, relational gespeicherten Knowledge Graph entwickeln, ohne eine Graphdatenbank vorauszusetzen.

### 7.2 Empfohlene Relation Types

```text
related_to
references
based_on
supports
contradicts
confirms
refutes
possibly_related_to
caused_by
part_of
precedes
follows
variant_of
replaces
uses
compares_with
```

### 7.3 Regeln

- Relations sind gerichtet, sofern die Semantik dies erfordert.
- Symmetrische Relations wie `related_to` dürfen in der UI symmetrisch dargestellt werden.
- Der Relation Type ist fachneutral.
- Profile dürfen zusätzliche Typen definieren.
- Keine Relation darf automatisch eine fachliche Wahrheit behaupten.
- Relations sollen später Backlinks und Graph-/Netzwerkansichten ermöglichen.

### 7.4 V1-Kompatibilität

Die bestehende Tabelle `entry_links` bleibt die Grundlage. Für die Erweiterung ist keine neue Hauptentität erforderlich.

---

## 8. CF-005 – Research Sources / References

### 8.1 Ziel

Quellen sollen profilübergreifend dokumentiert und mit Entries verknüpft werden können.

Mögliche Quellentypen:

- Buch,
- wissenschaftlicher Artikel,
- Webseite,
- PDF,
- Norm / Standard,
- Dokumentation,
- GitHub-Repository,
- Video,
- Datenquelle,
- historische Quelle,
- fachliche Publikation.

### 8.2 Frühe Umsetzung

Zunächst kann `research_source` als Entry Type verwendet werden.

Empfohlene Vorlage:

```markdown
# Quelle

## Typ

## Titel

## Autoren / Herausgeber

## URL / DOI / ISBN

## Abrufdatum

## Zusammenfassung

## Relevante Aussagen

## Zitate / Fundstellen

## Bewertung der Quelle

## Verknüpfte Einträge
```

### 8.3 Spätere strukturierte Metadaten

```text
SourceMetadata
  SourceType
  Title
  Authors
  Publisher
  PublicationDate
  Url
  DOI
  ISBN
  AccessedAt
  Citation
  ReliabilityAssessment?
```

### 8.4 Interoperabilität

Die Architektur soll spätere Import-/Exportmöglichkeiten über offene Formate zulassen, insbesondere:

- BibTeX,
- RIS,
- CSL JSON,
- Zotero-kompatible Austauschformate bzw. API-Integration.

Direkte Schreibzugriffe auf fremde SQLite-Datenbanken sind nicht Teil des Integrationskonzepts.

---

## 9. CF-006 – Context Snapshots

### 9.1 Ziel

Ein Context Snapshot friert relevanten Umgebungszustand zum Zeitpunkt eines Ereignisses oder einer Beobachtung ein.

Beispiele:

- Health: Wetter, Aktivität, Schlafkontext.
- Linux: Kernel, CPU/RAM, Load, Paketstände.
- Software: Commit, Runtime, App-Version, Konfiguration.
- Labor: Umgebung, Geräteversion, Temperatur, Softwarestand.

### 9.2 Grundprinzip

Der Snapshot ist eine **historische Momentaufnahme**. Er soll nicht nachträglich auf den aktuellen Zustand externer Systeme zeigen.

### 9.3 Vorläufiges Modell

```text
ContextSnapshot
  Id
  ProjectId
  EntryId?
  SnapshotType
  CapturedAt
  Source
  DataJson
  Hash?
```

### 9.4 Versionierung

V3. V1 und V2 müssen lediglich vermeiden, Kontextinformationen dauerhaft in fachlich feste Spalten des `entries`-Kerns einzubauen.

---

## 10. CF-007 – Resource / Media Library & Annotations

### 10.1 Ziel

Attachments sollen sich langfristig von einem reinen Entry-Anhang zu wiederverwendbaren Resources entwickeln können.

Mögliche Resources:

- Bild,
- PDF,
- Screenshot,
- Audio,
- Video,
- Textdatei,
- Datensatz,
- Diagramm.

### 10.2 V1

V1 behält das bestehende Attachment-Modell:

```text
Entry -> Attachment -> kontrollierter lokaler Dateispeicher
```

Das ist bewusst einfach und robust.

### 10.3 V2

Später können Resources mehrere Entries referenzieren und zusätzliche Metadaten erhalten:

```text
Resource
  Id
  Title
  Description
  ResourceType
  OriginalFileName
  RelativePath
  MimeType
  FileSize
  Sha256Hash
  SourceUri?
  Creator?
  CapturedAt?
  CreatedAt
```

Zuordnung:

```text
entry_resources
  entry_id
  resource_id
  relation_type?
```

### 10.4 Annotationen

PDFs, Bilder und andere Medien sollen später nicht destruktiv annotiert werden können. Annotationen werden als separate Datenobjekte gespeichert.

Beispiel:

```text
ResourceAnnotation
  Id
  ResourceId
  AnnotationType
  Page?
  PositionJson?
  Text?
  CreatedAt
```

Das Originalmedium soll dadurch unverändert bleiben.

---

## 11. CF-008 – Structured Data Blocks / Measurements / Tables

### 11.1 Ziel

Markdown bleibt das einfache V1-Inhaltsformat. Langfristig soll ein Entry jedoch zusätzlich strukturierte Daten enthalten können.

Mögliche Blocktypen:

```text
Markdown
Table
Measurement
Checklist
Image
SourceReference
Quote
Chart
Code
```

### 11.2 Measurement als allgemeines Konzept

Ein Measurement ist keine Gesundheitsfunktion, sondern ein generischer strukturierter Messwert.

Beispiele:

- Temperatur,
- Laufzeit,
- Antwortzeit,
- Speicherverbrauch,
- Laborwert,
- Gewicht,
- Messwert eines Geräts.

Vorläufiges Modell:

```text
Measurement
  Id
  EntryId
  MeasurementType
  ValueNumeric?
  ValueText?
  Unit?
  MeasuredAt
  ContextSnapshotId?
```

### 11.3 V1-Abgrenzung

V1 bleibt Markdown-first. Es wird kein komplexer Blockeditor erzwungen. Das Datenmodell und die Application-Schicht sollen jedoch nicht voraussetzen, dass `content_markdown` für immer die einzige Inhaltsform bleibt.

---

## 12. Versionszuordnung

### V1 / V1.1 – kleine, risikoarme Ergänzungen

- `research_question` als Entry Type und Template,
- `research_source` als Entry Type und Template,
- `observation` und `hypothesis` als Entry Types,
- erweiterte Relation Types in `entry_links`,
- bestehende Attachments und Tags weiterverwenden,
- Architekturgrenzen dokumentieren.

### V2 – Wissens- und Medienfunktionen

- Timeline & Events,
- Resource Library,
- Media-/PDF-Annotationen,
- Source-Metadaten,
- Backlinks / Relationship Explorer,
- Saved Searches,
- erste Structured Data Blocks,
- Tabellen,
- einfache Measurement-Unterstützung,
- Diagramme/Charts aus strukturierten Daten.

### V3 – Fachprofile und Kontext

- Context Snapshots,
- erweiterte Measurement-Modelle,
- profilabhängige Statusmodelle,
- profilspezifische Relation Types,
- spezialisierte Importer,
- fachliche Timeline-Darstellungen,
- tiefere Research-/Health-/Admin-/Biblical-Module.

### V4 – Professional

Die Querschnittsfunktionen können später in Review-, Signatur-, Rollen-, Audit-, Sync- und API-Funktionen eingebunden werden. Dieses Dokument definiert dafür noch keine Compliance-Zusage.

---

## 13. Auswirkungen auf bestehende Architektur

### 13.1 Domain

Der neutrale `Entry` bleibt das zentrale Inhaltsobjekt. Neue Querschnittsfunktionen dürfen ihn nicht mit fachlichen Feldern überladen.

### 13.2 Application

Spätere Services können beispielsweise entstehen als:

```text
TimelineService
RelationService
SourceMetadataService
ResourceService
AnnotationService
StructuredDataService
ContextSnapshotService
```

Diese Services bleiben unabhängig von der UI.

### 13.3 Infrastructure

SQLite bleibt geeignet. Neue Funktionen werden über Migrationen ergänzt. Große Binärdaten verbleiben im kontrollierten Dateispeicher und werden nicht unnötig als BLOB in SQLite abgelegt.

### 13.4 UI

Die allgemeine Desktop-Anwendung darf spätere Module integrieren, aber Fachanwendungen dürfen eigene Hosts oder angepasste Oberflächen verwenden. Die Core-Libraries bleiben davon unabhängig.

---

## 14. Auswirkungen auf Database Design

Die bestehenden Tabellen bleiben gültig:

```text
projects
entries
templates
tags
entry_tags
attachments
collections
entry_collections
entry_links
activity_log
```

V1.1 benötigt für die hier beschriebenen Funktionen voraussichtlich noch keine zusätzlichen Haupttabellen.

V2/V3 können ergänzen:

```text
timeline_events
source_metadata
resources
entry_resources
resource_annotations
structured_blocks
measurements
context_snapshots
```

Wichtig: Tabellen werden erst dann eingeführt, wenn der konkrete Use Case einen strukturierten Datentyp benötigt. Vorzeitige Spezialisierung ist zu vermeiden.

---

## 15. Akzeptanzregeln für künftige Erweiterungen

Eine neue Fachanforderung darf nur dann in den gemeinsamen Core verschoben werden, wenn:

- mindestens zwei unterschiedliche Profile realistisch davon profitieren,
- die Bezeichnung fachneutral formuliert werden kann,
- die Domain keine fachliche Interpretation vornehmen muss,
- der Core dadurch nicht zu einem Sammelbecken profilspezifischer Sonderfälle wird.

Wenn diese Bedingungen nicht erfüllt sind, bleibt die Funktion im Fachprofil.

---

## 16. Beispiele für bewusste Abgrenzung

Nicht Core:

- Bibelübersetzungsvergleich,
- Verwaltung biblischer Personen als eigenes Fachmodell,
- Predigtdienst-Unterstützung,
- Blutzucker- oder Blutdrucklogik,
- medizinische Referenzbereiche,
- Diagnose- oder Therapieunterstützung,
- spezielle Ernährungsauswertung,
- Linux-Paketverwaltung,
- Prompt-Modellparameter eines bestimmten Providers.

Core bzw. generisch:

- Entry,
- Source,
- Relation,
- Timeline Event,
- Observation,
- Hypothesis,
- Measurement,
- Resource,
- Annotation,
- Context Snapshot,
- Tag,
- Collection,
- Template,
- Search,
- Export,
- Backup.

---

## 17. Konsequenz für die Produktfamilie

Die SASD Workbench wird damit nicht als Sammlung getrennt entwickelter Notizprogramme verstanden, sondern als gemeinsame Plattform mit wiederverwendbaren Kernfähigkeiten.

Fachprojekte wie Biblical Research Notebook, Health Research Notebook, Linux Admin Workbench oder Lab Workbench liefern konkrete Anforderungen. Der gemeinsame Core übernimmt daraus nur die abstrahierbaren Mechanismen.

Das reduziert Doppelentwicklung, verbessert Testbarkeit und ermöglicht, dass spätere SASD Workbench-Anwendungen voneinander profitieren, ohne ihre fachliche Eigenständigkeit zu verlieren.
