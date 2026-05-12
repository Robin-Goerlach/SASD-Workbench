# Pflichtenheft – SASD Workbench Core

> **Dokumentstatus:** ausführlicher Arbeitsentwurf  
> **Projekt:** SASD Workbench  
> **Komponente:** SASD Workbench Core / lokaler Desktop-MVP  
> **Organisation:** SASD-GmbH – Scientific and Software Development  
> **Autor / Product Owner:** Robin Goerlach / SASD-GmbH  
> **Dokumenttyp:** Pflichtenheft / funktionale MVP-Spezifikation  
> **Sprache:** Deutsch  
> **Version:** 1.0 Draft  
> **Datum:** 2026-05-12  
> **Geplanter V1-Technologiestack:** C# / .NET 8 / Windows Forms / SQLite  
> **Geplanter Architekturansatz:** Domain / Application / Infrastructure / WinForms  

---

## Inhaltsverzeichnis

1. [Zweck des Dokuments](#1-zweck-des-dokuments)  
2. [Management Summary](#2-management-summary)  
3. [Ausgangslage und Motivation](#3-ausgangslage-und-motivation)  
4. [Produktvision](#4-produktvision)  
5. [Grundsatzentscheidung: Core zuerst, Profile später](#5-grundsatzentscheidung-core-zuerst-profile-später)  
6. [Produktabgrenzung](#6-produktabgrenzung)  
7. [Zielgruppen und Nutzungsszenarien](#7-zielgruppen-und-nutzungsszenarien)  
8. [Begriffe und Kernobjekte](#8-begriffe-und-kernobjekte)  
9. [Versionsstrategie](#9-versionsstrategie)  
10. [Funktionsumfang nach Versionen](#10-funktionsumfang-nach-versionen)  
11. [Funktionale Anforderungen im Detail](#11-funktionale-anforderungen-im-detail)  
12. [Fachprofile und Produktfamilie](#12-fachprofile-und-produktfamilie)  
13. [Datenmodell und Persistenz](#13-datenmodell-und-persistenz)  
14. [Benutzeroberfläche und Bedienkonzept](#14-benutzeroberfläche-und-bedienkonzept)  
15. [Export, Import, Backup und Portabilität](#15-export-import-backup-und-portabilität)  
16. [Sicherheit, Datenschutz und Compliance-Abgrenzung](#16-sicherheit-datenschutz-und-compliance-abgrenzung)  
17. [Nichtfunktionale Anforderungen](#17-nichtfunktionale-anforderungen)  
18. [Architekturvorgaben](#18-architekturvorgaben)  
19. [Teststrategie und Abnahmekriterien](#19-teststrategie-und-abnahmekriterien)  
20. [Entwicklungsreihenfolge](#20-entwicklungsreihenfolge)  
21. [Risiken, Annahmen und offene Entscheidungen](#21-risiken-annahmen-und-offene-entscheidungen)  
22. [Anhang A: Initiale Vorlagen](#22-anhang-a-initiale-vorlagen)  
23. [Anhang B: Anforderungskatalog Kurzmatrix](#23-anhang-b-anforderungskatalog-kurzmatrix)  
24. [Anhang C: Empfohlene GitHub-Repository-Struktur](#24-anhang-c-empfohlene-github-repository-struktur)  
25. [Zusammenfassung](#25-zusammenfassung)  

---

## 1. Zweck des Dokuments

Dieses Pflichtenheft beschreibt die geplante Entwicklung der **SASD Workbench** als lokale, modulare Arbeits-, Forschungs- und Dokumentationsplattform. Es konkretisiert, welche Funktionen in welcher Version umgesetzt werden sollen, welche Funktionen bewusst zurückgestellt werden und wie der Kern so gestaltet wird, dass daraus später mehrere spezialisierte SASD-Produkte oder Arbeitsprofile entstehen können.

Das Dokument dient als Grundlage für:

- die Anlage eines GitHub-Repositories,
- die spätere Erstellung eines Technical Design Documents,
- die Datenbankplanung,
- die Implementierung des ersten Prototyps,
- die Erstellung von Issues und Meilensteinen,
- spätere Anwender- und Entwicklerdokumentation.

Die SASD Workbench soll **kein 1:1-Nachbau** einer bestehenden Anwendung werden. Sie wird als eigenständiges SASD-Produkt geplant. Aus der vorangegangenen Analyse von Findings-ähnlichen Funktionen werden sinnvolle Konzepte übernommen, aber in eine eigene Architektur, eigene Terminologie und eine eigene Produktstrategie überführt.

---

## 2. Management Summary

Die **SASD Workbench** soll zuerst als lokale Desktop-Anwendung entstehen. Sie unterstützt strukturierte Arbeit über längere Zeiträume: Projekte, Einträge, Vorlagen, Anhänge, Tags, Status, Suche, Export und Backups. Der erste Produktkern soll bewusst einfach, robust und offline nutzbar sein.

Langfristig soll die Workbench nicht nur ein elektronisches Laborjournal sein, sondern eine modulare Plattform für strukturierte Erkenntnisarbeit. Verschiedene Fachbereiche sollen später über **Profile**, **Vorlagen** und **Module** unterstützt werden:

- Labor / Research / Experimente,
- Softwareentwicklung / SASD Engineering,
- Linux-Administration,
- Prompt- und KI-Experimente,
- Bibel- und Themenrecherche,
- Rezeptentwicklung,
- Food & Health / Ernährungstagebuch,
- allgemeine Projekt- und Arbeitsdokumentation.

Die wichtigste Architekturentscheidung lautet:

> **Zuerst entsteht ein neutraler Core. Fachliche Spezialisierungen werden später als Profile und Module ergänzt.**

Dadurch wird vermieden, mehrere ähnliche Anwendungen mit mehrfacher Codebasis zu bauen. Gemeinsame Funktionen wie Suche, Export, Backup, Anhänge, Tags und Vorlagen werden nur einmal entwickelt und später wiederverwendet.

Der empfohlene MVP-Technologiestack lautet:

```text
C# / .NET 8 / Windows Forms / SQLite
```

Die Implementierung soll dennoch sauber geschichtet werden, damit später andere Oberflächen möglich bleiben:

```text
UI → Application Services → Repositories → SQLite / File Storage
```

---

## 3. Ausgangslage und Motivation

Die Diskussion begann mit der Frage, ob eine Findings-ähnliche App nachprogrammiert werden kann. Im Verlauf wurde deutlich, dass eine solche Anwendung nicht nur eine Notiz-App ist. Der relevante Funktionsumfang umfasst unter anderem:

- elektronische Labor- oder Arbeitsjournale,
- Experimente und Forschungsnotizen,
- Meeting Notes,
- Schnellnotizen / Stickies,
- Protokolle und wiederverwendbare Vorlagen,
- Collections / Sammlungen,
- Statusmodell,
- Anhänge und Attachment Templates,
- Timer und Zeitstempel,
- Suche und Filter,
- Export als PDF, Markdown, HTML oder Archiv,
- automatische Journale,
- interne Links und Querverweise,
- Ressourcenbibliothek,
- lokale Datenhoheit,
- spätere Sync- oder Teamfähigkeit.

Für SASD ist dieses Projekt strategisch interessant, weil es mehrere vorhandene Arbeitsrichtungen verbindet:

- wissenschaftsnahe Softwareentwicklung,
- saubere technische Dokumentation,
- Forschungsdaten- und Projektverwaltung,
- Linux-Admin-Dokumentation,
- Prompt- und KI-Experimentverwaltung,
- Bibel-/Themen-Recherche über längere Zeiträume,
- Software- und Architekturentscheidungsdokumentation,
- Rezeptentwicklung und später Ernährungstagebuch.

Die App soll damit nicht nur ein Werkzeug zum Erfassen von Notizen sein, sondern ein Instrument, mit dem Arbeit nachvollziehbar, wiederverwendbar, exportierbar und langfristig nutzbar dokumentiert wird.

---

## 4. Produktvision

Die SASD Workbench soll eine lokale, modulare und langfristig erweiterbare Plattform für strukturierte Erkenntnisarbeit werden.

Ein Anwender soll damit Projekte anlegen, Einträge erfassen, Experimente oder Recherchen dokumentieren, Vorlagen verwenden, Anhänge verwalten, Inhalte durchsuchen, Ergebnisse exportieren und seine Arbeit über längere Zeiträume nachvollziehbar weiterentwickeln können.

Die Workbench soll besonders dort nützlich sein, wo Arbeit schrittweise entsteht:

- ein Thema wird über Wochen oder Monate recherchiert,
- ein technischer Fehler wird analysiert,
- ein Experiment wird geplant, durchgeführt und ausgewertet,
- ein Rezept wird mehrfach variiert,
- ein Prompt wird in mehreren Iterationen verbessert,
- ein Server wird über Jahre administriert,
- ein Softwareprojekt erzeugt Architekturentscheidungen, Testläufe und Deployment-Protokolle.

Die Produktvision lautet:

> **SASD Workbench ist ein lokales, modulares Arbeits- und Forschungsjournal für strukturierte Projekte, Experimente, Recherchen, technische Dokumentation und wiederverwendbares Wissen.**

---

## 5. Grundsatzentscheidung: Core zuerst, Profile später

### 5.1 Entscheidung

Es wird zunächst eine gemeinsame Kernanwendung entwickelt:

```text
SASD Workbench Core
```

Aus diesem Kern können später mehrere Produkte oder Profile entstehen:

```text
SASD Workbench Core
  ├── Profil: General Workbench
  ├── Profil: LabBook / Research
  ├── Profil: Software / Engineering
  ├── Profil: Linux Admin Notebook
  ├── Profil: Prompt Notebook
  ├── Profil: Biblical Research Notebook
  ├── Profil: Recipe / Cooking Experiments
  └── Profil: Food & Health Journal
```

### 5.2 Begründung

Viele Funktionen werden in allen Profilen benötigt:

- Projekte,
- Einträge,
- Sammlungen,
- Tags,
- Vorlagen,
- Anhänge,
- Suche,
- Export,
- Backup,
- Änderungsverlauf,
- Status,
- interne Verlinkung.

Separate Anwendungen für jedes Fachgebiet würden zu doppelter Arbeit führen. Die Wartung würde schwieriger, die Bedienung uneinheitlich und spätere Produktentscheidungen würden unnötig teuer.

### 5.3 Konsequenz

Der zentrale fachliche Begriff der Anwendung ist nicht „Experiment“, „Rezept“, „Prompt“ oder „Bibelthema“, sondern allgemeiner:

```text
Entry / Eintrag
```

Ein Eintrag kann je nach Profil unterschiedliche Bedeutung haben:

| Profil | Bedeutung eines Eintrags |
|---|---|
| Allgemein | Notiz, Protokoll, Aufgabe, Entscheidung |
| LabBook | Experiment, Messung, Beobachtung, Protokoll |
| Software | Fehleranalyse, ADR, Testlauf, Deployment-Protokoll |
| Linux Admin | Change, Incident, Servernotiz, Wartung |
| Prompt | Prompt-Test, Modellvergleich, Ergebnisanalyse |
| Biblical Research | Thema, Bibelstelle, Quellenanalyse, Argumentationskette |
| Recipe | Rezeptversuch, Variante, Bewertung |
| Food & Health | Mahlzeit, Verträglichkeit, Blutwertnotiz, Tagesprotokoll |

---

## 6. Produktabgrenzung

### 6.1 Was die SASD Workbench sein soll

Die Anwendung soll sein:

- eine lokale Desktop-App,
- ein strukturiertes Projekt- und Arbeitsjournal,
- ein Werkzeug zur Dokumentation von Erkenntnissen,
- ein Container für Einträge, Anhänge, Vorlagen und Querverweise,
- ein portables, exportierbares Wissenssystem,
- eine Basis für spätere Spezialprofile.

### 6.2 Was die SASD Workbench in V1 ausdrücklich nicht sein soll

Version 1 ist bewusst begrenzt. Nicht geplant sind:

- keine Cloud-Plattform,
- keine Mobile-App,
- keine Web-App,
- keine Team- oder Multiuser-Funktion,
- keine vollständige Laborverwaltungssoftware,
- kein vollständiges LIMS,
- kein vollständig validiertes regulatorisches System,
- keine rechtlich garantierte elektronische Signatur,
- keine medizinische Diagnostik,
- keine Therapieempfehlungen,
- keine automatische KI-Analyse,
- kein vollständiger Aufgabenmanager wie Trello oder Wunderlist,
- keine komplexe Statistiksoftware,
- kein vollwertiges Literaturverwaltungssystem wie Zotero,
- kein sofortiges Plugin-System.

### 6.3 Strategische Abgrenzung

Die Workbench soll bewusst **lokal und vertrauenswürdig** beginnen. Der erste Nutzen entsteht dadurch, dass der Anwender sofort Daten erfassen, wiederfinden, exportieren und sichern kann. Spätere Cloud-, KI- oder Teamfunktionen dürfen erst folgen, wenn der lokale Kern stabil ist.

---

## 7. Zielgruppen und Nutzungsszenarien

### 7.1 Primäre Zielgruppe V1

Die erste Version richtet sich an Einzelanwender, insbesondere:

- SASD intern,
- Softwareentwickler,
- Linux-Administratoren,
- technische Consultants,
- Maker,
- wissenschaftlich arbeitende Einzelpersonen,
- Personen mit langfristigen Rechercheprojekten,
- Personen, die strukturierte Notizen und Anhänge lokal organisieren möchten.

### 7.2 Sekundäre Zielgruppen späterer Versionen

Spätere Versionen können zusätzlich adressieren:

- kleine Labore,
- kleine Forschungsgruppen,
- IT-Dienstleister,
- Trainer und Ausbilder,
- Projektteams,
- Personen mit Ernährungs- und Rezeptdokumentation,
- wissenschaftlich oder theologisch recherchierende Einzelpersonen.

### 7.3 Beispielhafte Nutzungsszenarien

#### Szenario 1: Softwareprojekt dokumentieren

Ein Entwickler legt ein Projekt „SASD Workbench“ an. Darin erstellt er Einträge für Architekturentscheidungen, Fehleranalysen, Testläufe und Deployment-Protokolle. Anhänge wie Screenshots, SQL-Skripte oder Logdateien werden direkt am Eintrag gespeichert. Später exportiert er das Projekt als Markdown-Dokumentation.

#### Szenario 2: Linux-Server dokumentieren

Ein Administrator legt für einen Server ein Projekt an. Änderungen an Konfigurationsdateien, Wartungsarbeiten, Incidents und Rollback-Hinweise werden als Einträge dokumentiert. Anhänge können Konfigurationssicherungen, Screenshots oder Logauszüge sein.

#### Szenario 3: Bibelthema schrittweise erforschen

Ein Anwender legt ein Projekt „Was ist das Königreich?“ an. Innerhalb des Projekts sammelt er Bibelstellen, Quellen, offene Fragen, Argumentationsketten und Zwischenfazits. Da mehrere Themen parallel laufen können, helfen Status, Tags, Collections und Querverweise dabei, den Überblick zu behalten.

#### Szenario 4: Prompt-Experimente dokumentieren

Ein Prompt wird mehrfach variiert. Jede Variante wird mit Modell, Parametern, Ergebnis, Bewertung und Verbesserungsidee gespeichert. Später können Einträge miteinander verglichen oder als Markdown exportiert werden.

#### Szenario 5: Rezeptentwicklung dokumentieren

Ein Rezept wird in mehreren Varianten getestet. Zutaten, Mengen, Zubereitung, Geschmack, Verträglichkeit und Verbesserungsideen werden je Versuch erfasst. Später kann daraus ein eigenes Food-&-Health-Profil entstehen.

---

## 8. Begriffe und Kernobjekte

### 8.1 Project / Projekt

Ein Projekt ist der oberste Arbeitsraum. Es bündelt Einträge, Collections, Tags, Anhänge und Vorlagenverwendung.

Beispiele:

- „SASD Workbench Entwicklung“,
- „Linux Server Dokumentation“,
- „Prompt Experimente 2026“,
- „Bibelstudium Königreich“,
- „Rezepte und Diabetes-Ernährung“.

### 8.2 Collection / Sammlung

Eine Collection ist eine flexible Gruppe innerhalb eines Projekts. Sie kann Einträge nach Thema, Zeitraum, Arbeitsphase oder fachlicher Struktur bündeln.

Beispiele:

- „MVP“,
- „Offene Fragen“,
- „Serverwartung“,
- „Experimente 2026“,
- „Frühstücksrezepte“.

### 8.3 Entry / Eintrag

Der Eintrag ist das zentrale Inhaltsobjekt. Er besitzt Titel, Typ, Status, Inhalt, Tags, Anhänge und Metadaten.

### 8.4 Entry Type / Eintragstyp

Der Entry Type beschreibt die fachliche Art des Eintrags.

Beispiele:

- Allgemeine Notiz,
- Experiment,
- Research Note,
- Meeting Note,
- Sticky,
- Fehleranalyse,
- Architekturentscheidung,
- Prompt-Test,
- Linux-Admin-Notiz,
- Rezeptversuch,
- Bibel-/Themen-Recherche.

### 8.5 Status

Der Status beschreibt den Arbeitszustand eines Eintrags.

Standardstatus V1:

- Entwurf,
- Geplant,
- In Arbeit,
- In Prüfung,
- Abgeschlossen,
- Zurückgestellt,
- Abgebrochen,
- Archiviert.

Später möglich:

- Freigegeben,
- Signiert,
- Gesperrt.

### 8.6 Template / Vorlage

Eine Vorlage erzeugt einen neuen Eintrag mit vorbereiteter Struktur. Änderungen am erzeugten Eintrag verändern die ursprüngliche Vorlage nicht.

### 8.7 Attachment / Anhang

Ein Anhang ist eine Datei, die zu einem Eintrag gehört. In V1 werden Anhänge in den Workbench-Datenordner kopiert, damit sie nicht verloren gehen, wenn die Originaldatei verschoben wird.

### 8.8 Tag

Ein Tag ist ein freies Schlagwort. Tags dienen der Suche, Filterung und flexiblen Organisation.

### 8.9 EntryLink / Querverweis

Ein Querverweis verbindet zwei Einträge miteinander. So entstehen fachliche Beziehungen wie „basiert auf“, „ersetzt“, „bestätigt“ oder „gehört zu“.

### 8.10 Activity Log

Das Activity Log dokumentiert wichtige Aktionen. V1 enthält eine einfache Variante. Später kann daraus ein detaillierter Audit Trail entstehen.

---

## 9. Versionsstrategie

Die Entwicklung wird in klaren Versionen geplant. Jede Version soll einen eigenständigen, nutzbaren Fortschritt darstellen.

| Version | Name | Ziel |
|---:|---|---|
| 0.1 | Technischer Prototyp | Architektur, SQLite, erste Projekte und Einträge |
| 0.5 | Interner MVP | Projekte, Einträge, Typen, Status, Tags, einfache Suche, erste Vorlagen |
| 1.0 | Lokaler Desktop-MVP | produktiv nutzbarer lokaler Kern mit Anhängen, Export, Backup, Collections |
| 1.1 | Komfort- und Stabilitätsversion | bessere Usability, Attachment Templates, Checklisten, Linklisten, Fehlerbehandlung |
| 2.0 | Journal und Historie | PDF, automatisches Journal, Versionshistorie, Ressourcen, Timer, ZIP-Export |
| 3.0 | Fachprofile | LabBook, Software, Linux Admin, Prompt, Biblical Research, Food & Health |
| 4.0 | Profi- und Teamfunktionen | Rollen, Review, Signatur, Audit, Verschlüsselung, Sync, CLI/API, KI |

---

## 10. Funktionsumfang nach Versionen

### 10.1 Version 0.1 – Technischer Prototyp

Ziel dieser Version ist nicht Schönheit, sondern der Nachweis, dass die technische Struktur tragfähig ist.

Enthalten:

- GitHub-Repository-Struktur,
- .NET-Solution,
- Domain-, Application-, Infrastructure- und WinForms-Projekt,
- SQLite-Datenbankinitialisierung,
- Basistabellen für Projekte und Einträge,
- Projekt anlegen und anzeigen,
- einfachen Eintrag anlegen und anzeigen,
- Neustartfähigkeit der gespeicherten Daten.

Nicht enthalten:

- vollständige Vorlagen,
- Anhänge,
- Export,
- Backup,
- komplexe Suche,
- finale UI.

### 10.2 Version 0.5 – Interner MVP

Diese Version soll intern sinnvoll testbar sein.

Enthalten:

- Projektverwaltung,
- Eintragsverwaltung,
- Entry Types,
- Statusmodell,
- Tags,
- einfache Suche,
- erste Vorlagen,
- einfache Anhangsverwaltung optional,
- Drei-Bereich-Grundlayout.

### 10.3 Version 1.0 – Lokaler Desktop-MVP

Diese Version soll als erste lokale Einzelplatzversion praktisch nutzbar sein.

Enthalten:

- stabile Projektverwaltung,
- stabile Eintragsverwaltung,
- Collections,
- Vorlagenbibliothek,
- Anhänge mit kontrollierter Dateiablage,
- Suche und Filter,
- Markdown-Export,
- HTML-Export optional,
- Backup und Restore,
- Activity Log Light,
- vorbereitete Querverweise,
- erste fachliche Vorlagen.

### 10.4 Version 1.1 – Komfort und Stabilität

Diese Version verbessert V1 ohne den Produktkern radikal zu verändern.

Enthalten:

- verbesserte Fehlerbehandlung,
- bessere Such- und Filterbedienung,
- Attachment Templates,
- einfache Checklisten,
- externe Linklisten,
- bessere Vorlagenverwaltung,
- ausführlichere Anwenderdokumentation,
- erste lokale Einstellungen.

### 10.5 Version 2.0 – Journal, Historie und Ressourcen

Diese Version macht aus der Workbench ein ernsthaftes Arbeitsjournal.

Enthalten:

- PDF-Export,
- automatisches Journal,
- Tages-/Wochen-/Projektjournal,
- Versionshistorie,
- erweiterter Activity Log,
- interne Links und Backlinks,
- Projekt-Export als ZIP,
- Markdown-Import,
- CSV-Import/Vorschau,
- Ressourcen-Bibliothek,
- Timer und Zeitstempel,
- Dashboard.

### 10.6 Version 3.0 – Fachprofile

Diese Version macht aus dem Core eine Produktfamilie.

Enthalten:

- Profilverwaltung,
- LabBook-Profil,
- Software-/Engineering-Profil,
- Linux-Admin-Profil,
- Prompt-Notebook-Profil,
- Biblical-Research-Profil,
- Recipe/Food-Profil,
- profilabhängige Begriffe,
- profilabhängige Templates,
- profilabhängige Statusmodelle.

### 10.7 Version 4.0 – Profi-Funktionen

Diese Version ist für fortgeschrittene Nutzung geplant.

Optional enthalten:

- Benutzer und Rollen,
- Review-Workflow,
- Freigabe und Signieren,
- manipulationsärmerer Audit Trail,
- Projekt- oder Backup-Verschlüsselung,
- Nextcloud/WebDAV/Syncthing-freundlicher Sync,
- REST-API oder CLI,
- KI-Unterstützung als optionales Modul,
- Plugin-/Modulschnittstelle.

---

## 11. Funktionale Anforderungen im Detail

Die folgenden Anforderungen verwenden IDs, damit sie später in GitHub Issues, Tests und Dokumentation referenziert werden können.

### 11.1 Repository und technische Grundstruktur

#### F-001 Solution- und Projektstruktur

| Feld | Wert |
|---|---|
| Version | 0.1 |
| Priorität | Muss |
| Kategorie | Architektur |

Die Anwendung muss als sauber strukturierte .NET-Solution angelegt werden. Die Schichten Domain, Application, Infrastructure und WinForms müssen getrennt sein.

**Akzeptanzkriterien:**

- Solution lässt sich ohne manuelle Nacharbeit bauen.
- Projekte referenzieren sich nur in erlaubter Richtung.
- Domain kennt keine UI und keine SQLite-Details.
- UI enthält keine zentrale Geschäftslogik.

#### F-002 SQLite-Datenbankinitialisierung

| Feld | Wert |
|---|---|
| Version | 0.1 |
| Priorität | Muss |
| Kategorie | Persistenz |

Beim ersten Start muss die Anwendung eine lokale SQLite-Datenbank anlegen können.

**Akzeptanzkriterien:**

- Datenbankdatei wird im konfigurierten Datenordner erstellt.
- Basistabellen werden erzeugt.
- Fehler bei fehlenden Schreibrechten werden verständlich angezeigt.
- Daten bleiben nach Neustart erhalten.

### 11.2 Projektverwaltung

#### F-010 Projekt anlegen

| Feld | Wert |
|---|---|
| Version | 0.1 / 0.5 |
| Priorität | Muss |
| Kategorie | Projektverwaltung |

Der Anwender muss ein Projekt mit Name und Beschreibung anlegen können.

**Akzeptanzkriterien:**

- Projektname ist Pflichtfeld.
- Beschreibung ist optional.
- Projekt erhält eindeutige ID.
- Erstellungs- und Änderungsdatum werden gesetzt.
- Projekt erscheint nach Neustart wieder.

#### F-011 Projekt bearbeiten

| Feld | Wert |
|---|---|
| Version | 0.5 |
| Priorität | Muss |
| Kategorie | Projektverwaltung |

Projektname, Beschreibung, Status und optional Profil müssen bearbeitet werden können.

**Akzeptanzkriterien:**

- Änderungen werden gespeichert.
- Änderungsdatum wird aktualisiert.
- Leerer Name wird verhindert.

#### F-012 Projekt archivieren und löschen

| Feld | Wert |
|---|---|
| Version | 0.5 / 1.0 |
| Priorität | Muss |
| Kategorie | Projektverwaltung |

Projekte müssen archiviert werden können. Löschen muss abgesichert erfolgen.

**Akzeptanzkriterien:**

- Archivierte Projekte können ausgeblendet werden.
- Löschung erfordert Sicherheitsabfrage.
- V1 bevorzugt Soft Delete oder Archivierung vor endgültigem Löschen.

### 11.3 Eintragsverwaltung

#### F-020 Eintrag anlegen

| Feld | Wert |
|---|---|
| Version | 0.1 / 0.5 |
| Priorität | Muss |
| Kategorie | Einträge |

Der Anwender muss innerhalb eines Projekts einen Eintrag erstellen können.

**Pflichtfelder:**

- Titel,
- Projekt,
- Eintragstyp,
- Status,
- Inhalt optional,
- Erstellungsdatum,
- Änderungsdatum.

**Akzeptanzkriterien:**

- Eintrag wird gespeichert.
- Eintrag erscheint in der Liste.
- Eintrag kann geöffnet und bearbeitet werden.
- Eintrag bleibt nach Neustart erhalten.

#### F-021 Eintrag bearbeiten und speichern

| Feld | Wert |
|---|---|
| Version | 0.5 |
| Priorität | Muss |
| Kategorie | Einträge |

Einträge müssen bearbeitet und gespeichert werden können.

**Akzeptanzkriterien:**

- Titel, Inhalt, Typ, Status und Tags können geändert werden.
- Änderungsdatum wird aktualisiert.
- optional wird Versionsnummer erhöht.
- ungespeicherte Änderungen werden nicht stillschweigend verworfen.

#### F-022 Eintrag duplizieren

| Feld | Wert |
|---|---|
| Version | 0.5 / 1.0 |
| Priorität | Sollte |
| Kategorie | Einträge |

Ein vorhandener Eintrag soll dupliziert werden können.

**Nutzen:**

- Varianten von Experimenten,
- Prompt-Iterationen,
- Rezeptvarianten,
- ähnliche Wartungsprotokolle.

**Akzeptanzkriterien:**

- neuer Eintrag erhält eigene ID.
- Inhalt wird kopiert.
- Anhänge werden entweder nicht kopiert oder kontrolliert dupliziert; Verhalten muss dokumentiert sein.

#### F-023 Eintrag löschen oder archivieren

| Feld | Wert |
|---|---|
| Version | 0.5 / 1.0 |
| Priorität | Muss |
| Kategorie | Einträge |

Einträge müssen archiviert oder gelöscht werden können. Endgültiges Löschen muss abgesichert sein.

**Akzeptanzkriterien:**

- Archivieren ist möglich.
- Löschen erfordert Rückfrage.
- Verhalten zu Anhängen ist eindeutig dokumentiert.

### 11.4 Eintragstypen und Status

#### F-030 Eintragstypen

| Feld | Wert |
|---|---|
| Version | 0.5 |
| Priorität | Muss |
| Kategorie | Klassifikation |

Ein Eintrag muss einen Typ besitzen.

**Initiale Typen:**

- Allgemeine Notiz,
- Experiment,
- Research Note,
- Meeting Note,
- Sticky / Schnellnotiz,
- Fehleranalyse,
- Architekturentscheidung,
- Prompt-Test,
- Linux-Admin-Notiz,
- Rezeptversuch,
- Bibel-/Themen-Recherche.

**Akzeptanzkriterien:**

- Typ kann beim Erstellen gewählt werden.
- Typ kann später geändert werden.
- Typ wird in der Liste angezeigt.
- Filter nach Typ ist möglich.

#### F-031 Statusmodell

| Feld | Wert |
|---|---|
| Version | 0.5 |
| Priorität | Muss |
| Kategorie | Workflow |

Ein Eintrag muss einen Status besitzen.

**Standardstatus V1:**

- Entwurf,
- Geplant,
- In Arbeit,
- In Prüfung,
- Abgeschlossen,
- Zurückgestellt,
- Abgebrochen,
- Archiviert.

**Akzeptanzkriterien:**

- Status kann gesetzt werden.
- Status wird in der Liste angezeigt.
- Filter nach Status ist möglich.
- spätere Status wie Freigegeben oder Signiert sind architektonisch möglich.

### 11.5 Tags und Collections

#### F-040 Tags

| Feld | Wert |
|---|---|
| Version | 0.5 |
| Priorität | Muss |
| Kategorie | Organisation |

Einträge müssen mit Tags verschlagwortet werden können.

**Akzeptanzkriterien:**

- Tags können hinzugefügt werden.
- Tags können entfernt werden.
- Tags werden gespeichert.
- Tags können zur Filterung genutzt werden.

#### F-041 Collections / Sammlungen

| Feld | Wert |
|---|---|
| Version | 1.0 |
| Priorität | Muss |
| Kategorie | Organisation |

Collections gruppieren Einträge innerhalb eines Projekts.

**Akzeptanzkriterien:**

- Collection erstellen,
- Collection bearbeiten,
- Collection löschen oder archivieren,
- Eintrag einer Collection zuordnen,
- nach Collection filtern.

**Architekturhinweis:**

V1 darf eine einfache Zuordnung implementieren. Das Datenmodell soll aber Mehrfachzuordnungen vorbereiten.

### 11.6 Vorlagen

#### F-050 Vorlagen verwenden

| Feld | Wert |
|---|---|
| Version | 0.5 |
| Priorität | Muss |
| Kategorie | Templates |

Ein Anwender muss beim Erstellen eines Eintrags eine Vorlage auswählen können.

**Akzeptanzkriterien:**

- Vorlage kann ausgewählt werden.
- neuer Eintrag erhält den Vorlageninhalt.
- Änderungen am Eintrag verändern die Vorlage nicht.

#### F-051 Vorlagenbibliothek

| Feld | Wert |
|---|---|
| Version | 1.0 |
| Priorität | Muss |
| Kategorie | Templates |

Die Anwendung muss eine sichtbare Vorlagenbibliothek besitzen.

**Mindestvorlagen V1:**

- Allgemeine Notiz,
- Experiment,
- Fehleranalyse,
- Meeting-Protokoll,
- ADR / Architekturentscheidung,
- Prompt-Test,
- Linux-Wartungsprotokoll,
- Bibel-/Themen-Recherche,
- Rezeptversuch.

#### F-052 Vorlagen bearbeiten

| Feld | Wert |
|---|---|
| Version | 1.1 |
| Priorität | Sollte |
| Kategorie | Templates |

Der Anwender soll Vorlagen bearbeiten oder eigene Vorlagen anlegen können.

**Akzeptanzkriterien:**

- neue Vorlage erstellen,
- vorhandene Vorlage duplizieren,
- Vorlage bearbeiten,
- Vorlage deaktivieren oder löschen.

### 11.7 Anhänge und Attachment Templates

#### F-060 Datei als Anhang hinzufügen

| Feld | Wert |
|---|---|
| Version | 0.5 / 1.0 |
| Priorität | Muss für 1.0 |
| Kategorie | Anhänge |

Ein Eintrag muss Dateien als Anhänge erhalten können.

**Akzeptanzkriterien:**

- Datei auswählen,
- Datei wird in Datenordner kopiert,
- Anhang erscheint im Eintrag,
- ursprünglicher Dateiname bleibt sichtbar,
- Dateigröße wird gespeichert,
- Anhang bleibt erhalten, wenn Originaldatei verschoben wird.

#### F-061 Anhang öffnen

| Feld | Wert |
|---|---|
| Version | 1.0 |
| Priorität | Muss |
| Kategorie | Anhänge |

Der Anwender muss einen Anhang mit der Standardanwendung des Betriebssystems öffnen können.

#### F-062 Anhang im Dateimanager anzeigen

| Feld | Wert |
|---|---|
| Version | 1.0 |
| Priorität | Sollte |
| Kategorie | Anhänge |

Der Anwender soll den Speicherort eines Anhangs im Dateimanager öffnen können.

#### F-063 Anhang entfernen

| Feld | Wert |
|---|---|
| Version | 1.0 |
| Priorität | Muss |
| Kategorie | Anhänge |

Ein Anhang muss aus einem Eintrag entfernt werden können.

**Akzeptanzkriterien:**

- Entfernen erfordert Rückfrage.
- Verhalten zur physischen Datei ist klar: löschen, archivieren oder nur Verknüpfung entfernen.
- Aktion wird im Activity Log erfasst.

#### F-064 Attachment Templates

| Feld | Wert |
|---|---|
| Version | 1.1 |
| Priorität | Sollte |
| Kategorie | Anhänge |

Der Anwender soll neue Anhänge aus Dateivorlagen erzeugen können.

**Beispiele:**

- leere CSV-Datei,
- Messwerttabelle,
- Markdown-Checkliste,
- Konfigurationsdatei-Vorlage,
- Prompt-Ergebnisdatei.

### 11.8 Suche und Filter

#### F-070 Einfache Suche

| Feld | Wert |
|---|---|
| Version | 0.5 |
| Priorität | Muss |
| Kategorie | Suche |

Die Anwendung muss Einträge nach Titel und Inhalt durchsuchen können.

#### F-071 Erweiterte Filter

| Feld | Wert |
|---|---|
| Version | 1.0 |
| Priorität | Muss |
| Kategorie | Suche |

Die Anwendung muss Einträge filtern können nach:

- Projekt,
- Collection,
- Typ,
- Status,
- Tag,
- Datum optional.

#### F-072 Volltextsuche optimieren

| Feld | Wert |
|---|---|
| Version | 2.0 |
| Priorität | Sollte |
| Kategorie | Suche |

Die Suche soll später für größere Datenmengen verbessert werden, z. B. über SQLite FTS.

### 11.9 Querverweise und interne Links

#### F-080 Querverweise vorbereiten

| Feld | Wert |
|---|---|
| Version | 1.0 |
| Priorität | Sollte |
| Kategorie | Wissensnetz |

Das Datenmodell soll Querverweise zwischen Einträgen unterstützen.

**Relationstypen:**

- basiert auf,
- bestätigt,
- widerspricht,
- ersetzt,
- gehört zu,
- verwendet,
- vergleicht mit,
- Wiederholung von.

#### F-081 Interne Links und Backlinks

| Feld | Wert |
|---|---|
| Version | 2.0 |
| Priorität | Sollte |
| Kategorie | Wissensnetz |

Der Anwender soll Einträge intern verlinken können. Backlinks sollen anzeigen, welche Einträge auf einen Eintrag verweisen.

### 11.10 Activity Log und Historie

#### F-090 Activity Log Light

| Feld | Wert |
|---|---|
| Version | 1.0 |
| Priorität | Sollte |
| Kategorie | Nachvollziehbarkeit |

Die Anwendung soll wichtige Aktionen protokollieren.

**Aktionen V1:**

- Projekt erstellt,
- Projekt geändert,
- Eintrag erstellt,
- Eintrag geändert,
- Status geändert,
- Anhang hinzugefügt,
- Anhang entfernt,
- Export erstellt,
- Backup erstellt,
- Restore durchgeführt.

#### F-091 Versionshistorie

| Feld | Wert |
|---|---|
| Version | 2.0 |
| Priorität | Muss für V2 |
| Kategorie | Nachvollziehbarkeit |

Änderungen an Einträgen sollen versioniert oder als Snapshots gespeichert werden.

#### F-092 Audit Trail erweitert

| Feld | Wert |
|---|---|
| Version | 4.0 |
| Priorität | Optional / Profi |
| Kategorie | Compliance |

Ein manipulationsärmerer Audit Trail kann später umgesetzt werden.

Optionen:

- Append-only-Log,
- Hash-Kette,
- signierte Snapshots,
- Sperren freigegebener Einträge.

### 11.11 Checklisten, Aufgaben und Wiedervorlage

#### F-100 Markdown-Checklisten

| Feld | Wert |
|---|---|
| Version | 1.1 |
| Priorität | Sollte |
| Kategorie | Arbeitsfluss |

Einträge sollen einfache Checklisten enthalten können.

**V1.1-Ansatz:**

```markdown
- [ ] Messung durchführen
- [ ] Foto anhängen
- [ ] Ergebnis prüfen
- [ ] Export erstellen
```

#### F-101 Fälligkeit und Wiedervorlage

| Feld | Wert |
|---|---|
| Version | 2.0 |
| Priorität | Optional |
| Kategorie | Arbeitsfluss |

Einträge oder Checklistenpunkte können Fälligkeitsdaten oder Wiedervorlagen erhalten.

### 11.12 Timer und Zeitstempel

#### F-110 Zeitstempel einfügen

| Feld | Wert |
|---|---|
| Version | 2.0 |
| Priorität | Sollte |
| Kategorie | Zeitmanagement |

Der Anwender soll einen Zeitstempel in den Eintrag einfügen können.

#### F-111 Stoppuhr und Countdown

| Feld | Wert |
|---|---|
| Version | 2.0 |
| Priorität | Sollte |
| Kategorie | Zeitmanagement |

Die Anwendung soll einfache Timer, Stoppuhr und Countdown unterstützen.

### 11.13 Ressourcenbibliothek

#### F-120 Ressourcen verwalten

| Feld | Wert |
|---|---|
| Version | 2.0 |
| Priorität | Sollte |
| Kategorie | Referenzen |

Die Anwendung soll häufig verwendete Ressourcen verwalten können.

**Ressourcentypen:**

- PDF,
- Bild,
- Text,
- Link,
- HTML-/JavaScript-Minietool,
- Cheat Sheet.

**Beispiele:**

- Linux-Kommandos,
- Git-Cheat-Sheet,
- SQL-Referenz,
- Periodensystem,
- Prompt-Checkliste,
- Bibelstellen-/Quellenübersichten.

### 11.14 Dashboard

#### F-130 Dashboard

| Feld | Wert |
|---|---|
| Version | 2.0 |
| Priorität | Sollte |
| Kategorie | Übersicht |

Die Anwendung soll eine Startseite mit Arbeitsüberblick anbieten.

**Elemente:**

- laufende Einträge,
- Einträge in Prüfung,
- offene Checklisten,
- aktive Timer,
- kürzlich bearbeitet,
- zuletzt hinzugefügte Anhänge,
- fällige oder zurückgestellte Einträge.

### 11.15 Review und Freigabe

#### F-140 Review-Workflow

| Feld | Wert |
|---|---|
| Version | 4.0 |
| Priorität | Optional |
| Kategorie | Zusammenarbeit |

Einträge können später zur Prüfung gegeben, kommentiert, freigegeben oder zurückgewiesen werden.

#### F-141 Signieren / Freigeben

| Feld | Wert |
|---|---|
| Version | 4.0 |
| Priorität | Optional |
| Kategorie | Nachvollziehbarkeit |

Einträge können später intern signiert oder freigegeben werden.

**Wichtig:**

Dies ist zunächst keine automatisch rechtsverbindliche qualifizierte elektronische Signatur.

---

## 12. Fachprofile und Produktfamilie

### 12.1 Profilprinzip

Profile sind in frühen Versionen keine echten Plugins. Sie bestehen zunächst aus:

- Eintragstypen,
- Vorlagen,
- Standardstatus,
- Begriffen,
- empfohlenen Tags,
- Exportvorgaben.

Echte Module mit eigener Logik folgen später.

### 12.2 General Workbench Profil

Allgemeines Profil für Projekt- und Arbeitsdokumentation.

**Eintragstypen:**

- Notiz,
- Meeting,
- Entscheidung,
- Aufgabe,
- Recherche,
- Ergebnis.

### 12.3 LabBook / Research Profil

Profil für wissenschaftliche Experimente und Forschungsnotizen.

**Eintragstypen:**

- Experiment,
- Protokoll,
- Messung,
- Probe,
- Beobachtung,
- Forschungsnotiz,
- Literatur-/Paper-Notiz.

**Spätere Spezialfunktionen:**

- Messwerttabellen,
- Einheiten,
- Geräte,
- Kalibrierung,
- Probenverwaltung,
- PDF-Journal.

### 12.4 Software / Engineering Profil

Profil für SASD Engineering Standards und Softwareentwicklung.

**Eintragstypen:**

- Fehleranalyse,
- Testlauf,
- Architekturentscheidung,
- Deployment-Protokoll,
- Code-Review-Notiz,
- Release-Notiz,
- Risiko,
- technische Entscheidung.

**Spezielle Felder:**

- Git-Commit,
- Branch,
- Version,
- Umgebung,
- Reproduktionsschritte,
- Build-/Testergebnis,
- Rollback-Hinweis.

### 12.5 Linux Admin Profil

Profil für Systemadministration und Betrieb.

**Eintragstypen:**

- Serverdokumentation,
- Konfigurationsstand,
- Change,
- Incident,
- Wartungsprotokoll,
- Script-Dokumentation,
- Sicherheitsnotiz.

**Spätere Spezialfunktionen:**

- Script-Collector,
- regelmäßige Systemabfragen,
- Konfigurationsvergleich,
- Integration mit SASD Sentinel oder Monitoring.

### 12.6 Prompt Notebook Profil

Profil für KI- und Prompt-Experimente.

**Eintragstypen:**

- Prompt,
- Prompt-Test,
- Modellvergleich,
- Ergebnisbewertung,
- Prompt-Version,
- System-Prompt-Notiz.

**Spezielle Felder:**

- Modell,
- Parameter,
- Prompttext,
- Antwort,
- Bewertung,
- Datenschutzklassifikation,
- nächste Iteration.

### 12.7 Biblical Research Profil

Profil für länger laufende Themenrecherchen.

**Eintragstypen:**

- Thema,
- Bibelstelle,
- Quellenanalyse,
- Argumentationskette,
- offene Frage,
- Zwischenfazit,
- Studiennotiz.

**Besondere Anforderungen:**

- parallele Themen,
- schrittweise Forschungsstände,
- offene Fragen,
- Quellen und Links,
- Export nach Markdown,
- später Obsidian-/DokuWiki-Export.

### 12.8 Recipe / Cooking Experiments Profil

Profil für Rezeptentwicklung.

**Eintragstypen:**

- Rezept,
- Rezeptversuch,
- Variante,
- Geschmacksbewertung,
- Verbesserungsidee,
- Einkaufsidee.

### 12.9 Food & Health Profil

Profil für Ernährungstagebuch, Rezeptverträglichkeit und persönliche Beobachtungen.

**Wichtige Abgrenzung:**

Die Anwendung darf keine medizinische Diagnose stellen und keine Therapieempfehlungen geben. Sie kann Daten dokumentieren, strukturieren und exportieren. Medizinische Bewertung bleibt qualifizierten Fachpersonen vorbehalten.

**Eintragstypen:**

- Mahlzeit,
- Rezept,
- Verträglichkeitsnotiz,
- Blutwertnotiz,
- Tagesprotokoll,
- Einkaufsidee.

**Spätere Funktionen:**

- Export für Arztgespräch,
- CSV-Import von Messwerten,
- Rezeptvarianten,
- Unverträglichkeitsnotizen,
- Verschlüsselungsoptionen.

---

## 13. Datenmodell und Persistenz

### 13.1 Datenhaltung V1

V1 nutzt:

- SQLite-Datenbank für strukturierte Daten,
- lokalen Datenordner für Anhänge,
- eindeutige IDs für Projekte, Einträge, Anhänge, Tags und Vorlagen,
- keine Pflicht-Cloud.

### 13.2 Empfohlene Datenordnerstruktur

```text
SASDWorkbenchData/
  workbench.db
  attachments/
    project-{projectId}/
      entry-{entryId}/
        {attachmentId}_{originalFileName}
  backups/
  exports/
  templates/
  logs/
  resources/
```

### 13.3 Zentrale Tabellen V1

```text
projects
collections
entries
entry_types
entry_statuses
templates
attachments
tags
entry_tags
activity_log
```

### 13.4 Tabellen für spätere Versionen

```text
entry_links
entry_versions
external_links
resources
timers
checklist_items
profiles
profile_entry_types
profile_templates
users
roles
review_comments
signatures
audit_log
```

### 13.5 Grundsätze

- Alle zentralen Objekte erhalten stabile IDs.
- Zeitstempel werden konsistent gespeichert.
- Anhänge werden nicht nur verlinkt, sondern kontrolliert kopiert.
- Exportformate müssen nachvollziehbar und möglichst offen sein.
- Das Datenmodell soll spätere Profile nicht blockieren.

---

## 14. Benutzeroberfläche und Bedienkonzept

### 14.1 Grundlayout

Die erste Oberfläche soll ein Drei-Bereich-Layout verwenden:

```text
[Navigation] | [Eintragsliste] | [Editor / Details]
```

### 14.2 Linker Bereich: Navigation

Enthält:

- Projekte,
- Collections,
- Eintragstypen,
- Statusfilter,
- Tags,
- später gespeicherte Filter.

### 14.3 Mittlerer Bereich: Eintragsliste

Enthält:

- Liste der gefilterten Einträge,
- Titel,
- Typ,
- Status,
- Änderungsdatum,
- Tags optional,
- Suchfeld.

### 14.4 Rechter Bereich: Editor / Details

Enthält:

- Titel,
- Typ,
- Status,
- Tags,
- Inhalt,
- Anhänge,
- Querverweise,
- Aktionen wie Speichern, Exportieren, Anhang hinzufügen.

### 14.5 Bedienprinzipien

- Funktion vor Schönheit, aber nicht chaotisch.
- Ruhige, sachliche, technisch-professionelle Oberfläche.
- Häufige Aktionen mit wenigen Klicks.
- Keine versteckte Datenvernichtung.
- Verständliche Fehlermeldungen.
- Klarer Speicherzustand.

---

## 15. Export, Import, Backup und Portabilität

### 15.1 Markdown-Export

V1 muss Markdown-Export unterstützen.

**Exportumfang:**

- Einzeleintrag,
- Projekt als Ordnerstruktur,
- Metadaten,
- Anhänge optional,
- relative Links zu Anhängen.

### 15.2 HTML-Export

V1 oder V1.1 soll einfachen HTML-Export unterstützen.

**Ziel:**

- lokale Lesbarkeit im Browser,
- einfache Weitergabe,
- Grundlage für spätere PDF-Erzeugung.

### 15.3 PDF-Export

PDF-Export ist für V2 geplant.

**Varianten:**

- Einzeleintrag,
- Projektbericht,
- Tagesjournal,
- Wochenjournal,
- vollständiges Journal,
- Auswahl nach Zeitraum, Collection, Status oder Tag.

### 15.4 Backup

Backup ist V1-Muss.

**Backup umfasst:**

- SQLite-Datenbank,
- Anhänge,
- Vorlagen,
- Konfiguration,
- optional Manifest.

### 15.5 Restore

Restore ist V1-Muss.

**Anforderungen:**

- Backup-Struktur prüfen,
- vor Überschreiben warnen,
- Datenbank und Anhänge wiederherstellen,
- Restore protokollieren.

### 15.6 Projekt-Archiv

V2 soll Projektarchive ermöglichen.

Beispiel:

```text
project-export.zip
  manifest.json
  entries/
  attachments/
  templates/
  metadata/
```

### 15.7 Obsidian- und DokuWiki-Export

Spätere Versionen sollen Markdown-basierte Exporte so gestalten, dass sie mit Obsidian gut nutzbar sind. Für DokuWiki kann später ein eigener Exportfilter entstehen.

---

## 16. Sicherheit, Datenschutz und Compliance-Abgrenzung

### 16.1 Lokale Datenhoheit

V1 speichert Daten lokal. Es gibt keine automatische Cloud und keine automatische Verbindung zu externen Diensten.

### 16.2 Sensible Daten

Bei einigen Profilen können sensible Daten entstehen:

- Gesundheitsdaten,
- Kundendaten,
- interne Firmeninformationen,
- Forschungsdaten,
- vertrauliche Projektinformationen.

V1 schützt diese Daten nicht gegen Angreifer mit Zugriff auf den Rechner. Dies muss dokumentiert werden.

### 16.3 Medizinische Abgrenzung

Das Food-&-Health-Profil darf keine medizinische Diagnose oder Therapieempfehlung geben. Es dient nur zur Dokumentation, Strukturierung und zum Export eigener Beobachtungen.

### 16.4 Compliance-Abgrenzung

V1 ist keine validierte Laborsoftware und erfüllt nicht automatisch GLP, GxP, FDA 21 CFR Part 11 oder vergleichbare regulatorische Anforderungen. Einige Konzepte wie Historie, Signatur und Audit Trail können später vorbereitet werden.

### 16.5 Löschkonzept

V1 soll vorsichtig löschen:

- bevorzugt archivieren,
- harte Löschung nur mit Rückfrage,
- Anhänge nicht unkontrolliert entfernen,
- Löschaktionen im Activity Log erfassen.

### 16.6 Verschlüsselung

Verschlüsselung ist für V4 oder spezielle Profile vorgesehen. Vor Umsetzung müssen Recovery, Passwortverlust und Backup-Strategie geklärt werden.

---

## 17. Nichtfunktionale Anforderungen

### NF-001 Verständlichkeit des Codes

Der Code muss gut kommentiert sein. Öffentliche Klassen und Methoden erhalten C#-XML-Kommentare. Komplexe Stellen erhalten zusätzliche Inline-Kommentare.

### NF-002 Wartbarkeit

Die Geschäftslogik darf nicht in der UI verschwinden. Application Services, Repositories und Domain-Objekte müssen getrennt sein.

### NF-003 Erweiterbarkeit

Neue Eintragstypen, Vorlagen und Profile müssen später ergänzt werden können, ohne das System neu zu schreiben.

### NF-004 Offline-Fähigkeit

V1 muss vollständig offline funktionieren.

### NF-005 Portabilität

Daten müssen exportierbar sein. Markdown-Export und Backup sind strategisch wichtig.

### NF-006 Datensicherheit gegen Verlust

Backup, Restore, Rückfragen vor Löschung und kontrollierte Anhangsablage sind Pflicht.

### NF-007 Performance

V1 soll mit mehreren Tausend Einträgen sinnvoll arbeiten können. Die Suche darf zunächst einfach sein, muss aber später optimierbar bleiben.

### NF-008 Fehlertransparenz

Fehler beim Speichern, Exportieren, Backup oder Restore dürfen nicht stillschweigend verschwinden.

### NF-009 Dokumentation

Das Projekt muss mindestens enthalten:

- README,
- Pflichtenheft,
- technisches Design,
- Datenbankdesign,
- Anwenderdokumentation,
- Entwicklerdokumentation.

### NF-010 Testbarkeit

Application Services und Repositories müssen testbar sein. UI-Logik soll möglichst dünn bleiben.

---

## 18. Architekturvorgaben

### 18.1 Empfohlene Solution-Struktur

```text
sasd-workbench/
  README.md
  LICENSE
  docs/
    010_Lastenheft.md
    020_Pflichtenheft_MVP.md
    030_Technical_Design.md
    040_Database_Design.md
    050_User_Guide.md
    060_Developer_Guide.md

  src/
    SASD.Workbench.Domain/
      Entities/
      ValueObjects/
      Enums/

    SASD.Workbench.Application/
      Services/
      UseCases/
      DTOs/
      Interfaces/

    SASD.Workbench.Infrastructure/
      Database/
      Repositories/
      FileStorage/
      Export/
      Backup/

    SASD.Workbench.WinForms/
      Forms/
      Controls/
      ViewModels/

  tests/
    SASD.Workbench.Tests/
```

### 18.2 Abhängigkeitsrichtung

```text
WinForms → Application → Domain
Infrastructure → Application / Domain
```

Die Domain darf keine Infrastruktur- oder UI-Abhängigkeiten haben.

### 18.3 Verbotene Antipatterns

Zu vermeiden:

- ein riesiges `Form1.cs`,
- SQL direkt in Button-Click-Events,
- Geschäftslogik in UI-Controls,
- unkontrollierte Dateipfade,
- Anhänge nur als absolute Links,
- fehlende Backups,
- fehlende Fehlermeldungen.

---

## 19. Teststrategie und Abnahmekriterien

### 19.1 Unit-Tests

Unit-Tests prüfen:

- Projektservice,
- Eintragsservice,
- Vorlagenanwendung,
- Tag-Verwaltung,
- Exportlogik,
- Backupservice,
- Validierungen.

### 19.2 Integrationstests

Integrationstests prüfen:

- SQLite-Datenbank,
- Repositories,
- Datenbankinitialisierung,
- Anhänge im Dateisystem,
- Backup/Restore.

### 19.3 Manuelle UI-Tests

Manuelle Tests prüfen:

- erster Start,
- Projekt anlegen,
- Eintrag erfassen,
- Vorlage anwenden,
- Anhang hinzufügen,
- Suche,
- Export,
- Backup,
- Restore,
- Neustart.

### 19.4 Abnahmekriterien für Version 1.0

Version 1.0 gilt als erreicht, wenn:

1. Die Anwendung auf Windows startet.
2. Projekte angelegt, bearbeitet und archiviert werden können.
3. Einträge mit Typ, Status, Tags und Inhalt erfasst werden können.
4. Vorlagen angewendet werden können.
5. Anhänge hinzugefügt, geöffnet und im Dateisystem angezeigt werden können.
6. Anhänge in den Workbench-Datenordner kopiert werden.
7. Suche und Filter funktionieren.
8. Einträge oder Projekte als Markdown exportiert werden können.
9. Backup und Restore in einem Testfall funktionieren.
10. Daten nach Neustart erhalten bleiben.
11. Wichtige Aktionen im Activity Log Light erscheinen.
12. Die Architektur nicht aus einem UI-Monolithen besteht.
13. README und erste Anwenderdokumentation vorhanden sind.

---

## 20. Entwicklungsreihenfolge

Empfohlene Reihenfolge:

1. GitHub-Repository anlegen.
2. README, Lizenz, Screenshot-Mockup und Dokumentation ablegen.
3. .NET-Solution-Struktur erzeugen.
4. Domain-Entitäten definieren.
5. SQLite-Schema erstellen.
6. Datenbankinitialisierung implementieren.
7. Repositories implementieren.
8. Application Services implementieren.
9. einfache WinForms-Oberfläche erstellen.
10. Projektverwaltung umsetzen.
11. Eintragsverwaltung umsetzen.
12. Typen, Status und Tags umsetzen.
13. Vorlagen anwenden.
14. Anhänge speichern.
15. Suche und Filter einbauen.
16. Markdown-Export umsetzen.
17. Backup/Restore umsetzen.
18. Activity Log Light einbauen.
19. Tests schreiben.
20. Dokumentation ergänzen.
21. V1.0 stabilisieren.

---

## 21. Risiken, Annahmen und offene Entscheidungen

### 21.1 Risiken

| Risiko | Beschreibung | Gegenmaßnahme |
|---|---|---|
| Zu großer V1-Scope | Zu viele Fachprofile werden sofort umgesetzt | Core-MVP strikt begrenzen |
| UI-Monolith | Windows Forms verführt zu Logik in Formularen | Schichtenarchitektur erzwingen |
| Datenverlust | Lokale Daten ohne Backup gefährlich | Backup/Restore in V1 |
| Profil-Wildwuchs | Zu viele Spezialfälle im Kern | Profile zunächst als Vorlagen/Konfiguration |
| Gesundheitsdaten | Datenschutz- und Haftungsrisiken | klare Abgrenzung, keine Diagnosen |
| Sync-Komplexität | Konflikte bei Dateisync schwer lösbar | Sync erst später |
| KI-Datenschutz | externe KI könnte sensible Daten sehen | KI nur optional und transparent |

### 21.2 Offene Entscheidungen

1. Wird der Inhalt in V1 nur als Markdown-Text gespeichert oder zusätzlich als Blockmodell vorbereitet?
2. Werden Collections sofort als Mehrfachzuordnung umgesetzt?
3. Wird Soft Delete von Anfang an verwendet?
4. Welche Lizenz soll endgültig gelten: MIT, Apache 2.0 oder AGPL?
5. Wird HTML-Export in V1 oder V1.1 umgesetzt?
6. Wird SQLite FTS direkt vorbereitet?
7. Wie stark wird Activity Log Light in V1 umgesetzt?
8. Sind Vorlagen in V1 editierbar oder zunächst fest eingebaut?
9. Wird die App später als Community Edition und kommerzielle Support-Version positioniert?
10. Wann wird ein Wechsel zu Avalonia, WPF oder Web-UI geprüft?

---

## 22. Anhang A: Initiale Vorlagen

### 22.1 Allgemeine Notiz

```markdown
# Notiz

## Kontext

## Inhalt

## Nächste Schritte
```

### 22.2 Experiment

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

### 22.3 Fehleranalyse

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

### 22.4 Meeting-Protokoll

```markdown
# Meeting

## Datum / Teilnehmer

## Ziel

## Besprochene Punkte

## Entscheidungen

## Offene Punkte

## Nächste Schritte
```

### 22.5 ADR / Architekturentscheidung

```markdown
# Architekturentscheidung

## Kontext

## Problem

## Optionen

## Entscheidung

## Begründung

## Konsequenzen

## Status
```

### 22.6 Linux-Wartungsprotokoll

```markdown
# Linux-Wartungsprotokoll

## System

## Ausgangslage

## Durchgeführte Arbeiten

## Geänderte Dateien / Konfigurationen

## Befehle

## Ergebnis

## Rollback-Hinweise
```

### 22.7 Prompt-Test

```markdown
# Prompt-Test

## Ziel

## Prompt

## Modell / Einstellungen

## Ergebnis

## Bewertung

## Verbesserungen

## Nächste Iteration
```

### 22.8 Bibel-/Themen-Recherche

```markdown
# Thema / Frage

## Ausgangsfrage

## Bibelstellen

## Quellen / Literatur

## Beobachtungen

## Argumentationskette

## Offene Fragen

## Zwischenfazit

## Nächste Schritte
```

### 22.9 Rezeptversuch

```markdown
# Rezeptversuch

## Ziel

## Zutaten

## Mengen

## Zubereitung

## Variante

## Geschmack / Verträglichkeit

## Verbesserungsideen

## Ergebnis
```

---

## 23. Anhang B: Anforderungskatalog Kurzmatrix

| ID | Funktion | Version | Priorität |
|---|---|---:|---|
| F-001 | Solution- und Projektstruktur | 0.1 | Muss |
| F-002 | SQLite-Datenbankinitialisierung | 0.1 | Muss |
| F-010 | Projekt anlegen | 0.1/0.5 | Muss |
| F-011 | Projekt bearbeiten | 0.5 | Muss |
| F-012 | Projekt archivieren/löschen | 0.5/1.0 | Muss |
| F-020 | Eintrag anlegen | 0.1/0.5 | Muss |
| F-021 | Eintrag bearbeiten/speichern | 0.5 | Muss |
| F-022 | Eintrag duplizieren | 0.5/1.0 | Sollte |
| F-023 | Eintrag löschen/archivieren | 0.5/1.0 | Muss |
| F-030 | Eintragstypen | 0.5 | Muss |
| F-031 | Statusmodell | 0.5 | Muss |
| F-040 | Tags | 0.5 | Muss |
| F-041 | Collections | 1.0 | Muss |
| F-050 | Vorlagen verwenden | 0.5 | Muss |
| F-051 | Vorlagenbibliothek | 1.0 | Muss |
| F-052 | Vorlagen bearbeiten | 1.1 | Sollte |
| F-060 | Datei als Anhang hinzufügen | 1.0 | Muss |
| F-061 | Anhang öffnen | 1.0 | Muss |
| F-062 | Anhang im Dateimanager anzeigen | 1.0 | Sollte |
| F-063 | Anhang entfernen | 1.0 | Muss |
| F-064 | Attachment Templates | 1.1 | Sollte |
| F-070 | Einfache Suche | 0.5 | Muss |
| F-071 | Erweiterte Filter | 1.0 | Muss |
| F-072 | Volltextsuche optimieren | 2.0 | Sollte |
| F-080 | Querverweise vorbereiten | 1.0 | Sollte |
| F-081 | Interne Links und Backlinks | 2.0 | Sollte |
| F-090 | Activity Log Light | 1.0 | Sollte |
| F-091 | Versionshistorie | 2.0 | Muss |
| F-092 | Audit Trail erweitert | 4.0 | Optional |
| F-100 | Markdown-Checklisten | 1.1 | Sollte |
| F-101 | Fälligkeit/Wiedervorlage | 2.0 | Optional |
| F-110 | Zeitstempel | 2.0 | Sollte |
| F-111 | Stoppuhr/Countdown | 2.0 | Sollte |
| F-120 | Ressourcenbibliothek | 2.0 | Sollte |
| F-130 | Dashboard | 2.0 | Sollte |
| F-140 | Review-Workflow | 4.0 | Optional |
| F-141 | Signieren/Freigeben | 4.0 | Optional |

---

## 24. Anhang C: Empfohlene GitHub-Repository-Struktur

```text
sasd-workbench/
  README.md
  LICENSE
  .gitignore
  .editorconfig
  docs/
    010_Lastenheft.md
    020_Pflichtenheft_MVP.md
    030_Technical_Design.md
    040_Database_Design.md
    050_User_Guide.md
    060_Developer_Guide.md
    images/
      screenshot-sasd-workbench-v1-mockup.png
  src/
    SASD.Workbench.Domain/
    SASD.Workbench.Application/
    SASD.Workbench.Infrastructure/
    SASD.Workbench.WinForms/
  tests/
    SASD.Workbench.Tests/
  tools/
  samples/
  templates/
```

---

## 25. Zusammenfassung

Die SASD Workbench soll als lokale, robuste und erweiterbare Kernanwendung beginnen. Version 1 konzentriert sich auf die strukturierte Erfassung und Verwaltung von Projekten, Einträgen, Vorlagen, Anhängen, Tags, Suche, Export und Backup.

Die Anwendung wird bewusst nicht als Spezialprogramm für nur ein Thema entwickelt. Stattdessen entsteht ein neutraler Core, der später über Profile und Module zu mehreren SASD-Produkten erweitert werden kann.

Die wichtigsten Entscheidungen lauten:

1. **Core zuerst, Profile später.**
2. **Entry als zentrales, allgemeines Inhaltsobjekt.**
3. **V1 lokal, offline und ohne Cloud-Pflicht.**
4. **Markdown und Backup früh unterstützen.**
5. **Datenmodell für Historie, Querverweise, Profile und spätere Module vorbereiten.**
6. **Keine medizinischen, regulatorischen oder rechtlichen Versprechen in V1.**
7. **Saubere Architektur statt schneller UI-Monolith.**
8. **Dokumentation und Testbarkeit von Anfang an berücksichtigen.**

Damit entsteht ein realistischer Weg zu einem schnell nutzbaren Produkt, das später zu einer ernsthaften SASD-Plattform für strukturierte Erkenntnisarbeit ausgebaut werden kann.
