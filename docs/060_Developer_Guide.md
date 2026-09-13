# SASD Workbench – Developer Guide

> **Status:** Working Guide  
> **Stand:** 2026-09-13  
> **Scope:** gemeinsamer SASD Workbench Core und darauf aufbauende Hosts/Profile

## 1. Ziel dieses Dokuments

Dieser Guide beschreibt, wie die gemeinsame Workbench-Codebasis weiterentwickelt werden soll, ohne dass spätere SASD Notebook-/Workbench-Produkte dieselben Basiskomponenten erneut implementieren.

Die wichtigste technische Leitlinie lautet:

> **Gemeinsame Fähigkeit einmal im Core bauen; Fachbedeutung im Profil lassen.**

Ein neues Feature gehört nur dann in den gemeinsamen Core, wenn es fachneutral modelliert werden kann und plausibel mehr als einem Profil nutzt.

## 2. Technische Basis

Aktueller Baseline-Stack:

- C# / .NET 10
- Windows Forms als erster Desktop-Host
- SQLite über `Microsoft.Data.Sqlite`
- lokaler kontrollierter Dateispeicher für Attachments
- `Microsoft.Extensions.DependencyInjection` für Composition
- xUnit v3 für geschichtete Unit-/Integrationstests
- GitHub Actions auf Windows

Das Repository pinnt die .NET-SDK-Linie über `global.json`. `Directory.Build.props` aktiviert Nullable Reference Types, aktuelle Analyzer und `TreatWarningsAsErrors`.

## 3. Repository-Struktur

```text
src/
  SASD.Workbench.Domain/
  SASD.Workbench.Application/
  SASD.Workbench.Infrastructure/
  SASD.Workbench.WinForms/

tests/
  SASD.Workbench.Domain.Tests/
  SASD.Workbench.Application.Tests/
  SASD.Workbench.Infrastructure.Tests/
  SASD.Workbench.SmokeTests/

docs/
  adr/
```

### Domain

Enthält fachneutrale Core-Objekte und stabile gemeinsame Metadaten/Vokabulare.

Beispiele:

- `Project`
- `Entry`
- `Template`
- `Tag`
- `Collection`
- `EntryLink`
- `Attachment`
- `ActivityLogItem`
- `CoreEntryTypes`
- `EntryRelationTypes`
- `CoreActivityTypes`

Die Domain darf nicht von WinForms, SQLite oder Dateisystemimplementierungen abhängen.

### Application

Enthält Use Cases, Ports/Interfaces und fachneutrale Ablaufregeln.

Beispiele:

- `ProjectService`
- `EntryService`
- `TemplateService`
- `CollectionService`
- `EntryLinkService`
- `SearchService`
- Repository-/Storage-Interfaces
- `CoreTemplateCatalog`

Application kennt keine WinForms-Controls und kein konkretes SQLite-SQL.

### Infrastructure

Implementiert technische Adapter:

- SQLite-Repositories
- Migration Runner
- kontrollierten File Storage
- Markdown Export
- Backup/Restore
- Activity-Recording an der SQLite-Persistenzgrenze
- gemeinsame Dependency-Injection-Registrierung

### WinForms

Ist ein Host, nicht der Core.

Die UI darf:

- Eingaben sammeln,
- Application Services aufrufen,
- Resultate anzeigen,
- Benutzerbestätigungen einholen.

Die UI darf nicht:

- SQL ausführen,
- Datenbanktabellen kennen,
- eigene Backup-/Restore-Regeln implementieren,
- eigene Kopien der Core-Validierung pflegen,
- Activity-Einträge als Ersatz für die zentrale Persistenzlogik erzeugen.

## 4. Lokale Entwicklungsumgebung

Voraussetzung ist ein kompatibles .NET-10-SDK entsprechend `global.json`.

Vom Repository-Root:

```powershell
dotnet --version
dotnet restore SASD-Workbench.slnx
dotnet build SASD-Workbench.slnx --configuration Release --no-restore

dotnet run --project tests/SASD.Workbench.Domain.Tests/SASD.Workbench.Domain.Tests.csproj --configuration Release --no-build
dotnet run --project tests/SASD.Workbench.Application.Tests/SASD.Workbench.Application.Tests.csproj --configuration Release --no-build
dotnet run --project tests/SASD.Workbench.Infrastructure.Tests/SASD.Workbench.Infrastructure.Tests.csproj --configuration Release --no-build
dotnet run --project tests/SASD.Workbench.SmokeTests/SASD.Workbench.SmokeTests.csproj --configuration Release --no-build
```

Der Release-Build ist maßgeblich, weil Warnungen als Fehler behandelt werden. Die Tests werden bewusst in Schichten ausgeführt: reine Regeln zuerst, reale SQLite-/Dateisystemintegration danach und der breite End-to-End-Smoke-Test zuletzt.

Zum Starten des Desktop-Hosts:

```powershell
dotnet run --project src/SASD.Workbench.WinForms/SASD.Workbench.WinForms.csproj
```

## 5. Gemeinsamen Core korrekt registrieren

