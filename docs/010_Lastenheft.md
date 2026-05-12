# Lastenheft – SASD Workbench

> **Dokumentstatus:** ausführlicher Arbeitsentwurf  
> **Projekt:** SASD Workbench  
> **Komponente:** SASD Workbench Core und spätere Fachprofile  
> **Organisation:** SASD-GmbH – Scientific and Software Development  
> **Autor / Auftraggeber-Perspektive:** Robin Goerlach / SASD-GmbH  
> **Dokumenttyp:** Lastenheft / fachliche Anforderungen und Produktvision  
> **Sprache:** Deutsch  
> **Version:** 1.0 Draft  
> **Datum:** 2026-05-12  
> **Bezug:** aus dem Gespräch zur Findings-ähnlichen Anwendung und dem daraus abgeleiteten Pflichtenheft  

---

## Inhaltsverzeichnis

1. [Zweck des Dokuments](#1-zweck-des-dokuments)  
2. [Management Summary](#2-management-summary)  
3. [Ausgangssituation](#3-ausgangssituation)  
4. [Produktvision](#4-produktvision)  
5. [Grundsatzentscheidung: Core zuerst, Profile später](#5-grundsatzentscheidung-core-zuerst-profile-später)  
6. [Zielgruppen](#6-zielgruppen)  
7. [Nutzungsszenarien](#7-nutzungsszenarien)  
8. [Produktabgrenzung](#8-produktabgrenzung)  
9. [Leitprinzipien](#9-leitprinzipien)  
10. [Versionsstrategie](#10-versionsstrategie)  
11. [Funktionale Anforderungen nach Versionen](#11-funktionale-anforderungen-nach-versionen)  
12. [Fachprofile und spätere Produktfamilie](#12-fachprofile-und-spätere-produktfamilie)  
13. [Informationsobjekte aus Anwendersicht](#13-informationsobjekte-aus-anwendersicht)  
14. [Benutzeroberfläche und Bedienerlebnis](#14-benutzeroberfläche-und-bedienerlebnis)  
15. [Export, Import, Backup und Portabilität](#15-export-import-backup-und-portabilität)  
16. [Sicherheit, Datenschutz und Compliance-Abgrenzung](#16-sicherheit-datenschutz-und-compliance-abgrenzung)  
17. [Nichtfunktionale Anforderungen](#17-nichtfunktionale-anforderungen)  
18. [Abnahmekriterien je Hauptversion](#18-abnahmekriterien-je-hauptversion)  
19. [Bewusst zurückgestellte Funktionen](#19-bewusst-zurückgestellte-funktionen)  
20. [Risiken, Annahmen und offene Fragen](#20-risiken-annahmen-und-offene-fragen)  
21. [Anhang A: Initiale Vorlagen aus Anwendersicht](#21-anhang-a-initiale-vorlagen-aus-anwendersicht)  
22. [Anhang B: Kompakte Anforderungsliste](#22-anhang-b-kompakte-anforderungsliste)  
23. [Zusammenfassung](#23-zusammenfassung)  

---

## 1. Zweck des Dokuments

Dieses Lastenheft beschreibt aus Auftraggeber- und Anwendersicht, **was** die SASD Workbench leisten soll. Es legt die fachlichen Ziele, die gewünschten Funktionen, die Versionierung des Funktionsumfangs, die Zielgruppen, die Abgrenzungen und die nichtfunktionalen Erwartungen fest.

Das Lastenheft ist bewusst nicht identisch mit dem Pflichtenheft. Während das Pflichtenheft stärker beschreibt, **wie** die Anforderungen umgesetzt werden sollen, beschreibt dieses Lastenheft vor allem:

- welchen Nutzen die Anwendung stiften soll,
- welche fachlichen Funktionen benötigt werden,
- welche Produktstufen geplant sind,
- welche Fachprofile später entstehen sollen,
- welche Funktionen in welcher Version erwartet werden,
- welche Grenzen und Risiken berücksichtigt werden müssen.

Das Dokument soll als Grundlage dienen für:

- das GitHub-Repository,
- spätere Meilensteine und Issues,
- eine Priorisierung der Entwicklung,
- die Ableitung des Pflichtenhefts,
- technische Designentscheidungen,
- spätere Anwenderdokumentation,
- eine mögliche Produkt- und Portfolio-Beschreibung der SASD-GmbH.

---

## 2. Management Summary

Die **SASD Workbench** soll eine lokale, modulare Anwendung zur strukturierten Dokumentation von Projekten, Experimenten, Recherchen und technischen Arbeitsprozessen werden. Sie ist inspiriert von der Grundidee eines elektronischen Laborjournals, soll aber wesentlich breiter einsetzbar sein.

Die Anwendung soll zunächst als lokale Einzelplatzanwendung entstehen. Die erste produktive Version soll Anwendern ermöglichen, Projekte anzulegen, Einträge zu erfassen, Vorlagen zu verwenden, Anhänge zu speichern, Inhalte zu suchen, Arbeitsergebnisse zu exportieren und regelmäßig Backups zu erstellen.

Langfristig soll daraus eine Plattform entstehen, die über Profile und Module unterschiedliche Arbeitsbereiche unterstützt:

- allgemeine Projekt- und Arbeitsdokumentation,
- wissenschaftliche Experimente und Laborjournal,
- Softwareentwicklung und SASD Engineering Standards,
- Linux-Administration und Systemdokumentation,
- Prompt-Engineering und KI-Experimente,
- Bibel-/Themenrecherche über längere Zeiträume,
- Rezepte, Ernährung, Verträglichkeit und persönliche Gesundheitsdokumentation.

Die zentrale Produktentscheidung lautet:

> **Es wird zuerst ein neutraler, robuster Core entwickelt. Fachliche Spezialisierungen entstehen später über Profile, Vorlagen und Module.**

Dadurch wird vermieden, mehrere ähnliche Anwendungen getrennt zu entwickeln. Gemeinsame Funktionen wie Einträge, Anhänge, Tags, Suche, Export, Backup und Vorlagen werden nur einmal gebaut und später in verschiedenen Profilen wiederverwendet.

---

## 3. Ausgangssituation

Die Ausgangsfrage war, ob eine Anwendung ähnlich einer elektronischen Forschungs- oder Laborjournal-App nachprogrammiert werden kann. Bei der Analyse wurde deutlich, dass eine solche Anwendung mehr ist als eine einfache Notizverwaltung.

Die relevante Funktionslandschaft umfasst unter anderem:

- Projekte,
- Collections / Sammlungen,
- verschiedene Eintragstypen,
- Experimente,
- Forschungsnotizen,
- Meeting Notes,
- Schnellnotizen / Stickies,
- Protokolle / Procedures / Recipes als wiederverwendbare Vorlagen,
- Anhänge,
- Attachment Templates,
- Tabellen und CSV-Dateien,
- Timer und Zeitstempel,
- Statusmodelle,
- Suche und Filter,
- PDF-/Markdown-/HTML-Export,
- automatische Journale,
- Import-/Export-Archive,
- interne Links und Querverweise,
- Ressourcenbibliothek,
- lokale Datenhoheit,
- spätere Sync-, Team- und Sicherheitsfunktionen.

Für SASD ist dieses Projekt besonders interessant, weil es zu mehreren langfristigen Themen passt: wissenschaftsnahe Software, strukturierte Dokumentation, Projektarbeit, Softwareentwicklung, Linux-Administration, KI-Unterstützung, Forschungsdatenmanagement und persönliche Wissensorganisation.

---

## 4. Produktvision

Die SASD Workbench soll eine Anwendung werden, mit der Anwender über lange Zeiträume strukturiert arbeiten und Erkenntnisse nachvollziehbar dokumentieren können.

Die Anwendung soll nicht nur lose Notizen sammeln, sondern Arbeitsprozesse unterstützen:

- ein Thema beginnen,
- Informationen sammeln,
- Experimente oder Tests durchführen,
- Ergebnisse dokumentieren,
- Varianten vergleichen,
- offene Fragen verwalten,
- Entscheidungen begründen,
- Anhänge und Quellen sichern,
- Zwischenstände exportieren,
- später Entwicklungen nachvollziehen.

Die Workbench soll insbesondere helfen bei:

- **Nachvollziehbarkeit:** Was wurde wann warum getan?
- **Reproduzierbarkeit:** Unter welchen Bedingungen entstand ein Ergebnis?
- **Strukturierung:** Welche Informationen gehören zu welchem Projekt oder Thema?
- **Portabilität:** Wie können Daten exportiert, gesichert und weiterverwendet werden?
- **Langfristigkeit:** Wie lassen sich Themen über Wochen, Monate oder Jahre bearbeiten?
- **Modularität:** Wie kann dieselbe Kern-App für verschiedene Fachbereiche genutzt werden?

Langfristiges Ziel ist eine SASD-Plattform für strukturierte Erkenntnisarbeit.

---

## 5. Grundsatzentscheidung: Core zuerst, Profile später

### 5.1 Entscheidung

Es soll **eine gemeinsame Kernanwendung** entwickelt werden: **SASD Workbench Core**.

Darauf aufbauend können später verschiedene Profile oder Editionen entstehen:

```text
SASD Workbench Core
  ├── Allgemeines Projektjournal
  ├── SASD Workbench Lab
  ├── SASD Workbench Engineering
  ├── SASD Workbench Admin
  ├── SASD Workbench Prompt
  ├── SASD Workbench Biblical Research
  ├── SASD Workbench Recipe
  └── SASD Workbench Food & Health
```

### 5.2 Begründung

Viele benötigte Funktionen sind in allen Fachbereichen gleich:

- Projekte,
- Einträge,
- Vorlagen,
- Anhänge,
- Suche,
- Tags,
- Status,
- Export,
- Backup,
- interne Links,
- Änderungsverlauf.

Es wäre unwirtschaftlich und wartungsintensiv, für Labor, Linux-Administration, Prompt-Experimente, Rezepte und Bibelrecherche jeweils eine vollständig getrennte Anwendung zu entwickeln.

### 5.3 Konsequenz

Die Fachlichkeit wird zunächst über **Eintragstypen**, **Vorlagen**, **Tags**, **Statusmodelle** und später **Profile** abgebildet. Erst wenn ein Bereich echte Spezialfunktionen braucht, wird daraus ein Modul.

---

## 6. Zielgruppen

### 6.1 Primäre Zielgruppe der ersten Version

Die erste Version richtet sich an Einzelanwender, insbesondere:

- SASD intern,
- Entwickler,
- technische Dokumentatoren,
- Linux-Administratoren,
- Berater,
- wissenschaftlich arbeitende Einzelpersonen,
- Maker,
- Personen mit länger laufenden Recherchen oder Experimenten.

### 6.2 Spätere Zielgruppen

Spätere Versionen können zusätzlich adressieren:

- kleine Forschungseinrichtungen,
- kleine Labore,
- IT-Dienstleister,
- Trainer und Dozenten,
- Projektteams,
- Personen mit strukturierter Rezept- oder Ernährungsdokumentation,
- Anwender, die Themen in Etappen ausarbeiten möchten.

### 6.3 Nicht primäre Zielgruppen der frühen Version

Nicht primär adressiert in V1:

- große Unternehmen mit komplexen Compliance-Anforderungen,
- regulierte Labore mit validierter ELN-/LIMS-Pflicht,
- medizinische Einrichtungen mit Diagnose-/Therapieprozessen,
- Teams mit sofortiger Echtzeit-Kollaboration,
- mobile-first Anwender.

---

## 7. Nutzungsszenarien

### 7.1 Allgemeines Projektjournal

Ein Anwender dokumentiert ein Softwareprojekt. Er legt Projektnotizen, Entscheidungen, offene Fragen, Testergebnisse, Dateianhänge und Exportberichte ab.

### 7.2 Software-Fehleranalyse

Ein Entwickler erfasst ein Problem, dokumentiert Umgebung, Reproduktionsschritte, Logs, Screenshots, Analyse, Lösung und Testergebnis.

### 7.3 Linux-Admin-Dokumentation

Ein Administrator dokumentiert Server, Konfigurationen, Änderungen, Wartungsarbeiten, Incidents, ausgeführte Befehle, betroffene Dateien und Rollback-Hinweise.

### 7.4 Experiment- und Forschungsjournal

Ein Anwender beschreibt ein Experiment mit Ziel, Hypothese, Material, Durchführung, Beobachtungen, Ergebnis, Anhängen und Fazit.

### 7.5 Prompt-Experiment

Ein Anwender dokumentiert einen Prompt, das verwendete Modell, Parameter, Antwort, Bewertung, Schwächen und nächste Verbesserungsvariante.

### 7.6 Langfristige Bibel-/Themenrecherche

Ein Anwender bearbeitet über längere Zeit mehrere parallele Themen, sammelt Bibelstellen, Quellen, Argumentationsketten, offene Fragen und Zwischenfazits.

### 7.7 Rezeptentwicklung

Ein Anwender entwickelt Rezepte, dokumentiert Zutaten, Mengen, Zubereitung, Varianten, Geschmack, Verträglichkeit und Verbesserungsideen.

### 7.8 Food & Health / Ernährungstagebuch

Ein Anwender dokumentiert Mahlzeiten, Rezepte, Verträglichkeit, persönliche Beobachtungen und optional Blutwerte. Die Anwendung strukturiert Daten, stellt aber keine Diagnose und gibt keine Therapieempfehlung.

---

## 8. Produktabgrenzung

### 8.1 Was die Anwendung sein soll

Die SASD Workbench soll sein:

- eine lokale Dokumentations- und Arbeitsplattform,
- ein strukturiertes Projekt- und Erkenntnisjournal,
- ein Werkzeug für wiederverwendbare Vorlagen,
- eine Anhangs- und Quellenverwaltung für Arbeitseinträge,
- eine Plattform für spätere Fachprofile,
- eine exportierbare und backupfähige Wissensbasis.

### 8.2 Was die Anwendung nicht sein soll

Die frühe Workbench-Version soll ausdrücklich nicht sein:

- ein vollständiges LIMS,
- ein zertifiziertes elektronisches Laborjournal,
- eine regulierte Medizinsoftware,
- ein Therapie- oder Diagnosewerkzeug,
- ein vollständiges Projektmanagementsystem,
- ein Ersatz für professionelle Literaturverwaltung,
- ein vollständiger Passwortmanager,
- ein Echtzeit-Kollaborationstool,
- eine Cloud-Pflicht-Anwendung,
- ein 1:1-Klon einer bestehenden App.

---

## 9. Leitprinzipien

### 9.1 Lokal zuerst

Die erste Version soll vollständig lokal und offline funktionieren. Anwender sollen ihre Daten kontrollieren können.

### 9.2 Einfach nutzbar, aber erweiterbar

V1 soll schnell nutzbar sein. Gleichzeitig darf das Daten- und Funktionsmodell spätere Erweiterungen nicht blockieren.

### 9.3 Keine zu frühe Spezialisierung

Die App soll nicht von Anfang an nur „Labor“, „Linux“ oder „Rezepte“ sein. Der Kern bleibt neutral.

### 9.4 Exportierbarkeit statt Lock-in

Daten sollen in offenen oder gut lesbaren Formaten exportierbar sein, insbesondere Markdown, HTML, später PDF und ZIP-Archive.

### 9.5 Nachvollziehbarkeit

Die Anwendung soll Änderungen, Status, Anhänge und Arbeitsschritte nachvollziehbar machen.

### 9.6 Datenschutzbewusstsein

Die Anwendung soll keine Daten ungefragt an Cloud-, KI- oder Sync-Dienste senden.

### 9.7 Qualität vor Funktionsüberladung

Lieber eine kleine, stabile V1 als eine große, instabile Anwendung mit zu vielen unfertigen Funktionen.

---

## 10. Versionsstrategie

Die Entwicklung erfolgt in mehreren Stufen.

| Version | Ziel | Charakter |
|---:|---|---|
| 0.1 | Technischer Prototyp | Machbarkeit und Grundstruktur |
| 0.5 | Interner MVP | erste testbare Arbeitsversion |
| 1.0 | Lokaler Desktop-MVP | erste produktiv nutzbare Einzelplatzversion |
| 1.1 | Komfort- und Qualitätsausbau | bessere Nutzbarkeit und Datenqualität |
| 2.0 | Journal, Export, Historie | professionelleres Arbeitsjournal |
| 3.0 | Fachprofile | Produktfamilie auf gemeinsamem Core |
| 4.0 | Team, Sicherheit, Sync, KI | fortgeschrittene Profi-Funktionen |

---

# 11. Funktionale Anforderungen nach Versionen

Die folgenden Anforderungen beschreiben den gewünschten Leistungsumfang aus Auftraggeber- und Anwendersicht. Die genaue technische Umsetzung wird im Pflichtenheft und in späteren Design-Dokumenten beschrieben.

---

## 11.1 Version 0.1 – Technischer Prototyp

### LH-001 Grundanwendung starten

**Version:** 0.1  
**Priorität:** Muss

Die Anwendung soll grundsätzlich startbar sein und eine erkennbare Oberfläche besitzen. Es muss möglich sein, die Idee der SASD Workbench anhand eines einfachen Prototyps zu demonstrieren.

**Gewünschter Nutzen:**  
Frühzeitig soll sichtbar werden, ob sich das Bedienmodell und die Grundstruktur eignen.

### LH-002 Lokalen Datenbereich anlegen

**Version:** 0.1  
**Priorität:** Muss

Die Anwendung soll einen lokalen Datenbereich anlegen oder erkennen können. In diesem Bereich werden später Datenbank, Anhänge, Exporte, Backups und Vorlagen gespeichert.

**Gewünschter Nutzen:**  
Der Anwender soll verstehen, wo seine Daten liegen, und die Anwendung soll keine versteckte Cloud-Pflicht haben.

### LH-003 Erstes Projekt anlegen

**Version:** 0.1  
**Priorität:** Muss

Der Anwender soll ein erstes Projekt mit Titel und Beschreibung anlegen können.

**Gewünschter Nutzen:**  
Projekte bilden den obersten Arbeitskontext. Bereits im Prototyp soll klar sein, dass die Workbench projektorientiert arbeitet.

### LH-004 Einfachen Eintrag anlegen

**Version:** 0.1  
**Priorität:** Muss

Der Anwender soll innerhalb eines Projekts einen einfachen Eintrag mit Titel und Text erfassen können.

**Gewünschter Nutzen:**  
Der zentrale Arbeitsfluss „Projekt öffnen → Eintrag erfassen → speichern → wiederfinden“ muss früh nachgewiesen werden.

### LH-005 Daten nach Neustart wiederfinden

**Version:** 0.1  
**Priorität:** Muss

Erfasste Projekte und Einträge sollen nach einem Neustart der Anwendung wieder vorhanden sein.

**Gewünschter Nutzen:**  
Der Prototyp muss echte Speicherung demonstrieren, nicht nur eine temporäre Oberfläche.

---

## 11.2 Version 0.5 – Interner MVP

### LH-010 Projektverwaltung

**Version:** 0.5  
**Priorität:** Muss

Der Anwender soll Projekte erstellen, bearbeiten, archivieren und löschen können. Projekte sollen einen Namen, eine Beschreibung, einen Status und Zeitstempel besitzen.

**Gewünschte Funktionen:**

- Projekt anlegen,
- Projekt bearbeiten,
- Projekt archivieren,
- Projekt löschen mit Sicherheitsabfrage,
- Projektliste anzeigen,
- archivierte Projekte optional ausblenden.

**Nutzen:**  
Die Anwendung wird als Arbeitsraum für mehrere parallele Themen nutzbar.

### LH-011 Eintragsverwaltung

**Version:** 0.5  
**Priorität:** Muss

Der Anwender soll Einträge erstellen, öffnen, bearbeiten, speichern, duplizieren und löschen können.

**Gewünschte Funktionen:**

- Eintrag anlegen,
- Eintrag speichern,
- Eintrag bearbeiten,
- Eintrag duplizieren,
- Eintrag löschen oder archivieren,
- Änderungsdatum anzeigen,
- Erstellungsdatum anzeigen.

**Nutzen:**  
Einträge sind das zentrale Arbeitsobjekt. Die Verwaltung muss zuverlässig und verständlich sein.

### LH-012 Eintragstypen

**Version:** 0.5  
**Priorität:** Muss

Ein Eintrag soll einen fachlichen Typ besitzen. Der Typ hilft beim Filtern, Sortieren und Anwenden passender Vorlagen.

**Initiale Eintragstypen:**

- Allgemeine Notiz,
- Experiment,
- Research Note,
- Meeting Note,
- Sticky / Schnellnotiz,
- Fehleranalyse,
- Testlauf,
- Architekturentscheidung,
- Linux-Admin-Notiz,
- Prompt-Test,
- Rezeptversuch,
- Bibel-/Themen-Recherche.

**Nutzen:**  
Die Anwendung bleibt allgemein, kann aber unterschiedliche Arbeitsformen abbilden.

### LH-013 Statusmodell

**Version:** 0.5  
**Priorität:** Muss

Einträge sollen einen Status besitzen, der den Bearbeitungsstand ausdrückt.

**Initiale Statuswerte:**

- Entwurf,
- Geplant,
- In Arbeit,
- In Prüfung,
- Abgeschlossen,
- Zurückgestellt,
- Abgebrochen,
- Archiviert.

**Später mögliche Statuswerte:**

- Signiert,
- Freigegeben,
- Gesperrt.

**Nutzen:**  
Der Anwender erkennt offene, laufende, abgeschlossene und zurückgestellte Arbeiten.

### LH-014 Tags

**Version:** 0.5  
**Priorität:** Muss

Einträge sollen frei verschlagwortet werden können.

**Gewünschte Funktionen:**

- Tags hinzufügen,
- Tags entfernen,
- vorhandene Tags wiederverwenden,
- nach Tags filtern,
- Tags in Listen anzeigen.

**Nutzen:**  
Tags erlauben flexible Querverbindungen über Projekte und Sammlungen hinweg.

### LH-015 Suche

**Version:** 0.5  
**Priorität:** Muss

Der Anwender soll Einträge über Titel und Inhalt durchsuchen können.

**Gewünschte Funktionen:**

- Suchfeld,
- Suche im aktuellen Projekt,
- Suche in Titeln,
- Suche im Inhalt,
- Anzeige der Trefferliste.

**Nutzen:**  
Eine Dokumentations-App ist nur wertvoll, wenn Inhalte später wiedergefunden werden.

### LH-016 Erste Vorlagen

**Version:** 0.5  
**Priorität:** Muss

Der Anwender soll neue Einträge aus Vorlagen erzeugen können. Eine Vorlage soll kopiert werden, sodass spätere Änderungen am Eintrag die Vorlage nicht verändern.

**Initiale Vorlagen:**

- Allgemeine Notiz,
- Experiment,
- Fehleranalyse,
- Meeting-Protokoll,
- Architekturentscheidung / ADR,
- Linux-Wartungsprotokoll,
- Prompt-Test,
- Rezeptversuch,
- Bibel-/Themenrecherche.

**Nutzen:**  
Vorlagen machen die App sofort praktisch nutzbar und bilden die erste Stufe der späteren Profile.

### LH-017 Grundlegende Anhangsverwaltung

**Version:** 0.5  
**Priorität:** Sollte

Einträge sollen Dateien als Anhänge aufnehmen können. Beim Hinzufügen soll die Datei in den Workbench-Datenbereich kopiert werden.

**Gewünschte Funktionen:**

- Datei anhängen,
- angehängte Datei anzeigen,
- Datei öffnen,
- Datei im Dateimanager anzeigen,
- ursprünglichen Dateinamen behalten.

**Nutzen:**  
Experimente, Tests, Admin-Dokumentation und Recherchen benötigen Screenshots, PDFs, CSV-Dateien, Logdateien, Bilder und andere Nachweise.

---

## 11.3 Version 1.0 – Lokaler Desktop-MVP

### LH-020 Drei-Bereich-Oberfläche

**Version:** 1.0  
**Priorität:** Muss

Die Anwendung soll ein übersichtliches Arbeitslayout besitzen:

```text
[Navigation] | [Eintragsliste] | [Editor / Details]
```

**Gewünschte Bereiche:**

- linke Navigation für Projekte, Sammlungen, Typen, Status, Tags,
- mittlere Liste für Einträge,
- rechter Bereich für Inhalt, Metadaten, Anhänge und Aktionen.

**Nutzen:**  
Der Anwender kann Projekte, Einträge und Inhalte gleichzeitig im Blick behalten.

### LH-021 Collections / Sammlungen

**Version:** 1.0  
**Priorität:** Muss

Der Anwender soll Einträge innerhalb eines Projekts in Sammlungen organisieren können.

**Gewünschte Funktionen:**

- Sammlung erstellen,
- Sammlung bearbeiten,
- Sammlung löschen oder archivieren,
- Einträge einer Sammlung zuordnen,
- nach Sammlung filtern.

**Nutzen:**  
Sammlungen erlauben thematische Ordnung, z. B. „Experimente 2026“, „Offene Fragen“, „Server“, „Rezepte“, „Königreich Gottes“.

### LH-022 Erweiterte Filter

**Version:** 1.0  
**Priorität:** Muss

Die Anwendung soll nicht nur Volltextsuche, sondern kombinierbare Filter bieten.

**Filterkriterien:**

- Projekt,
- Collection,
- Eintragstyp,
- Status,
- Tag,
- Datum / Änderungsdatum optional.

**Nutzen:**  
Bei wachsender Datenmenge bleiben Inhalte auffindbar.

### LH-023 Vorlagenbibliothek

**Version:** 1.0  
**Priorität:** Muss

Vorlagen sollen nicht nur versteckt im Code existieren, sondern als Bibliothek sichtbar und nachvollziehbar sein.

**Gewünschte Funktionen:**

- Vorlagenliste anzeigen,
- Vorlage auswählen,
- aus Vorlage Eintrag erzeugen,
- Vorlagen nach Kategorie gruppieren,
- Beschreibung einer Vorlage anzeigen.

**Nutzen:**  
Die Vorlagenbibliothek wird zum Einstiegspunkt für verschiedene Workflows.

### LH-024 Stabile Anhangsverwaltung

**Version:** 1.0  
**Priorität:** Muss

Die Anhangsverwaltung soll zuverlässig genug für produktive Nutzung sein.

**Gewünschte Funktionen:**

- Datei anhängen,
- Datei in Workbench-Datenordner kopieren,
- Datei öffnen,
- Datei im Dateimanager anzeigen,
- Kommentar zum Anhang erfassen,
- Anhang entfernen,
- Dateigröße anzeigen,
- ursprünglichen Dateinamen anzeigen,
- Sicherheitsabfrage beim Entfernen.

**Nutzen:**  
Anwender sollen sicher sein, dass wichtige Dateien nicht verloren gehen, nur weil die Originaldatei verschoben wurde.

### LH-025 Markdown-Export

**Version:** 1.0  
**Priorität:** Muss

Einträge und Projekte sollen als Markdown exportiert werden können.

**Gewünschte Funktionen:**

- Einzeleintrag als Markdown exportieren,
- Projekt als Markdown-Ordner exportieren,
- Metadaten in den Export aufnehmen,
- Anhänge optional mit exportieren,
- relative Links zu Anhängen erzeugen.

**Nutzen:**  
Markdown unterstützt GitHub, Obsidian, Doku-Workflows und langfristige Portabilität.

### LH-026 HTML-Export

**Version:** 1.0  
**Priorität:** Sollte

Einträge und Projekte sollen optional als einfache HTML-Dokumente exportiert werden können.

**Gewünschte Funktionen:**

- Einzeleintrag als HTML,
- Projektübersicht als HTML,
- Anhänge verlinken,
- Metadaten anzeigen,
- lokale Browseranzeige ermöglichen.

**Nutzen:**  
HTML eignet sich für schnelle Vorschau, Weitergabe und Dokumentation ohne Spezialsoftware.

### LH-027 Backup

**Version:** 1.0  
**Priorität:** Muss

Die Anwendung soll vollständige Backups erstellen können.

**Backup-Inhalt:**

- Datenbank,
- Anhänge,
- Vorlagen,
- Konfiguration,
- optional Exportmanifest.

**Nutzen:**  
Lokale Daten sind wertvoll. Backup ist daher Kernfunktion, nicht Zusatzkomfort.

### LH-028 Restore

**Version:** 1.0  
**Priorität:** Muss

Die Anwendung soll ein Backup wiederherstellen können.

**Gewünschte Funktionen:**

- Backup auswählen,
- Struktur prüfen,
- vor Überschreiben warnen,
- Datenbank und Anhänge wiederherstellen,
- erfolgreiche Wiederherstellung bestätigen.

**Nutzen:**  
Der Anwender soll im Fehlerfall oder bei Rechnerwechseln seine Daten wiederherstellen können.

### LH-029 Activity Log Light

**Version:** 1.0  
**Priorität:** Sollte

Die Anwendung soll wichtige Aktionen in einfacher Form protokollieren.

**Zu protokollierende Aktionen:**

- Projekt erstellt,
- Eintrag erstellt,
- Eintrag geändert,
- Status geändert,
- Anhang hinzugefügt,
- Anhang entfernt,
- Export erstellt,
- Backup erstellt,
- Restore durchgeführt.

**Nutzen:**  
Der Anwender erhält eine einfache Nachvollziehbarkeit, ohne dass V1 bereits eine manipulationssichere Audit-Lösung sein muss.

### LH-030 Interne Querverweise vorbereiten

**Version:** 1.0  
**Priorität:** Sollte

Einträge sollen perspektivisch miteinander verknüpft werden können.

**Mögliche Beziehungstypen:**

- basiert auf,
- bestätigt,
- widerspricht,
- ersetzt,
- gehört zu,
- verwendet,
- wiederholt,
- vergleicht mit.

**Nutzen:**  
Die Workbench entwickelt sich vom Notizspeicher zum Wissensnetz.

---

## 11.4 Version 1.1 – Komfort- und Qualitätsausbau

### LH-040 Attachment Templates

**Version:** 1.1  
**Priorität:** Sollte

Der Anwender soll neue Anhänge aus Dateivorlagen erzeugen können.

**Beispiele:**

- leere CSV-Tabelle,
- Messwerttabelle,
- Markdown-Checkliste,
- Konfigurationsdatei-Beispiel,
- Prompt-Testdatei,
- Rezept-Tabelle.

**Nutzen:**  
Wiederkehrende Arbeitsdateien können standardisiert erzeugt werden.

### LH-041 Checklisten

**Version:** 1.1  
**Priorität:** Sollte

Einträge sollen einfache Checklisten enthalten können.

**Gewünschte Funktionen:**

- Checkliste in Markdown,
- offene und erledigte Punkte,
- später optional direkte UI-Unterstützung zum Abhaken.

**Nutzen:**  
Checklisten unterstützen Experimente, Wartung, Tests, Rezepte und Rechercheschritte.

### LH-042 Linkliste pro Eintrag

**Version:** 1.1  
**Priorität:** Sollte

Ein Eintrag soll strukturierte externe Links speichern können.

**Felder:**

- Titel,
- URL,
- Kommentar,
- Abrufdatum,
- Linktyp.

**Nutzen:**  
Quellen, Dokumentationen, Videos, Paper, GitHub-Repositories und Normen bleiben beim Eintrag nachvollziehbar.

### LH-043 Verbesserte Fehlerbehandlung

**Version:** 1.1  
**Priorität:** Muss

Die Anwendung soll typische Fehler verständlich anzeigen.

**Beispiele:**

- Datenordner nicht beschreibbar,
- Datenbank nicht zugreifbar,
- Anhang kann nicht gelesen werden,
- Exportziel nicht beschreibbar,
- Backup unvollständig,
- ungültige Eingaben.

**Nutzen:**  
Der Anwender soll verstehen, was schiefgelaufen ist und was er tun kann.

### LH-044 Anwenderhandbuch

**Version:** 1.1  
**Priorität:** Muss

Es soll ein Anwenderhandbuch geben.

**Inhalte:**

- Installation,
- erster Start,
- Projekt anlegen,
- Eintrag erfassen,
- Vorlage verwenden,
- Anhang hinzufügen,
- Suche,
- Export,
- Backup,
- Restore,
- typische Fehler.

**Nutzen:**  
Das Projekt wird nachvollziehbar, testbar und später besser veröffentlichbar.

---

## 11.5 Version 2.0 – Journal, PDF, Historie und Ressourcen

### LH-050 PDF-Export

**Version:** 2.0  
**Priorität:** Muss für V2

Die Anwendung soll Einträge, Projekte und ausgewählte Datensätze als PDF exportieren können.

**Gewünschte Funktionen:**

- Einzeleintrag als PDF,
- Projektjournal als PDF,
- Zeitraum auswählen,
- Collection auswählen,
- Status oder Tags auswählen,
- Deckblatt mit Metadaten,
- Inhaltsverzeichnis optional,
- Anhänge optional einbeziehen oder verlinken.

**Nutzen:**  
PDF-Exporte eignen sich für Archivierung, Weitergabe, Berichte und Portfolio-Unterlagen.

### LH-051 Automatisches Journal

**Version:** 2.0  
**Priorität:** Sollte

Die Anwendung soll aus Einträgen automatisch Journale erzeugen können.

**Journalarten:**

- Tagesjournal,
- Wochenjournal,
- Projektjournal,
- Arbeitsjournal nach Collection,
- vollständiges chronologisches Journal.

**Nutzen:**  
Der Anwender kann seine Arbeit als fortlaufende Dokumentation ausgeben.

### LH-052 Versionshistorie

**Version:** 2.0  
**Priorität:** Muss für V2

Die Anwendung soll frühere Versionen eines Eintrags nachvollziehbar machen.

**Gewünschte Funktionen:**

- Versionen anzeigen,
- Änderungszeitpunkt anzeigen,
- Autor anzeigen,
- Änderungsnotiz optional,
- frühere Versionen einsehen,
- Wiederherstellung diskutieren oder vorbereiten.

**Nutzen:**  
Wichtige Arbeitsstände gehen nicht verloren und Entwicklungen werden nachvollziehbar.

### LH-053 Erweiterter Activity Log

**Version:** 2.0  
**Priorität:** Muss für V2

Das einfache Aktivitätsprotokoll soll detaillierter werden.

**Gewünschte Informationen:**

- Aktion,
- Zeitpunkt,
- betroffener Eintrag,
- alter und neuer Status,
- optional alte/neue Werte,
- Autor,
- Kommentar.

**Nutzen:**  
Die Workbench wird besser für professionelle Dokumentation geeignet.

### LH-054 Timer und Zeitstempel

**Version:** 2.0  
**Priorität:** Sollte

Die Anwendung soll Zeitstempel, Stoppuhren und Countdowns unterstützen.

**Gewünschte Funktionen:**

- Zeitstempel in Eintrag einfügen,
- Stoppuhr starten/stoppen,
- Countdown setzen,
- Timer einem Eintrag zuordnen,
- aktive Timer anzeigen,
- später Benachrichtigungen.

**Nutzen:**  
Experimente, Kochversuche, Tests und Arbeitsabläufe profitieren von dokumentierten Zeiten.

### LH-055 Ressourcen-Bibliothek

**Version:** 2.0  
**Priorität:** Sollte

Die Anwendung soll eine Ressourcenbibliothek bieten.

**Mögliche Ressourcen:**

- PDF-Referenzen,
- Bilder,
- Textdateien,
- Weblinks,
- HTML-/JavaScript-Minertools,
- Cheat Sheets,
- Periodensystem,
- Git-Referenz,
- Linux-Kommandos,
- SQL-Cheat-Sheet,
- Prompt-Checklisten,
- Bibel-/Quellenhilfen.

**Nutzen:**  
Häufig benötigte Referenzen stehen innerhalb des Arbeitskontexts bereit.

### LH-056 CSV-Import und Tabellenvorschau

**Version:** 2.0  
**Priorität:** Sollte

CSV-Dateien sollen importiert und tabellarisch angezeigt werden können.

**Gewünschte Funktionen:**

- CSV als Anhang importieren,
- CSV-Vorschau,
- Spalten anzeigen,
- einfache Datenprüfung,
- später Auswertungen.

**Nutzen:**  
Messwerte, Logauszüge, Blutwerte, Exportdaten oder Tabellen können besser eingebunden werden.

### LH-057 Projekt-Export als ZIP

**Version:** 2.0  
**Priorität:** Muss für V2

Ein Projekt soll als portables Archiv exportiert werden können.

**Gewünschte Inhalte:**

- Manifest,
- Einträge,
- Anhänge,
- Vorlagenbezug,
- Metadaten,
- optional Markdown-/HTML-Versionen.

**Nutzen:**  
Projekte können archiviert, weitergegeben oder zwischen Installationen übertragen werden.

### LH-058 Markdown-Import

**Version:** 2.0  
**Priorität:** Sollte

Markdown-Dateien sollen als Einträge importiert werden können.

**Nutzen:**  
Vorhandene Dokumentation aus GitHub, Obsidian oder anderen Markdown-Quellen kann übernommen werden.

### LH-059 Dashboard

**Version:** 2.0  
**Priorität:** Sollte

Die Anwendung soll eine Startansicht mit Arbeitsüberblick anbieten.

**Mögliche Elemente:**

- heute relevante Einträge,
- laufende Einträge,
- Einträge in Prüfung,
- offene Checklisten,
- aktive Timer,
- kürzlich bearbeitet,
- zuletzt hinzugefügte Anhänge.

**Nutzen:**  
Der Anwender findet schneller zurück in laufende Arbeit.

---

## 11.6 Version 3.0 – Fachprofile und Produktfamilie

### LH-070 Profilverwaltung

**Version:** 3.0  
**Priorität:** Muss für V3

Die Anwendung soll fachliche Profile unterstützen.

**Ein Profil definiert:**

- bevorzugte Eintragstypen,
- Vorlagen,
- Begriffe in der Oberfläche,
- empfohlene Tags,
- sinnvolle Statusmodelle,
- Exportvorgaben,
- spätere Zusatzmodule.

**Nutzen:**  
Aus dem neutralen Core entstehen zielgerichtete Workflows.

### LH-071 LabBook-Profil

**Version:** 3.0  
**Priorität:** Sollte

Das LabBook-Profil soll Experimente, Protokolle, Messungen und Forschungsnotizen unterstützen.

**Gewünschte Eintragstypen:**

- Experiment,
- Protokoll,
- Messung,
- Probe,
- Beobachtung,
- Forschungsnotiz,
- Literatur-/Paper-Notiz.

**Spätere Funktionen:**

- Messwerttabellen,
- Einheiten,
- Geräte,
- Kalibrierung,
- Probenverwaltung.

### LH-072 Software-/Engineering-Profil

**Version:** 3.0  
**Priorität:** Sollte

Das Engineering-Profil soll SASD-Projektarbeit, Softwaretests, Architekturentscheidungen und Releases unterstützen.

**Gewünschte Eintragstypen:**

- Fehleranalyse,
- Testlauf,
- Architekturentscheidung,
- Deployment-Protokoll,
- Release-Notiz,
- Risiko,
- Code-Review-Notiz.

**Nutzen:**  
Die Workbench unterstützt SASD Engineering Standards und nachvollziehbare Softwareentwicklung.

### LH-073 Linux-Admin-Profil

**Version:** 3.0  
**Priorität:** Sollte

Das Linux-Admin-Profil soll Systemdokumentation und Wartungsarbeit unterstützen.

**Gewünschte Eintragstypen:**

- Serverdokumentation,
- Konfigurationsstand,
- Änderung,
- Incident,
- Wartungsprotokoll,
- Script-Dokumentation,
- Sicherheitsnotiz.

**Spätere Funktionen:**

- Script-Collector,
- regelmäßige Systemabfragen,
- Vergleich von Konfigurationsständen,
- Schnittstelle zu SASD Sentinel oder Monitoring.

### LH-074 Prompt-Notebook-Profil

**Version:** 3.0  
**Priorität:** Sollte

Das Prompt-Profil soll KI-Experimente strukturiert dokumentieren.

**Gewünschte Eintragstypen:**

- Prompt,
- Prompt-Test,
- Modellvergleich,
- Ergebnisbewertung,
- Prompt-Version,
- System-Prompt-Notiz.

**Wichtige Felder:**

- Modell,
- Parameter,
- Prompttext,
- Antwort,
- Bewertung,
- Datenschutzklassifikation,
- Verbesserungsansatz.

### LH-075 Biblical-Research-Profil

**Version:** 3.0  
**Priorität:** Sollte

Das Biblical-Research-Profil soll langfristige Themenrecherche unterstützen.

**Gewünschte Eintragstypen:**

- Thema,
- Bibelstelle,
- Quellenanalyse,
- Argumentationskette,
- offene Frage,
- Zwischenfazit,
- Studiennotiz.

**Spezielle Anforderungen:**

- mehrere parallele Themen,
- Forschungsstand pro Thema,
- Quellen und Bibelstellen,
- offene Fragen,
- Zwischenfazits,
- Export nach Markdown,
- später Export nach Obsidian oder DokuWiki.

### LH-076 Recipe-Profil

**Version:** 3.0  
**Priorität:** Sollte

Das Recipe-Profil soll Rezeptentwicklung und Kochversuche unterstützen.

**Gewünschte Eintragstypen:**

- Rezept,
- Rezeptversuch,
- Variante,
- Geschmackstest,
- Einkaufsidee.

**Wichtige Felder:**

- Zutaten,
- Mengen,
- Zubereitung,
- Variante,
- Bewertung,
- Verbesserungsideen.

### LH-077 Food-&-Health-Profil

**Version:** 3.0 oder später  
**Priorität:** Optional / sensibel

Das Food-&-Health-Profil soll Ernährung, Verträglichkeit und persönliche Beobachtungen dokumentieren.

**Wichtige Abgrenzung:**  
Die Anwendung darf keine medizinische Diagnose stellen und keine Therapieempfehlung geben.

**Gewünschte Eintragstypen:**

- Mahlzeit,
- Rezept,
- Verträglichkeitsnotiz,
- Blutwertnotiz,
- Tagesprotokoll,
- Arztgespräch-Export.

**Spätere Funktionen:**

- CSV-Import von Blutwerten,
- Export für Arztgespräch,
- Datenschutz-/Verschlüsselungsoptionen,
- Allergien und Unverträglichkeiten,
- persönliche Beobachtungen.

---

## 11.7 Version 4.0 – Team, Sicherheit, Sync, KI und Profi-Funktionen

### LH-090 Benutzer und Rollen

**Version:** 4.0  
**Priorität:** Optional

Die Anwendung soll später mehrere Benutzer und Rollen unterstützen können.

**Mögliche Rollen:**

- Owner,
- Editor,
- Reviewer,
- Reader,
- Admin.

**Nutzen:**  
Team- und Reviewprozesse werden möglich.

### LH-091 Review-Workflow

**Version:** 4.0  
**Priorität:** Optional

Einträge sollen zur Prüfung gegeben, kommentiert, freigegeben oder zurückgewiesen werden können.

**Nutzen:**  
Die Anwendung wird für Teams, Qualitätsprozesse und strukturierte Freigaben brauchbarer.

### LH-092 Signieren / Freigeben

**Version:** 4.0  
**Priorität:** Optional

Einträge sollen intern signiert oder freigegeben werden können.

**Abgrenzung:**  
Dies ist zunächst keine automatisch rechtsverbindliche qualifizierte elektronische Signatur.

### LH-093 Manipulationsärmerer Audit Trail

**Version:** 4.0  
**Priorität:** Optional / Profi-Funktion

Die Anwendung soll später einen stärkeren Audit Trail unterstützen.

**Mögliche Ansätze:**

- Append-only-Log,
- Hash-Kette,
- signierte Snapshots,
- Export-Prüfsummen,
- Sperren signierter Einträge.

### LH-094 Verschlüsselung

**Version:** 4.0  
**Priorität:** Optional / abhängig vom Profil

Sensible Projekte oder Backups sollen optional verschlüsselt werden können.

**Anwendungsfälle:**

- Gesundheitsdaten,
- Kundendaten,
- interne Firmeninformationen,
- vertrauliche Forschungsnotizen.

**Risiko:**  
Bei verlorenen Passwörtern droht Datenverlust. Recovery muss gesondert konzipiert werden.

### LH-095 Synchronisation

**Version:** 4.0  
**Priorität:** Optional

Die Anwendung soll später Synchronisation unterstützen können.

**Bevorzugte Entwicklung:**

1. Backup und Export,
2. Nextcloud-/WebDAV-/Syncthing-freundliche Ablage,
3. später echter Server-Sync.

### LH-096 CLI / API

**Version:** 4.0  
**Priorität:** Optional

Eine CLI oder API soll Automatisierung ermöglichen.

**Beispiele:**

```bash
sasd-workbench export --project "LogSink"
sasd-workbench backup
sasd-workbench import ./project.zip
sasd-workbench add-note --title "Testlauf 1"
```

### LH-097 KI-Unterstützung

**Version:** 4.0  
**Priorität:** Optional

KI soll später optional bei Strukturierung und Auswertung helfen.

**Mögliche Funktionen:**

- Eintrag zusammenfassen,
- Titel vorschlagen,
- Tags vorschlagen,
- offene Punkte erkennen,
- Experimentbericht erzeugen,
- Prompt-Ergebnisse vergleichen,
- Checklisten generieren.

**Datenschutzanforderung:**  
KI darf nicht ungefragt Daten an externe Dienste senden. Bei vertraulichen oder personenbezogenen Daten muss die Nutzung transparent und abschaltbar sein.

---

# 12. Fachprofile und spätere Produktfamilie

Die SASD Workbench soll langfristig mehrere Produkte oder Profile aus demselben Core ermöglichen.

## 12.1 Allgemeines Profil

Das allgemeine Profil ist der Standardmodus. Es eignet sich für Projektjournal, Notizen, Entscheidungen, Anhänge und einfache Recherchen.

## 12.2 LabBook-Profil

Das LabBook-Profil richtet sich an wissenschaftliche und technische Experimente. Es soll Protokolle, Messungen, Beobachtungen und später Messwerttabellen unterstützen.

## 12.3 Engineering-Profil

Das Engineering-Profil richtet sich an Softwareentwicklung, technische Entscheidungen, Tests, Releases und Fehleranalysen. Es soll zu den SASD Engineering Standards passen.

## 12.4 Linux-Admin-Profil

Das Linux-Admin-Profil dokumentiert Server, Änderungen, Wartungen, Incidents, Konfigurationen und Skripte. Später kann es durch Script-Collector oder Monitoring-Anbindung erweitert werden.

## 12.5 Prompt-Profil

Das Prompt-Profil verwaltet KI-Prompts, Modellvergleiche, Ergebnisse, Varianten und Bewertungen. Datenschutzklassifikation ist hier besonders wichtig.

## 12.6 Biblical-Research-Profil

Dieses Profil unterstützt langfristige Themenrecherche, Bibelstellen, Quellen, Argumentationsketten, offene Fragen, Zwischenfazits und Export nach Markdown/Obsidian/DokuWiki.

## 12.7 Recipe- und Food-&-Health-Profil

Dieses Profil unterstützt Rezeptentwicklung, Varianten, Verträglichkeit, persönliche Beobachtungen und optional Blutwerte. Es bleibt klar von medizinischer Diagnose und Therapieempfehlung abgegrenzt.

---

# 13. Informationsobjekte aus Anwendersicht

## 13.1 Projekt

Ein Projekt ist der oberste Arbeitsraum. Es bündelt Einträge, Sammlungen, Vorlagen, Anhänge und spätere Profile.

## 13.2 Collection / Sammlung

Eine Sammlung gruppiert Einträge innerhalb eines Projekts thematisch oder organisatorisch.

## 13.3 Eintrag

Der Eintrag ist das zentrale Inhaltsobjekt. Ein Eintrag kann viele fachliche Bedeutungen haben: Notiz, Experiment, Fehleranalyse, Prompt-Test, Bibelstellenanalyse oder Rezeptversuch.

## 13.4 Vorlage

Eine Vorlage ist eine wiederverwendbare Struktur für neue Einträge. Beim Anwenden wird eine Kopie erzeugt.

## 13.5 Anhang

Ein Anhang ist eine Datei, die mit einem Eintrag verbunden ist. Die Anwendung soll Anhänge kontrolliert in den eigenen Datenbereich übernehmen.

## 13.6 Tag

Ein Tag ist ein frei wählbares Schlagwort.

## 13.7 Status

Der Status beschreibt den Bearbeitungsstand eines Eintrags.

## 13.8 Querverweis

Ein Querverweis verbindet Einträge miteinander und hilft beim Aufbau eines Wissensnetzes.

## 13.9 Activity Log

Das Activity Log dokumentiert wichtige Aktionen und Änderungen.

---

# 14. Benutzeroberfläche und Bedienerlebnis

## 14.1 Grundlayout

Die Anwendung soll ruhig, technisch-professionell und übersichtlich wirken. Das Grundlayout soll drei Bereiche enthalten:

```text
Navigation | Eintragsliste | Editor / Details
```

## 14.2 Navigation

Die Navigation soll Zugriff bieten auf:

- Projekte,
- Sammlungen,
- Eintragstypen,
- Status,
- Tags,
- spätere Profile,
- Ressourcen optional.

## 14.3 Eintragsliste

Die Eintragsliste soll Treffer und Inhalte kompakt darstellen:

- Titel,
- Typ,
- Status,
- Änderungsdatum,
- Tags,
- kurze Vorschau optional.

## 14.4 Editor / Details

Der Editor soll enthalten:

- Titel,
- Typ,
- Status,
- Tags,
- Inhalt,
- Anhänge,
- Links,
- Querverweise,
- Activity Log optional.

## 14.5 Bedienprinzipien

- Häufige Aktionen sollen schnell erreichbar sein.
- Speichern und Datenverlustvermeidung sind wichtiger als optische Spielerei.
- Fehlermeldungen sollen verständlich sein.
- Die App soll nicht überladen wirken.
- Fachprofile sollen später Begriffe und Vorlagen anpassen, nicht den Core unverständlich machen.

---

# 15. Export, Import, Backup und Portabilität

## 15.1 Exportziele

Die Anwendung soll Daten langfristig nutzbar halten. Wichtige Exportformate sind:

- Markdown,
- HTML,
- PDF,
- ZIP-Archiv,
- später JSON/CSV je nach Zweck.

## 15.2 Markdown-Export

Markdown ist V1-Pflicht, weil es gut zu GitHub, Obsidian, Dokumentation und SASD-Prozessen passt.

## 15.3 HTML-Export

HTML ist sinnvoll für einfache Weitergabe und Browseranzeige.

## 15.4 PDF-Export

PDF ist ab V2 wichtig für Berichte, Archivierung und Portfolio-Unterlagen.

## 15.5 Projektarchiv

Ein Projektarchiv soll später Einträge, Anhänge, Vorlagen und Metadaten transportierbar machen.

## 15.6 Backup und Restore

Backup und Restore sind für V1 Kernfunktionen. Die Anwendung darf nicht davon ausgehen, dass Anwender selbst manuell Datenbank und Anhänge korrekt sichern.

---

# 16. Sicherheit, Datenschutz und Compliance-Abgrenzung

## 16.1 Lokale Datenhoheit

Die erste Version speichert Daten lokal. Es erfolgt keine automatische Cloud-Übertragung.

## 16.2 Gesundheitsdaten

Food-&-Health-Funktionen sind sensibel. Die Anwendung darf Daten dokumentieren, aber keine medizinische Diagnose stellen und keine Therapie empfehlen.

## 16.3 Kundendaten und vertrauliche Inhalte

Für Kundendaten und vertrauliche Projekte sind spätere Verschlüsselungs- und Klassifizierungsfunktionen sinnvoll.

## 16.4 KI-Unterstützung

KI darf nur optional und transparent eingesetzt werden. Der Anwender muss wissen, ob Daten an externe Dienste gesendet werden.

## 16.5 Compliance-Abgrenzung

Die frühe Workbench ist keine validierte GxP-/GLP-/FDA-konforme Laborsoftware. Funktionen wie Audit Trail und Signatur können später vorbereitet werden, ersetzen aber nicht automatisch regulatorische Validierung.

---

# 17. Nichtfunktionale Anforderungen

## 17.1 Verständlichkeit

Die Anwendung und ihre Dokumentation sollen für technisch interessierte Anwender verständlich sein.

## 17.2 Wartbarkeit

Die Anwendung soll so entwickelt werden, dass spätere Profile und Module ohne kompletten Neubau möglich sind.

## 17.3 Robustheit

Speichern, Export, Backup und Restore müssen zuverlässig funktionieren.

## 17.4 Offline-Fähigkeit

V1 muss vollständig offline nutzbar sein.

## 17.5 Portabilität

Daten müssen exportierbar sein. Ein Lock-in in ein undurchsichtiges Format soll vermieden werden.

## 17.6 Performance

Die Anwendung soll auch mit vielen Einträgen noch sinnvoll nutzbar sein.

## 17.7 Dokumentation

Es sollen README, Lastenheft, Pflichtenheft, technische Dokumentation, Anwenderhandbuch und Entwicklerhinweise entstehen.

## 17.8 Qualität des Codes

Der spätere Code soll sauber strukturiert und gut kommentiert sein. Große unübersichtliche Formulardateien mit eingebetteter Geschäftslogik sollen vermieden werden.

---

# 18. Abnahmekriterien je Hauptversion

## 18.1 Abnahme Version 0.1

Version 0.1 ist erfüllt, wenn:

- die Anwendung startet,
- ein lokaler Datenbereich angelegt wird,
- ein Projekt erstellt werden kann,
- ein Eintrag erstellt werden kann,
- Daten nach Neustart erhalten bleiben.

## 18.2 Abnahme Version 0.5

Version 0.5 ist erfüllt, wenn:

- Projekte verwaltet werden können,
- Einträge erstellt, bearbeitet und gelöscht werden können,
- Eintragstypen funktionieren,
- Status gesetzt werden können,
- Tags vergeben werden können,
- Suche nach Titel und Inhalt funktioniert,
- erste Vorlagen verwendet werden können,
- erste Anhänge unterstützt werden.

## 18.3 Abnahme Version 1.0

Version 1.0 ist erfüllt, wenn:

- die Anwendung lokal produktiv für Einzelanwender nutzbar ist,
- Collections vorhanden sind,
- Suche und Filter brauchbar funktionieren,
- Vorlagenbibliothek vorhanden ist,
- Anhänge stabil verwaltet werden,
- Markdown-Export funktioniert,
- Backup und Restore funktionieren,
- wichtige Aktionen protokolliert werden,
- eine erste Anwenderdokumentation vorhanden ist.

## 18.4 Abnahme Version 2.0

Version 2.0 ist erfüllt, wenn:

- PDF-Export verfügbar ist,
- Journale erzeugt werden können,
- Versionshistorie vorhanden ist,
- Activity Log erweitert ist,
- Timer/Zeitstempel nutzbar sind,
- Ressourcenbibliothek vorhanden ist,
- Projekt-ZIP-Export funktioniert,
- Markdown-Import grundsätzlich möglich ist.

## 18.5 Abnahme Version 3.0

Version 3.0 ist erfüllt, wenn:

- Profile verwaltet werden können,
- mehrere Fachprofile sinnvoll nutzbar sind,
- profilabhängige Vorlagen angeboten werden,
- zentrale Fachworkflows abgebildet sind,
- der Core weiterhin gemeinsam genutzt wird.

## 18.6 Abnahme Version 4.0

Version 4.0 ist erfüllt, wenn je nach Priorisierung:

- Benutzer/Rollen vorhanden sind,
- Review-Workflow nutzbar ist,
- Signatur/Freigabe vorbereitet ist,
- Verschlüsselung oder sichere Backups verfügbar sind,
- Sync-Konzept umgesetzt ist,
- CLI/API nutzbar ist,
- KI-Unterstützung optional und datenschutzbewusst integriert ist.

---

# 19. Bewusst zurückgestellte Funktionen

| Funktion | Früheste Version | Grund |
|---|---:|---|
| PDF-Export | 2.0 | Layout und Qualität benötigen eigene Planung |
| Automatisches Journal | 2.0 | baut auf Export und Historie auf |
| Versionshistorie | 2.0 | erhöht Datenmodell- und UI-Komplexität |
| Timer | 2.0 | nützlich, aber nicht MVP-kritisch |
| Ressourcen-Bibliothek | 2.0 | wertvoll, aber nachrangig gegenüber Erfassung/Export |
| CSV-Import | 2.0 | wichtig, aber für V1 nicht zwingend |
| Profilverwaltung | 3.0 | V1 nutzt Vorlagen statt echter Profile |
| Linux Script-Collector | 3.0+ | braucht Sicherheits- und Schnittstellenkonzept |
| Obsidian-/DokuWiki-Export | 3.0+ | sinnvoll für Research-Profile, aber nicht Kern-MVP |
| Benutzer/Rollen | 4.0 | Einzelplatz-V1 braucht das nicht |
| Verschlüsselung | 4.0 | Recovery und Usability müssen sorgfältig geplant werden |
| Sync | 4.0 | Konfliktlösung komplex |
| KI-Unterstützung | 4.0 | Datenschutz und Transparenz erforderlich |
| Plugin-System | 4.0+ | für V1 zu komplex |
| Mobile App | später | erst nach stabilem Core sinnvoll |

---

# 20. Risiken, Annahmen und offene Fragen

## 20.1 Risiken

- Der Funktionsumfang kann zu groß werden, wenn zu viele Profile zu früh umgesetzt werden.
- Gesundheits- und Forschungsfunktionen können falsche Erwartungen bezüglich Medizin oder Compliance wecken.
- Sync und Multiuser sind komplexer als sie zunächst wirken.
- Verschlüsselung verbessert Datenschutz, kann aber Recovery erschweren.
- Zu frühe KI-Integration kann Datenschutz und Vertrauen gefährden.

## 20.2 Annahmen

- V1 wird als lokale Einzelplatzanwendung entwickelt.
- Der Anwender ist bereit, mit einem pragmatischen Desktop-MVP zu starten.
- Der Core ist wichtiger als sofortige Spezialfunktionen.
- Markdown-Export ist ein wichtiger strategischer Vorteil.
- Backup/Restore ist für lokale Daten unverzichtbar.

## 20.3 Offene Fragen

1. Soll V1 strikt Windows Forms bleiben oder früh eine spätere Avalonia-/Web-Option vorbereiten?
2. Werden Einträge in V1 als Markdown-Text oder bereits als Blockmodell gespeichert?
3. Soll Soft Delete von Anfang an Pflicht sein?
4. Welche Lizenzstrategie ist gewünscht: MIT, AGPL oder duale Strategie?
5. Welche Profile sollen zuerst produktiv werden?
6. Wie stark soll das Food-&-Health-Profil aus Datenschutzgründen zurückgestellt werden?
7. Wann soll Obsidian-/DokuWiki-Export eingeplant werden?
8. Welche Funktionen braucht ein Linux-Admin-Profil, bevor ein Script-Collector sinnvoll ist?
9. Soll es später eine Community Edition und eine Pro-/Support-Edition geben?

---

# 21. Anhang A: Initiale Vorlagen aus Anwendersicht

## 21.1 Allgemeine Notiz

```markdown
# Notiz

## Kontext

## Inhalt

## Nächste Schritte
```

## 21.2 Experiment

```markdown
# Ziel

# Hypothese

# Material / Umgebung

# Durchführung

# Beobachtungen

# Ergebnis

# Fazit

# Nächste Schritte
```

## 21.3 Fehleranalyse

```markdown
# Problem

# Umgebung

# Reproduktionsschritte

# Erwartetes Verhalten

# Tatsächliches Verhalten

# Analyse

# Lösung

# Test

# Ergebnis
```

## 21.4 Architekturentscheidung / ADR

```markdown
# Kontext

# Entscheidung

# Optionen

# Begründung

# Konsequenzen

# Status
```

## 21.5 Linux-Wartungsprotokoll

```markdown
# System

# Ausgangslage

# Durchgeführte Arbeiten

# Geänderte Dateien / Konfigurationen

# Befehle

# Ergebnis

# Rollback-Hinweise
```

## 21.6 Prompt-Test

```markdown
# Ziel

# Prompt

# Modell / Einstellungen

# Ergebnis

# Bewertung

# Verbesserungen

# Variante / nächste Iteration
```

## 21.7 Bibel-/Themen-Recherche

```markdown
# Thema / Frage

# Ausgangsfrage

# Bibelstellen

# Quellen / Literatur

# Beobachtungen

# Argumentationskette

# Offene Fragen

# Zwischenfazit

# Nächste Schritte
```

## 21.8 Rezeptversuch

```markdown
# Ziel

# Zutaten

# Mengen

# Zubereitung

# Variante

# Geschmack / Verträglichkeit

# Verbesserungsideen

# Ergebnis
```

---

# 22. Anhang B: Kompakte Anforderungsliste

| ID | Anforderung | Version | Priorität |
|---|---|---:|---|
| LH-001 | Grundanwendung starten | 0.1 | Muss |
| LH-002 | Lokalen Datenbereich anlegen | 0.1 | Muss |
| LH-003 | Erstes Projekt anlegen | 0.1 | Muss |
| LH-004 | Einfachen Eintrag anlegen | 0.1 | Muss |
| LH-005 | Daten nach Neustart wiederfinden | 0.1 | Muss |
| LH-010 | Projektverwaltung | 0.5 | Muss |
| LH-011 | Eintragsverwaltung | 0.5 | Muss |
| LH-012 | Eintragstypen | 0.5 | Muss |
| LH-013 | Statusmodell | 0.5 | Muss |
| LH-014 | Tags | 0.5 | Muss |
| LH-015 | Suche | 0.5 | Muss |
| LH-016 | Erste Vorlagen | 0.5 | Muss |
| LH-017 | Grundlegende Anhangsverwaltung | 0.5 | Sollte |
| LH-020 | Drei-Bereich-Oberfläche | 1.0 | Muss |
| LH-021 | Collections / Sammlungen | 1.0 | Muss |
| LH-022 | Erweiterte Filter | 1.0 | Muss |
| LH-023 | Vorlagenbibliothek | 1.0 | Muss |
| LH-024 | Stabile Anhangsverwaltung | 1.0 | Muss |
| LH-025 | Markdown-Export | 1.0 | Muss |
| LH-026 | HTML-Export | 1.0 | Sollte |
| LH-027 | Backup | 1.0 | Muss |
| LH-028 | Restore | 1.0 | Muss |
| LH-029 | Activity Log Light | 1.0 | Sollte |
| LH-030 | Interne Querverweise vorbereiten | 1.0 | Sollte |
| LH-040 | Attachment Templates | 1.1 | Sollte |
| LH-041 | Checklisten | 1.1 | Sollte |
| LH-042 | Linkliste pro Eintrag | 1.1 | Sollte |
| LH-043 | Verbesserte Fehlerbehandlung | 1.1 | Muss |
| LH-044 | Anwenderhandbuch | 1.1 | Muss |
| LH-050 | PDF-Export | 2.0 | Muss |
| LH-051 | Automatisches Journal | 2.0 | Sollte |
| LH-052 | Versionshistorie | 2.0 | Muss |
| LH-053 | Erweiterter Activity Log | 2.0 | Muss |
| LH-054 | Timer und Zeitstempel | 2.0 | Sollte |
| LH-055 | Ressourcen-Bibliothek | 2.0 | Sollte |
| LH-056 | CSV-Import und Tabellenvorschau | 2.0 | Sollte |
| LH-057 | Projekt-Export als ZIP | 2.0 | Muss |
| LH-058 | Markdown-Import | 2.0 | Sollte |
| LH-059 | Dashboard | 2.0 | Sollte |
| LH-070 | Profilverwaltung | 3.0 | Muss |
| LH-071 | LabBook-Profil | 3.0 | Sollte |
| LH-072 | Software-/Engineering-Profil | 3.0 | Sollte |
| LH-073 | Linux-Admin-Profil | 3.0 | Sollte |
| LH-074 | Prompt-Notebook-Profil | 3.0 | Sollte |
| LH-075 | Biblical-Research-Profil | 3.0 | Sollte |
| LH-076 | Recipe-Profil | 3.0 | Sollte |
| LH-077 | Food-&-Health-Profil | 3.0+ | Optional |
| LH-090 | Benutzer und Rollen | 4.0 | Optional |
| LH-091 | Review-Workflow | 4.0 | Optional |
| LH-092 | Signieren / Freigeben | 4.0 | Optional |
| LH-093 | Manipulationsärmerer Audit Trail | 4.0 | Optional |
| LH-094 | Verschlüsselung | 4.0 | Optional |
| LH-095 | Synchronisation | 4.0 | Optional |
| LH-096 | CLI / API | 4.0 | Optional |
| LH-097 | KI-Unterstützung | 4.0 | Optional |

---

# 23. Zusammenfassung

Die SASD Workbench soll als lokale, modulare Arbeits-, Forschungs- und Dokumentationsplattform entstehen. Der erste Schritt ist ein robuster Core, der Projekte, Einträge, Vorlagen, Anhänge, Suche, Export und Backup unterstützt.

Die spätere Produktstrategie basiert nicht auf vielen getrennten Einzelprogrammen, sondern auf einem gemeinsamen Kern mit Fachprofilen. Dadurch kann dieselbe Plattform später für Laborjournal, Softwareentwicklung, Linux-Administration, Prompt-Experimente, Bibelrecherche, Rezeptentwicklung und Food-&-Health-Dokumentation genutzt werden.

Die wichtigste Priorisierung lautet:

1. Erst stabiler lokaler Core.
2. Dann komfortable Workflows und Datenqualität.
3. Danach Journal, PDF, Historie und Ressourcen.
4. Danach Fachprofile.
5. Erst später Team, Sync, Verschlüsselung, KI und Profi-Compliance-Funktionen.

Damit bleibt das Projekt realistisch, schnell nutzbar und zugleich strategisch ausbaufähig.