Hosts sollen die Core-Abhängigkeiten nicht einzeln verdrahten.

Verwende:

```csharp
services.AddSasdWorkbenchCore(paths);
```

Die zentrale Registrierung enthält Repositories, Application Services, File Storage, Export, Backup/Restore und weitere profile-neutrale Infrastruktur.

Ein Fachhost ergänzt danach nur seine eigenen Komponenten:

```text
AddSasdWorkbenchCore(...)
        +
Profil-/Host-spezifische Services
        +
Profil-/Host-spezifische UI
```

Siehe `docs/adr/ADR-001-shared-core-composition.md`.

## 6. Core oder Profil?

Vor jeder neuen Klasse/Tabelle ist zu prüfen:

1. Nutzen mindestens zwei plausible Profile diese Fähigkeit?
2. Kann sie fachneutral benannt werden?
3. Muss der Core die fachliche Bedeutung verstehen?
4. Reicht ein neuer Entry Type, Relation Type oder Template?
5. Ist wirklich eine neue Tabelle nötig?

Beispiele für Core:

- Relation zwischen zwei Entries
- Timeline Event
- Attachment
- Measurement als neutraler Wert mit Einheit
- Source Reference

Beispiele für Profil:

- medizinische Interpretation eines Messwerts
- theologische Bewertung einer Bibelstelle
- Linux-spezifische Incident-Klassifikation
- Prompt-Modell-spezifische Bewertungslogik

## 7. Offene Vokabulare statt vorschneller Enums

Entry- und Relation-Typen sind bewusst erweiterbar. Gemeinsame Keys werden zentral definiert, aber Profile dürfen zusätzliche valide Keys ergänzen.

Dadurch muss ein späteres Fachprofil nicht den Core ändern, nur um einen profilspezifischen Begriff einzuführen.

Siehe `docs/adr/ADR-002-open-core-vocabulary.md`.

## 8. Datenbankänderungen und Migrationen

Schemaänderungen erfolgen ausschließlich über versionierte Migrationen unter:

```text
src/SASD.Workbench.Infrastructure/Database/Migrations/
```

Regeln:

- bestehende veröffentlichte Migrationen nicht nachträglich umdeuten,
- neue Änderung = neue Migration,
- Migrationen müssen wiederholt ausführbar bzw. vom Migrator eindeutig als bereits angewendet erkennbar sein,
- Foreign Keys und relevante Indizes explizit berücksichtigen,
- Backup/Restore-Kompatibilität prüfen,
- Infrastructure-/Smoke-Tests um den neuen Roundtrip erweitern.

Eine neue Tabelle ist kein Standardmittel für ein neues Fachfeature. Erst prüfen, ob vorhandene Core-Primitiven ausreichen.

## 9. Persistenz und Activity History

Automatische Core-Aktivitäten werden bei SQLite-Mutationen an der Repository-Grenze geschrieben.

Mutation und Activity-Eintrag verwenden dieselbe SQLite-Transaktion. Ein fehlgeschlagener Activity-Insert darf deshalb keine erfolgreich persistierte Core-Mutation zurücklassen.

Wichtige Regeln:

- nur tatsächliche Änderungen protokollieren,
- idempotente No-ops erzeugen keine doppelten Activities,
- stabile Action Keys aus `CoreActivityTypes` verwenden,
- Activity Log nicht als manipulationssicheren Audit Trail bezeichnen,
- explizite fachliche Ereignisse dürfen weiterhin über `ActivityLogService` geschrieben werden.

Attachments bilden eine Sondergrenze: Dateisystembytes und SQLite können keine gemeinsame ACID-Transaktion bilden. Der Application Service verwendet bei fehlgeschlagenem Metadaten-/Activity-Commit eine kompensierende Dateilöschung. Der Infrastructure-Test erzwingt genau diesen Teilfehler und prüft beide Seiten der Kompensation.

Siehe `docs/adr/ADR-003-transactional-lightweight-activity-history.md`.

## 10. Kommentierungsstandard

Kommentare sollen Entscheidungen und nicht offensichtliche Randbedingungen erklären.

Öffentliche Klassen und relevante öffentliche Methoden erhalten XML-Dokumentation (`///`).

Inline-Kommentare (`//`) sind sinnvoll für:

- Sicherheitsgrenzen,
- Transaktionsgrenzen,
- Recovery-Verhalten,
- absichtliche Idempotenz,
- ungewöhnliche SQLite-/Windows-Randbedingungen,
- Architekturentscheidungen, die beim Refactoring leicht versehentlich zerstört würden.

Nicht sinnvoll sind Kommentare, die lediglich den unmittelbar lesbaren Code wiederholen.

## 11. UI-Entwicklung

Neue größere Desktop-Funktionen werden nicht direkt in `MainForm` eingebaut.

Bevorzugt:

- fokussierte Dialoge,
- später bei wachsender Komplexität eigene Controls/Presenter/ViewModels,
- Application Services als einzige Use-Case-Grenze.

`MainForm` soll primär Navigation, Selektion und Host-Koordination übernehmen.

## 12. Fehlerbehandlung

Grundsätze:

- Domain-/Application-Regeln sollen früh und verständlich fehlschlagen,
- Repositories prüfen betroffene Zeilen und Concurrency,
- Benutzerfehler werden in der UI verständlich angezeigt,
- technische Ausnahmen werden nicht in Erfolg umgedeutet,
- destructive operations benötigen sichtbare Bestätigung,
- Restore erzeugt vor dem Austausch des Live-Zustands ein Safety Backup.

### Optimistic Concurrency für Hosts

Project- und Entry-Mutationen verwenden eine zweistufige Optimistic-Concurrency-Garantie:

1. Der Host gibt die `Version` weiter, die gemeinsam mit dem angezeigten Entity geladen wurde.
2. Der Application Service vergleicht diese Caller-Version mit dem aktuell gelesenen Entity.
3. Das SQLite-Repository wiederholt den Vergleich atomar im bedingten `UPDATE`, um das Rennen zwischen Application-Read und tatsächlichem Write zu schließen.

Wichtig für alle heutigen und zukünftigen Hosts:

- Die erwartete Version darf **nicht** unmittelbar vor dem Speichern neu aus der Datenbank geholt werden. Das würde einen stale Editor wieder als aktuell erscheinen lassen und die Schutzwirkung umgehen.
- Bei `OptimisticConcurrencyException` dürfen ungespeicherte Nutzereingaben nicht still verworfen werden.
- Der Host soll verständlich erklären, dass sich der Datensatz inzwischen geändert hat, und anschließend einen bewussten Reload/Merge/Retry ermöglichen.
- Ein automatischer Retry mit denselben stale Feldern gegen die neueste Version ist kein zulässiger Konflikt-Handler.
- Profile sollen dieselbe Application-Exception behandeln und keine SQLite-spezifischen Concurrency-Ausnahmen voraussetzen.

Der erste WinForms-Host übergibt deshalb beim Entry-Save die Version des aktuell ausgewählten Entries und lässt den Editorinhalt bei einem Konflikt stehen.

## 13. Backup und Datenhoheit

Backup umfasst Datenbank und kontrollierten Attachment-Speicher.

Restore arbeitet über validiertes Staging und schützt gegen Path Traversal. Änderungen an Storage, neuen Dateitypen oder neuen persistenten Bereichen müssen deshalb immer beantworten:

> Ist dieser Zustand im Full Backup enthalten und nach Restore vollständig wiederherstellbar?

Wenn nicht, ist die Core-Funktion nicht fertig.

## 14. Tests vor Merge

Mindestens dieselben Gates wie in CI ausführen:

```powershell
dotnet restore SASD-Workbench.slnx
dotnet build SASD-Workbench.slnx --configuration Release --no-restore

dotnet run --project tests/SASD.Workbench.Domain.Tests/SASD.Workbench.Domain.Tests.csproj --configuration Release --no-build
dotnet run --project tests/SASD.Workbench.Application.Tests/SASD.Workbench.Application.Tests.csproj --configuration Release --no-build
dotnet run --project tests/SASD.Workbench.Infrastructure.Tests/SASD.Workbench.Infrastructure.Tests.csproj --configuration Release --no-build
dotnet run --project tests/SASD.Workbench.SmokeTests/SASD.Workbench.SmokeTests.csproj --configuration Release --no-build
```

Für neue Core-Funktionen gilt zusätzlich:

- reine Domain-/Use-Case-Regel möglichst im schnellsten passenden Testprojekt absichern,
- SQLite-/File-I/O-Vertrag als Infrastructure-Test absichern,
- breiten Smoke-Test nur dort erweitern, wo die vollständige Composition/Recovery-Kette relevant ist,
- Migration/Roundtrip prüfen,
- Fehlerpfad prüfen,
- Activity-Auswirkung prüfen,
- Backup/Restore-Auswirkung prüfen,
- Idempotenz prüfen, wenn relevant,
- Profilneutralität prüfen.

Details stehen in `070_Test_Strategy.md`.

## 15. Git-/PR-Arbeitsweise

Für zusammenhängende Architekturänderungen einen eigenen Feature-/Test-Branch verwenden.

Ein PR soll erklären:

- Problem und Ziel,
- Architekturentscheidung,
- geänderte Schichten,
- Schemaänderungen oder explizit „keine Schemaänderung“,
- Tests/CI,
- bekannte Grenzen.

Nicht mergen, solange Release-Build, geschichtete Tests oder der V1-Core-Smoke-Test fehlschlagen.

## 16. Definition of Done für gemeinsame Core-Funktionen

Eine Funktion ist erst fertig, wenn:

- Schichtengrenzen eingehalten sind,
- öffentliche APIs ausreichend dokumentiert sind,
- persistente Daten roundtrip-getestet sind,
- Fehlerpfade berücksichtigt sind,
- Activity/Backup/Restore nicht inkonsistent werden,
- der schnellste sinnvolle Regressionstest existiert,
- CI grün ist,
- Roadmap/CHANGELOG/Dokumentation den echten Stand wiedergeben,
- keine unnötige profilspezifische Logik in den Core gezogen wurde.
